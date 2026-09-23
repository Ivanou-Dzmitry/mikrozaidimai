using System;
using UnityEngine;
using UnityEngine.Localization;

// Base class for every mini-game's main controller. Provides the scaffolding shared
// by all games (score, record, elapsed time, description, play/pause/win/lose/cancel state).
// Game-specific scripts (Assets/Scripts/GameXX) derive from this and add their own rules,
// calling AddScore/SetScore and Play/Pause/Resume/Win/Lose/Cancel as the game dictates.
public abstract class GameSession : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] string gameId;
    [SerializeField] LocalizedString description;

    public string GameId => gameId;

    // Synchronous resolve — fine as long as the string table is already loaded.
    // Switch to description.StringChanged if a table can still be loading when this is read.
    // Falls back instead of throwing if the reference is empty or unresolved.
    public string Description => LocalizationUtil.Resolve(description, "No description yet");

    public int Score { get; private set; }
    public int Record { get; private set; }
    public float ElapsedTime { get; private set; }
    // How many tries the player has made this session, right or wrong — what
    // counts as one is up to each game (a shot in Game 1, a 3-word round in Game 2).
    public int Attempts { get; private set; }
    // The attempts that failed (a wrong shot in Game 1, a failed/skipped 3-word round in Game 2).
    public int Misses { get; private set; }
    public GameState State { get; private set; } = GameState.None;

    public event Action<int> ScoreChanged;
    public event Action<int> AttemptsChanged;
    public event Action<int> MissesChanged;
    public event Action<GameState> StateChanged;

    protected virtual void Awake()
    {
        // launched via a game button that carried a GameInfo -> use that
        // identity instead of whatever's serialized on this scene's own object,
        // so the same shared scene can run as any of several games
        if (GameLaunchConfig.Current != null)
        {
            gameId = GameLaunchConfig.Current.gameId;
            description = GameLaunchConfig.Current.description;
        }

        Record = Records.GetRecord(gameId);
    }

    // Auto-starts the session as soon as the scene loads. Override this in a subclass
    // if a game needs a menu/countdown state before Play (e.g. skip calling base.Start()).
    protected virtual void Start()
    {
        Play();
    }

    protected virtual void Update()
    {
        if (State == GameState.Play)
        {
            ElapsedTime += Time.deltaTime;
        }
    }

    public void AddScore(int amount)
    {
        SetScore(Score + amount);
    }

    public void SetScore(int value)
    {
        Score = value;
        ScoreChanged?.Invoke(Score);
    }

    // One call per try: pass missed = true when it failed, so Attempts and
    // Misses can't drift apart (Attempts - Misses = successful tries).
    public void AddAttempt(bool missed = false)
    {
        Attempts++;
        AttemptsChanged?.Invoke(Attempts);

        if (missed)
        {
            Misses++;
            MissesChanged?.Invoke(Misses);
        }
    }

    public void Play()
    {
        SetState(GameState.Play);
    }

    public void Pause()
    {
        if (State == GameState.Play) SetState(GameState.Pause);
    }

    public void Resume()
    {
        if (State == GameState.Pause) SetState(GameState.Play);
    }

    public void Win() => EndSession(GameState.Win);
    public void Lose() => EndSession(GameState.Lose);
    public void Cancel() => EndSession(GameState.Cancel);

    void EndSession(GameState endState)
    {
        SetState(endState);

        if (Records.TrySetRecord(gameId, Score))
        {
            Record = Score;
        }

        Log.Info($"[{gameId}] session ended: {endState}, score={Score}, time={ElapsedTime:F1}s");
    }

    void SetState(GameState newState)
    {
        if (State == newState) return;
        GameState previous = State;
        State = newState;
        Debug.Log($"[{gameId}] State: {previous} -> {newState}");
        StateChanged?.Invoke(State);
    }
}
