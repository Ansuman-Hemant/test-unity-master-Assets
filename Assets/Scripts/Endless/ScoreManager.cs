using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<string, int> foundWords = new Dictionary<string, int>();
    private int totalScore = 0;

    public int TotalScore => totalScore;
    public float AverageScore => foundWords.Count > 0 ? (float)totalScore / foundWords.Count : 0f;
    public int WordCount => foundWords.Count;

    public void AddWord(string word, int points)
    {
        if (!foundWords.ContainsKey(word))
        {
            foundWords[word] = points;
            totalScore += points;

            Debug.Log($"New word added: {word} ({points} points)");
            Debug.Log($"Total Score: {totalScore}, Words Found: {foundWords.Count}, Average: {AverageScore:F1}");
        }
        else
        {
            Debug.Log($"Word '{word}' already found!");
        }
    }

    public bool HasWord(string word)
    {
        return foundWords.ContainsKey(word);
    }

    public List<string> GetFoundWords()
    {
        return foundWords.Keys.ToList();
    }

    public void ResetScore()
    {
        foundWords.Clear();
        totalScore = 0;
    }

    public void DisplayStats()
    {
        Debug.Log("=== GAME STATS ===");
        Debug.Log($"Total Words Found: {WordCount}");
        Debug.Log($"Total Score: {TotalScore}");
        Debug.Log($"Average Score per Word: {AverageScore:F1}");

        var sortedWords = foundWords.OrderByDescending(w => w.Value).ToList();
        Debug.Log("Words found (by score):");
        foreach (var kvp in sortedWords)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} points");
        }
    }
}
