using UnityEngine;

public class VomitProjectile : MonoBehaviour
{
  [SerializeField, Tooltip("in UV space")] float _splatRadius = 0.15f; // in UV space
  [SerializeField] float _lifeTime = 5f;

  void Start() => Destroy(gameObject, _lifeTime);

  void OnCollisionEnter(Collision col)
  {
    ContactPoint contact = col.GetContact(0);
    VomitSurface surface = col.collider.GetComponent<VomitSurface>();
    if (surface == null) { Destroy(gameObject); return; }

    // Get UV at hit point via mesh raycast
    if (col.collider is MeshCollider)
    {
      GetComponent<Collider>().enabled = false;
      Ray ray = new Ray(contact.point + contact.normal * 0.1f, -contact.normal);
      if (Physics.Raycast(ray, out RaycastHit hit, 0.2f))
        surface.Paint(hit.textureCoord, _splatRadius);
    }

    Destroy(gameObject);
  }
}
