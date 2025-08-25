using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class WordSelector : MonoBehaviour
{
    public enum GameMode { Endless, Levels }
    public static WordSelector Instance { get; private set; }

    [SerializeField] private GameMode currentMode = GameMode.Endless;
    private List<object> selectedTiles = new List<object>();
    private bool isSelecting = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetMode(GameMode mode) => currentMode = mode;

    public void StartSelection(object tile)
    {
        Debug.Log($"Starting selection with tile: {GetTileLetter(tile)}");
        isSelecting = true;
        selectedTiles.Clear();
        AddTile(tile);
    }

    public void AddTile(object tile)
    {
        if (!isSelecting) return;
        if (selectedTiles.Contains(tile)) return;

        // Check if this tile is adjacent to the last selected tile (except for first tile)
        if (selectedTiles.Count > 0)
        {
            object lastTile = selectedTiles[selectedTiles.Count - 1];
            if (!AreAdjacent(lastTile, tile))
            {
                Debug.Log($"Tile {GetTileLetter(tile)} is not adjacent to {GetTileLetter(lastTile)}");
                return;
            }
        }

        selectedTiles.Add(tile);

        // Highlight the selected tile
        if (currentMode == GameMode.Endless)
            (tile as Tile).Highlight(true);
        else
            (tile as LevelTile).Highlight(true);

        Debug.Log($"Added tile: {GetTileLetter(tile)} (Total: {selectedTiles.Count})");
    }

    bool AreAdjacent(object tile1, object tile2)
    {
        Vector2Int pos1 = GetTilePosition(tile1);
        Vector2Int pos2 = GetTilePosition(tile2);
        int deltaX = Mathf.Abs(pos1.x - pos2.x);
        int deltaY = Mathf.Abs(pos1.y - pos2.y);
        // Adjacent if within 1 tile distance (including diagonals)
        return deltaX <= 1 && deltaY <= 1 && (deltaX != 0 || deltaY != 0);
    }

    Vector2Int GetTilePosition(object tile)
    {
        if (currentMode == GameMode.Endless)
            return (tile as Tile).GridPosition;
        else
            return (tile as LevelTile).GridPosition;
    }

    string GetTileLetter(object tile)
    {
        if (currentMode == GameMode.Endless)
            return (tile as Tile).Letter;
        else
            return (tile as LevelTile).Letter;
    }

    void Update()
    {
        if (isSelecting)
        {
            // For mouse input
            if (Input.GetMouseButtonUp(0))
            {
                EndSelection();
            }

            // For touch input
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                EndSelection();
            }
        }
    }

    public void EndSelection()
    {
        isSelecting = false;
        if (selectedTiles.Count == 0) return;

        string word = BuildWord();
        Debug.Log($"Word formed: {word} with {selectedTiles.Count} tiles");

        if (WordValidator.Instance != null)
        {
            WordValidator.Instance.ValidateAndRelay(word, selectedTiles, currentMode);
        }
        else
        {
            Debug.LogError("WordValidator.Instance is null!");
        }

        // Un-highlight all tiles after selection ends
        foreach (var tile in selectedTiles)
        {
            if (currentMode == GameMode.Endless)
                (tile as Tile).Highlight(false);
            else
                (tile as LevelTile).Highlight(false);
        }

        selectedTiles.Clear();
    }

    string BuildWord()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var tile in selectedTiles)
        {
            if (currentMode == GameMode.Endless)
                sb.Append((tile as Tile).Letter);
            else
                sb.Append((tile as LevelTile).Letter);
        }
        return sb.ToString().ToUpper();
    }
}
