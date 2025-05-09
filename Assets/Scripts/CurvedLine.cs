using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CurvedLine : MonoBehaviour
{
    [Header("Line Connection Points")]
    [SerializeField] private Transform startPoint;  // Fishing rod tip
    [SerializeField] private Transform endPoint;    // Bobber

    [Header("Line Properties")]
    [Tooltip("Higher values create more curve/sag in the line")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float slack = 0.15f;
    
    [Tooltip("Number of segments in the line (higher = smoother curve)")]
    [Range(2, 50)]
    [SerializeField] private int lineSegments = 20;
    
    [Tooltip("Maximum line thickness")]
    [SerializeField] private float lineWidth = 0.03f;
    
    [Tooltip("Apply subtle animation to the line")]
    [SerializeField] private bool animateLine = true;
    [SerializeField] private float waveSpeed = 1.5f;
    [SerializeField] private float waveHeight = 0.05f;

    // Components
    private LineRenderer lineRenderer;

    private void Awake()
    {
        // Get the LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        
        // Set up basic line renderer properties
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = lineSegments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth * 0.7f; // Slight taper toward the end
        
        // You can assign a material in the inspector or do it here
        // lineRenderer.material = lineMaterial;
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null)
            return;

        UpdateLinePositions();
    }

    private void UpdateLinePositions()
    {
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        
        // Calculate the line positions with curve based on slack
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            
            // Calculate base curved position
            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);
            
            // Apply curve (slack)
            // The slack creates a downward curve in the middle of the line
            float curveHeight = slack * Vector3.Distance(startPos, endPos) * 0.5f;
            float curveValue = Mathf.Sin(t * Mathf.PI) * curveHeight;
            
            // Apply the curve in the downward direction (assuming Y is up)
            Vector3 curvedPos = basePos + Vector3.down * curveValue;
            
            // Add subtle animation if enabled
            if (animateLine)
            {
                float waveOffset = Mathf.Sin((t * 8f + Time.time * waveSpeed) * Mathf.PI) * waveHeight;
                float waveMultiplier = t * (1 - t) * 4; // Maximum effect in the middle of the line
                
                // Apply wave animation perpendicular to the line direction
                Vector3 lineDirection = (endPos - startPos).normalized;
                Vector3 waveDirection = Vector3.Cross(lineDirection, Vector3.up).normalized;
                
                curvedPos += waveDirection * waveOffset * waveMultiplier;
            }
            
            lineRenderer.SetPosition(i, curvedPos);
        }
    }

    // Public methods to set start and end points at runtime
    public void SetStartPoint(Transform point)
    {
        startPoint = point;
    }

    public void SetEndPoint(Transform point)
    {
        endPoint = point;
    }

    // Method to adjust slack at runtime
    public void SetSlack(float newSlack)
    {
        slack = Mathf.Clamp01(newSlack);
    }
}