using System.Collections.Generic;
using UnityEngine;

// Owns the grid of WordBubbles for one game: spawns the initial field inside
// PlayField's bounds, resolves hits from the current ShooterProjectile, grows
// the field on wrong answers, and reports score/win/lose to the shared GameSession.
//
// PlayField only defines the bounds the grid is laid out inside (resize the
// box, the grid adapts) — it is NOT a physical wall. Real bouncing comes from
// 3 thin wall colliders (left/right/top) generated at Start() around those
// bounds, since a fast Rigidbody starting fully enclosed inside one big convex
// collider doesn't reliably bounce off its inner faces in practice.
public class BubbleField : MonoBehaviour
{
    public static BubbleField Instance { get; private set; }

    [Header("Wiring")]
    [SerializeField] GameSession session;
    [Tooltip("Used only when launched directly (e.g. testing in the editor) — a game button's GameLaunchConfig.Current.wordSet takes priority.")]
    [SerializeField] WordSet wordSet;
    [Tooltip("Used only when launched directly (e.g. testing in the editor) — a game button's GameLaunchConfig.Current.bubblePrefab takes priority.")]
    [SerializeField] WordBubble bubblePrefab;
    [SerializeField] ShooterProjectile projectilePrefab;
    [SerializeField] Transform shooterOrigin;
    [SerializeField] BoxCollider playField;
    [SerializeField] PhysicsMaterial wallMaterial;
    [Tooltip("Parent for procedurally spawned bubbles/projectiles (e.g. GameplayObjects). Falls back to this object if left empty.")]
    [SerializeField] Transform spawnParent;

    [Header("Grid")]
    [SerializeField] int columns = 5;
    [SerializeField] int initialRows = 2;
    [SerializeField] int maxRows = 8;
    [SerializeField] float wallThickness = 0.5f;
    [Tooltip("Vertical row spacing as a multiple of the horizontal cell size. 1 = square spacing; ~0.87 = tight hex packing; raise/lower to match your bubble mesh's actual proportions.")]
    [SerializeField] float rowSpacingMultiplier = 1f;

    [Header("Safety net")]
    [Tooltip("If the launched projectile ends up this far outside the play field bounds, it's destroyed and a fresh one is spawned.")]
    [SerializeField] float outOfBoundsMargin = 2f;

    [Header("Feedback")]
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip wrongSound;
    [SerializeField] ParticleSystem correctEffectPrefab;
    [SerializeField] ParticleSystem wrongEffectPrefab;

    [Header("Reachability")]
    [Tooltip("Max launch angle from straight up, in degrees — must match ShooterAimController's limit, since it reads this value from here.")]
    [SerializeField] float maxAimAngle = 45f;
    [SerializeField] int reachabilityBounces = 3;
    [SerializeField] int reachabilityAngleSamples = 25;

    readonly List<WordBubble> activeBubbles = new List<WordBubble>();
    readonly List<WordPair> shuffledPairs = new List<WordPair>();
    readonly List<bool[]> rowOccupancy = new List<bool[]>(); // per row, which columns are filled

    int rowCount;
    int wrongAnswerColumn; // how many cells of the current growth row have been filled
    int bubbleSpawnCounter;
    float cellSize;
    float rowSpacing;
    ShooterProjectile currentProjectile;

    public Vector3 ShooterOrigin => shooterOrigin.position;
    public Transform ShooterTransform => shooterOrigin;
    public ShooterProjectile CurrentProjectile => currentProjectile;
    public GameSession Session => session;
    public float MaxAimAngle => maxAimAngle;

    Transform SpawnParent => spawnParent != null ? spawnParent : transform;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (currentProjectile == null || !currentProjectile.Launched) return;

        Bounds bounds = playField.bounds;
        bounds.Expand(outOfBoundsMargin * 2f); // Expand grows by the amount on each side, so double the margin

