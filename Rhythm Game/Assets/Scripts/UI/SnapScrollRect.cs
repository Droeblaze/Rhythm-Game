using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class SnapScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Snap Settings")]
    public float snapSpeed = 10f;
    public float snapThreshold = 0.1f;
    
    [Header("Layout Settings")]
    [Tooltip("If true, automatically calculate spacing from layout. If false, use manual itemSpacing.")]
    public bool autoCalculateSpacing = true;
    
    [Tooltip("Manual spacing between items (only used if autoCalculateSpacing is false)")]
    public float itemSpacing = 1000f;
    
    private ScrollRect scrollRect;
    public RectTransform contentRect { get; private set; }
    private RectTransform viewportRect;
    private bool isSnapping = false;
    private float targetPosition;
    private int currentIndex = 0;
    private float calculatedSpacing;
    
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentRect = scrollRect.content;
        viewportRect = scrollRect.viewport;
        
        CalculateSpacing();
        
        // Snap to first item on start
        SnapToIndex(0);
    }
    
    void CalculateSpacing()
    {
        if (autoCalculateSpacing && contentRect.childCount >= 2)
        {
            // Calculate actual spacing between first two items
            RectTransform firstChild = contentRect.GetChild(0) as RectTransform;
            RectTransform secondChild = contentRect.GetChild(1) as RectTransform;
            
            if (firstChild != null && secondChild != null)
            {
                calculatedSpacing = Mathf.Abs(secondChild.anchoredPosition.y - firstChild.anchoredPosition.y) / 2f;
                Debug.Log($"Auto-calculated spacing: {calculatedSpacing}");
            }
            else
            {
                calculatedSpacing = itemSpacing / 2f;
            }
        }
        else
        {
            calculatedSpacing = itemSpacing / 2f;
        }
    }
    
    void Update()
    {
        if (isSnapping)
        {
            // Smoothly move to target position
            float newPos = Mathf.Lerp(contentRect.anchoredPosition.y, targetPosition, Time.deltaTime * snapSpeed);
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, newPos);
            
            // Stop snapping when close enough
            if (Mathf.Abs(contentRect.anchoredPosition.y - targetPosition) < snapThreshold)
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, targetPosition);
                isSnapping = false;
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isSnapping = false;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToNearest();
    }
    
    private void SnapToNearest()
    {
        if (contentRect.childCount == 0) return;
        
        // Calculate which snap position we're closest to based on spacing
        float currentY = contentRect.anchoredPosition.y;
        int nearestIndex = Mathf.RoundToInt(currentY / calculatedSpacing);
        
        // Clamp to valid range
        nearestIndex = Mathf.Clamp(nearestIndex, 0, contentRect.childCount - 1);
        
        SnapToIndex(nearestIndex);
    }
    
    public void SnapToIndex(int index)
    {
        if (contentRect.childCount == 0) return;
        
        index = Mathf.Clamp(index, 0, contentRect.childCount - 1);
        currentIndex = index;
        
        targetPosition = index * calculatedSpacing;
        isSnapping = true;
    }
    
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    
    public void NavigateUp()
    {
        if (currentIndex > 0)
        {
            SnapToIndex(currentIndex - 1);
        }
    }
    
    public void NavigateDown()
    {
        if (currentIndex < contentRect.childCount - 1)
        {
            SnapToIndex(currentIndex + 1);
        }
    }
    
    // Call this if items are added/removed dynamically
    public void RefreshSpacing()
    {
        CalculateSpacing();
    }
}