using UnityEngine;

// Gives a Renderer a random color sampled from a Gradient each time Apply() is
// called, via MaterialPropertyBlock — no material instancing, batching stays intact.
public class RandomGradientColor : MonoBehaviour
{
    [SerializeField] Renderer targetRenderer;
    [SerializeField] Gradient gradient;
    [Tooltip("Shader property the color is written to. URP Lit/custom-lit materials usually use _BaseColor.")]
    [SerializeField] string colorPropertyName = "_BaseColor";

    static MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
    }

    // Lets a spawner (e.g. BubbleField, from a GameInfo appearance override)
    // swap the gradient per-instance without needing a different prefab.
    public void SetGradient(Gradient newGradient)
    {
        gradient = newGradient;
    }

    public void Apply()
    {
        if (gradient == null || targetRenderer == null) return;

        propertyBlock ??= new MaterialPropertyBlock();
        Color color = gradient.Evaluate(Random.value);
        color.a = 1f; // this only picks a hue — a gradient's separate alpha track shouldn't leak through

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyName, color);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
