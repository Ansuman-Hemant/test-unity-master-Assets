using UnityEngine;
using UnityEngine.UI;

public class LevelsModeState : BaseState
{
    public Button backToMenuButton;

    private int currentLevelIndex = 0;

    void OnEnable()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);

        GridManager.Instance.SwitchMode(GridManager.Mode.Levels);
        WordSelector.Instance.SetMode(WordSelector.GameMode.Levels);
        LoadLevel(currentLevelIndex);
    }
    void OnDisable()
    {
        backToMenuButton.onClick.RemoveListener(BackToMenu);
    }

    public void LoadLevel(int index)
    {
        GridManager.Instance.LoadLevel(index);
        ScoreManager.Instance.ResetScore();
    }

    // Call this from your level completion logic when last level finishes
    public void OnAllLevelsCleared()
    {
        Debug.Log("All levels cleared!");
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.mainMenuState);
    }

    void BackToMenu()
    {
        UIStateManager.Instance.SwitchState(UIStateManager.Instance.mainMenuState);
        ScoreManager.Instance.ResetScore();
    }
}
