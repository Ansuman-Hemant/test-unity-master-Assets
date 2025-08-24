using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class LevelWordSelector : MonoBehaviour
{
    private List<LevelTile> selectedTiles = new List<LevelTile>();
    private bool isSelecting = false;
    private WordValidator validator;
    private LevelManager levelManager;

    // ⭐ NEW: Scoring system from WordSelector
    [Header("UI References - TextMeshPro")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI averageScoreText;

    // Score tracking
    private int totalScore = 0;
    private int wordCount = 0;
    private List<string> foundWords = new List<string>();

    void Start()
    {
        validator = FindObjectOfType<WordValidator>();
        levelManager = FindObjectOfType<LevelManager>();
        UpdateUI(); // Initialize UI
    }

    void Update()
    {
        if (isSelecting && Input.GetMouseButtonUp(0))
            EndSelection();
    }

    public void StartSelection(LevelTile startTile)
    {
        isSelecting = true;
        selectedTiles.Clear();
        AddTile(startTile);
    }

    public void AddTile(LevelTile tile)
    {
        if (!isSelecting || tile.IsBlocked) return;

        // Check for backtracking like in WordSelector
        int existingIndex = selectedTiles.IndexOf(tile);
        if (existingIndex != -1)
        {
            // BACKTRACKING: Remove tiles after this position
            if (existingIndex < selectedTiles.Count - 1)
            {
                for (int i = selectedTiles.Count - 1; i > existingIndex; i--)
                {
                    Highlight(selectedTiles[i], false);
                    selectedTiles.RemoveAt(i);
                }
            }
            return;
        }

        // Check adjacency
        if (selectedTiles.Count > 0 && !IsAdjacent(selectedTiles[selectedTiles.Count - 1], tile))
            return;

        selectedTiles.Add(tile);
        Highlight(tile, true);
    }

    void EndSelection()
    {
        isSelecting = false;
        StringBuilder sb = new StringBuilder();
        int bonusCount = 0;

        foreach (var tile in selectedTiles)
        {
            sb.Append(tile.Letter);
            if (tile.IsBonus && !tile.BonusUsed) bonusCount++; // ✨ Check BonusUsed
        }

        string word = sb.ToString().ToUpper();

        // Reset highlighting
        foreach (var tile in selectedTiles)
            Highlight(tile, false);

        // Validate word
        if (word.Length < 3 || word.Length > 8)
        {
            Debug.Log($"INVALID WORD (length): {word}");
        }
        else if (validator != null && validator.IsValidWord(word))
        {
            // ⭐ Check if word already found (like WordSelector)
            if (!foundWords.Contains(word))
            {
                // Calculate score using same system as WordSelector
                int score = CalculateCustomScore(word.Length);
                totalScore += score;
                wordCount++;
                foundWords.Add(word);
                Debug.Log($"VALID WORD: {word} — Score: {score}");

                // ✨ NEW: Consume bonus letters BEFORE unlocking adjacent tiles
                foreach (var tile in selectedTiles)
                {
                    if (tile.IsBonus && !tile.BonusUsed)
                    {
                        tile.ConsumeBonus();
                        Debug.Log($"🎯 Consumed bonus on tile: {tile.Letter} at {tile.GridPosition}");
                    }
                }

                // Unlock blocked neighbors
                foreach (var tile in selectedTiles)
                    TryUnblockAdjacent(tile);

                // Notify level manager
                levelManager.OnWordMade(word, bonusCount, selectedTiles);

                // Update UI
                UpdateUI();
            }
            else
            {
                Debug.Log($"WORD ALREADY FOUND: {word}");
            }
        }
        else
        {
            Debug.Log($"INVALID WORD (not in wordlist): {word}");
        }

        selectedTiles.Clear();
    }

    // ⭐ Same scoring system as WordSelector
    private int CalculateCustomScore(int length)
    {
        if (length >= 3) return length - 2; // 3=1pt, 4=2pts, 5=3pts, 6=4pts, 7=5pts, 8=6pts
        return 0;
    }

    // ⭐ UI update system from WordSelector
    private void UpdateUI()
    {
        // Update score display
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}";

        // Update average score display
        if (averageScoreText != null)
        {
            float average = wordCount > 0 ? (float)totalScore / wordCount : 0f;
            averageScoreText.text = $"Average Word Score: {average:F1}";
        }
    }

    // ⭐ Public methods for score management (like WordSelector)
    public void ResetScore()
    {
        totalScore = 0;
        wordCount = 0;
        foundWords.Clear();
        UpdateUI();
    }

    public int GetTotalScore() => totalScore;
    public int GetWordCount() => wordCount;
    public List<string> GetFoundWords() => new List<string>(foundWords);

    void TryUnblockAdjacent(LevelTile tile)
    {
        // Unblock tiles adjacent to this tile (check 8 neighbors)
        Vector2Int pos = tile.GridPosition;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int neighborPos = new Vector2Int(pos.x + dx, pos.y + dy);
                LevelTile neighbor = FindTileAtGrid(neighborPos);
                if (neighbor != null && neighbor.IsBlocked)
                    neighbor.Unblock();
            }
        }
    }

    LevelTile FindTileAtGrid(Vector2Int pos)
    {
        foreach (var tile in FindObjectsOfType<LevelTile>())
            if (tile.GridPosition == pos) return tile;
        return null;
    }

    bool IsAdjacent(LevelTile a, LevelTile b)
    {
        var dx = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        var dy = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
    }

    void Highlight(LevelTile tile, bool selected)
    {
        var img = tile.GetComponent<Image>();
        if (img != null)
        {
            if (selected)
                img.color = Color.green;
            else
                img.color = tile.IsBlocked ? Color.gray : (tile.IsBonus && !tile.BonusUsed ? Color.magenta : Color.yellow); // ✨ Updated highlight logic
        }
    }
}
