using UnityEngine;

[ExecuteAlways]
public class ShapeOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineThickness = 1f;

    private SpriteRenderer spriteRenderer;
    private GameObject[] outlineObjects;

    void Start()
    {
        RefreshOutline();
    }

    void OnEnable()
    {
        RefreshOutline();
    }

    void RefreshOutline()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("ShapeOutline requires a SpriteRenderer component!");
            return;
        }

        // Clean up existing outlines before creating new ones
        CleanupOutlines();
        CreateOutline();
    }

    void CreateOutline()
    {
        // Create 8 outline sprites (in 8 directions around the original)
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0),      // Right
            new Vector2(-1, 0),     // Left
            new Vector2(0, 1),      // Up
            new Vector2(0, -1),     // Down
            new Vector2(1, 1),      // Top-Right
            new Vector2(-1, 1),     // Top-Left
            new Vector2(1, -1),     // Bottom-Right
            new Vector2(-1, -1)     // Bottom-Left
        };

        outlineObjects = new GameObject[directions.Length];

        for (int i = 0; i < directions.Length; i++)
        {
            // Create a child object for each outline direction
            GameObject outlineObj = new GameObject($"Outline_{i}");
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = directions[i].normalized * outlineThickness * 0.01f;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;

            // Add and configure SpriteRenderer
            SpriteRenderer outlineSR = outlineObj.AddComponent<SpriteRenderer>();
            outlineSR.sprite = spriteRenderer.sprite;
            outlineSR.color = outlineColor;
            outlineSR.sortingLayerID = spriteRenderer.sortingLayerID;
            outlineSR.sortingOrder = spriteRenderer.sortingOrder - 1;

            outlineObjects[i] = outlineObj;
        }
    }

    void OnDestroy()
    {
        CleanupOutlines();
    }

    void CleanupOutlines()
    {
        // Clean up outline objects
        if (outlineObjects != null)
        {
            foreach (GameObject obj in outlineObjects)
            {
                if (obj != null)
                {
                    if (Application.isPlaying)
                        Destroy(obj);
                    else
                        DestroyImmediate(obj);
                }
            }
        }
        outlineObjects = null;
    }

    // Update outline if values change in editor
    void OnValidate()
    {
        if (spriteRenderer != null && outlineObjects != null)
        {
            UpdateOutline();
        }
        else
        {
            // Delay the refresh to avoid issues during serialization
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    RefreshOutline();
            };
            #endif
        }
    }

    void UpdateOutline()
    {
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1)
        };

        for (int i = 0; i < outlineObjects.Length; i++)
        {
            if (outlineObjects[i] != null)
            {
                outlineObjects[i].transform.localPosition = directions[i].normalized * outlineThickness * 0.01f;
                SpriteRenderer sr = outlineObjects[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = outlineColor;
                }
            }
        }
    }
}
