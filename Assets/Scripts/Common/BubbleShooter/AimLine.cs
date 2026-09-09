using UnityEngine;

// Dashed aim-guide line shown while dragging the shooter, hidden otherwise.
// Builds its own tiny dashed texture at runtime, so no texture asset is needed —
// but the BASE material must be assigned on the LineRenderer itself (Universal
// Render Pipeline/Unlit, Surface Type = Transparent), because Sprites/Default
// ignores texture tiling/scale entirely (sprite shaders assume atlas UVs, so
// they don't apply _MainTex_ST) — Dash Length would visibly do nothing on it.
[RequireComponent(typeof(LineRenderer))]
public class AimLine : MonoBehaviour
{
    [SerializeField] Color color = Color.green;
    [SerializeField] float width = 0.08f;
    [SerializeField] float dashLength = 0.25f;

    LineRenderer line;
    Material material;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.textureMode = LineTextureMode.Tile;
        line.widthMultiplier = width;

        // build on top of whatever material is assigned on the LineRenderer
        // (needs Transparent surface type for the dash gaps to actually be see-through)
        material = line.sharedMaterial != null
            ? new Material(line.sharedMaterial)
            : new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = BuildDashTexture();
        material.color = color;
        line.material = material;

        Hide();
    }

    void OnValidate()
    {
        if (material != null) material.color = color;
        if (line != null) line.widthMultiplier = width;
    }

    public void Show(Vector3 from, Vector3 to)
    {
        Show(new[] { from, to });
    }

    // Draws a (possibly bent) path, e.g. a shot that bounces off a wall —
    // one LineRenderer segment per pair of consecutive points.
    public void Show(Vector3[] points)
    {
        line.enabled = true;
        line.positionCount = points.Length;
        line.SetPositions(points);

        float length = 0f;
        for (int i = 1; i < points.Length; i++) length += Vector3.Distance(points[i - 1], points[i]);
        material.mainTextureScale = new Vector2(length / Mathf.Max(dashLength, 0.001f), 1f);
    }

    public void Hide()
    {
        line.enabled = false;
    }

    static Texture2D BuildDashTexture()
    {
        var tex = new Texture2D(4, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(new[] { Color.white, Color.white, Color.clear, Color.clear });
        tex.Apply();
        return tex;
    }
}
