using UnityEngine;

public class CapeWindDriver : MonoBehaviour
{
    [SerializeField] Cloth _cloth;
    [SerializeField] Rigidbody _rb;

    [Header("Wind")]
    [SerializeField] float _minSpeed = 0.5f;
    [SerializeField] float _maxSpeed = 8f;
    [SerializeField] float _minForce = 2f;
    [SerializeField] float _maxForce = 20f;
    [SerializeField] float _blendSpeed = 6f;

    Vector3 _currentWind;

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        Vector3 targetWind = Vector3.zero;
        if (speed > _minSpeed)
        {
            float t = Mathf.InverseLerp(_minSpeed, _maxSpeed, speed);
            float force = Mathf.Lerp(_minForce, _maxForce, t);
            targetWind = (-horizontalVelocity.normalized) * force;
        }

        _currentWind = Vector3.Lerp(_currentWind, targetWind, Time.deltaTime * _blendSpeed);
        _cloth.externalAcceleration = _currentWind;
    }
}
