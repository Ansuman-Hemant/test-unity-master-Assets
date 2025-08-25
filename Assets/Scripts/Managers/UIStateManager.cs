using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public static UIStateManager Instance { get; private set; }

    
    public MainMenuState mainMenuState;
    public GameMenuState gameMenuState;
    public EndlessModeState endlessModeState;
    public LevelsModeState levelsModeState;

    private BaseState currentState;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Disable all except main menu
        if (gameMenuState != null) gameMenuState.DisableState();
        if (endlessModeState != null) endlessModeState.DisableState();
        if (levelsModeState != null) levelsModeState.DisableState();

        // Ensure main menu is enabled
        if (mainMenuState != null) mainMenuState.EnableState();
        currentState = mainMenuState;
    }


    public void SwitchState(BaseState nextState)
    {
        if (currentState != null)
            currentState.DisableState();
        nextState.EnableState();
        currentState = nextState;
    }

    private void DisableAllStates()
    {
        mainMenuState.DisableState();
        gameMenuState.DisableState();
        endlessModeState.DisableState();
        levelsModeState.DisableState();
    }
}
