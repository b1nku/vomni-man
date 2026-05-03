using UnityEngine;

public class StomachLiquidRenderer : MonoBehaviour
{
  /*
  Reads FillFraction each frame and pushes it to the material via MaterialPropertyBlock.
  */
  [SerializeField] Renderer _renderer;

  static readonly int _fillId = Shader.PropertyToID("_FillFraction");
  MaterialPropertyBlock _mpb;

  void Awake() => _mpb = new MaterialPropertyBlock();

  void Start()
  {
    StomachSystem.Instance.OnChanged.AddListener(Refresh);
    Refresh();
  }

  void OnDestroy() => StomachSystem.Instance.OnChanged.RemoveListener(Refresh);

  void Refresh()
  {
    _renderer.GetPropertyBlock(_mpb);
    _mpb.SetFloat(_fillId, StomachSystem.Instance.FillFraction);
    _renderer.SetPropertyBlock(_mpb);
  }
}
