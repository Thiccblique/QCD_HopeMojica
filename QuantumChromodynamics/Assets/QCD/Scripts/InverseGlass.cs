using UnityEngine;
using DG.Tweening;

public class InverseGlass : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothTime = 0.1f;
    
    [Header("Scale Animation")]
    [SerializeField] private float dragScaleMax = 1.15f;
    [SerializeField] private float dragScaleMin = 0.95f;
    [SerializeField] private float dragScaleDuration = 1.5f;
    [SerializeField] private Ease dragScaleEase = Ease.InOutSine;
    
    [Header("Placement Animation")]
    [SerializeField] private float placementScaleMax = 1.1f;
    [SerializeField] private float placementScaleMin = 0.9f;
    [SerializeField] private float placementDuration = 0.6f;
    [SerializeField] private Ease placementEase = Ease.OutElastic;

    [Header("Toggle Animation")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;
    [SerializeField] private float toggleDuration = 0.25f;
    [SerializeField] private Ease toggleEase = Ease.OutBack;
    [SerializeField] private float hiddenScaleFactor = 0.01f;
    
    private Vector3 originalScale;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private Tweener scaleTweener;
    private Tweener placementTweener;
    private Tweener toggleTweener;
    private bool isVisible = true;

    void Start()
    {
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
      
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleVisibility();
        }

        HandleInput();
    }

    void HandleInput()
    {
        if (!isVisible)
        {
            return;
        }

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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        
        return hit != null && hit.gameObject == gameObject;
    }

    void StartDrag()
    {
        isDragging = true;
        
        // Calculate offset between mouse and sprite center
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;
        
        // Kill any existing tweens
        scaleTweener?.Kill();
        placementTweener?.Kill();
        toggleTweener?.Kill();
        
        // Start breathing/pulsing animation during drag
        StartDragPulse();
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
        scaleTweener?.Kill();
        
        // Play placement animation
        PlayPlacementAnimation();
    }

    void ToggleVisibility()
    {
        if (isDragging)
        {
            EndDrag();
        }

        scaleTweener?.Kill();
        placementTweener?.Kill();
        toggleTweener?.Kill();

        if (isVisible)
        {
            isVisible = false;
            toggleTweener = transform.DOScale(originalScale * hiddenScaleFactor, toggleDuration)
                .SetEase(toggleEase)
                .OnComplete(() =>
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                        polygonCollider.enabled = false;
                    }
                });
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                polygonCollider.enabled = true;
            }
            transform.localScale = originalScale * hiddenScaleFactor;
            isVisible = true;
            toggleTweener = transform.DOScale(originalScale, toggleDuration)
                .SetEase(toggleEase);
        }
    }

    void StartDragPulse()
    {
        // Continuously pulse between scale values while dragging
        scaleTweener = transform.DOScale(originalScale * dragScaleMax, dragScaleDuration / 2f)
            .SetEase(dragScaleEase)
            .OnComplete(() =>
            {
                if (isDragging)
                {
                    scaleTweener = transform.DOScale(originalScale * dragScaleMin, dragScaleDuration / 2f)
                        .SetEase(dragScaleEase)
                        .OnComplete(() =>
                        {
                            if (isDragging)
                            {
                                StartDragPulse();
                            }
                        });
                }
            });
    }

    void PlayPlacementAnimation()
    {
        // Reset to original scale first
        transform.localScale = originalScale;
        
        // Bouncy placement animation
        placementTweener = transform.DOScale(originalScale * placementScaleMax, placementDuration * 0.6f)
            .SetEase(placementEase)
            .OnComplete(() =>
            {
                placementTweener = transform.DOScale(originalScale, placementDuration * 0.4f)
                    .SetEase(Ease.OutQuad);
            });
    }

    void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        scaleTweener?.Kill();
        placementTweener?.Kill();
        toggleTweener?.Kill();
    }
}
