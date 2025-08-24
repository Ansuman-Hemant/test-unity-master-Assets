using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] TMP_Text letterText;
    [SerializeField] Image tileImage;

    public string Letter => letterText.text;

    // Grid coordinates for adjacency check
    public Vector2Int GridPosition { get; private set; }

    private WordSelector wordSelector;

    void Start()
    {
        wordSelector = FindObjectOfType<WordSelector>();

        // Ensure the tile has proper raycast blocking
        if (tileImage == null)
            tileImage = GetComponent<Image>();

        if (tileImage != null)
        {
            tileImage.raycastTarget = true;
        }

        // Make sure the text doesn't interfere with raycasts
        if (letterText != null)
        {
            letterText.raycastTarget = false;
        }
    }

    // Called from GridManager when spawning
    public void SetGridPosition(int row, int col)
    {
        GridPosition = new Vector2Int(row, col);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (wordSelector != null)
            wordSelector.StartSelection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (wordSelector != null)
            wordSelector.AddTile(this);
    }
}
