using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] private Transform _body;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private IKFootSolver _otherFoot;
    [SerializeField] private float _footSpacing;
    [SerializeField] private float _stepDistance;
    [SerializeField] private float _stepLength;
    [SerializeField] private float _stepHeight;
    [SerializeField] private float _speed;
    [SerializeField] Vector3 _footOffset;

    Quaternion _newFootRotation;
    Vector3 _currentPosition;
    Vector3 _oldPosition;
    Vector3 _newPosition;
    Vector3 _oldNormal, _currentNormal, _newNormal;
    float _lerp;

    private void Start()
    {
        _footSpacing = transform.localPosition.x;
        _currentPosition = _newPosition = _oldPosition = transform.position;
        _currentNormal = _newNormal = _oldNormal = transform.up;
        _lerp = 1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _currentPosition;
        transform.up = _currentNormal;

        Ray ray = new Ray(_body.position + (_body.right * _footSpacing), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit rayHit, 10f, _layerMask.value))
        {
            if (Vector3.Distance(_newPosition, rayHit.point) > _stepDistance && !_otherFoot.IsMoving() && _lerp >= 1)
            {
                _lerp = 0;
                int direction = _body.InverseTransformPoint(rayHit.point).z > _body.InverseTransformPoint(_newPosition).z ? 1 : -1;
                _newPosition = rayHit.point + (_body.forward * _stepLength * direction) + _footOffset;
                _newNormal = rayHit.normal;
            }
        }

        if (_lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(_oldPosition, _newPosition, _lerp);
            tempPosition.y += Mathf.Sin(_lerp * Mathf.PI) * _stepHeight;

            _currentPosition = tempPosition;
            _currentNormal = Vector3.Lerp(_oldNormal, _newNormal, _lerp);
            _lerp += Time.deltaTime * _speed;
        }
        else
        {
            _oldPosition = _newPosition;
            _oldNormal = _newNormal;
        }
    }

    public bool IsMoving()
    {
        return _lerp < 1;
    }
}
