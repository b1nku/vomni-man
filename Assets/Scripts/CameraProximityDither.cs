using UnityEngine;

public class CameraProximityDither : MonoBehaviour
{
    [Tooltip("Distance at which dithering begins")]
    [SerializeField] float _fadeStartDistance = 2f;
    [Tooltip("Distance at which mesh is fully dithered away")]
    [SerializeField] float _fadeEndDistance = 0.5f;

    static readonly int _fadeId = Shader.PropertyToID("_Fade");
    MaterialPropertyBlock _mpb;
    MeshRenderer[] _renderers;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
    }

    void Update()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
        float fade = Mathf.InverseLerp(_fadeEndDistance, _fadeStartDistance, dist);

        foreach (MeshRenderer r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_fadeId, fade);
            r.SetPropertyBlock(_mpb);
        }
    }
}
