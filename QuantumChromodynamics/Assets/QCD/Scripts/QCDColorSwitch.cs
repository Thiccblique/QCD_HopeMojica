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
    [Tooltip("Circle or sprite that shows the required beam combination.")]
    public SpriteRenderer feedbackSprite;

    [Header("Target Sprite")]
    [Tooltip("SpriteRenderer of the object to change color when activated.")]
    public SpriteRenderer targetSprite;

    private LightFlags activeBeams = LightFlags.None;
    private float activationTimer = 0f;
    private bool isActive = false;

    [System.Flags]
    public enum LightFlags
    {
        None = 0,
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2
    }

    private void Update()
    {
        // Only activate if exact required beams are present
        if (activeBeams == requiredBeams && !isActive)
        {
            activationTimer += Time.deltaTime;

            if (activationTimer >= activationDelay)
                ActivateObject();
        }
        else if (!isActive)
        {
            activationTimer = 0f;

            //if (isActive)
                //DeactivateObject();
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
        }

        // DEBUG
        Debug.Log("Active Beams: " + activeBeams);
    }

    private void ActivateObject()
    {
        isActive = true;
        Debug.Log("Activated by exact beams: " + requiredBeams);

        // Change the object's sprite color to match the required beams
        if (targetSprite != null)
            targetSprite.color = GetColorFromFlags(requiredBeams);
    }

    private void DeactivateObject()
    {
        isActive = false;
        Debug.Log("Deactivated or wrong beam combination");

        // Optional: reset color when deactivated
        if (targetSprite != null)
            targetSprite.color = Color.white;
    }

#if UNITY_EDITOR
    // Shows the required beam combination on the feedback sprite in the Editor
    private void UpdateFeedbackColor()
    {
        if (feedbackSprite == null) return;

        feedbackSprite.color = GetColorFromFlags(requiredBeams);
    }
#endif

    // Helper function: converts a LightFlags combination to a Unity color
    private Color GetColorFromFlags(LightFlags flags)
    {
        Color col = Color.black;
        if (flags.HasFlag(LightFlags.Red)) col.r = 1f;
        if (flags.HasFlag(LightFlags.Green)) col.g = 1f;
        if (flags.HasFlag(LightFlags.Blue)) col.b = 1f;
        return col;
    }
}
