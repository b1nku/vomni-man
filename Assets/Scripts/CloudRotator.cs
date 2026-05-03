using UnityEngine;

public class CloudRotator : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    [SerializeField] float _bobHeight = 0.5f;
    [SerializeField] float _bobSpeed = 0.4f;

    float _originY;

    void Start() => _originY = transform.position.y;

    void Update()
    {
        transform.Rotate(Vector3.up, _speed * Time.deltaTime);
        Vector3 pos = transform.position;
        pos.y = _originY + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
        transform.position = pos;
    }
}
