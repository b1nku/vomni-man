using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;

public class PhysicsController : MonoBehaviour
{
    private enum lookDirectionOptions { velocity, acceleration, moveInput }
    // reference variables
    PlayerInputActions _playerInput;
    PlayerInputActions PlayerInput 
    {
        get 
        { 
            if (_playerInput != null) { return _playerInput; }
            return _playerInput = new PlayerInputActions(); 
        }
    }
    Rigidbody _rb;
    CapsuleCollider _col;

    Quaternion _uprightTargetRotation = Quaternion.identity;
    Quaternion _lastTargetRotation;
    Vector3 _jumpInput;
    Vector2 _previousMovement;
    Vector3 _moveInput;
    Vector3 _gravitationalForce;
    Vector3 _previousVelocity = Vector3.zero;
    Vector3 m_TargetVelocity;
    Vector3 _groundVelocity;
    Vector2 _currentMovementInput;
    Vector3 _platformInitialRotation;

    ParticleSystem.EmissionModule _emission;
    
    bool _isMovementPressed;
    bool _isJumpPressed;
    bool didLastRayHit;

    [Header("Other fields")]
    [SerializeField] MeshRenderer _playerBodyMesh;
    [SerializeField] Material[] _playerMaterial;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] ParticleSystem _dustParticleSystem;
    [SerializeField] SquashAndStretchOscillator _squashAndStretchOscillator;
    [SerializeField] CinemachineCamera _vcam;
    [SerializeField] CartController _cart;
    [Space]

    bool _shouldMaintainHeight = true;

    [Header("Locomotion")]
    [Tooltip("Maximal speed")] [SerializeField] float _maxSpeed = 8f;
    [Tooltip("Acceleration with which max speed achieved")] [SerializeField] float _acceleration = 200f;
    [Tooltip("Curve on which acceleration should be evaluated")] [SerializeField] AnimationCurve _accelerationFactorFromDot;
    [Tooltip("Maximal force applied with acceleration")] [SerializeField] float _maxAccelerationForce = 150f;
    [SerializeField] float _leanFactor = 0.25f;
    [Tooltip("Curve on which maximal acceleration force should be evaluated")] [SerializeField] AnimationCurve _maxAccelerationForceFromDot;
    [SerializeField] Vector3 _forceScale = new Vector3(1,0,1);
    //[SerializeField] float _gravityScaleDrop = 10f;
    [Space]

    [Header("Jumping")]
    [Tooltip("Amount of force applied with jump")] [SerializeField] float _jumpForceFactor = 10f;
    [Tooltip("Gravity applied during rise")] [SerializeField] float _riseGravityFactor = 5f;
    [Tooltip("Scalar of gravity during fall, typically above 1, i.e. 5-10")] [SerializeField] float _fallGravityFactor = 10f;
    [Tooltip("Scalar for smaller jumps")] [SerializeField] float _lowJumpFactor = 2.5f;
    [Tooltip("Buffer time for jump. Note: Shouldn't exceed total jump time!")] [SerializeField] float _jumpBuffer = 0.15f;
    [Tooltip("Coyote time for jump.")] [SerializeField] float _coyoteTime = 0.25f;
    [Space]


    [Header("Spring variables")]
    [Tooltip("Desired height of floatation")] [SerializeField] float _rideHeight = 1.75f;
    [Tooltip("Length of ground raycast")] [SerializeField] float _rayToGroundLength = 3f;
    [Tooltip("Spring carry strength")] [SerializeField] float _rideSpringStrength = 50f;
    [Tooltip("Factor of spring damping")] [SerializeField] float _rideSpringDamper = 5f;
    [Space]
    [Header("Rotation Spring Variables:")]
    [SerializeField] private lookDirectionOptions _characterLookDirection = lookDirectionOptions.moveInput;
    [Tooltip("Strength of angular spring snap")] [SerializeField] float _uprightJointSpringStrength = 40f;
    [Tooltip("Factor of angular spring damping")] [SerializeField] float _uprightJointSpringDamper = 5f;

    [Header("Water")]
    [SerializeField] float _bouyancyForce;
    [SerializeField] LayerMask _layerWaterMask;
    [SerializeField] float _floatingRayToGroundLength;
    [SerializeField] float _floatingRideHeight;
    bool _isFloating;

    [Space]


    float _zero = 0f;
    float _speedFactor = 1f;
    float _maxAccelerationForceFactor = 1f;
    float _timeSinceJumpPressed = 0f;
    float _timeSinceUnground = 0f;
    float _timeSinceJump = 0f;
    bool _jumpReady = true;
    bool _isJumping = false;

    /*void Awake()
    {
        _playerSpawnSystem = GameObject.Find("SpawnSystem(Clone)").GetComponent<PlayerSpawnSystem>();
    }*/

    void Start()
    {
        enabled = true;

        _playerInput = new PlayerInputActions();
        _col = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _gravitationalForce = Physics.gravity * _rb.mass;

        _floatingRayToGroundLength = 2 * _rayToGroundLength;
        _floatingRideHeight = 3 * _rideHeight;

        // Set player input callbacks
        /*InputManager.Controls.Player.Move.started    += OnCharacterMove;
        InputManager.Controls.Player.Move.canceled   += OnCharacterMove;
        InputManager.Controls.Player.Move.performed  += OnCharacterMove;

        InputManager.Controls.Player.Jump.started    += OnCharacterJump;
        InputManager.Controls.Player.Jump.canceled   += OnCharacterJump;*/

        _playerInput.Player.Move.started    += ctx => SetMovement(ctx.ReadValue<Vector2>());
        _playerInput.Player.Move.canceled   += ctx => ResetMovement();
        _playerInput.Player.Move.performed  += ctx => SetMovement(ctx.ReadValue<Vector2>());

        _playerInput.Player.Jump.started    += OnCharacterJump;
        _playerInput.Player.Jump.canceled   += OnCharacterJump;
        _playerInput.Player.Enable();

        _vcam = GameObject.FindAnyObjectByType<CinemachineCamera>();
        _vcam.LookAt = this.gameObject.transform;
        _vcam.Follow = this.gameObject.transform;
        _cart = GameObject.FindAnyObjectByType<CartController>();
    }

    void OnEnable()
    {
        _playerInput?.Player.Enable();
    }

    void OnDisable()
    {
        _playerInput?.Player.Disable();
    }

    void OnDestroy()
    {
        _playerInput?.Dispose();
    }

    bool IsGrounded(bool rayHitGround, RaycastHit rayHit)
    {
        bool grounded;
        if (rayHitGround) {
            grounded = rayHit.distance <= _rideHeight * 1.3f;
        } else {
            grounded = false;
        }

        return grounded;
    }

    Vector3 GetLookDirection(lookDirectionOptions lookDirectionOption)
    {
        Vector3 lookDirection = Vector3.zero;
        if (lookDirectionOption == lookDirectionOptions.velocity || lookDirectionOption == lookDirectionOptions.acceleration)
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            if (lookDirectionOption == lookDirectionOptions.velocity) 
            {
                lookDirection = velocity;
            } 
            else if (lookDirectionOption == lookDirectionOptions.acceleration) 
            {
                Vector3 deltaVelocity = velocity - _previousVelocity;
                _previousVelocity = velocity;
                Vector3 acceleration = deltaVelocity / Time.fixedDeltaTime;
                lookDirection = acceleration;
            }
        } 
        else if (lookDirectionOption == lookDirectionOptions.moveInput) 
        {
            lookDirection = _moveInput;
        }
        return lookDirection;
    }

    private bool _previousGrounded = false;
    void FixedUpdate()
    {
        var splineContainer = _cart.Cart.Spline;
        float3 localPos = splineContainer.transform.InverseTransformPoint(transform.GetChild(0).position);
        SplineUtility.GetNearestPoint(splineContainer.Spline, localPos, out _, out float t);
        _cart.Cart.SplinePosition = Mathf.Lerp(_cart.Cart.SplinePosition, t * splineContainer.Spline.GetLength(), 5f * Time.fixedDeltaTime);
        _moveInput = new Vector3(_previousMovement.x, 0f, _previousMovement.y);
        _moveInput = AdjustInputToFaceCamera(_moveInput);

        _rayToGroundLength = _isFloating ? 6f : 3f;
        _rideHeight = _isFloating ? 3.5f : 1.75f;

        (bool rayHitGround, RaycastHit rayHit) = RaycastToGround();
        SetPlatform(rayHit);

        bool grounded = IsGrounded(rayHitGround, rayHit);
        if (grounded) 
        {
            if (!_previousGrounded) 
            {
                //Play landing Feedback
            }

            if (_moveInput.magnitude != 0) 
            {
                //Play walking Feedback/sound
            } else 
            {
                //Stop walking Feedback/sound
            }

            if (_isFloating)
            {
                _shouldMaintainHeight = false;
                float waterLevel = 6f;
                if (transform.localPosition.y < waterLevel) 
                {
                    Debug.Log("transform.position.y: " + transform.localPosition.y + ", waterLevel: " + waterLevel);
                    _rb.AddForce(transform.up * _bouyancyForce);
                }
            }

            _timeSinceUnground = 0f;

            if (_timeSinceJump > 0.2f)
            {
                _isJumping = false;
            }
        } else 
        {
            _timeSinceUnground += Time.fixedDeltaTime;
        }

        CharacterMove(_moveInput, rayHit);
        CharacterJump(_jumpInput, grounded, rayHit);

        if (rayHitGround && _shouldMaintainHeight) 
        {
            MaintainHeight(rayHit);
        }

        Vector3 lookDirection = GetLookDirection(_characterLookDirection);
        MaintainUpright(lookDirection, rayHit);

        _previousGrounded = grounded;
    }

    (bool, RaycastHit) RaycastToGround()
    {
        Vector3 rayDirection = Vector3.down;
        RaycastHit rayHit;
        Ray rayToGround = new Ray(transform.GetChild(0).position, rayDirection);
        bool rayHitGround = Physics.Raycast(rayToGround, out rayHit, _rayToGroundLength, _layerMask.value);
        return (rayHitGround, rayHit);
    }

    void MaintainHeight(RaycastHit rayHit)
    {
        Vector3 velocity = _rb.linearVelocity;
        Vector3 otherVelocity = Vector3.zero;
        Vector3 rayDirection = Vector3.down;

        Rigidbody hitBody = rayHit.rigidbody;
        if (hitBody != null) 
        {
            otherVelocity = hitBody.linearVelocity;
        }

        float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
        float otherDirectionVelocity = Vector3.Dot(rayDirection, otherVelocity);

        float relativeVelocity = rayDirectionVelocity - otherDirectionVelocity;
        float currentHeight = rayHit.distance - _rideHeight;
        float springForce = (currentHeight * _rideSpringStrength) - (relativeVelocity * _rideSpringDamper);

        Vector3 maintainHeightForce = -_gravitationalForce + springForce * Vector3.down;
        Vector3 oscillationForce = springForce * Vector3.down;

        _rb.AddForce(maintainHeightForce);
        if (_squashAndStretchOscillator != null) _squashAndStretchOscillator.ApplyForce(oscillationForce);

        if (hitBody != null) 
        {
            hitBody.AddForceAtPosition(-maintainHeightForce, rayHit.point);
        }
    }

    void CalculateTargetRotation(Vector3 yLookAt, RaycastHit rayHit = new RaycastHit())
    {
        if (didLastRayHit)
        {
            _lastTargetRotation = _uprightTargetRotation;
            try 
            {
                _platformInitialRotation = transform.parent.rotation.eulerAngles;
            } catch 
            {
                _platformInitialRotation = Vector3.zero;
            }
        }

        if (rayHit.rigidbody == null)
        {
            didLastRayHit = true;
        } else 
        {
            didLastRayHit = false;
        }

        if (yLookAt != Vector3.zero)
        {
            _uprightTargetRotation = Quaternion.LookRotation(yLookAt, Vector3.up);
            _lastTargetRotation = _uprightTargetRotation;
            try 
            {
                _platformInitialRotation = transform.parent.rotation.eulerAngles;
            } catch 
            {
                _platformInitialRotation = Vector3.zero;
            }
        } else 
        {
            try 
            {
                Vector3 platformRotation = transform.parent.rotation.eulerAngles;
                Vector3 deltaPlatformRotation = platformRotation - _platformInitialRotation;
                float yAngle = _lastTargetRotation.eulerAngles.y + deltaPlatformRotation.y;
                _uprightTargetRotation = Quaternion.Euler(new Vector3(0f, yAngle, 0f));
            } catch {}
        }
    }

    void MaintainUpright(Vector3 yLookAt, RaycastHit rayHit = new RaycastHit())
    {
        CalculateTargetRotation(yLookAt, rayHit);

        Quaternion currentRotation = transform.rotation;
        Quaternion desiredRotation = UtilsMath.ShortestRotation(_uprightTargetRotation, currentRotation);

        Vector3 rotationAxis;
        float rotationDegrees;

        desiredRotation.ToAngleAxis(out rotationDegrees, out rotationAxis);
        rotationAxis.Normalize();

        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;

        _rb.AddTorque((rotationAxis * (rotationRadians * _uprightJointSpringStrength)) - (_rb.angularVelocity * _uprightJointSpringDamper));
    }

    private Vector3 AdjustInputToFaceCamera(Vector3 moveInput)
    {
        float facing = Camera.main.transform.eulerAngles.y;
        return (Quaternion.Euler(0, facing, 0) * moveInput);
    }

    void SetPlatform(RaycastHit rayHit)
    {
        try {
            RigidPlatform rigidPlatform = rayHit.transform.GetComponent<RigidPlatform>();
            RigidParent rigidParent = rigidPlatform.RigidParent;
            transform.SetParent(rigidParent.transform);
        } catch {
            transform.SetParent(null);
        }
    }

    void CharacterMove(Vector3 moveInput, RaycastHit rayHit)
    {
        Vector3 m_UnitGoal = moveInput;
        Vector3 unitVelocity = m_TargetVelocity.normalized;

        float velocityDot = Vector3.Dot(m_UnitGoal, unitVelocity);
        float acceleration = _acceleration * _accelerationFactorFromDot.Evaluate(velocityDot);

        Vector3 desiredVelocity = m_UnitGoal * _maxSpeed * _speedFactor;
        Vector3 otherVelocity = Vector3.zero;

        Rigidbody hitBody = rayHit.rigidbody;
        m_TargetVelocity = Vector3.MoveTowards(m_TargetVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

        Vector3 neededAcceleration = (m_TargetVelocity - _rb.linearVelocity) / Time.fixedDeltaTime;
        float maxAcceleration = _maxAccelerationForce * _maxAccelerationForceFromDot.Evaluate(velocityDot) * _maxAccelerationForceFactor;
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);

        _rb.AddForceAtPosition(Vector3.Scale(neededAcceleration * _rb.mass, _forceScale), transform.position + new Vector3(0f, transform.localScale.y * _leanFactor, 0f));
    }

    void CharacterJump(Vector3 jumpInput, bool grounded, RaycastHit rayHit)
    {
        _timeSinceJumpPressed += Time.fixedDeltaTime;
        _timeSinceJump += Time.fixedDeltaTime;
        if (_rb.linearVelocity.y < 0 && _isFloating == false) 
        {
            _shouldMaintainHeight = true;
            _jumpReady = true;
            if (!grounded) 
            {
                _rb.AddForce(_gravitationalForce * (_fallGravityFactor - 1f));
            }
        } else if (_rb.linearVelocity.y > 0) 
        {
            if (!grounded) {
                if (_isJumping) 
                {
                    _rb.AddForce(_gravitationalForce * (_riseGravityFactor - 1f));
                }
                if (jumpInput == Vector3.zero) 
                {
                    _rb.AddForce(_gravitationalForce * (_lowJumpFactor -1f));
                }
            }
        }

        if (_timeSinceJumpPressed < _jumpBuffer) 
        {
            if (_timeSinceUnground < _coyoteTime) 
            {
                if (_jumpReady) 
                {
                    _jumpReady = false;
                    _shouldMaintainHeight = false;
                    _isJumping = true;
                    _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                    if (rayHit.distance != 0) 
                    {
                        _rb.position = new Vector3(_rb.position.x, _rb.position.y - (rayHit.distance - _rideHeight), _rb.position.z);
                    }
                    _rb.AddForce(Vector3.up * _jumpForceFactor, ForceMode.Impulse);
                    CmdJumpParticles(rayHit.point);//_jumpFeedback?.PlayFeedbacks(rayHit.point);
                    _timeSinceJumpPressed = _jumpBuffer;
                    _timeSinceJump = 0f;
                }
            }
        }
    }

    void SetMovement(Vector2 movement) => _previousMovement = movement;

    void ResetMovement() => _previousMovement = Vector2.zero;

    void CmdJumpParticles(Vector3 point)
    {
        if (_dustParticleSystem != null)
            _dustParticleSystem.Play();
    }

    void OnCharacterMove(InputAction.CallbackContext context)
    {
        _currentMovementInput   = context.ReadValue<Vector2>();
        _isMovementPressed  = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }

    void OnCharacterJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();

        if (_isJumpPressed) {
            _jumpInput = new Vector3(0, 1f, 0);
        } else {
            _jumpInput = Vector3.zero;
        }
        if (context.started)
        {
            _timeSinceJumpPressed = 0f;
        }
    }

     void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            _isFloating = true;
            Debug.Log("I'm floating" + _isFloating);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            _shouldMaintainHeight = true;
            _isFloating = false;
            Debug.Log("Time to dry off" + _isFloating);
        }
    }
}
