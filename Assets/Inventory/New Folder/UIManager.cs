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

    // Global event for player control state
    //public delegate void PlayerControlsEvent(bool enabled);
    //public static event PlayerControlsEvent OnPlayerControlsChanged;

    // References
    [SerializeField] private List<PanelInfo> panels = new List<PanelInfo>();
    [SerializeField] private GameObject basePanel;

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

        // Initialize input controls
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        SetupInputListeners();

        if (fishingMinigame != null)
        {
            fishingMinigame.onMinigameSuccess.AddListener(CloseMinigamePanel);
            fishingMinigame.onMinigameFail.AddListener(CloseMinigamePanel);
        }
        OpenMinigamePanel();
    }

    private void OnDisable()
    {
        RemoveInputListeners();
        playerControls.Disable();

        if (fishingMinigame != null)
        {
            fishingMinigame.onMinigameSuccess.RemoveListener(CloseMinigamePanel);
            fishingMinigame.onMinigameFail.RemoveListener(CloseMinigamePanel);
        }
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

// Toggle panel based on input action name
public void TogglePanel(string inputActionName)
{
    // Check if any non-base panel is already open (except the one we're trying to toggle)
    bool anotherPanelOpen = false;
    GameObject currentOpenPanel = null;
    
    foreach (var checkPanel in panels)
    {
        if (checkPanel.panel != basePanel && 
            checkPanel.panel.activeSelf && 
            checkPanel.inputActionName != inputActionName)
        {
            anotherPanelOpen = true;
            currentOpenPanel = checkPanel.panel;
            break;
        }
    }
    
    // Find the panel we want to toggle
    foreach (var panelInfo in panels)
    {
        if (panelInfo.inputActionName == inputActionName)
        {
            bool isActive = panelInfo.panel.activeSelf;
            
            // If this panel is already active, close it
            if (isActive)
            {
                // If we're closing a panel that disables controls
                if (panelInfo.disablePlayerControls)
                {
                    controlDisablingPanelsOpen--;
                    if (controlDisablingPanelsOpen <= 0)
                    {
                        controlDisablingPanelsOpen = 0;
                        PlayerEvents.RaisePlayerEnterMenu(false); // Enable controls
                    }
                }
                
                // Deactivate the panel
                panelInfo.panel.SetActive(false);
                
                // If closing a non-base panel, show the base panel
                if (panelInfo.panel != basePanel)
                {
                    basePanel.SetActive(true);
                }
            }
            // If trying to open this panel
            else
            {
                // If another panel is open, ignore this request
                if (anotherPanelOpen)
                {
                    Debug.Log($"Cannot open panel '{panelInfo.panel.name}' because '{currentOpenPanel.name}' is already open.");
                    return;
                }
                
                // If this is not the base panel, hide the base panel
                if (panelInfo.panel != basePanel)
                {
                    basePanel.SetActive(false);
                }
                
                // If opening a panel that disables controls
                if (panelInfo.disablePlayerControls)
                {
                    if (controlDisablingPanelsOpen == 0)
                    {
                        PlayerEvents.RaisePlayerEnterMenu(true); // Disable controls
                    }
                    controlDisablingPanelsOpen++;
                }
                
                // Activate the panel
                panelInfo.panel.SetActive(true);
            }
            
            return;
        }
    }
}

// Open a specific panel by reference
public void OpenPanel(GameObject panel)
{
    // Don't do anything if this panel is already open
    if (panel.activeSelf)
    {
        return;
    }
    
    // Check if any non-base panel is already open
    bool anotherPanelOpen = false;
    GameObject currentOpenPanel = null;
    
    foreach (var checkPanel in panels)
    {
        if (checkPanel.panel != basePanel && 
            checkPanel.panel != panel && 
            checkPanel.panel.activeSelf)
        {
            anotherPanelOpen = true;
            currentOpenPanel = checkPanel.panel;
            break;
        }
    }
    
    // If another panel is open, ignore this request
    if (anotherPanelOpen)
    {
        Debug.Log($"Cannot open panel '{panel.name}' because '{currentOpenPanel.name}' is already open.");
        return;
    }
    
    foreach (var panelInfo in panels)
    {
        if (panelInfo.panel == panel)
        {
            // If this is not the base panel, hide the base panel
            if (panel != basePanel)
            {
                basePanel.SetActive(false);
            }
            
            // If opening a panel that disables player controls
            if (panelInfo.disablePlayerControls)
            {
                if (controlDisablingPanelsOpen == 0)
                {
                    PlayerEvents.RaisePlayerEnterMenu(true); // Disable controls
                }
                controlDisablingPanelsOpen++;
            }
            
            // Activate the panel
            panelInfo.panel.SetActive(true);
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
                // If panel disables player controls
                if (panelInfo.disablePlayerControls)
                {
                    controlDisablingPanelsOpen--;
                    if (controlDisablingPanelsOpen <= 0)
                    {
                        controlDisablingPanelsOpen = 0;
                        PlayerEvents.RaisePlayerEnterMenu(false); // Enable controls
                    }
                }
                
                // Deactivate the panel
                panelInfo.panel.SetActive(false);
                
                // If we're closing a non-base panel, check if we should show the base panel
                if (panel != basePanel)
                {
                    // Check if any other non-base panels are still open
                    bool anyPanelOpen = false;
                    foreach (var otherPanel in panels)
                    {
                        if (otherPanel.panel != basePanel && otherPanel.panel != panel && otherPanel.panel.activeSelf)
                        {
                            anyPanelOpen = true;
                            break;
                        }
                    }
                    
                    // If no other panels open, show base panel
                    if (!anyPanelOpen)
                    {
                        basePanel.SetActive(true);
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
            PlayerEvents.RaisePlayerEnterMenu(false); // Re-enable controls
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

    public void OpenMinigamePanel()
    {
        // Special logic before opening
        // e.g. play sound, pause background music, etc.

        if (minigamePanel != null && !minigamePanel.activeSelf)
        {
            minigamePanel.SetActive(true);

            // If you want to keep basePanel active, do NOT deactivate it here

            // Raise player menu event if needed
            PlayerEvents.RaisePlayerEnterMenu(true);
            PlayerEvents.RaisePlayerEnterMenu(true);
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
            PlayerEvents.RaisePlayerEnterMenu(false);
        }

        // TODO: Add more minigame-specific logic here
    }

    public List<PanelInfo> GetPanels()
    {
        return panels;
    }
}
