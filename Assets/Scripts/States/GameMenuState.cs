using UnityEngine;
using UnityEngine.UI;

public class GameMenuState : BaseState
{
    public Button endlessButton;
    public Button levelsButton;

    void OnEnable()
    {
        endlessButton.onClick.AddListener(GoEndless);
        levelsButton.onClick.AddListener(GoLevels);
    }
    void OnDisable()
    {
        endlessButton.onClick.RemoveListener(GoEndless);
        levelsButton.onClick.RemoveListener(GoLevels);
    }

    void GoEndless()
    {
        GridManager.Instance.SwitchMode(GridManager.Mode.Endless);
        WordSelector.Instance.SetMode(WordSelector.GameMode.Endless);
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.endlessModeState);
    }
    void GoLevels()
    {
        GridManager.Instance.SwitchMode(GridManager.Mode.Levels);
        WordSelector.Instance.SetMode(WordSelector.GameMode.Levels);
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.levelsModeState);
    }
}
