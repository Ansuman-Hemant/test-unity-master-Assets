using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WordValidator : MonoBehaviour
{
    public static WordValidator Instance { get; private set; }

    [Header("Assign your wordlist.txt file here")]
    public TextAsset wordListFile;

    private HashSet<string> dictionary;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        LoadDictionary();
    }

    void LoadDictionary()
    {
        if (wordListFile == null) { Debug.LogError("No word list assigned to WordValidator!"); return; }
        dictionary = new HashSet<string>(
            wordListFile.text.Split('\n').Select(w => w.Trim().ToUpper()).Where(w => w.Length > 0)
        );
        Debug.Log($"Loaded {dictionary.Count} words into dictionary.");
    }

    public void ValidateAndRelay(string word, List<object> selectedTiles, WordSelector.GameMode mode)
    {
        // Length check and dictionary check
        if (word.Length < 3 || word.Length > 8)
        {
            Debug.Log($"INVALID WORD (length): {word}");
            return;
        }

        if (ScoreManager.Instance.HasFoundWord(word))
        {
            Debug.Log($"WORD ALREADY FOUND: {word}");
            return;
        }

        if (!IsValidWord(word))
        {
            Debug.Log($"INVALID WORD: {word}");
            return;
        }

        Debug.Log($"✅ VALID WORD: {word}");

        // Calculate bonus tiles used and handle blocked tiles (for levels mode)
        int bonusCount = 0;
        if (mode == WordSelector.GameMode.Levels)
        {
            List<LevelTile> levelTiles = new List<LevelTile>();

            foreach (var tile in selectedTiles)
            {
                LevelTile levelTile = tile as LevelTile;
                if (levelTile != null)
                {
                    levelTiles.Add(levelTile);

                    // Count and consume bonus tiles
                    if (levelTile.IsBonus && !levelTile.BonusUsed)
                    {
                        bonusCount++;
                        levelTile.ConsumeBonus();
                    }
                }
            }

            // Check for adjacent blocked tiles to unblock
            UnblockAdjacentTiles(levelTiles);

            // Notify LevelManager with proper type
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.OnWordMade(word, bonusCount, levelTiles);
            }
            else
            {
                Debug.LogWarning("LevelManager not found!");
            }
        }

        // Notify ScoreManager (handles scoring and tile replacement)
        ScoreManager.Instance.OnWordValidated(word, selectedTiles.Count, selectedTiles, mode);
    }

    void UnblockAdjacentTiles(List<LevelTile> usedTiles)
    {
        // Find all tiles in the grid
        LevelTile[] allTiles = FindObjectsOfType<LevelTile>();

        foreach (var usedTile in usedTiles)
        {
            Vector2Int usedPos = usedTile.GridPosition;

            // Check all adjacent positions
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip the used tile itself

                    Vector2Int adjacentPos = new Vector2Int(usedPos.x + dx, usedPos.y + dy);

                    // Find tile at adjacent position
                    foreach (var tile in allTiles)
                    {
                        if (tile.GridPosition == adjacentPos && tile.IsBlocked)
                        {
                            tile.Unblock();
                            Debug.Log($"🔓 Unblocked tile at {adjacentPos}");
                        }
                    }
                }
            }
        }
    }

    public bool IsValidWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        return dictionary != null && dictionary.Contains(word.ToUpper().Trim());
    }

    public bool IsValidPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || dictionary == null) return false;
        prefix = prefix.ToUpper().Trim();
        return dictionary.Any(w => w.StartsWith(prefix));
    }
}
