using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaVerticalStack : MonoBehaviour
{
    [Header("Stack panels (top to bottom)")]
    [SerializeField] RectTransform topPanel;
    [SerializeField] RectTransform middlePanel;
    [SerializeField] RectTransform bottomPanel;

    [Header("Panel heights (px in Canvas coordinate system)")]
    [SerializeField] float topHeight = 120f;
    [SerializeField] bool useBottomPanel = false;
    [SerializeField] float bottomHeight = 120f;

    RectTransform rectTransform;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;

    // how far the top/bottom panels should extend past the safe area so their background covers the notch/gesture bar
    float topInset;
    float bottomInset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        // On some Android devices safe area changes without a RectTransform resize
        // (e.g. auto-hiding gesture bar), so also check it every frame.
        ApplyIfChanged();
    }

    // Apply() itself changes this RectTransform's anchors/offsets, which makes
    // Unity fire OnRectTransformDimensionsChange again — without this guard
    // that's an infinite Apply->change->Apply loop (freezes the editor solid,
    // e.g. the instant a prefab using this script is dropped into a scene).
    void OnRectTransformDimensionsChange()
    {
        ApplyIfChanged();
    }

    void ApplyIfChanged()
    {
        if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            Apply();
        }
    }

    public void SetHeights(float top, float bottom, bool enableBottom)
    {
        topHeight = top;
        bottomHeight = bottom;
        useBottomPanel = enableBottom;
        Apply();
    }

    void Apply()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (Screen.width <= 0 || Screen.height <= 0) return;

        // Mark as up to date BEFORE touching the RectTransform: ApplySafeArea()
        // below changes anchors/offsets, which synchronously re-enters
        // OnRectTransformDimensionsChange while we're still inside this call —
        // ApplyIfChanged's guard has to already see current values at that
        // point, or it recurses (and previously stack-overflowed).
        lastSafeArea = Screen.safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        ApplySafeArea();
        ApplyStack();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        float canvasHeight = GetCanvasHeight();
        topInset = (Screen.height - safeArea.yMax) / Screen.height * canvasHeight;
        bottomInset = safeArea.yMin / Screen.height * canvasHeight;
    }

    // canvas height in its own units — this component must sit directly on the Canvas,
    // so the parent rect always equals the full screen regardless of CanvasScaler
    float GetCanvasHeight()
    {
        RectTransform parentRect = rectTransform.parent as RectTransform;
        return parentRect != null ? parentRect.rect.height : Screen.height;
    }

    void ApplyStack()
    {
        float top = Mathf.Max(0f, topHeight);
        float bottom = (useBottomPanel && bottomPanel != null) ? Mathf.Max(0f, bottomHeight) : 0f;

        // safety net in case top+bottom don't fit into the safe area height (small screen, wrong inspector values)
        float availableHeight = rectTransform.rect.height;
        if (availableHeight > 0f && top + bottom > availableHeight)
        {
            top = Mathf.Min(top, availableHeight);
            bottom = Mathf.Min(bottom, availableHeight - top);
        }

        if (topPanel != null)
        {
            // background extends upward past the safe area by topInset to cover the notch/status bar;
            // topPanel.rect.height ends up being top + topInset — aligning content inside it
            // (to the panel's bottom edge) is handled outside this script
            topPanel.anchorMin = new Vector2(0f, 1f);
            topPanel.anchorMax = new Vector2(1f, 1f);
            topPanel.pivot = new Vector2(0.5f, 1f);
            topPanel.anchoredPosition = new Vector2(0f, topInset);
            topPanel.sizeDelta = new Vector2(0f, top + topInset);
        }

        if (bottomPanel != null)
        {
            bottomPanel.gameObject.SetActive(useBottomPanel);
            if (useBottomPanel)
            {
                bottomPanel.anchorMin = new Vector2(0f, 0f);
                bottomPanel.anchorMax = new Vector2(1f, 0f);
                bottomPanel.pivot = new Vector2(0.5f, 0f);
                bottomPanel.anchoredPosition = new Vector2(0f, -bottomInset);
                bottomPanel.sizeDelta = new Vector2(0f, bottom + bottomInset);
            }
        }

        if (middlePanel != null)
        {
            middlePanel.anchorMin = new Vector2(0f, 0f);
            middlePanel.anchorMax = new Vector2(1f, 1f);
            middlePanel.pivot = new Vector2(0.5f, 0.5f);
            middlePanel.offsetMin = new Vector2(0f, bottom);
            middlePanel.offsetMax = new Vector2(0f, -top);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Apply();
        };
    }
#endif
}
