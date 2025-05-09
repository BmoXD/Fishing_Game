using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for all interactive UI elements in the game.
/// Supports hover, click, and tooltip functionality with customizable appearance options.
/// </summary>
public class InteractiveUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [Header("Icon Settings")]
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Sprite icon;
    [SerializeField] protected Sprite emptyIcon; // Optional

    [Header("Hover Settings")]
    [SerializeField] protected bool resizeOnHover = true;
    [SerializeField] [Range(0.1f, 2f)] protected float resizePercentageOnHover = 1.1f;
    [SerializeField] protected Image hoverBorder;

    [Header("Click Settings")]
    [SerializeField] protected bool resizeOnClick = true;
    [SerializeField] [Range(0.1f, 2f)] protected float resizePercentageOnClick = 0.9f;

    [Header("Clicked State Settings")]
    [SerializeField] protected bool isClicked = false;
    [SerializeField] protected bool resizeOnClicked = false;
    [SerializeField] [Range(0.1f, 2f)] protected float resizePercentageOnClicked = 1.05f;

    [Header("Tooltip Settings")]
    [SerializeField] protected bool hasHoverTooltip = false;
    [SerializeField] protected string hoverTooltipTitle = "";
    [SerializeField] [TextArea] protected string hoverTooltipDescription = "";
    [SerializeField] [TextArea] protected string hoverTooltipExtraInfo = "";
    
    // References
    protected Vector3 originalScale;
    protected bool isHovered = false;
    protected bool isPressed = false;
    protected GameObject tooltipInstance;
    protected bool ignoreInput = false;

    [SerializeField] protected GameObject tooltipPrefab; // Assign your tooltip prefab in the inspector
    protected Canvas parentCanvas; // Reference to the parent canvas

    protected virtual void Awake()
    {
        originalScale = transform.localScale;
        
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }
        
        if (hoverBorder != null)
        {
            hoverBorder.gameObject.SetActive(false);
        }

        // Find the parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("InteractiveUIElement: No parent Canvas found. Tooltips may not function correctly.");
        }

        ApplyClickedState();
    }

    protected virtual void OnEnable()
    {
        // Reset states when re-enabled
        isHovered = false;
        isPressed = false;
        transform.localScale = originalScale;

        PlayerEvents.OnPlayerEnterMinigame += OnHandleEnterMinigame;

        ApplyClickedState();
    }

    private void OnHandleEnterMinigame(bool inMinigame)
    {
        ignoreInput = inMinigame;
    }

    #region Mouse Interaction
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (ignoreInput)
            return;
        
        isHovered = true;
        
        if (hoverBorder != null)
        {
            hoverBorder.gameObject.SetActive(true);
        }
        
        if (resizeOnHover && !isClicked)
        {
            transform.localScale = originalScale * resizePercentageOnHover;
        }
        
        if (hasHoverTooltip)
        {
            //ShowTooltip();
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (ignoreInput)
            return;

        isHovered = false;
        
        if (hoverBorder != null)
        {
            hoverBorder.gameObject.SetActive(false);
        }
        
        if (!isPressed && !isClicked)
        {
            transform.localScale = originalScale;
        }
        else if (isClicked && resizeOnClicked)
        {
            transform.localScale = originalScale * resizePercentageOnClicked;
        }
        
        //HideTooltip();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (ignoreInput)
            return;
        
        isPressed = true;
        
        if (resizeOnClick)
        {
            transform.localScale = originalScale * resizePercentageOnClick;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (ignoreInput)
            return;
        
        isPressed = false;
        
        if (isHovered)
        {
            if (resizeOnHover && !isClicked)
            {
                transform.localScale = originalScale * resizePercentageOnHover;
            }
            else if (isClicked && resizeOnClicked)
            {
                transform.localScale = originalScale * resizePercentageOnClicked;
            }
            
            // Handle click action
            OnElementClicked();
        }
        else
        {
            if (!isClicked)
            {
                transform.localScale = originalScale;
            }
            else if (resizeOnClicked)
            {
                transform.localScale = originalScale * resizePercentageOnClicked;
            }
        }
    }
    #endregion

    #region Keyboard/Controller Interaction
    public virtual void OnSelect(BaseEventData eventData)
    {
        // Called when navigated to with keyboard/controller
        isHovered = true;
        
        if (hoverBorder != null)
        {
            hoverBorder.gameObject.SetActive(true);
        }
        
        if (resizeOnHover && !isClicked)
        {
            transform.localScale = originalScale * resizePercentageOnHover;
        }
        
        // if (hasHoverTooltip)
        // {
        //     ShowTooltip();
        // }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        // Called when navigated away with keyboard/controller
        isHovered = false;
        
        if (hoverBorder != null)
        {
            hoverBorder.gameObject.SetActive(false);
        }
        
        if (!isPressed && !isClicked)
        {
            transform.localScale = originalScale;
        }
        else if (isClicked && resizeOnClicked)
        {
            transform.localScale = originalScale * resizePercentageOnClicked;
        }
        
        //HideTooltip();
    }

    // Call this from InputSystem's "Submit" action
    public virtual void OnInputSystemSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isPressed = true;
            
            if (resizeOnClick)
            {
                transform.localScale = originalScale * resizePercentageOnClick;
            }
            
            // Invoke click action on release
            OnElementClicked();
            
            // Reset to hover state
            if (isHovered)
            {
                if (resizeOnHover && !isClicked)
                {
                    transform.localScale = originalScale * resizePercentageOnHover;
                }
                else if (isClicked && resizeOnClicked)
                {
                    transform.localScale = originalScale * resizePercentageOnClicked;
                }
            }
            
            isPressed = false;
        }
    }
    #endregion

    // #region Tooltip Management
    // protected virtual void ShowTooltip()
    // {
    //     if (!hasHoverTooltip || string.IsNullOrEmpty(hoverTooltipTitle) || tooltipPrefab == null)
    //         return;

    //     // Find the root Canvas if one wasn't found in Awake
    //     if (parentCanvas == null)
    //     {
    //         parentCanvas = GetComponentInParent<Canvas>();
    //         if (parentCanvas == null)
    //         {
    //             Debug.LogWarning("InteractiveUIElement: No parent Canvas found. Cannot show tooltip.");
    //             return;
    //         }
    //     }

    //     // Get the root Canvas transform to ensure tooltip appears within the UI hierarchy
    //     Transform canvasTransform = parentCanvas.transform;
        
    //     if (tooltipInstance == null)
    //     {
    //         // Instantiate the tooltip as a child of the canvas
    //         tooltipInstance = Instantiate(tooltipPrefab, canvasTransform);
            
    //         // Assign content
    //         UITooltip tooltip = tooltipInstance.GetComponent<UITooltip>();
    //         if (tooltip != null)
    //         {
    //             tooltip.SetContent(hoverTooltipTitle, hoverTooltipDescription, hoverTooltipExtraInfo);
    //         }
    //     }
    // }


    // protected virtual void HideTooltip()
    // {
    //     if (tooltipInstance != null)
    //     {
    //         Destroy(tooltipInstance);
    //         tooltipInstance = null;
    //     }
    // }
    // // #endregion

    #region Click State Management
    protected virtual void OnElementClicked()
    {
        // Override in child classes to handle click behavior
        // For toggle-like behavior:
        // SetClickedState(!isClicked);
    }

    public virtual void SetClickedState(bool clicked)
    {
        isClicked = clicked;
        ApplyClickedState();
    }

    protected virtual void ApplyClickedState()
    {
        if (isClicked)
        {
            if (resizeOnClicked)
            {
                transform.localScale = originalScale * resizePercentageOnClicked;
            }
        }
        else
        {
            if (!isHovered && !isPressed)
            {
                transform.localScale = originalScale;
            }
            else if (isHovered && resizeOnHover)
            {
                transform.localScale = originalScale * resizePercentageOnHover;
            }
        }
    }
    #endregion
    
    #region Public Accessors
    public virtual void SetIcon(Sprite newIcon)
    {
        icon = newIcon;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }

    // public virtual void SetTooltipContent(string title, string description, string extraInfo)
    // {
    //     hoverTooltipTitle = title;
    //     hoverTooltipDescription = description;
    //     hoverTooltipExtraInfo = extraInfo;
        
    //     // Update the tooltip if it's currently visible
    //     if (tooltipInstance != null)
    //     {
    //         UITooltip tooltip = tooltipInstance.GetComponent<UITooltip>();
    //         if (tooltip != null)
    //         {
    //             tooltip.SetContent(hoverTooltipTitle, hoverTooltipDescription, hoverTooltipExtraInfo);
    //         }
    //     }
    // }

    public bool IsClicked()
    {
        return isClicked;
    }

    public bool IsHovered()
    {
        return isHovered;
    }
    #endregion
}