using UnityEngine;
using DG.Tweening;

public class InverseGlass_Behaviour : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.1f;
    
    [Header("Scale Animation")]
    [SerializeField] private float dragScaleMax = 1.15f;
    [SerializeField] private float dragScaleMin = 0.95f;
    [SerializeField] private float dragScaleDuration = 1.5f;
    [SerializeField] private float dragScaleEase = Ease.InOutSine;
    
    [Header("Placement Animation")]
    [SerializeField] private float placementScaleMax = 1.1f;
    [SerializeField] private float placementScaleMin = 0.9f;
    [SerializeField] private float placementDuration = 0.6f;
    [SerializeField] private float placementEase = Ease.OutElastic;
    
    private Vector3 originalScale;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Tweener scaleTweener;
    private Tweener placementTweener;

    void Start()
    {
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ensure Rigidbody2D is set to kinematic for smooth dragging
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && IsClickedOnSprite())
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
        
        if (isDragging)
        {
            DragUpdate();
        }
    }

    bool IsClickedOnSprite()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }

    void StartDrag()
    {
        isDragging = true;
        
        // Calculate offset between mouse and sprite center
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;
        
        // Kill any existing coroutines
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        if (placementCoroutine != null) StopCoroutine(placementCoroutine);
        
        // Start breathing/pulsing animation during drag
        scaleCoroutine = StartCoroutine(DragPulseCoroutine());
    }

    void DragUpdate()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = mouseWorldPos + dragOffset;
        targetPos.z = transform.position.z; // Maintain Z position
        
        // Smooth movement toward target
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / dragSmoothTime);
    }

    void EndDrag()
    {
        isDragging = false;
        
        // Kill the drag pulse animation
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        // Play placement animation
        if (placementCoroutine != null) StopCoroutine(placementCoroutine);
        placementCoroutine = StartCoroutine(PlacementAnimationCoroutine());
    }

    IEnumerator DragPulseCoroutine()
    {
        while (isDragging)
        {
            // Scale up
            yield return ScaleToCoroutine(originalScale * dragScaleMax, dragScaleDuration / 2f);
            
            if (!isDragging) break;
            
            // Scale down
            yield return ScaleToCoroutine(originalScale * dragScaleMin, dragScaleDuration / 2f);
        }
    }

    IEnumerator PlacementAnimationCoroutine()
    {
        // Reset to original scale first
        transform.localScale = originalScale;
        
        // Scale up with ease
        yield return ScaleToCoroutine(originalScale * placementScaleMax, placementDuration * 0.6f);
        
        // Scale down smoothly
        yield return ScaleToCoroutine(originalScale, placementDuration * 0.4f);
    }
    
    IEnumerator ScaleToCoroutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth easing (SmoothStep)
            float smoothT = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    void OnDestroy()
    {
        // Clean up coroutines when object is destroyed
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        if (placementCoroutine != null) StopCoroutine(placementCoroutine);
    }
}

