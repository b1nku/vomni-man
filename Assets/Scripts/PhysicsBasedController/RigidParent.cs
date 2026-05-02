using UnityEngine;

public class RigidParent : MonoBehaviour
{
    public Rigidbody _targetRb;

    void Awake()
    {
        if (_targetRb != null)
        {
            transform.position = _targetRb.transform.position;
            transform.rotation = _targetRb.transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (_targetRb != null)
        {
            transform.position = _targetRb.transform.position;
            transform.rotation = _targetRb.transform.rotation;
        }
    }
}
