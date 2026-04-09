using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private Sprite[] digitSprites;
    [SerializeField] private float digitSpacing = 24f;
    [SerializeField] private int maxDigits = 4;

    private List<Image> digitImages = new List<Image>();

    private void Awake()
    {
        for (int i = 0; i < maxDigits; i++)
        {
            GameObject go = new GameObject($"Digit_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24f, 36f);
            digitImages.Add(img);
            go.SetActive(false);
        }
    }

    public void SetScore(int score)
    {
        string s = score.ToString();

        for (int i = 0; i < digitImages.Count; i++)
            digitImages[i].gameObject.SetActive(false);

        float totalWidth = s.Length * digitSpacing;
        float startX = -(totalWidth - digitSpacing) * 0.5f;

        for (int i = 0; i < s.Length && i < maxDigits; i++)
        {
            int digit = s[i] - '0';
            digitImages[i].sprite = digitSprites[digit];
            digitImages[i].SetNativeSize();
            RectTransform rt = digitImages[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * digitSpacing, 0f);
            digitImages[i].gameObject.SetActive(true);
        }
    }
}
