using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShockAbsorber : MonoBehaviour
{
  Rigidbody _rb;
  Quaternion _uprightTargetRotation = Quaternion.identity;
  Quaternion _lastTargetRotation;
  Vector3 _platformInitialRotation;

  [SerializeField] LayerMask _layerMask;
  [SerializeField] float _rideHeight = 1.75f;
  [SerializeField] float _rideSpringStrength = 150f;
  [SerializeField] float _rideSpringDamper = 5f;
  [Tooltip("Strength of angular spring snap")] [SerializeField] float _uprightJointSpringStrength = 40f;
  [Tooltip("Factor of angular spring damping")] [SerializeField] float _uprightJointSpringDamper = 5f;
  [SerializeField] float _rayToGroundLength = 3f;
  [SerializeField] bool _shouldMaintainHeight = true;
  [SerializeField] bool _shouldMaintainUpright = false;

  bool didLastRayHit;

  Vector3 _gravitationalForce = Vector3.zero;

  void Awake()
  {
    _rb = GetComponent<Rigidbody>();
    _gravitationalForce = Physics.gravity * _rb.mass;
  }

  (bool, RaycastHit) RaycastToGround()
  {
    Vector3 rayDirection = Vector3.down;
    RaycastHit rayHit;
    Ray rayToGround = new Ray(transform.position, rayDirection);
    bool rayHitGround = Physics.Raycast(rayToGround, out rayHit, _rayToGroundLength, _layerMask.value);
    return (rayHitGround, rayHit);
  }

  void FixedUpdate()
  {
    Vector3 lookDirection = _rb.linearVelocity;
    lookDirection.y = 0;
    (bool rayHitGround, RaycastHit rayHit) = RaycastToGround();

    if (rayHitGround && _shouldMaintainHeight) 
    {
      MaintainHeight(rayHit);
    }

    if (_shouldMaintainUpright)
    {
      MaintainUpright(lookDirection, rayHit);
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
    /*_squashAndStretchOscillator.ApplyForce(oscillationForce);*/

    if (hitBody != null) 
    {
      hitBody.AddForceAtPosition(-maintainHeightForce, rayHit.point);
    }
  }
}
