using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    [SerializeField] float _fillAmount = 25f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StomachSystem.Instance.Add(_fillAmount);
            Destroy(gameObject);
        }
    }
}
