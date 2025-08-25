using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public enum Mode { Endless, Levels }
    public static GridManager Instance { get; private set; }

    [Header("General Settings")]
    [SerializeField] private Mode currentMode = Mode.Endless;

    [Header("Endless Mode Settings")]
    [SerializeField] GameObject tilePrefab;
    [SerializeField] RectTransform endlessBoardRoot;
    [SerializeField] int endlessRows = 4, endlessCols = 4;

    [Header("Levels Mode Settings")]
    [SerializeField] GameObject levelTilePrefab;
    [SerializeField] RectTransform levelsBoardRoot;
    [SerializeField] TextAsset levelDataFile;
    [SerializeField] int currentLevel = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("GridManager initialized. Waiting for state to trigger grid building.");
    }

    public void SwitchMode(Mode newMode)
    {
        currentMode = newMode;
        if (newMode == Mode.Endless)
            BuildSeededGrid();
        else
            BuildLevelGrid();
    }

    public void LoadLevel(int index)
    {
        if (currentMode == Mode.Levels)
        {
            currentLevel = index;
            BuildLevelGrid();
        }
        else Debug.LogWarning("LoadLevel() only works in Levels mode.");
    }

    public void RegenerateGrid()
    {
        switch (currentMode)
        {
            case Mode.Endless:
                BuildSeededGrid();
                break;
            case Mode.Levels:
                BuildLevelGrid();
                break;
        }
    }

    public void ReplaceTiles(List<object> tilesUsed)
    {
        if (currentMode != Mode.Endless) return;
        foreach (Tile tile in tilesUsed)
        {
            char newLetter = GenerateSmartLetter(tile);
            TMP_Text txt = tile.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = newLetter.ToString();
        }
    }

    char GenerateSmartLetter(Tile targetTile)
    {
        string allowedLetters = "EEEEEEEEEEEEAAAAAAAAAIIIIIIIIIOOOOOOOONNNNNNRRRRRRTTTTTTLLLLSSSSUUUUDDDDGGGBBCCMMPPFFHHVVWWYYKJXQZ";
        List<char> goodLetters = new List<char>();
        HashSet<char> allowedSet = new HashSet<char>(allowedLetters.ToCharArray());
        List<char> neighborLetters = GetNeighborLetters(targetTile);
        if (neighborLetters.Count == 0)
            return allowedLetters[Random.Range(0, allowedLetters.Length)];
        foreach (char candidateLetter in allowedSet)
        {
            if (CanFormWordWithNeighbors(candidateLetter, neighborLetters))
                goodLetters.Add(candidateLetter);
        }
        return goodLetters.Count > 0 ?
            goodLetters[Random.Range(0, goodLetters.Count)] :
            allowedLetters[Random.Range(0, allowedLetters.Length)];
    }

    List<char> GetNeighborLetters(Tile centerTile)
    {
        List<char> neighbors = new List<char>();
        Vector2Int centerPos = centerTile.GridPosition;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int checkPos = new Vector2Int(centerPos.x + dx, centerPos.y + dy);
                Tile neighborTile = FindTileAtPosition(checkPos);
                if (neighborTile != null && !string.IsNullOrEmpty(neighborTile.Letter))
                    neighbors.Add(char.ToUpper(neighborTile.Letter[0]));
            }
        }
        return neighbors;
    }

    bool CanFormWordWithNeighbors(char candidateLetter, List<char> neighborLetters)
    {
        foreach (char neighbor in neighborLetters)
        {
            string combo1 = candidateLetter.ToString() + neighbor.ToString();
            string combo2 = neighbor.ToString() + candidateLetter.ToString();
            if (WordValidator.Instance != null && WordValidator.Instance.IsValidPrefix(combo1) || WordValidator.Instance.IsValidPrefix(combo2))
                return true;
        }
        return false;
    }

    Tile FindTileAtPosition(Vector2Int position)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
            if (tile.GridPosition == position) return tile;
        return null;
    }

    void BuildSeededGrid()
    {
        Debug.Log("Building Endless Grid.");
        ClearExistingTiles(endlessBoardRoot);
        if (tilePrefab == null)
        {
            Debug.LogError("tilePrefab is not assigned in GridManager.");
            return;
        }
        if (endlessBoardRoot == null)
        {
            Debug.LogError("endlessBoardRoot is not assigned in GridManager.");
            return;
        }
        char[,] tempGrid = new char[endlessRows, endlessCols];
        for (int i = 0; i < endlessRows; i++)
            for (int j = 0; j < endlessCols; j++)
                tempGrid[i, j] = '\0';
        System.Random rng = new System.Random();
        List<string> seedWords = new List<string>
        {
            "CAR", "ART", "EAR", "STAR", "CARE", "REAL", "AREA", "TEAR", "CLEAR"
        };
        for (int i = 0; i < seedWords.Count; i++)
        {
            string temp = seedWords[i];
            int randomIndex = rng.Next(i, seedWords.Count);
            seedWords[i] = seedWords[randomIndex];
            seedWords[randomIndex] = temp;
        }
        int wordsToPlace = rng.Next(3, 5);
        for (int i = 0; i < wordsToPlace && i < seedWords.Count; i++)
            TryPlaceWord(tempGrid, seedWords[i], rng);
        string allowedLetters = "EEEEEEEEEEEEAAAAAAARRRRRRIIIIIIIOOOOOOTTTTTTNNNNNNSSSSSSLLLLCCCCDUUUM";
        for (int i = 0; i < endlessRows; i++)
            for (int j = 0; j < endlessCols; j++)
                if (tempGrid[i, j] == '\0')
                    tempGrid[i, j] = allowedLetters[rng.Next(allowedLetters.Length)];
        var gridLayout = endlessBoardRoot.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = endlessCols;
        }
        for (int i = 0; i < endlessRows; i++)
        {
            for (int j = 0; j < endlessCols; j++)
            {
                GameObject t = Instantiate(tilePrefab, endlessBoardRoot);
                TMP_Text txt = t.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = tempGrid[i, j].ToString();
                Tile tileScript = t.GetComponent<Tile>();
                if (tileScript != null)
                    tileScript.SetGridPosition(i, j);
                else
                    Debug.LogError("Tile prefab missing Tile script.");
            }
        }
        Debug.Log($"Endless grid built with {endlessRows * endlessCols} tiles.");
    }

    void BuildLevelGrid()
    {
        Debug.Log("Building Level Grid.");
        ClearExistingTiles(levelsBoardRoot);
        if (levelDataFile == null)
        {
            Debug.LogError("levelDataFile is not assigned in GridManager.");
            return;
        }
        if (levelTilePrefab == null)
        {
            Debug.LogError("levelTilePrefab is not assigned in GridManager.");
            return;
        }
        if (levelsBoardRoot == null)
        {
            Debug.LogError("levelsBoardRoot is not assigned in GridManager.");
            return;
        }
        LevelDataRoot levelData = JsonUtility.FromJson<LevelDataRoot>(levelDataFile.text);
        if (levelData == null || levelData.data.Length == 0)
        {
            Debug.LogError("Failed to parse level data or data is empty.");
            return;
        }
        if (currentLevel >= levelData.data.Length)
        {
            Debug.LogError($"Level {currentLevel} doesn't exist. Max level: {levelData.data.Length - 1}");
            return;
        }
        var level = levelData.data[currentLevel];
        int rows = level.gridSize.y;
        int cols = level.gridSize.x;
        Debug.Log($"Building level {currentLevel}: {rows}x{cols} grid.");
        var gridLayout = levelsBoardRoot.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = cols;
        }
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                if (index >= level.gridData.Length)
                {
                    Debug.LogError($"Grid data index {index} out of range. Max: {level.gridData.Length - 1}");
                    continue;
                }
                var tileData = level.gridData[index];
                GameObject tileGO = Instantiate(levelTilePrefab, levelsBoardRoot);
                LevelTile tileScript = tileGO.GetComponent<LevelTile>();
                if (tileScript != null)
                {
                    tileScript.SetLetter(tileData.letter);
                    tileScript.SetGridPosition(row, col);
                    switch (tileData.tileType)
                    {
                        case 0:
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(false);
                            break;
                        case 4:
                            tileScript.SetBonus(true);
                            tileScript.SetBlocked(false);
                            break;
                        case 2:
                        case 3:
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(true);
                            break;
                        case 5:
                            tileScript.SetBonus(true);
                            tileScript.SetBlocked(true);
                            break;
                        default:
                            Debug.LogWarning($"Unknown tile type: {tileData.tileType}");
                            tileScript.SetBonus(false);
                            tileScript.SetBlocked(false);
                            break;
                    }
                }
                else
                {
                    Debug.LogError("LevelTile prefab missing LevelTile script.");
                }
            }
        }
        Debug.Log($"Level grid built with {rows * cols} tiles.");
    }

    void ClearExistingTiles(RectTransform boardRoot)
    {
        if (boardRoot == null) return;
        foreach (Transform child in boardRoot)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void TryPlaceWord(char[,] grid, string word, System.Random rng)
    {
        int attempts = 0;
        while (attempts < 100)
        {
            attempts++;
            int x = rng.Next(endlessRows);
            int y = rng.Next(endlessCols);
            int[,] dirs = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { -1, 1 } };
            int choice = rng.Next(4);
            int dx = dirs[choice, 0], dy = dirs[choice, 1];
            int endX = x + dx * (word.Length - 1);
            int endY = y + dy * (word.Length - 1);
            if (endX < 0 || endX >= endlessRows || endY < 0 || endY >= endlessCols)
                continue;
            bool fits = true;
            for (int k = 0; k < word.Length; k++)
            {
                int cx = x + dx * k;
                int cy = y + dy * k;
                char existing = grid[cx, cy];
                if (existing != '\0' && existing != word[k])
                {
                    fits = false;
                    break;
                }
            }
            if (!fits) continue;
            for (int k = 0; k < word.Length; k++)
            {
                int cx = x + dx * k;
                int cy = y + dy * k;
                grid[cx, cy] = word[k];
            }
            break;
        }
    }

    [System.Serializable]
    public class GridTileData { public int tileType; public string letter; }
    [System.Serializable]
    public class GridSize { public int x, y; }
    [System.Serializable]
    public class LevelDataModel
    {
        public int bugCount, wordCount, timeSec, totalScore, levelType;
        public GridSize gridSize;
        public GridTileData[] gridData;
    }
    [System.Serializable]
    public class LevelDataRoot { public LevelDataModel[] data; }
}
