using UnityEngine;
using UnityEngine.UI;

public class MainMenuState : BaseState
{
    public Button startButton;
    public Button quitButton;

    void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }
    void OnDisable()
    {
        startButton.onClick.RemoveListener(StartGame);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    void StartGame()
    {
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.gameMenuState);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
