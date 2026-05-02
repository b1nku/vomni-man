using UnityEngine;

public class IKGizmoHelper : MonoBehaviour
{
    public Color _color = Color.blue;
    void OnDrawGizmos()
    {
        Gizmos.color = _color;
        Gizmos.DrawSphere(transform.position, .03f);
    }
}
