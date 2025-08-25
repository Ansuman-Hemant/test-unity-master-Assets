using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] TMP_Text letterText;
    [SerializeField] Image tileImage;

    public string Letter => letterText != null ? letterText.text : "";
    public Vector2Int GridPosition { get; private set; }

    private WordSelector wordSelector;
    private Color normalColor = Color.white;
    private Color selectedColor = Color.green;

    void Start()
    {
        wordSelector = WordSelector.Instance;

        if (tileImage == null)
            tileImage = GetComponent<Image>();

        if (tileImage != null)
        {
            tileImage.raycastTarget = true;
            normalColor = tileImage.color;
        }

        if (letterText != null)
            letterText.raycastTarget = false;
    }

    public void SetGridPosition(int row, int col)
    {
        GridPosition = new Vector2Int(row, col);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Tile {Letter} at {GridPosition} clicked.");
        if (wordSelector != null)
            wordSelector.StartSelection(this);
        else
            Debug.LogError("WordSelector is null in Tile.");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (wordSelector != null)
            wordSelector.AddTile(this);
    }

    public void Highlight(bool selected)
    {
        if (tileImage != null)
            tileImage.color = selected ? selectedColor : normalColor;
    }
}
