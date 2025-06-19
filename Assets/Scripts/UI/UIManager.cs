using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }

    // Panel information class
    [System.Serializable]
    public class PanelInfo
    {
        public GameObject panel;
        public string inputActionName;
        public bool disablePlayerControls;
    }

    [Header("Dialog Box")]
    [SerializeField] private GameObject dialogPanel;
    private DialogBox dialogBox;


    // Global event for player control state
    //public delegate void PlayerControlsEvent(bool enabled);
    //public static event PlayerControlsEvent OnPlayerControlsChanged;

    [Header("Panels")]
    // References
    [SerializeField] private List<PanelInfo> panels = new List<PanelInfo>();
    [SerializeField] private GameObject basePanel;
    [Header("Escape Panel")]
    [SerializeField] private GameObject escapePanel;

    private bool isAnotherPanelOpen = false;

    [Header("Minigame")]
    [SerializeField] private GameObject minigamePanel;
    private FishingMinigame fishingMinigame;
    private PlayerControls playerControls;

    // Track control-disabling panels that are open
    private int controlDisablingPanelsOpen = 0;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (minigamePanel != null)
        {
            fishingMinigame = minigamePanel.GetComponent<FishingMinigame>();
        }

        if (dialogPanel != null)
        {
            dialogBox = dialogPanel.GetComponent<DialogBox>();
            if (dialogBox == null)
                Debug.LogError("DialogBox component missing on dialogPanel!");
        }

        // Initialize input controls
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        SetupInputListeners();

        playerControls.UI.Escape.performed += OnEscape;

        if (fishingMinigame != null)
        {
            fishingMinigame.onMinigameSuccess.AddListener(CloseMinigamePanel);
            fishingMinigame.onMinigameFail.AddListener(CloseMinigamePanel);
        }
        //ShowDialog("Test", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }

    private void OnDisable()
    {
        RemoveInputListeners();
        playerControls.Disable();

        playerControls.UI.Escape.performed += OnEscape;

        if (fishingMinigame != null)
        {
            fishingMinigame.onMinigameSuccess.RemoveListener(CloseMinigamePanel);
            fishingMinigame.onMinigameFail.RemoveListener(CloseMinigamePanel);
        }
    }

    private void OnEscape(InputAction.CallbackContext context)
    {
        Debug.Log("A");
        // If escape panel is open, close it and show base panel
        if (escapePanel != null && escapePanel.activeSelf)
        {
            Debug.Log("B");
            escapePanel.SetActive(false);
            if (basePanel != null)
                basePanel.SetActive(true);
            PlayerOpensPanel(false);
            return;
        }

        // If only base panel is open (all others closed), open escape panel
        bool onlyBasePanelOpen = true;
        foreach (var panelInfo in panels)
        {
            Debug.Log("C");
            if (panelInfo.panel != basePanel && panelInfo.panel.activeSelf)
            {
                onlyBasePanelOpen = false;
                break;
            }
        }
        if (onlyBasePanelOpen && escapePanel != null)
        {
            Debug.Log("D");
            escapePanel.SetActive(true);
            if (basePanel != null)
                basePanel.SetActive(false);
            PlayerOpensPanel(true);
            return;
        }

        // Otherwise, close all panels (default behavior)
        CloseAllPanels();
    }

    private void Start()
    {
        // Initialize panels - make sure base panel is active and others are inactive
        foreach (var panelInfo in panels)
        {
            if (panelInfo.panel == basePanel)
            {
                panelInfo.panel.SetActive(true);
            }
            else
            {
                panelInfo.panel.SetActive(false);
            }
        }
    }

    private void SetupInputListeners()
    {
        // Hook up input actions to panel toggles
        foreach (var panelInfo in panels)
        {
            if (!string.IsNullOrEmpty(panelInfo.inputActionName))
            {
                InputAction action = playerControls.asset.FindAction(panelInfo.inputActionName);
                if (action != null)
                {
                    // We need a local copy for the closure to capture the correct value
                    string actionName = panelInfo.inputActionName;
                    action.performed += ctx => TogglePanel(actionName);
                }
            }
        }
    }

    private void RemoveInputListeners()
    {
        // Clean up input listeners
        foreach (var panelInfo in panels)
        {
            if (!string.IsNullOrEmpty(panelInfo.inputActionName))
            {
                InputAction action = playerControls.asset.FindAction(panelInfo.inputActionName);
                if (action != null)
                {
                    // We can only do this generic cleanup - specific delegates are hard to remove
                    action.performed -= ctx => TogglePanel(panelInfo.inputActionName);
                }
            }
        }
    }
    
    // Show dialog with title and message
    public void ShowDialog(string title, string message)
    {
        if (dialogBox != null)
        {
            dialogBox.Show(title, message);
            if (basePanel != null)
                basePanel.SetActive(false); // Hide base panel when dialog is shown
            PlayerOpensPanel(true);
        }
    }

    // Close dialog
    public void CloseDialog()
    {
        if (dialogBox != null)
        {
            dialogBox.Close();
            if (basePanel != null)
                basePanel.SetActive(true); // Show base panel when dialog is closed
            PlayerOpensPanel(false);
        }
    }

    private void PlayerOpensPanel(bool isInMenu)
    {
        isAnotherPanelOpen = isInMenu;
        PlayerEvents.RaisePlayerEnterMenu(isInMenu);
    }

    // Toggle panel based on input action name
    public void TogglePanel(string inputActionName)
    {
        foreach (var panelInfo in panels)
        {
            if (panelInfo.inputActionName == inputActionName)
            {
                if (panelInfo.panel.activeSelf)
                {
                    ClosePanel(panelInfo.panel);
                }
                else
                {
                    OpenPanel(panelInfo.panel);
                }
                return;
            }
        }
    }

    // Open a specific panel by reference
    public void OpenPanel(GameObject panel)
    {
        if (panel.activeSelf)
        {
            return;
        }
        if (isAnotherPanelOpen)
        {
            Debug.Log($"Cannot open panel '{panel.name}' because another panel is already open.");
            return;
        }
        foreach (var panelInfo in panels)
        {
            if (panelInfo.panel == panel)
            {
                if (panel != basePanel)
                {
                    basePanel.SetActive(false);
                }
                if (panelInfo.disablePlayerControls)
                {
                    if (controlDisablingPanelsOpen == 0)
                    {
                        PlayerOpensPanel(true);
                    }
                    controlDisablingPanelsOpen++;
                }
                panelInfo.panel.SetActive(true);
                // After open, update isAnotherPanelOpen
                PlayerOpensPanel(true);
                return;
            }
        }
    }

    // Close a specific panel by reference
    public void ClosePanel(GameObject panel)
    {
        foreach (var panelInfo in panels)
        {
            if (panelInfo.panel == panel && panelInfo.panel.activeSelf)
            {
                if (panelInfo.disablePlayerControls)
                {
                    controlDisablingPanelsOpen--;
                    if (controlDisablingPanelsOpen <= 0)
                    {
                        controlDisablingPanelsOpen = 0;
                        PlayerOpensPanel(false);
                    }
                }
                panelInfo.panel.SetActive(false);
                if (panel != basePanel)
                {
                    bool anyPanelOpen = false;
                    foreach (var otherPanel in panels)
                    {
                        if (otherPanel.panel != basePanel && otherPanel.panel != panel && otherPanel.panel.activeSelf)
                        {
                            anyPanelOpen = true;
                            break;
                        }
                    }
                    if (!anyPanelOpen)
                    {
                        basePanel.SetActive(true);
                        PlayerOpensPanel(false);
                    }
                }
                return;
            }
        }
    }

    // Close all panels except the base panel
    public void CloseAllPanels()
    {
        bool playerControlsNeedRestore = false;
        
        foreach (var panelInfo in panels)
        {
            if (panelInfo.panel != basePanel && panelInfo.panel.activeSelf)
            {
                if (panelInfo.disablePlayerControls)
                {
                    controlDisablingPanelsOpen--;
                    playerControlsNeedRestore = true;
                }
                
                panelInfo.panel.SetActive(false);
            }
        }
        
        // Always show the base panel when closing all others
        if (basePanel != null)
        {
            basePanel.SetActive(true);
        }
        
        if (playerControlsNeedRestore && controlDisablingPanelsOpen <= 0)
        {
            controlDisablingPanelsOpen = 0;
            PlayerOpensPanel(false); // Re-enable controls
        }
    }

    // Add a new panel at runtime
    public void AddPanel(GameObject panel, string inputActionName, bool disablePlayerControls)
    {
        // Check if panel already exists
        foreach (var existingPanel in panels)
        {
            if (existingPanel.panel == panel)
            {
                Debug.LogWarning("Panel already exists in UI Manager!");
                return;
            }
        }
        
        // Create new panel info
        PanelInfo newPanel = new PanelInfo
        {
            panel = panel,
            inputActionName = inputActionName,
            disablePlayerControls = disablePlayerControls
        };
        
        panels.Add(newPanel);
        
        // Set up input listener if we have an action name
        if (!string.IsNullOrEmpty(inputActionName))
        {
            InputAction action = playerControls.asset.FindAction(inputActionName);
            if (action != null)
            {
                action.performed += ctx => TogglePanel(inputActionName);
            }
        }
        
        // Start with panel inactive
        panel.SetActive(false);
    }

    // Remove a panel at runtime
    public void RemovePanel(GameObject panel)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].panel == panel)
            {
                // If the panel is active and disables controls, restore controls
                if (panels[i].panel.activeSelf && panels[i].disablePlayerControls)
                {
                    controlDisablingPanelsOpen--;
                    if (controlDisablingPanelsOpen <= 0)
                    {
                        controlDisablingPanelsOpen = 0;
                        PlayerEvents.RaisePlayerEnterMenu(false);
                    }
                }
                
                // Remove from list
                panels.RemoveAt(i);
                return;
            }
        }
    }

    // Check if a specific panel is open
    public bool IsPanelOpen(GameObject panel)
    {
        foreach (var panelInfo in panels)
        {
            if (panelInfo.panel == panel)
            {
                return panelInfo.panel.activeSelf;
            }
        }
        return false;
    }

    public void OpenMinigamePanel(float leftDriftIntensity, float rightPushIntensity)
    {
        // Special logic before opening
        // e.g. play sound, pause background music, etc.

        if (minigamePanel != null && !minigamePanel.activeSelf)
        {
            fishingMinigame.Configure(leftDriftIntensity, rightPushIntensity);
            minigamePanel.SetActive(true);
            fishingMinigame.StartMinigame();

            // Raise player menu event if needed
            PlayerEvents.RaisePlayerEnterMenu(true);
            PlayerEvents.RaisePlayerEnterMinigame(true);
        }

        // TODO: Add more minigame-specific logic here
    }

    public void CloseMinigamePanel()
    {
        // Special logic before closing
        // e.g. resume music, show results, etc.

        if (minigamePanel != null && minigamePanel.activeSelf)
        {
            minigamePanel.SetActive(false);

            // Raise player menu event if needed
            PlayerEvents.RaisePlayerEnterMenu(false);
            PlayerEvents.RaisePlayerEnterMinigame(false);
        }

        // TODO: Add more minigame-specific logic here
    }

    public List<PanelInfo> GetPanels()
    {
        return panels;
    }
}
