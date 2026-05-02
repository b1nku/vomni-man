using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KeepUpright : MonoBehaviour
{
    [SerializeField] float _springStrength = 40f;
    [SerializeField] float _damper = 5f;

    Rigidbody _rb;

    void Awake() => _rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (forward == Vector3.zero) return;

        Quaternion targetRotation  = Quaternion.LookRotation(forward, Vector3.up);
        Quaternion currentRotation = transform.rotation;
        Quaternion delta           = UtilsMath.ShortestRotation(targetRotation, currentRotation);

        delta.ToAngleAxis(out float degrees, out Vector3 axis);
        axis.Normalize();

        _rb.AddTorque((axis * (degrees * Mathf.Deg2Rad * _springStrength)) - (_rb.angularVelocity * _damper));
    }
}
