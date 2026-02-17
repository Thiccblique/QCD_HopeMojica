using UnityEngine;

public class ProbeBehavior : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] public GameObject lightObject;
    [SerializeField] private bool lightStartsOn = false;

    void Start()
    {
        if (lightObject != null)
        {
            lightObject.SetActive(lightStartsOn);
        }

        // Check if collider exists
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Collider2D found! Add a Collider2D component for click detection.");
        }
    }

    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate direction from this object to mouse
        Vector3 direction = mousePosition - transform.position;

        // Calculate angle and rotate
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}
