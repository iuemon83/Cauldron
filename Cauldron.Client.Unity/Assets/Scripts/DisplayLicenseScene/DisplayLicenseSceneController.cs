using Assets.Scripts;
using TMPro;
using UnityEngine;

public class DisplayLicenseSceneController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI LicenseText = default;

    // Start is called before the first frame update
    void Start()
    {
        var t = Resources.Load<TextAsset>("LICENSE");
        if (t != null)
        {
            this.LicenseText.text = t.text;
        }
    }

    public async void OnCloseButtonClick()
    {
        Debug.Log("close button click");

        await Utility.LoadAsyncScene(SceneNames.TitleScene);
    }
}
