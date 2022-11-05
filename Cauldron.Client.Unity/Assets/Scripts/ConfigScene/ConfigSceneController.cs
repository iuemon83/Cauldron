using UnityEngine;
using System.Linq;
using TMPro;
using Assets.Scripts;

public class ConfigSceneController : MonoBehaviour
{
    private static readonly (int w, int h)[] screenSizeList = new[] {
        (480, 272),
        (1280, 720),
        (1366, 768),
        (1920, 1080),
        (2048, 1152),
        (2560, 1440),
        (3200, 1800),
        (3840, 2160),
        (7680, 4320),
    };

    [SerializeField]
    private TMP_Dropdown screenSizeDropdown = default;

    private void Start()
    {
        this.screenSizeDropdown.ClearOptions();
        this.screenSizeDropdown.AddOptions(screenSizeList.Select(x => $"{x.w}×{x.h}").ToList());
        this.screenSizeDropdown.value =
            screenSizeList.Select((x, i) => (x, i)).FirstOrDefault(x => x.x.w == Screen.width && x.x.h == Screen.height).i;
    }

    public void OnValueChanged(int value)//値更新後の処理
    {
        Debug.Log($"{value}番目の要素が選ばれた");

        var size = screenSizeList[value];
        Screen.SetResolution(size.w, size.h, FullScreenMode.Windowed, 60);
    }

    public async void OnCloseButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.TitleScene);
    }
}