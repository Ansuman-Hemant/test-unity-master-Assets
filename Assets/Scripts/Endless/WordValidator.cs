using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WordValidator : MonoBehaviour
{
    [Header("Assign your wordlist.txt file here")]
    public TextAsset wordListFile;
    private HashSet<string> dictionary;

    void Awake()
    {
        LoadDictionary();
    }

    void LoadDictionary()
    {
        if (wordListFile == null)
        {
            Debug.LogError("No word list assigned to WordValidator!");
            return;
        }

        dictionary = new HashSet<string>(
            wordListFile.text.Split('\n')
            .Select(w => w.Trim().ToUpper())
            .Where(w => w.Length > 0)
        );

        Debug.Log($"Loaded {dictionary.Count} words into dictionary.");
    }

    public bool IsValidWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        string upperWord = word.ToUpper().Trim();
        bool isValid = dictionary != null && dictionary.Contains(upperWord);
        if (!isValid)
        {
            Debug.Log($"Word '{word}' not found in dictionary");
        }
        return isValid;
    }

    // ✨ NEW METHOD: Check if string could be prefix of valid words
    public bool IsValidPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || dictionary == null) return false;

        prefix = prefix.ToUpper().Trim();

        // Check if any word in dictionary starts with this prefix
        foreach (string word in dictionary)
        {
            if (word.StartsWith(prefix))
                return true;
        }

        return false;
    }

    public bool ContainsWord(string word)
    {
        return IsValidWord(word);
    }
}
