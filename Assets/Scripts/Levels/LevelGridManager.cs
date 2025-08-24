using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LevelGridManager : MonoBehaviour
{
    [SerializeField] RectTransform boardRoot;
    [SerializeField] GameObject levelTilePrefab; // Must have LevelTile script
    [SerializeField] TextAsset levelDataFile; // Assign your levelData.txt here

    [Header("Level Settings")]
    [SerializeField] int currentLevel = 0; // Which level to load (0-based index)

    void Start()
    {
        if (levelDataFile != null)
            BuildLevelGrid();
        else
            Debug.LogError("Level data file not assigned!");
    }

    public void BuildLevelGrid()
    {
        // Parse JSON level data
        LevelDataRoot levelData = JsonUtility.FromJson<LevelDataRoot>(levelDataFile.text);
        if (levelData == null || levelData.data.Length == 0)
        {
            Debug.LogError("Failed to parse level data!");
            return;
        }

        if (currentLevel >= levelData.data.Length)
        {
            Debug.LogError($"Level {currentLevel} doesn't exist! Max level: {levelData.data.Length - 1}");
            return;
        }

        var level = levelData.data[currentLevel];
        int rows = level.gridSize.y;
        int cols = level.gridSize.x;

        // Clear existing tiles
        foreach (Transform child in boardRoot)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        // Set up grid layout
        var gridLayout = boardRoot.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = cols;
        }

        // Create tiles from level data
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                if (index >= level.gridData.Length)
                {
                    Debug.LogError($"Grid data index {index} out of range! Max: {level.gridData.Length - 1}");
                    continue;
                }

                var tileData = level.gridData[index];
                GameObject tileGO = Instantiate(levelTilePrefab, boardRoot);
                LevelTile tileScript = tileGO.GetComponent<LevelTile>();
                if (tileScript != null)
                {
                    tileScript.SetLetter(tileData.letter);
                    tileScript.SetGridPosition(row, col);

                    // Updated: Explicit handling based on tileType
                    switch (tileData.tileType)
                    {
                        case 0: // Normal
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(false);
                            break;
                        case 4: // Bonus
                            tileScript.SetBonus(true);
                            tileScript.SetBlocked(false);
                            break;
                        case 2: // Blocked
                        case 3: // (Stronger Blocked, if needed)
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(true);
                            break;
                        case 5: // Blocked+Bonus
                            tileScript.SetBonus(true);
                            tileScript.SetBlocked(true);
                            break;
                        default:
                            Debug.LogWarning($"⚠️ Unknown tile type: {tileData.tileType}");
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(false);
                            break;
                    }
                }
                else
                {
                    Debug.LogError("LevelTile prefab doesn't have LevelTile script component!");
                }
            }
        }
        Debug.Log($"Grid building complete! Created {rows * cols} tiles.");
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        BuildLevelGrid();
    }

    // Data structures
    [System.Serializable]
    public class GridTileData { public int tileType; public string letter; }
    [System.Serializable]
    public class GridSize { public int x, y; }
    [System.Serializable]
    public class LevelDataModel
    {
        public int bugCount;
        public int wordCount;
        public int timeSec;
        public int totalScore;
        public int levelType;
        public GridSize gridSize;
        public GridTileData[] gridData;
    }
    [System.Serializable]
    public class LevelDataRoot { public LevelDataModel[] data; }
}
