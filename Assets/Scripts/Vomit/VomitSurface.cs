using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class VomitSurface : MonoBehaviour
{
    [SerializeField] int _resolution = 1024;
    [SerializeField] Texture2D _splat;
    [SerializeField] Shader _splatShader;

    static readonly int _inkTexId    = Shader.PropertyToID("_InkTex");
    static readonly int _splatParams = Shader.PropertyToID("_SplatParams");
    static readonly int _splatTex    = Shader.PropertyToID("_SplatTex");

    RenderTexture _inkRT;
    Material _splatMat;

    void Awake()
    {
        _inkRT = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGB32);
        _inkRT.filterMode = FilterMode.Bilinear;
        _inkRT.Create();

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = _inkRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;

        if (_splatShader == null) { Debug.LogError("VomitSurface: _splatShader not assigned", this); return; }
        if (_splat == null)       { Debug.LogError("VomitSurface: _splat texture not assigned", this); return; }

        _splatMat = new Material(_splatShader);
        _splatMat.SetTexture(_splatTex, _splat);

        foreach (Material m in GetComponent<Renderer>().materials)
            m.SetTexture(_inkTexId, _inkRT);
    }

    public void Paint(Vector2 uv, float radius)
    {
        _splatMat.SetVector(_splatParams, new Vector4(uv.x, uv.y, radius, radius));
        Graphics.Blit(null, _inkRT, _splatMat);
    }

    void OnDestroy() => _inkRT.Release();
}
