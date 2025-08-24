using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Collections;

public class WordSelector : MonoBehaviour
{
    private List<Tile> selectedTiles = new List<Tile>();
    private bool isSelecting = false;
    private WordValidator validator;
    private GridManager gridManager;

    [Header("UI References - TextMeshPro")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI averageScoreText;

    // Store original tile color
    private Color originalTileColor = Color.yellow;

    // Score tracking
    private int totalScore = 0;
    private int wordCount = 0;
    private List<string> foundWords = new List<string>();

    void Start()
    {
        validator = FindObjectOfType<WordValidator>();
        gridManager = FindObjectOfType<GridManager>();
        UpdateUI();
    }

    void Update()
    {
        // Mouse / Touch release
        if (isSelecting && (Input.GetMouseButtonUp(0) || (Input.touchCount == 0 && Input.GetMouseButton(0) == false)))
        {
            EndSelection();
        }
    }

    public void StartSelection(Tile startTile)
    {
        isSelecting = true;
        selectedTiles.Clear();
        AddTile(startTile);
    }

    public void AddTile(Tile tile)
    {
        if (!isSelecting) return;

        // Check if this tile is already selected
        int existingIndex = selectedTiles.IndexOf(tile);
        if (existingIndex != -1)
        {
            // BACKTRACKING: If we hover over a previously selected tile,
            // remove all tiles after it (allowing user to go back)
            if (existingIndex < selectedTiles.Count - 1)
            {
                // Remove tiles from the end back to this position
                for (int i = selectedTiles.Count - 1; i > existingIndex; i--)
                {
                    Highlight(selectedTiles[i], false);
                    selectedTiles.RemoveAt(i);
                }
            }
            return; // Don't add the same tile twice
        }

        // If this is the first tile, always allow
        if (selectedTiles.Count == 0)
        {
            selectedTiles.Add(tile);
            Highlight(tile, true);
            return;
        }

        // Must be adjacent to the last selected tile
        Tile lastTile = selectedTiles[selectedTiles.Count - 1];
        if (IsAdjacent(lastTile, tile))
        {
            selectedTiles.Add(tile);
            Highlight(tile, true);
        }
    }

    private void EndSelection()
    {
        isSelecting = false;

        // Build word using selected tiles
        StringBuilder sb = new StringBuilder();
        foreach (var tile in selectedTiles)
        {
            sb.Append(tile.Letter);
        }

        string word = sb.ToString().ToUpper();

        // Reset all selected tiles to original color first
        foreach (var tile in selectedTiles)
        {
            Highlight(tile, false);
        }

        // Boggle rules: only 3-8 letter words
        if (word.Length < 3 || word.Length > 8)
        {
            Debug.Log($"INVALID WORD (length): {word}");
        }
        else if (validator != null && validator.IsValidWord(word))
        {
            // Check if word already found
            if (!foundWords.Contains(word))
            {
                // VALID WORD: Give points and replace tiles
                int score = CalculateCustomScore(word.Length);
                totalScore += score;
                wordCount++;
                foundWords.Add(word);
                Debug.Log($"VALID WORD: {word} — Score: {score}");

                // ✨ Replace tiles with ONLY allowed letters
                ReplaceTilesWithAllowedLetters();
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

    // ✨ UPDATED: Only use allowed letters
    private void ReplaceTilesWithAllowedLetters()
    {
        foreach (var tile in selectedTiles)
        {
            // Generate letter from ONLY allowed letters
            char newLetter = GenerateAllowedLetter(tile);

            // Update tile's letter
            TMP_Text letterText = tile.GetComponentInChildren<TMP_Text>();
            if (letterText != null)
                letterText.text = newLetter.ToString();

            // Reset tile color to original
            var img = tile.GetComponent<Image>();
            if (img != null)
                img.color = originalTileColor;
        }
    }

    // ✨ UPDATED: Generate letters based on neighbors but ONLY from allowed set
    private char GenerateAllowedLetter(Tile targetTile)
    {
        List<char> goodLetters = new List<char>();
        string allowedLetters = "EEEEEEEEEEEEAAAAAAARRRRRRIIIIIIIOOOOOOTTTTTTNNNNNNSSSSSSLLLLCCCCDUUUM";

        // Convert allowed letters to unique set for testing
        HashSet<char> allowedSet = new HashSet<char>(allowedLetters.ToCharArray());

        // Get all adjacent tiles and their letters
        List<char> neighborLetters = GetNeighborLetters(targetTile);

        if (neighborLetters.Count == 0)
        {
            // No neighbors, use weighted random from allowed letters
            return allowedLetters[Random.Range(0, allowedLetters.Length)];
        }

        // Try each ALLOWED letter only
        foreach (char candidateLetter in allowedSet)
        {
            if (CanFormWordWithNeighbors(candidateLetter, neighborLetters))
            {
                goodLetters.Add(candidateLetter);
            }
        }

        if (goodLetters.Count > 0)
        {
            // Return random letter from good options
            return goodLetters[Random.Range(0, goodLetters.Count)];
        }
        else
        {
            // Fallback to weighted random from allowed letters
            return allowedLetters[Random.Range(0, allowedLetters.Length)];
        }
    }

    // ✨ NEW METHOD: Get letters from adjacent tiles
    private List<char> GetNeighborLetters(Tile centerTile)
    {
        List<char> neighbors = new List<char>();
        Vector2Int centerPos = centerTile.GridPosition;

        // Check all 8 directions around the tile
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center tile

                Vector2Int checkPos = new Vector2Int(centerPos.x + dx, centerPos.y + dy);
                Tile neighborTile = FindTileAtPosition(checkPos);

                if (neighborTile != null && !string.IsNullOrEmpty(neighborTile.Letter))
                {
                    neighbors.Add(char.ToUpper(neighborTile.Letter[0]));
                }
            }
        }

        return neighbors;
    }

    // ✨ NEW METHOD: Check if candidate letter can form words
    private bool CanFormWordWithNeighbors(char candidateLetter, List<char> neighborLetters)
    {
        if (validator == null) return true;

        // Check common 2-3 letter combinations
        foreach (char neighbor in neighborLetters)
        {
            // Check 2-letter combinations
            string combo1 = candidateLetter.ToString() + neighbor.ToString();
            string combo2 = neighbor.ToString() + candidateLetter.ToString();

            if (IsValidWordPrefix(combo1) || IsValidWordPrefix(combo2))
                return true;

            // Check 3-letter combinations with other neighbors
            foreach (char otherNeighbor in neighborLetters)
            {
                if (otherNeighbor == neighbor) continue;

                string combo3 = neighbor.ToString() + candidateLetter.ToString() + otherNeighbor.ToString();
                string combo4 = candidateLetter.ToString() + neighbor.ToString() + otherNeighbor.ToString();

                if (IsValidWordPrefix(combo3) || IsValidWordPrefix(combo4))
                    return true;
            }
        }

        return false;
    }

    // ✨ NEW METHOD: Check if string could start a valid word
    private bool IsValidWordPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || prefix.Length < 2) return false;

        // Use validator's prefix checking if available
        if (validator != null)
        {
            return validator.IsValidPrefix(prefix);
        }

        // Fallback: check common English prefixes
        string[] commonPrefixes = {
            "TH", "HE", "IN", "ER", "AN", "RE", "ED", "ND", "ON", "EN", "AT", "OU", "IT", "BE", "WA", "OF", "ET", "TO",
            "ST", "AR", "SE", "AS", "OR", "HA", "NG", "ME", "VE", "TE", "ES", "TI", "AL", "LE", "SA", "IS", "WH", "NE"
        };

        foreach (string common in commonPrefixes)
        {
            if (prefix.StartsWith(common) || common.StartsWith(prefix))
                return true;
        }

        return true; // Give it a chance
    }

