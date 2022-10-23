using Cysharp.Threading.Tasks;
using UnityEngine;

public class ToastController : MonoBehaviour
{
    private int showMillseconds = 1000;

    public async void Show(GameObject parent, string message)
    {
        this.gameObject.transform.SetParent(parent.transform, false);

        await UniTask.Delay(this.showMillseconds);

        Destroy(this.gameObject);
    }
}
