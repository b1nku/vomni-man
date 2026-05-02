using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] Transform target;

    void Awake()
    {
        target = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - target.position);
        }
    }
}
