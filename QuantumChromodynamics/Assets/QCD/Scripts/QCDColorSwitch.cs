using UnityEngine;

[ExecuteAlways]
public class QCDColorSwitch : MonoBehaviour
{
    [Header("Beams required to activate this object")]
    [Tooltip("Select the exact beam combination required to activate this object.")]
    public LightFlags requiredBeams;

    [Header("Activation Settings")]
    [Tooltip("Seconds the beam(s) must stay on the object before activation.")]
    public float activationDelay = 2f;

    [Header("Optional Feedback Sprite")]
    public SpriteRenderer feedbackSprite;

    [Header("Target Sprite")]
    public SpriteRenderer targetSprite;

    [Header("Dimension State (Temporary)")]
    [Tooltip("Is the inverse dimension currently active?")]
    public bool isInverseDimension;

    private LightFlags activeBeams = LightFlags.None;
    private float activationTimer = 0f;
    private bool isActive = false;

    [System.Flags]
    public enum LightFlags
    {
        None = 0,
        Red = 1 << 0,  // 001
        Green = 1 << 1,  // 010
        Blue = 1 << 2   // 100
    }

    private void Update()
    {
        LightFlags effectiveBeams = GetEffectiveBeams();

        // Only activate if exact required beams are present
        if (effectiveBeams == requiredBeams && !isActive)
        {
            activationTimer += Time.deltaTime;

            if (activationTimer >= activationDelay)
                ActivateObject();
        }
        else if (!isActive)
        {
            activationTimer = 0f;
        }

#if UNITY_EDITOR
        UpdateFeedbackColor();
#endif
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UpdateFeedbackColor();
#endif
    }

    private void OnTriggerStay2D(Collider2D collision) => UpdateActiveBeams(collision, true);
    private void OnTriggerExit2D(Collider2D collision) => UpdateActiveBeams(collision, false);

    private void UpdateActiveBeams(Collider2D collision, bool entering)
    {
        switch (collision.tag)
        {
            case "RedBeam":
                if (entering) activeBeams |= LightFlags.Red;
                else activeBeams &= ~LightFlags.Red;
                break;

            case "GreenBeam":
                if (entering) activeBeams |= LightFlags.Green;
                else activeBeams &= ~LightFlags.Green;
                break;

            case "BlueBeam":
                if (entering) activeBeams |= LightFlags.Blue;
                else activeBeams &= ~LightFlags.Blue;
                break;

            // Example dimension detection
            case "InverseCrystal":
                isInverseDimension = entering;
                break;
        }
    }

    // Inverse Dimension Check
    private LightFlags GetEffectiveBeams()
    {
        if (isInverseDimension)
        {
            // Flip the 3 RGB bits (001,010,100)
            return activeBeams ^ (LightFlags)0b111;
        }

        return activeBeams;
    }

    private void ActivateObject()
    {
        isActive = true;
        Debug.Log("Activated by exact beams: " + requiredBeams);

        if (targetSprite != null)
            targetSprite.color = GetColorFromFlags(requiredBeams);
    }

    private void DeactivateObject()
    {
        isActive = false;

        if (targetSprite != null)
            targetSprite.color = Color.white;
    }

#if UNITY_EDITOR
    private void UpdateFeedbackColor()
    {
        if (feedbackSprite == null) return;

        feedbackSprite.color = GetColorFromFlags(requiredBeams);
    }
#endif

    private Color GetColorFromFlags(LightFlags flags)
    {
        Color col = Color.black;

        if ((flags & LightFlags.Red) != 0) col.r = 1f;
        if ((flags & LightFlags.Green) != 0) col.g = 1f;
        if ((flags & LightFlags.Blue) != 0) col.b = 1f;

        return col;
    }
}
