using System.Collections;
using UnityEngine;

// Compresses a spring mesh via a blend shape ("Compressed", weight 0-100)
// instead of scaling — the artist defines exactly how compression looks in
// Blender (coils bunch together, wire thickness stays correct), so there's no
// geometry distortion like a plain axis-scale would cause.
public class SpringVisual : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    [SerializeField] string blendShapeName = "Compressed";
    [SerializeField] float releaseDuration = 0.25f;
    [SerializeField] AnimationCurve releaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    int blendShapeIndex = -1;
    Coroutine releaseRoutine;

    void Awake()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (meshRenderer != null)
        {
            blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex < 0)
            {
                Debug.LogWarning($"[SpringVisual] Blend shape \"{blendShapeName}\" not found on {meshRenderer.sharedMesh.name}.", this);
            }
            else
            {
                // don't trust whatever weight the imported asset happened to
                // have baked in (e.g. the shape key slider was left at 1.0 in
                // Blender at export time) — always start fully relaxed
                SetWeight(0f);
            }
        }
    }

    // t: 0 = fully relaxed, 1 = fully compressed.
    public void SetCompression(float t)
    {
        if (blendShapeIndex < 0) return;

        CancelRelease();
        SetWeight(Mathf.Clamp01(t) * 100f);
    }

    // Animates back to fully relaxed — call this on launch/release.
    public void ReleaseSpring()
    {
        if (blendShapeIndex < 0) return;

        CancelRelease();
        releaseRoutine = StartCoroutine(AnimateRelease());
    }

    void CancelRelease()
    {
        if (releaseRoutine != null)
        {
            StopCoroutine(releaseRoutine);
            releaseRoutine = null;
        }
    }

    IEnumerator AnimateRelease()
    {
        float startWeight = meshRenderer.GetBlendShapeWeight(blendShapeIndex);
        float t = 0f;

        while (t < releaseDuration)
        {
            t += Time.deltaTime;
            float progress = releaseCurve.Evaluate(Mathf.Clamp01(t / releaseDuration));
            SetWeight(Mathf.Lerp(startWeight, 0f, progress));
            yield return null;
        }

        SetWeight(0f);
        releaseRoutine = null;
    }

    void SetWeight(float weight)
    {
        meshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
    }
}