        if (!bounds.Contains(currentProjectile.transform.position))
        {
            Destroy(currentProjectile.gameObject);
            SpawnProjectile();
        }
    }

    void Start()
    {
        if (GameLaunchConfig.Current != null && GameLaunchConfig.Current.wordSet != null)
        {
            wordSet = GameLaunchConfig.Current.wordSet;
        }

        if (GameLaunchConfig.Current != null && GameLaunchConfig.Current.bubblePrefab != null)
        {
            bubblePrefab = GameLaunchConfig.Current.bubblePrefab;
        }

        if (GameLaunchConfig.Current != null && GameLaunchConfig.Current.appearance.rowSpacingMultiplier > 0f)
        {
            rowSpacingMultiplier = GameLaunchConfig.Current.appearance.rowSpacingMultiplier;
        }

        cellSize = playField.bounds.size.x / columns;
        rowSpacing = cellSize * rowSpacingMultiplier;

        playField.isTrigger = true; // bounds/layout reference only, walls below do the actual bouncing
        CreateWalls();

        RefillShuffledPairs();

        for (int i = 0; i < initialRows; i++) SpawnRow();

        SpawnProjectile();
    }

    void CreateWalls()
    {
        Bounds b = playField.bounds;

        CreateWall("Wall_Left",
            new Vector3(b.min.x - wallThickness / 2f, b.center.y, b.center.z),
            new Vector3(wallThickness, b.size.y, b.size.z));

        CreateWall("Wall_Right",
            new Vector3(b.max.x + wallThickness / 2f, b.center.y, b.center.z),
            new Vector3(wallThickness, b.size.y, b.size.z));

        CreateWall("Wall_Top",
            new Vector3(b.center.x, b.max.y + wallThickness / 2f, b.center.z),
            new Vector3(b.size.x + wallThickness * 2f, wallThickness, b.size.z));
    }

    void CreateWall(string wallName, Vector3 position, Vector3 size)
    {
        var wall = new GameObject(wallName);
        wall.transform.SetParent(SpawnParent, false);
        wall.transform.position = position;

        var collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
        if (wallMaterial != null) collider.material = wallMaterial;
    }

    void RefillShuffledPairs()
    {
        shuffledPairs.Clear();
        shuffledPairs.AddRange(wordSet.pairs);

        for (int i = shuffledPairs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledPairs[i], shuffledPairs[j]) = (shuffledPairs[j], shuffledPairs[i]);
        }
    }

    WordPair NextPair()
    {
        if (shuffledPairs.Count == 0) RefillShuffledPairs();

        WordPair pair = shuffledPairs[0];
        shuffledPairs.RemoveAt(0);
        return pair;
    }

    // odd rows have one less bubble, shifted half a cell over — classic
    // bubble-shooter checkerboard/hex packing (5, 4, 5, 4, ...).
    void SpawnRow()
    {
        if (rowCount >= maxRows)
        {
            session.Lose();
            return;
        }

        int rowColumns = IsOffsetRow(rowCount) ? columns - 1 : columns;

        for (int c = 0; c < rowColumns; c++)
        {
            SpawnBubbleAt(rowCount, c);
        }

        rowCount++;
    }

    static bool IsOffsetRow(int row) => row % 2 == 1;

    // row 0 sits at the top-left of the box; each further row (spawned on
    // wrong answers) goes further down, closer to the shooter — that's the
    // "field grows / bubbles get too low" escalation.
    Vector3 GridPosition(int row, int column)
    {
        Bounds b = playField.bounds;
        float rowShift = IsOffsetRow(row) ? cellSize / 2f : 0f;
        float x = b.min.x + cellSize * (column + 0.5f) + rowShift;
        float y = b.max.y - rowSpacing / 2f - row * rowSpacing;
        return new Vector3(x, y, b.center.z);
    }

    void SpawnBubbleAt(int row, int column)
    {
        EnsureRowOccupancyExists(row);

        WordPair pair = NextPair();
        WordBubble bubble = Instantiate(bubblePrefab, GridPosition(row, column), Quaternion.identity, SpawnParent);
        Gradient gradientOverride = GameLaunchConfig.Current != null ? GameLaunchConfig.Current.appearance.bubbleGradient : null;
        bubble.Setup(pair, row, column, gradientOverride);
        bubble.name = $"sphere_{pair.target}_{++bubbleSpawnCounter}";

        activeBubbles.Add(bubble);
        rowOccupancy[row][column] = true;
    }

    void EnsureRowOccupancyExists(int row)
    {
        while (rowOccupancy.Count <= row)
        {
            int rowColumns = IsOffsetRow(rowOccupancy.Count) ? columns - 1 : columns;
            rowOccupancy.Add(new bool[rowColumns]);
        }
    }

    // Finds the first empty cell (top row first, left to right) left behind by
    // a correctly-matched bubble that was removed. Only looks at fully-formed
    // rows (0..rowCount-1) — the current in-progress growth row (rowCount)
    // has genuinely-not-yet-spawned columns that would otherwise look like
    // gaps too, double-filling them out of order with the growth counter below.
    bool TryFindEmptyCell(out int row, out int column)
    {
        for (int r = 0; r < rowCount; r++)
        {
            bool[] occupied = rowOccupancy[r];
            for (int c = 0; c < occupied.Length; c++)
            {
                if (!occupied[c])
                {
                    row = r;
                    column = c;
                    return true;
                }
            }
        }

        row = 0;
        column = 0;
        return false;
    }

    void SpawnProjectile()
    {
        WordBubble target = PickReachableTarget();
        currentProjectile = Instantiate(projectilePrefab, shooterOrigin.position, Quaternion.identity, SpawnParent);
        currentProjectile.Setup(target.Pair);
    }

    // Only ever assigns a word whose bubble can actually be hit from some angle
    // within +/- maxAimAngle (accounting for wall bounces) — otherwise the
    // player could know the right answer and have no way to land it, a real
    // deadlock since (unlike color-matching bubble shooters) each word has
    // exactly one correct target, no substitute bubble will do.
    WordBubble PickReachableTarget()
    {
        var reachable = new List<WordBubble>();
        foreach (WordBubble bubble in activeBubbles)
        {
            if (IsReachable(bubble)) reachable.Add(bubble);
        }

        // fall back to "any bubble" only if the reachability probe itself found
        // nothing at all (should be rare) — better an imperfect shot than none
        List<WordBubble> pool = reachable.Count > 0 ? reachable : activeBubbles;
        return pool[Random.Range(0, pool.Count)];
    }

    bool IsReachable(WordBubble target)
    {
        SphereCollider sphere = projectilePrefab.GetComponent<SphereCollider>();
        float radius = sphere != null ? sphere.radius : 0.1f;

        for (int i = 0; i < reachabilityAngleSamples; i++)
        {
            float t = reachabilityAngleSamples <= 1 ? 0.5f : (float)i / (reachabilityAngleSamples - 1);
            float angle = Mathf.Lerp(-maxAimAngle, maxAimAngle, t);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f);

            if (PathHitsTarget(shooterOrigin.position, dir, radius, target)) return true;
        }

        return false;
    }

    // Traces the same reflect-off-walls path the aim line predicts, and reports
    // whether the FIRST bubble it would actually hit is this specific target
    // (if some other bubble blocks the way first, this angle doesn't count).
    bool PathHitsTarget(Vector3 origin, Vector3 dir, float radius, WordBubble target)
    {
        Vector3 pos = origin + dir * (radius + 0.02f);
        float remaining = playField.bounds.size.y + playField.bounds.size.x; // generous probe budget

        for (int bounce = 0; bounce <= reachabilityBounces && remaining > 0f; bounce++)
        {
            if (Physics.SphereCast(pos, radius, dir, out RaycastHit hit, remaining))
            {
                WordBubble hitBubble = hit.collider.GetComponentInParent<WordBubble>();
                if (hitBubble != null) return hitBubble == target;

                remaining -= hit.distance;
                pos = hit.point;
                dir = Vector3.Reflect(dir, hit.normal);
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public void ResolveHit(ShooterProjectile projectile, WordBubble target)
    {
        Vector3 hitPosition = target.transform.position;
        Destroy(projectile.gameObject);

        // matched by the target term, not by reference to one specific pair —
        // some terms (e.g. "yra") are correct for several different entries,
        // so any bubble with the same term as the target pair counts as a hit
        if (target.Pair.target == projectile.Pair.target)
        {
            activeBubbles.Remove(target);
            rowOccupancy[target.Row][target.Column] = false;
            Destroy(target.gameObject);
            session.AddScore(1);
            PlayFeedback(correctSound, correctEffectPrefab, hitPosition);

            if (activeBubbles.Count == 0)
            {
                session.Win();
                return;
            }
        }
        else
        {
            PlayFeedback(wrongSound, wrongEffectPrefab, hitPosition);

            if (HelperPanel.Instance != null)
            {
                string variant = projectile.Pair.GetVariant(Settings.Data.language);

                ExamplePair example = projectile.Pair.example != null ? projectile.Pair.example.GetRandom() : null;
                string exampleText = example != null ? $"{example.from} → {example.to}" : null;

                HelperPanel.Instance.ShowHint(variant, projectile.Pair.target, exampleText);
            }

            RegisterWrongAnswer();
        }

        SpawnProjectile();
    }

    // one place for match/no-match feedback so the player always knows whether a shot landed
    void PlayFeedback(AudioClip sound, ParticleSystem effectPrefab, Vector3 position)
    {
        if (sound != null) AudioManager.Instance.PlaySfx(sound);

        if (effectPrefab != null)
        {
            ParticleSystem effect = Instantiate(effectPrefab, position, Quaternion.identity, SpawnParent);
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }
    }

    // Adds exactly one bubble per wrong answer. Any gap left by a correctly
    // matched bubble gets filled first; only once there are no gaps left does
    // this grow the field, filling the current growth row cell by cell (a real,
    // non-overlapping grid slot) — once that row is full, the next wrong
    // answer starts a new row below it.
    void RegisterWrongAnswer()
    {
        if (TryFindEmptyCell(out int row, out int column))
        {
            SpawnBubbleAt(row, column);
            return;
        }

        if (rowCount >= maxRows)
        {
            session.Lose();
            return;
        }

        int rowColumns = IsOffsetRow(rowCount) ? columns - 1 : columns;

        SpawnBubbleAt(rowCount, wrongAnswerColumn);
        wrongAnswerColumn++;

        if (wrongAnswerColumn >= rowColumns)
        {
            wrongAnswerColumn = 0;
            rowCount++;
        }
    }
}
