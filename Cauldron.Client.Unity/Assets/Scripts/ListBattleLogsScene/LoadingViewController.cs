using TMPro;
using UnityEngine;

public class LoadingViewController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerNameText = default;

    private void Update()
    {
        this.UpdateOutlineColorByTime();
    }

    private void UpdateOutlineColorByTime()
    {
        var color = this.playerNameText.color;
        color.a = Mathf.Sin(2 * Mathf.PI * 0.5f * Time.time) * 0.5f + 0.5f;
        this.playerNameText.color = color;
    }

    public void Show(GameObject parent)
    {
        this.gameObject.transform.SetParent(parent.transform, false);
    }

    public void Hide()
    {
        Destroy(this.gameObject);
    }
}
