using UnityEngine;
using UnityEngine.UI;

// Via https://assetstore.unity.com/publishers/23053

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    [Header("Title Setting")]
    public string Title;
    public Color TitleColor;
    public Font TitleFont;
    public int TitleFontSize = 10;

    [Header("Bar Setting")]
    public Color BarColor;
    public Color BarBackGroundColor;
    public Sprite BarBackGroundSprite;

    private Image bar, barBackground;
    private Text txtTitle;
    private float barValue;

    public void SetValue(string val, float ratio)
    {
        txtTitle.text = Title + " " + val;
        barValue = ratio;
    }

    private void Awake()
    {
        bar = transform.Find("Bar").GetComponent<Image>();
        barBackground = GetComponent<Image>();
        txtTitle = transform.Find("Text").GetComponent<Text>();
        barBackground = transform.Find("BarBackground").GetComponent<Image>();
    }

    private void Start()
    {
        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bar.color = BarColor;
        barBackground.color = BarBackGroundColor;
        barBackground.sprite = BarBackGroundSprite;

        UpdateValue(barValue);
    }

    private void UpdateValue(float val)
    {
        bar.fillAmount = val / 100f;
        txtTitle.text = Title + " " + val;
        bar.color = BarColor;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateValue(50);
            txtTitle.color = TitleColor;
            txtTitle.font = TitleFont;
            txtTitle.fontSize = TitleFontSize;

            bar.color = BarColor;
            barBackground.color = BarBackGroundColor;
            barBackground.sprite = BarBackGroundSprite;
        }
        else
        {
            bar.fillAmount = Mathf.Lerp(bar.fillAmount, barValue, Time.deltaTime * 10);
        }
    }
}
