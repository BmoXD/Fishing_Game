using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public abstract class ItemFunctionality : MonoBehaviour
{
    private PlayerControls controls;
    protected bool isMenuOpen = false;

    public abstract void Use();

    protected virtual void OnEnable()
    {
        PlayerEvents.OnPlayerEnterMenu += HandleMenuState;
        controls = new PlayerControls();
        controls.Item.Use.started += OnUseStarted;
        controls.Item.Use.canceled += OnUseCanceled;
        controls.Item.Enable();

    }

    protected virtual void OnDisable()
    {
        PlayerEvents.OnPlayerEnterMenu -= HandleMenuState;
        if (controls != null)
        {
            controls.Item.Use.started -= OnUseStarted;
            controls.Item.Use.canceled -= OnUseCanceled;
            controls.Item.Disable();
            controls = null;
        }
    }
    
    protected bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            //Debug.Log("Not using item because we're clicking on UI");
            return true;
        }
        return false;
    }

    private void HandleMenuState(bool menuOpen)
    {
        isMenuOpen = menuOpen;
    }

    protected virtual void OnUseStarted(InputAction.CallbackContext context)
    {

    }

    protected virtual void OnUseCanceled(InputAction.CallbackContext context)
    {

    }
}
