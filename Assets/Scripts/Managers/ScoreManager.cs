using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI averageScoreText;

    private int totalScore = 0;
    private int wordCount = 0;
    private List<string> foundWords = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called only by WordValidator
    public void OnWordValidated(string word, int selectionLength, List<object> tilesUsed, WordSelector.GameMode mode)
    {
        int score = CalculateScore(selectionLength);
        totalScore += score;
        wordCount++;
        foundWords.Add(word);
        UpdateUI();

        if (mode == WordSelector.GameMode.Endless)
        {
            GridManager.Instance.ReplaceTiles(tilesUsed);
        }
    }

    int CalculateScore(int length)
    {
        return (length >= 3) ? length - 2 : 0;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {totalScore}";
        if (averageScoreText != null)
        {
            float average = wordCount > 0 ? (float)totalScore / wordCount : 0f;
            averageScoreText.text = $"Average Word Score: {average:F1}";
        }
    }

    public bool HasFoundWord(string word)
    {
        return foundWords.Contains(word);
    }

    public void ResetScore()
    {
        totalScore = 0;
        wordCount = 0;
        foundWords.Clear();
        UpdateUI();
    }
}
