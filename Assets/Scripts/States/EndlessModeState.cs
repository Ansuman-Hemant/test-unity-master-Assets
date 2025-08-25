using UnityEngine;
using UnityEngine.UI;

public class EndlessModeState : BaseState
{
    public Button resetButton;
    public Button backToMenuButton;

    void OnEnable()
    {
        resetButton.onClick.AddListener(ResetLetters);
        backToMenuButton.onClick.AddListener(BackToMenu);
        GridManager.Instance.SwitchMode(GridManager.Mode.Endless);
        WordSelector.Instance.SetMode(WordSelector.GameMode.Endless);
    }

    void OnDisable()
    {
        resetButton.onClick.RemoveListener(ResetLetters);
        backToMenuButton.onClick.RemoveListener(BackToMenu);
    }

    void ResetLetters()
    {
        GridManager.Instance.RegenerateGrid();
        ScoreManager.Instance.ResetScore();
    }

    void BackToMenu()
    {
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.mainMenuState);
        ScoreManager.Instance.ResetScore();
        GridManager.Instance.SwitchMode(GridManager.Mode.Endless);
    }
}
