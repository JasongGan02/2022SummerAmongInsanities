using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageHoverOnGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Reference to the Image component attached to the GameObject
    private Image imageComponent;

    // The sprites to switch to when hovering
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite originalSprite;

    void Start()
    {
        // Get the Image component attached to this GameObject
        imageComponent = GetComponent<Image>();

        // Optionally, set the original sprite if it's not set in the Inspector
        if (imageComponent != null && originalSprite == null)
        {
            originalSprite = imageComponent.sprite;
        }
    }

    // This method is called when the mouse enters the GameObject
    public void OnPointerEnter(PointerEventData eventData)
    {
        // If the Image component and hover sprite are assigned, switch the sprite
        if (imageComponent != null && hoverSprite != null)
        {
            imageComponent.sprite = hoverSprite;
        }
    }

    // This method is called when the mouse exits the GameObject
    public void OnPointerExit(PointerEventData eventData)
    {
        // If the Image component and original sprite are assigned, revert to the original sprite
        if (imageComponent != null && originalSprite != null)
        {
            imageComponent.sprite = originalSprite;
        }
    }
}
