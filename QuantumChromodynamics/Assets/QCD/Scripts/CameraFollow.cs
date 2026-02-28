using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [Range(0.01f, 1f)]
    public float smoothTime = 0.15f;

    [Header("Zoom")]
    public float minZoom = 7f;
    public float maxZoom = 12f;
    [Range(0.01f, 1f)]
    public float zoomTweenDuration = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float targetZoom;
    private Coroutine zoomTweenCoroutine;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            return;
        }

        targetZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        ApplyZoomValue(targetZoom);
    }

    void LateUpdate()
    {
        HandleZoomInput();

        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    void HandleZoomInput()
    {
        if (cam == null)
        {
            return;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        float newTargetZoom = scroll > 0f ? minZoom : maxZoom;
        if (Mathf.Approximately(newTargetZoom, targetZoom))
        {
            return;
        }

        targetZoom = newTargetZoom;
        StartZoomTween();
    }

    void StartZoomTween()
    {
        if (cam == null)
        {
            return;
        }

        if (zoomTweenCoroutine != null)
        {
            StopCoroutine(zoomTweenCoroutine);
        }

        zoomTweenCoroutine = StartCoroutine(TweenZoom());
    }

    System.Collections.IEnumerator TweenZoom()
    {
        float startValue = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        float elapsed = 0f;

        while (elapsed < zoomTweenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomTweenDuration);
            t = t * t * (3f - 2f * t);

            float zoomValue = Mathf.Lerp(startValue, targetZoom, t);
            ApplyZoomValue(zoomValue);

            yield return null;
        }

        ApplyZoomValue(targetZoom);
        zoomTweenCoroutine = null;
    }

    void ApplyZoomValue(float zoomValue)
    {
        if (cam == null)
        {
            return;
        }

        float clampedZoom = Mathf.Clamp(zoomValue, minZoom, maxZoom);

        if (cam.orthographic)
        {
            cam.orthographicSize = clampedZoom;
        }
        else
        {
            cam.fieldOfView = clampedZoom;
        }
    }
}
