using Assets.Scripts;
using System.IO;
using TMPro;
using UnityEngine;

public class DisplayLicenseSceneController : MonoBehaviour
{
    private static string LicenseFilePath => Path.Combine(Application.dataPath, "LICENSE");

    [SerializeField]
    private TextMeshProUGUI LicenseText = default;

    // Start is called before the first frame update
    void Start()
    {
        if (File.Exists(LicenseFilePath))
        {
            this.LicenseText.text = File.ReadAllText(LicenseFilePath);
        }
    }

    public async void OnCloseButtonClick()
    {
        Debug.LogWarning("close button click");

        await Utility.LoadAsyncScene(SceneNames.TitleScene);
    }
}
