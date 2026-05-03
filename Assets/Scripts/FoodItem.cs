using UnityEngine;

public class FoodItem : MonoBehaviour
{
  [SerializeField] float _bobHeight = 0.15f;
  [SerializeField] float _bobSpeed = 2f;
  [SerializeField] float _rotateSpeed = 90f;

  Vector3 _origin;

  void Start() => _origin = transform.position;

  void Update()
  {
    transform.position = _origin + Vector3.up * Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
    transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
  }
}
