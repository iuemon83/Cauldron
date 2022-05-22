using Assets.Scripts;
using System.Linq;
using UnityEngine;

public class ListBattleLogsSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject battleLogList = default;
    [SerializeField]
    private ListBattleLogNodeController listNodePrefab = default;

    private Transform listContent;

    // Start is called before the first frame update
    void Start()
    {
        this.listContent = this.battleLogList.transform.Find("Viewport").transform.Find("Content");

        this.RefreshLogList();
    }

    private void RefreshLogList()
    {
        var logs = LocalData.LoadLocalData().BattleLogs
            .OrderByDescending(l => l.TimestampText);

        foreach (var n in logs)
        {
            this.AddListNode(n);
        }
    }

    private void AddListNode(LocalBattleLog source)
    {
        var controller = Instantiate(this.listNodePrefab, this.listContent.transform);
        controller.Set(source);
    }

    public async void OnCloseButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListGameScene);
    }
}
