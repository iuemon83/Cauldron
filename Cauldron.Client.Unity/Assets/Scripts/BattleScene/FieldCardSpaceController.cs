using UnityEngine;
using UnityEngine.UI;

public class FieldCardSpaceController : MonoBehaviour
{
    [SerializeField]
    private Image DisableImage = default;

    public void SetEnable(bool value)
    {
        this.DisableImage.gameObject.SetActive(!value);
    }
}
