using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] RectTransform boardRoot; // assign BoardRoot (Panel with GridLayoutGroup)
    [SerializeField] GameObject tilePrefab; // assign TilePrefab

    int rows = 4, cols = 4;

    void Start()
    {
        BuildSeededGrid();
    }

    void BuildSeededGrid()
    {
        char[,] tempGrid = new char[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                tempGrid[i, j] = '\0';

        System.Random rng = new System.Random();

        // ✨ UPDATED: Only seed words that use allowed letters
        List<string> seedWords = new List<string> {
            "CAR", "ART", "EAR", "STAR", "CARE", "REAL", "AREA", "TEAR", "CLEAR"
        };

        // Shuffle and take random subset
        for (int i = 0; i < seedWords.Count; i++)
        {
            string temp = seedWords[i];
            int randomIndex = rng.Next(i, seedWords.Count);
            seedWords[i] = seedWords[randomIndex];
            seedWords[randomIndex] = temp;
        }

        // Place 3-4 words maximum to avoid overcrowding
        int wordsToPlace = rng.Next(3, 5);
        for (int i = 0; i < wordsToPlace && i < seedWords.Count; i++)
        {
            TryPlaceWord(tempGrid, seedWords[i], rng);
        }

        // ✨ UPDATED: Fill with ONLY allowed letters
        string allowedLetters = "EEEEEEEEEEEEAAAAAAARRRRRRIIIIIIIOOOOOOTTTTTTNNNNNNSSSSSSLLLLCCCCDUUUM";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (tempGrid[i, j] == '\0')
                    tempGrid[i, j] = allowedLetters[rng.Next(allowedLetters.Length)];
            }
        }

        // Spawn tiles
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject t = Instantiate(tilePrefab, boardRoot);
                TMP_Text txt = t.GetComponentInChildren<TMP_Text>();
                txt.text = tempGrid[i, j].ToString();
                // Get the Tile component and set grid position
                Tile tileScript = t.GetComponent<Tile>();
                tileScript.SetGridPosition(i, j);
            }
        }
    }

    void TryPlaceWord(char[,] grid, string word, System.Random rng)
    {
        int attempts = 0;
        while (attempts < 100) // Prevent infinite loops
        {
            attempts++;

            int x = rng.Next(rows);
            int y = rng.Next(cols);

            // directions: horizontal, vertical, diag↘, diag↗
            int[,] dirs = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { -1, 1 } };
            int choice = rng.Next(4);
            int dx = dirs[choice, 0], dy = dirs[choice, 1];

            int endX = x + dx * (word.Length - 1);
            int endY = y + dy * (word.Length - 1);

            if (endX < 0 || endX >= rows || endY < 0 || endY >= cols)
                continue;

            // check overlap
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

            // place the word
            for (int k = 0; k < word.Length; k++)
            {
                int cx = x + dx * k;
                int cy = y + dy * k;
                grid[cx, cy] = word[k];
            }

            break; // Successfully placed
        }
    }
}
