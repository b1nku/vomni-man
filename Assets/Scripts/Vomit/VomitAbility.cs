using UnityEngine;

public class VomitAbility : MonoBehaviour
{
  /*
  Attached to player. Handles input, spawns projectile or fires spray depending on context.
  */
  [SerializeField] GameObject _projectilePrefab;
  [SerializeField] Transform _mouthTransform;
  [SerializeField] float _projectileForce = 20f;

  [Header("Spray")]
  [SerializeField] float _sprayConeAngle = 30f;
  [SerializeField] int _sprayRayCount = 12;
  [SerializeField] float _sprayInterval = 0.05f;

  [Header("Stomach")]
  [SerializeField] float _projectileDrainAmount = 20f;
  [SerializeField] float _sprayDrainAmount = 3f;

  PlayerInputActions _input;
  float _nextSprayTime;

  void Awake()
  {
    _input = new PlayerInputActions();
    _input.Player.VomitFire.performed += _ =>
    {
      if (_input.Player.VomitAim.IsPressed())
        FireProjectile();
    };
  }

  void Update()
  {
    if (_input.Player.VomitFire.IsPressed() && !_input.Player.VomitAim.IsPressed()
        && Time.time >= _nextSprayTime)
    {
      FireSpray();
      _nextSprayTime = Time.time + _sprayInterval;
    }
  }

  void OnEnable()   => _input.Player.Enable();
  void OnDisable()  => _input.Player.Disable();
  void OnDestroy()  => _input.Dispose();

  void FireProjectile()
  {
    if (!StomachSystem.Instance.TryDrain(_projectileDrainAmount)) return;
    GameObject p = Instantiate(_projectilePrefab, _mouthTransform.position, _mouthTransform.rotation);
    IgnorePlayerCollision(p);
    p.GetComponent<Rigidbody>().AddForce(_mouthTransform.forward * _projectileForce, ForceMode.Impulse);
  }

  void IgnorePlayerCollision(GameObject projectile)
  {
    Collider proj = projectile.GetComponent<Collider>();
    foreach (Collider c in GetComponentsInChildren<Collider>())
      Physics.IgnoreCollision(proj, c);
  }

  void FireSpray()
  {
    if (!StomachSystem.Instance.TryDrain(_sprayDrainAmount)) return;
    for (int i = 0; i < _sprayRayCount; i++)
    {
      Vector3 dir = Quaternion.Euler(
          Random.Range(-_sprayConeAngle, _sprayConeAngle),
          Random.Range(-_sprayConeAngle, _sprayConeAngle), 0
          ) * _mouthTransform.forward;

      GameObject p = Instantiate(_projectilePrefab, _mouthTransform.position, _mouthTransform.rotation);
      IgnorePlayerCollision(p);
      p.GetComponent<Rigidbody>().AddForce(dir * (_projectileForce / 4f), ForceMode.Impulse);
    }
  }
}
