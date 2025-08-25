using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] TextAsset levelDataFile;
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI objectiveText;
    [SerializeField] TextMeshProUGUI timerText;

    public int currentLevel = 1;
    private LevelDataModel currentGridData;
    private LevelDataRoot allGrids;

    public int requiredWords = 5;
    public int requiredBonus = 0;
    public int wordsMade = 0;
    public int bonusUsed = 0;
    public float timer = 0;
    public float timeLimit = 0;
    public bool timerActive = false;

    private int initialBlockedTiles = 0;
    private int currentBlockedTiles = 0;

    private GridManager gridManager;
    private WordSelector wordSelector;

    void Start()
    {
        gridManager = GridManager.Instance;
        wordSelector = WordSelector.Instance;
        LoadGridData();
        SetupLevel(currentLevel);
    }

    void LoadGridData()
    {
        if (levelDataFile == null)
        {
            Debug.LogError("Level data file not assigned.");
            return;
        }
        allGrids = JsonUtility.FromJson<LevelDataRoot>(levelDataFile.text);
        Debug.Log($"Loaded {allGrids.data.Length} grid layouts from JSON.");
    }

    public void SetupLevel(int levelNum)
    {
        currentLevel = levelNum;
        if (allGrids != null && levelNum <= allGrids.data.Length)
        {
            currentGridData = allGrids.data[levelNum - 1];
            CountBlockedTiles();
        }
        else
        {
            Debug.LogError($"No grid data available for level {levelNum}");
            return;
        }

        SetCustomLevelRules(levelNum);
        wordsMade = 0;
        bonusUsed = 0;
        timer = timeLimit;
        timerActive = timeLimit > 0;

        if (wordSelector != null)
            ScoreManager.Instance.ResetScore();

        if (gridManager != null)
            gridManager.LoadLevel(levelNum - 1);

        UpdateUI();
        Debug.Log($"Level {levelNum} setup complete.");

        if (currentLevel == 5)
            Debug.Log($"Blocked tiles to unlock: {initialBlockedTiles}");
    }

    void CountBlockedTiles()
    {
        initialBlockedTiles = 0;
        currentBlockedTiles = 0;
        if (currentGridData?.gridData != null)
        {
            foreach (var tile in currentGridData.gridData)
            {
                if (tile.tileType == 2 || tile.tileType == 3 || tile.tileType == 5)
                {
                    initialBlockedTiles++;
                    currentBlockedTiles++;
                }
            }
            Debug.Log($"Found {initialBlockedTiles} blocked tiles in level data.");
        }
    }

    public void OnTileUnblocked()
    {
        currentBlockedTiles--;
        Debug.Log($"Tile unblocked, {currentBlockedTiles}/{initialBlockedTiles} remaining.");
        if (currentLevel == 5)
        {
            UpdateUI();
            CheckLevelComplete();
        }
    }

    void SetCustomLevelRules(int levelNum)
    {
        switch (levelNum)
        {
            case 1: requiredWords = 5; requiredBonus = 0; timeLimit = 0; break;
            case 2: requiredWords = 7; requiredBonus = 0; timeLimit = 45; break;
            case 3: requiredWords = 10; requiredBonus = 0; timeLimit = 120; break;
            case 4: requiredWords = 1; requiredBonus = 4; timeLimit = 0; break;
            case 5: requiredWords = 1; requiredBonus = 4; timeLimit = 0; break;
            default:
                requiredWords = 5 + (levelNum - 1) * 2;
                requiredBonus = 0;
                timeLimit = Mathf.Max(30, 60 - (levelNum - 1) * 5);
                break;
        }
        Debug.Log($"Level {levelNum} rules: {requiredWords} words, {requiredBonus} bonus, {timeLimit}s.");
    }

    void Update()
    {
        if (timerActive && timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
            if (timer <= 0)
                LevelFailed("Time's up.");
        }
    }

    public void OnWordMade(string word, int bonusInWord, List<LevelTile> usedTiles)
    {
        wordsMade++;
        bonusUsed += bonusInWord;
        Debug.Log($"Word '{word}' made. Progress: {wordsMade}/{requiredWords} words, Bonus: {bonusUsed}/{requiredBonus}");
        UpdateUI();
        CheckLevelComplete();
    }

    void CheckLevelComplete()
    {
        bool wordGoalMet = wordsMade >= requiredWords;
        bool bonusGoalMet = requiredBonus == 0 || bonusUsed >= requiredBonus;
        bool timeOk = !timerActive || timer > 0;
        bool blockedTilesCleared = currentLevel != 5 || currentBlockedTiles == 0;

        Debug.Log($"Level {currentLevel} progress check:");
        Debug.Log($"Words: {wordsMade}/{requiredWords} {(wordGoalMet ? "✓" : "")}");
        Debug.Log($"Bonus: {bonusUsed}/{requiredBonus} {(bonusGoalMet ? "✓" : "")}");
        Debug.Log($"Time: {(timeOk ? "✓" : "")}");
        Debug.Log($"Blocked tiles: {currentBlockedTiles} {(blockedTilesCleared ? "✓" : "")}");

        if (wordGoalMet && bonusGoalMet && timeOk && blockedTilesCleared)
            LevelComplete();
    }

    void LevelComplete()
    {
        timerActive = false;
        Debug.Log($"Level {currentLevel} complete.");
        StartCoroutine(AdvanceToNextLevel());
    }

    IEnumerator AdvanceToNextLevel()
    {
        yield return new WaitForSeconds(2f);
        NextLevel();
    }

    void LevelFailed(string reason)
    {
        timerActive = false;
        Debug.Log($"Level {currentLevel} failed: {reason}");
        StartCoroutine(RestartLevelDelayed());
    }

    IEnumerator RestartLevelDelayed()
    {
        yield return new WaitForSeconds(2f);
        RestartLevel();
    }

    void RestartLevel()
    {
        SetupLevel(currentLevel);
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level {currentLevel}";
        if (objectiveText != null)
            objectiveText.text = BuildObjectiveText();
        UpdateTimerUI();
    }

    string BuildObjectiveText()
    {
        switch (currentLevel)
        {
            case 1: return $"Make {requiredWords} words\n({wordsMade}/{requiredWords})";
            case 2: return $"Make {requiredWords} words in {timeLimit}s\n({wordsMade}/{requiredWords})";
            case 3: return $"Make {requiredWords} words in {timeLimit}s\n({wordsMade}/{requiredWords})";
            case 4: return $"Use {requiredBonus} bonus letters\n(Bonus used: {bonusUsed}/{requiredBonus})";
            case 5: return $"Unlock all blocked tiles & use {requiredBonus} bonus\nBonus: {bonusUsed}/{requiredBonus} | Blocked: {currentBlockedTiles} remaining";
            default: return $"Words: {wordsMade}/{requiredWords}, Time: {timer:F0}s";
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (timerActive && timer > 0)
            {
                int minutes = Mathf.FloorToInt(timer / 60);
                int seconds = Mathf.FloorToInt(timer % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
                if (timer <= 10)
                    timerText.color = Color.red;
                else if (timer <= 20)
                    timerText.color = Color.yellow;
                else
                    timerText.color = Color.white;
            }
            else if (timerActive)
            {
                timerText.text = "00:00";
                timerText.color = Color.red;
            }
            else
            {
                timerText.text = "No Limit";
                timerText.color = Color.white;
            }
        }
    }

    public void NextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel <= allGrids.data.Length)
        {
            Debug.Log($"Loading level {nextLevel}...");
            SetupLevel(nextLevel);
        }
        else
        {
            Debug.Log("All levels completed.");
            LevelsModeState levelsModeState = FindObjectOfType<LevelsModeState>();
            if (levelsModeState != null)
            {
                levelsModeState.OnAllLevelsCleared();
            }
            else
            {
                UIStateManager.Instance.SwitchState(UIStateManager.Instance.mainMenuState);
            }
        }
    }

    [System.Serializable] public class GridTileData { public int tileType; public string letter; }
    [System.Serializable] public class GridSize { public int x, y; }
    [System.Serializable]
    public class LevelDataModel
    {
        public int bugCount, wordCount, timeSec, totalScore, levelType;
        public GridSize gridSize;
        public GridTileData[] gridData;
    }
    [System.Serializable] public class LevelDataRoot { public LevelDataModel[] data; }
}
