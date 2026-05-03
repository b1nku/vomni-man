using UnityEngine;
using UnityEngine.UI;

public class StomachUI : MonoBehaviour
{
    [SerializeField] Image _fillImage;

    void Start()
    {
        StomachSystem.Instance.OnChanged.AddListener(Refresh);
        Refresh();
    }

    void OnDestroy() => StomachSystem.Instance.OnChanged.RemoveListener(Refresh);

    void Refresh() => _fillImage.fillAmount = StomachSystem.Instance.FillFraction;
}
