using UnityEngine;
using UnityEngine.Events;

public class StomachSystem : MonoBehaviour
{
    public static StomachSystem Instance { get; private set; }

    [SerializeField] float _maxAmount = 100f;
    [SerializeField] float _startAmount = 0f;

    float _currentAmount;

    public float FillFraction => _currentAmount / _maxAmount;
    public UnityEvent OnChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _currentAmount = _startAmount;
    }

    public void Add(float amount)
    {
        _currentAmount = Mathf.Clamp(_currentAmount + amount, 0f, _maxAmount);
        OnChanged?.Invoke();
    }

    public bool TryDrain(float amount)
    {
        if (_currentAmount < amount) return false;
        _currentAmount = Mathf.Max(0f, _currentAmount - amount);
        OnChanged?.Invoke();
        return true;
    }
}
