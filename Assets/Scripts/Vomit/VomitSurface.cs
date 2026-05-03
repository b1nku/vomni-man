using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class VomitSurface : MonoBehaviour
{
    [SerializeField] int      _inkResolution    = 1024;
    [SerializeField] int      _rippleResolution = 256;
    [SerializeField] Texture2D _splat;
    [SerializeField] Shader   _splatShader;
    [SerializeField] Shader   _normalGenShader;
    [SerializeField] Shader   _rippleShader;
    [SerializeField] Shader   _rippleInjectShader;

    [Header("Ripple")]
    [SerializeField] float _waveSpeed      = 0.5f;
    [SerializeField] float _damping        = 0.985f;
    [SerializeField] float _injectRadius   = 0.06f;
    [SerializeField, Range(0f, 2f)] float _injectStrength = 0.9f;
    [SerializeField] float _normalStrength = 2f;

    // Shader property IDs
    static readonly int _inkTexId          = Shader.PropertyToID("_InkTex");
    static readonly int _inkNormalTexId    = Shader.PropertyToID("_InkNormalTex");
    static readonly int _rippleTexId       = Shader.PropertyToID("_RippleTex");
    static readonly int _splatParamsId     = Shader.PropertyToID("_SplatParams");
    static readonly int _splatTexId        = Shader.PropertyToID("_SplatTex");
    static readonly int _stateTexId        = Shader.PropertyToID("_StateTex");
    static readonly int _injectUVId        = Shader.PropertyToID("_InjectUV");
    static readonly int _injectRadiusId    = Shader.PropertyToID("_InjectRadius");
    static readonly int _injectStrengthId  = Shader.PropertyToID("_InjectStrength");
    static readonly int _texelSizeId       = Shader.PropertyToID("_TexelSize");
    static readonly int _waveSpeedId       = Shader.PropertyToID("_WaveSpeed");
    static readonly int _dampingId         = Shader.PropertyToID("_Damping");
    static readonly int _strengthId        = Shader.PropertyToID("_Strength");
    static readonly int _rippleTexelSizeId = Shader.PropertyToID("_RippleTexelSize");

    RenderTexture _inkRT;
    RenderTexture _inkNormalRT;
    RenderTexture _rippleA, _rippleB;   // ping-pong; each packs (curr, prev) in RG
    bool _readFromA = true;

    Material   _splatMat;
    Material   _normalGenMat;
    Material   _rippleMat;
    Material   _rippleInjectMat;
    Material[] _surfaceMaterials;

    bool _inkDirty;

    void Awake()
    {
        float inkTexel    = 1f / _inkResolution;
        float rippleTexel = 1f / _rippleResolution;

        _inkRT       = CreateRT(_inkResolution,    RenderTextureFormat.ARGB32);
        _inkNormalRT = CreateRT(_inkResolution,    RenderTextureFormat.ARGB32);
        _rippleA     = CreateRT(_rippleResolution, RenderTextureFormat.RGFloat);
        _rippleB     = CreateRT(_rippleResolution, RenderTextureFormat.RGFloat);

        Clear(_inkRT,       Color.black);
        Clear(_inkNormalRT, new Color(0.5f, 0.5f, 1f, 1f)); // flat normal
        Clear(_rippleA,     Color.black);
        Clear(_rippleB,     Color.black);

        if (!ValidateShaders()) return;

        _splatMat        = new Material(_splatShader);
        _normalGenMat    = new Material(_normalGenShader);
        _rippleMat       = new Material(_rippleShader);
        _rippleInjectMat = new Material(_rippleInjectShader);

        _splatMat.SetTexture(_splatTexId, _splat);

        _normalGenMat.SetFloat(_texelSizeId, inkTexel);
        _normalGenMat.SetFloat(_strengthId,  _normalStrength);

        _rippleMat.SetFloat(_texelSizeId, rippleTexel);
        _rippleMat.SetFloat(_waveSpeedId, _waveSpeed);
        _rippleMat.SetFloat(_dampingId,   _damping);

        _surfaceMaterials = GetComponent<Renderer>().materials;
        foreach (Material m in _surfaceMaterials)
        {
            m.SetTexture(_inkTexId,        _inkRT);
            m.SetTexture(_inkNormalTexId,  _inkNormalRT);
            m.SetTexture(_rippleTexId,     _rippleA);
            m.SetFloat(_rippleTexelSizeId, rippleTexel);
        }
    }

    void Update()
    {
        // Regenerate ink normal map only when new ink landed
        if (_inkDirty)
        {
            Graphics.Blit(_inkRT, _inkNormalRT, _normalGenMat);
            _inkDirty = false;
        }

        // Wave simulation: read current buffer → write into the other
        RenderTexture src  = _readFromA ? _rippleA : _rippleB;
        RenderTexture dest = _readFromA ? _rippleB : _rippleA;

        _rippleMat.SetTexture(_stateTexId, src);
        Graphics.Blit(null, dest, _rippleMat);

        _readFromA = !_readFromA;

        foreach (Material m in _surfaceMaterials)
            m.SetTexture(_rippleTexId, dest);
    }

    public void Paint(Vector2 uv, float radius)
    {
        // Stamp ink
        _splatMat.SetVector(_splatParamsId, new Vector4(uv.x, uv.y, radius, radius));
        Graphics.Blit(null, _inkRT, _splatMat);
        _inkDirty = true;

        // Additively inject a wave depression at the hit point
        RenderTexture curr = _readFromA ? _rippleA : _rippleB;
        _rippleInjectMat.SetVector(_injectUVId,        new Vector4(uv.x, uv.y, 0, 0));
        _rippleInjectMat.SetFloat(_injectRadiusId,  _injectRadius);
        _rippleInjectMat.SetFloat(_injectStrengthId, _injectStrength);
        Graphics.Blit(null, curr, _rippleInjectMat);
    }

    RenderTexture CreateRT(int resolution, RenderTextureFormat fmt)
    {
        var rt = new RenderTexture(resolution, resolution, 0, fmt);
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode   = TextureWrapMode.Clamp;
        rt.Create();
        return rt;
    }

    void Clear(RenderTexture rt, Color col)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, col);
        RenderTexture.active = prev;
    }

    bool ValidateShaders()
    {
        if (_splatShader        == null) { Debug.LogError("VomitSurface: _splatShader not assigned",        this); return false; }
        if (_normalGenShader    == null) { Debug.LogError("VomitSurface: _normalGenShader not assigned",    this); return false; }
        if (_rippleShader       == null) { Debug.LogError("VomitSurface: _rippleShader not assigned",       this); return false; }
        if (_rippleInjectShader == null) { Debug.LogError("VomitSurface: _rippleInjectShader not assigned", this); return false; }
        if (_splat              == null) { Debug.LogError("VomitSurface: _splat texture not assigned",      this); return false; }
        return true;
    }

    void OnDestroy()
    {
        _inkRT.Release();
        _inkNormalRT.Release();
        _rippleA.Release();
        _rippleB.Release();
    }
}
