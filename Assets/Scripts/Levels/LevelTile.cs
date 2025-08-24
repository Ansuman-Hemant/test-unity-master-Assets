using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelTile : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] TMP_Text letterText;
    [SerializeField] Image tileImage;
    [Header("Overlay Sprites (Children)")]
    [SerializeField] GameObject bonusSprite;  // Assign in inspector
    [SerializeField] GameObject blockedSprite;  // Assign in inspector

    public string Letter => letterText != null ? letterText.text : "";
    public Vector2Int GridPosition { get; private set; }
    public bool IsBonus { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool BonusUsed { get; private set; }

    private LevelWordSelector wordSelector;
    private Coroutine blinkCoroutine;

    void Start()
    {
        wordSelector = FindObjectOfType<LevelWordSelector>();
        if (tileImage == null) tileImage = GetComponent<Image>();
        if (tileImage != null) tileImage.raycastTarget = true;
        if (letterText != null) letterText.raycastTarget = false;

        if (bonusSprite != null) bonusSprite.SetActive(false);
        if (blockedSprite != null) blockedSprite.SetActive(false);

        BonusUsed = false;
        UpdateVisual();
    }

    public void SetLetter(string letter)
    {
        if (letterText != null)
            letterText.text = letter;
    }

    public void SetGridPosition(int row, int col)
    {
        GridPosition = new Vector2Int(row, col);
    }

    public void SetBonus(bool isBonus)
    {
        IsBonus = isBonus;
        BonusUsed = false;
        UpdateVisual();
    }

    public void SetBlocked(bool isBlocked)
    {
        IsBlocked = isBlocked;
        UpdateVisual();
    }

    // Call this when the bonus letter is actually used in a word
    public void ConsumeBonus()
    {
        if (IsBonus && !BonusUsed)
        {
            BonusUsed = true;
            StopBlinking();
            UpdateVisual();
        }
    }

    void UpdateVisual()
    {
        if (tileImage != null)
            tileImage.color = Color.yellow;

        if (bonusSprite != null)
        {
            bool showBonus = IsBonus && !BonusUsed;
            bonusSprite.SetActive(showBonus);
            if (showBonus)
                StartBlinking();
            else
                StopBlinking();
        }

        if (blockedSprite != null)
            blockedSprite.SetActive(IsBlocked);
    }

    void StartBlinking()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkBonus());
    }

    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    IEnumerator BlinkBonus()
    {
        if (bonusSprite == null || !bonusSprite.activeInHierarchy) yield break;
        Image bonusImage = bonusSprite.GetComponent<Image>();
        if (bonusImage == null) yield break;
        while (IsBonus && !BonusUsed && bonusSprite.activeInHierarchy)
        {
            yield return StartCoroutine(FadeAlpha(bonusImage, 1f, 0f, 1f));
            yield return StartCoroutine(FadeAlpha(bonusImage, 0f, 1f, 1f));
        }
    }

    IEnumerator FadeAlpha(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = endAlpha;
        image.color = color;
    }

    public void Unblock()
    {
        if (IsBlocked)
        {
            IsBlocked = false;
            UpdateVisual();
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
                levelManager.OnTileUnblocked();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (wordSelector != null) wordSelector.StartSelection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (wordSelector != null) wordSelector.AddTile(this);
    }

    void OnDestroy()
    {
        StopBlinking();
    }
}