    // ✨ NEW METHOD: Find tile at specific grid position
    private Tile FindTileAtPosition(Vector2Int position)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            if (tile.GridPosition == position)
                return tile;
        }
        return null;
    }

    private bool IsAdjacent(Tile a, Tile b)
    {
        Vector2Int posA = a.GridPosition;
        Vector2Int posB = b.GridPosition;
        int dx = Mathf.Abs(posA.x - posB.x);
        int dy = Mathf.Abs(posA.y - posB.y);
        // Adjacent if dx ≤ 1 and dy ≤ 1 and not the same tile
        return (dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0));
    }

    private void Highlight(Tile tile, bool selected)
    {
        var img = tile.GetComponent<Image>();
        if (img != null)
            img.color = selected ? Color.green : originalTileColor;
    }

    // YOUR CUSTOM SCORING SYSTEM
    private int CalculateCustomScore(int length)
    {
        if (length >= 3) return length - 2; // 3=1pt, 4=2pts, 5=3pts, 6=4pts, 7=5pts, 8=6pts
        return 0;
    }

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

    // Public method to reset the game
    public void ResetGame()
    {
        totalScore = 0;
        wordCount = 0;
        foundWords.Clear();
        UpdateUI();
    }

    // Public method to get found words (for display purposes)
    public List<string> GetFoundWords()
    {
        return new List<string>(foundWords);
    }
}
