using UnityEngine;

[ExecuteInEditMode]
public class InverseKinematics : MonoBehaviour 
{
	public Transform UpperArm;
	public Transform Forearm;
	public Transform Hand;
	public Transform Elbow;
	public Transform Target;
	[Space(20)]
	public Vector3 UpperArmOffsetRotation;
	public Vector3 ForearmOffsetRotation;
	public Vector3 HandOffsetRotation;
	[Space(20)]
	public bool HandMatchesTargetRotation = true;
	[Space(20)]
	public bool debug;

	float _angle;
	float _upperArmLength;
	float _foreArmLength;
	float _armLength;
	float _targetDistance;
	float _adjacent;
	
	// Update is called once per frame
	void LateUpdate() 
    {
		if(UpperArm != null && Forearm != null && Hand != null && Elbow != null && Target != null)
        {
			UpperArm.LookAt(Target, Elbow.position - UpperArm.position);
			UpperArm.Rotate(UpperArmOffsetRotation);

			Vector3 cross = Vector3.Cross(Elbow.position - UpperArm.position, Forearm.position - UpperArm.position);

			_upperArmLength = Vector3.Distance(UpperArm.position, Forearm.position);
			_foreArmLength =  Vector3.Distance(Forearm.position, Hand.position);
			_armLength = _upperArmLength + _foreArmLength;
			_targetDistance = Vector3.Distance(UpperArm.position, Target.position);
			_targetDistance = Mathf.Min(_targetDistance, _armLength - _armLength * 0.001f);

			_adjacent =((_upperArmLength * _upperArmLength) -(_foreArmLength * _foreArmLength) +(_targetDistance * _targetDistance)) /(2*_targetDistance);

			_angle = Mathf.Acos(_adjacent / _upperArmLength) * Mathf.Rad2Deg;

			UpperArm.RotateAround(UpperArm.position, cross, -_angle);

			Forearm.LookAt(Target, cross);
			Forearm.Rotate(ForearmOffsetRotation);

			if(HandMatchesTargetRotation)
            {
				Hand.rotation = Target.rotation;
				Hand.Rotate(HandOffsetRotation);
			}
			
			if(debug)
            {
				if(Forearm != null && Elbow != null) 
                {
					Debug.DrawLine(Forearm.position, Elbow.position, Color.blue);
				}

				if(UpperArm != null && Target != null) 
                {
					Debug.DrawLine(UpperArm.position, Target.position, Color.red);
				}
			}
		}
	}

	void OnDrawGizmos()
    {
		if(debug)
        {
			if(UpperArm != null && Elbow != null && Hand != null && Target != null && Elbow != null)
            {
				Gizmos.color = Color.gray;
				Gizmos.DrawLine(UpperArm.position, Forearm.position);
				Gizmos.DrawLine(Forearm.position, Hand.position);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(UpperArm.position, Target.position);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(Forearm.position, Elbow.position);
			}
		}
	}
}
