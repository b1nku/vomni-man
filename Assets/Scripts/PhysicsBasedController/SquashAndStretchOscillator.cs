using UnityEngine;

public class SquashAndStretchOscillator : MonoBehaviour
{
    [SerializeField] float _springStrength = 300f;
    [SerializeField] float _damping = 10f;
    [Tooltip("How much the spring force translates to visible scale change. Start small and tune up.")]
    [SerializeField] float _scaleMultiplier = 1.5f;

    float _compression;
    float _velocity;
    Vector3 _defaultScale;

    void Start()
    {
        _defaultScale = transform.localScale;
    }

    public void ApplyForce(Vector3 force)
    {
        // Negative sign: upward impact force (landing) drives compression (squash),
        // downward force (takeoff) drives extension (stretch).
        _velocity -= force.y * Time.fixedDeltaTime;
    }

    void Update()
    {
        float springForce = -_springStrength * _compression;
        float dampingForce = -_damping * _velocity;
        _velocity += (springForce + dampingForce) * Time.deltaTime;
        _compression += _velocity * Time.deltaTime;

        float yScale = Mathf.Max(0.1f, 1f + _compression * _scaleMultiplier);
        float xzScale = 1f / Mathf.Sqrt(yScale);

        transform.localScale = new Vector3(
            _defaultScale.x * xzScale,
            _defaultScale.y * yScale,
            _defaultScale.z * xzScale
        );
    }
}
