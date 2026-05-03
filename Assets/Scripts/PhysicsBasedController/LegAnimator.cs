using UnityEngine;

public class LegAnimator : MonoBehaviour
{
    [SerializeField] Transform _thighL;
    [SerializeField] Transform _thighR;
    [SerializeField] Rigidbody _rb;

    [Header("Swing")]
    [SerializeField] float _minAngle = 10f;
    [SerializeField] float _maxAngle = 30f;
    [SerializeField] float _minSpeed = 0.5f;
    [SerializeField] float _maxSpeed = 8f;
    [SerializeField] float _cycleSpeed = 5f;
    [SerializeField] float _blendSpeed = 8f;

    Quaternion _restL;
    Quaternion _restR;
    float _phase;
    float _currentAngle;

    void Start()
    {
        _restL = _thighL.localRotation;
        _restR = _thighR.localRotation;
    }

    void Update()
    {
        float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).magnitude;
        bool isMoving = horizontalSpeed > _minSpeed;

        float targetAngle = isMoving
            ? Mathf.Lerp(_minAngle, _maxAngle, Mathf.InverseLerp(_minSpeed, _maxSpeed, horizontalSpeed))
            : 0f;

        _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, Time.deltaTime * _blendSpeed);

        if (isMoving)
            _phase += Time.deltaTime * _cycleSpeed;

        float swing = _currentAngle * Mathf.Sin(_phase);

        _thighL.localRotation = _restL * Quaternion.Euler(swing, 0f, 0f);
        _thighR.localRotation = _restR * Quaternion.Euler(-swing, 0f, 0f);
    }
}
