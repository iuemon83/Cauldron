using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingViewController : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Image loadingImage = default;
    [SerializeField]
    private TextMeshProUGUI loadingText = default;

    private void Update()
    {
        this.UpdateOutlineColorByTime();
    }

    private void UpdateOutlineColorByTime()
    {
        var color = this.loadingImage.color;
        color.a = this.CalcAlphaByTime(Time.time);
        this.loadingImage.color = color;

        color = this.loadingText.color;
        color.a = this.CalcAlphaByTime(Time.time);
        this.loadingText.color = color;
    }

    private float CalcAlphaByTime(float time)
        => Mathf.Sin(2 * Mathf.PI * 0.5f * time) * 0.5f + 0.5f;

    public void Show(GameObject parent)
    {
        this.gameObject.transform.SetParent(parent.transform, false);
    }

    public void Hide()
    {
        if (this.gameObject != null)
        {
            Destroy(this.gameObject);
        }
    }

    public void Dark()
    {
        var color = this.backgroundImage.color;
        color.a = 1;
        this.backgroundImage.color = color;
    }
}
