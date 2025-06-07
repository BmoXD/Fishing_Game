using UnityEngine;
using UnityEngine.UI;

public class TooltipFollower : MonoBehaviour
{
    [Header("Assign the tooltip panel (child)")]
    public RectTransform tooltipPanel;
    [Header("Optional: Padding from screen edge")]
    public Vector2 screenPadding = new Vector2(8, 8);

    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private Vector2 defaultAnchor;
    private Vector2 defaultPivot;

    void Awake()
    {
        if (tooltipPanel == null && transform.childCount > 0)
            tooltipPanel = transform.GetChild(0).GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
            canvasRect = rootCanvas.GetComponent<RectTransform>();

        if (tooltipPanel != null)
        {
            defaultAnchor = tooltipPanel.anchorMin;
            defaultPivot = tooltipPanel.pivot;
        }
    }

    void Update()
    {
        FollowMouse();
        EdgeDetectAndAdjust();
    }

    void FollowMouse()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out mousePos);
        ((RectTransform)transform).anchoredPosition = mousePos;
    }

    void EdgeDetectAndAdjust()
    {
        if (tooltipPanel == null || canvasRect == null) return;

        // Predict tooltip position as if at default anchor/pivot
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out mousePos);

        // Set to default anchor/pivot for prediction
        tooltipPanel.anchorMin = defaultAnchor;
        tooltipPanel.anchorMax = defaultAnchor;
        tooltipPanel.pivot = defaultPivot;
        tooltipPanel.anchoredPosition = Vector2.zero;

        // Get panel corners in canvas space after prediction
        Vector3[] corners = new Vector3[4];
        tooltipPanel.GetWorldCorners(corners);
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        Vector2 canvasMin = canvasRect.TransformPoint(canvasRect.rect.min);
        Vector2 canvasMax = canvasRect.TransformPoint(canvasRect.rect.max);

        Vector2 anchor = defaultAnchor;
        Vector2 pivot = defaultPivot;
        bool changed = false;

        // Left edge
        if (min.x < canvasMin.x + screenPadding.x)
        {
            anchor.x = 0;
            pivot.x = 0;
            changed = true;
        }
        // Right edge
        else if (max.x > canvasMax.x - screenPadding.x)
        {
            anchor.x = 1;
            pivot.x = 1;
            changed = true;
        }
        // Top edge
        if (max.y > canvasMax.y - screenPadding.y)
        {
            anchor.y = 1;
            pivot.y = 1;
            changed = true;
        }
        // Bottom edge
        else if (min.y < canvasMin.y + screenPadding.y)
        {
            anchor.y = 0;
            pivot.y = 0;
            changed = true;
        }

        // Only set anchors/pivots if changed, otherwise leave at default
        if (changed)
        {
            tooltipPanel.anchorMin = anchor;
            tooltipPanel.anchorMax = anchor;
            tooltipPanel.pivot = pivot;
            tooltipPanel.anchoredPosition = Vector2.zero;
        }
        else
        {
            tooltipPanel.anchorMin = defaultAnchor;
            tooltipPanel.anchorMax = defaultAnchor;
            tooltipPanel.pivot = defaultPivot;
            tooltipPanel.anchoredPosition = Vector2.zero;
        }
    }
}
