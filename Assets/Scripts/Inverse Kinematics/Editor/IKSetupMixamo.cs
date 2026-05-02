using UnityEditor;
using UnityEngine;

public class IKSetupMixamo : MonoBehaviour
{
    InverseKinematics IKLimb1, IKLimb2, IKLimb3, IKLimb4;
    public GameObject HelpersParent,
        RightArmTarget, LeftArmTarget, RightLegTarget, LeftLegTarget,
        RightArmKnee, LeftArmKnee, RightLegKnee, LeftLegKnee;
    public Transform RFoot1, LFoot1, RArm1, LArm1, Head1;
    public Transform RFoot2, LFoot2, RArm2, LArm2, Head2;
    public Transform RFoot3, LFoot3, RArm3, LArm3, Head3;
    Transform _hipTransform;

    public void SetupIK()
    {
        GetIKScripts();
        SetupHelpers();
        GetSkeletonParts();
        MoveHelpersToPosition();
        IKLimbSetup();
    }

    private void IKLimbSetup()
    {
        IKLimb1.Target = RightArmTarget.transform;
        IKLimb2.Target = LeftArmTarget.transform;
        IKLimb3.Target = RightLegTarget.transform;
        IKLimb4.Target = LeftLegTarget.transform;

        IKLimb1.Elbow = RightArmKnee.transform;
        IKLimb2.Elbow = LeftArmKnee.transform;
        IKLimb3.Elbow = RightLegKnee.transform;
        IKLimb4.Elbow = LeftLegKnee.transform;
        ////////////////////////
        IKLimb1.UpperArm = RArm3;
        IKLimb1.Forearm = RArm2;
        IKLimb1.Hand = RArm1;

        IKLimb1.UpperArmOffsetRotation = new Vector3(0, 90, 90);
        IKLimb1.ForearmOffsetRotation = new Vector3(-90, 180, 0);
        IKLimb1.HandOffsetRotation = new Vector3(90, 0, 0);

        IKLimb2.UpperArm = LArm3;
        IKLimb2.Forearm = LArm2;
        IKLimb2.Hand = LArm1;

        IKLimb2.UpperArmOffsetRotation = new Vector3(180, 90, 90);
        IKLimb2.ForearmOffsetRotation = new Vector3(90, 0, 0);
        IKLimb2.HandOffsetRotation = new Vector3(90, 180, 0);
        ////////////////////////
        IKLimb3.UpperArm = RFoot3;
        IKLimb3.Forearm = RFoot2;
        IKLimb3.Hand = RFoot1;

        IKLimb3.UpperArmOffsetRotation = new Vector3(270,90,90);
        IKLimb3.ForearmOffsetRotation = new Vector3(90,0,0);
        IKLimb3.HandOffsetRotation = new Vector3(-130, 90, 0);

        IKLimb4.UpperArm = LFoot3;
        IKLimb4.Forearm = LFoot2;
        IKLimb4.Hand = LFoot1;

        IKLimb4.UpperArmOffsetRotation = new Vector3(270,90,90);
        IKLimb4.ForearmOffsetRotation = new Vector3(90,0,0);
        IKLimb4.HandOffsetRotation = new Vector3(230, 90, 0);

    }

    private void MoveHelpersToPosition()
    {
        RightArmTarget.transform.position = RArm1.position + transform.right / 5;
        LeftArmTarget.transform.position = LArm1.position - transform.right / 5;
        RightLegTarget.transform.position = RFoot1.position - transform.up / 10;
        LeftLegTarget.transform.position = LFoot1.position - transform.up / 10;

        RightArmKnee.transform.position = RArm2.position - transform.forward / 3;
        LeftArmKnee.transform.position = LArm2.position - transform.forward / 3;
        RightLegKnee.transform.position = RFoot2.position + transform.forward / 3;
        LeftLegKnee.transform.position = LFoot2.position + transform.forward / 3;

        RightArmTarget.transform.localEulerAngles = Vector3.up * (90 + transform.eulerAngles.y);
        LeftArmTarget.transform.localEulerAngles = Vector3.up * (90 + transform.eulerAngles.y);
        RightLegTarget.transform.localEulerAngles = Vector3.up * (90+ transform.eulerAngles.y);
        LeftLegTarget.transform.localEulerAngles = Vector3.up * (90+ transform.eulerAngles.y);
    }

    private void GetSkeletonParts()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name== "mixamorig:Hips")
            {
                _hipTransform = transform.GetChild(i);
            }
        }
        if (_hipTransform!=null)
        {
            RFoot1 = _hipTransform.GetChild(1).GetChild(0).GetChild(0);
            RFoot2 = _hipTransform.GetChild(1).GetChild(0);
            RFoot3 = _hipTransform.GetChild(1);

            LFoot1 = _hipTransform.GetChild(0).GetChild(0).GetChild(0);
            LFoot2 = _hipTransform.GetChild(0).GetChild(0);
            LFoot3 = _hipTransform.GetChild(0);

            RArm1 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0);
            RArm2 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0);
            RArm3 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(2).GetChild(0);

            LArm1 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            LArm2 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            LArm3 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0);

            Head1 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(0);
            Head2 = _hipTransform.GetChild(2).GetChild(0).GetChild(0).GetChild(1);
            Head3 = _hipTransform.GetChild(2).GetChild(0).GetChild(0);

        }
    }

    private void GetIKScripts()
    {
        IKLimb1 = GetComponents<InverseKinematics>()[0];
        IKLimb2 = GetComponents<InverseKinematics>()[1];
        IKLimb3 = GetComponents<InverseKinematics>()[2];
        IKLimb4 = GetComponents<InverseKinematics>()[3];
    }

    private void SetupHelpers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "HelpersParent")
            {
                HelpersParent = transform.GetChild(i).gameObject;
                DestroyImmediate(HelpersParent);
            }
        }

        HelpersParent = new GameObject("HelpersParent");
        HelpersParent.transform.parent = transform;
        HelpersParent.transform.localPosition = Vector3.zero;

        RightArmTarget = new GameObject("RightArmTarget"); ;
        RightArmTarget.transform.parent = HelpersParent.transform;
        RightArmTarget.AddComponent<IKGizmoHelper>();

        LeftArmTarget = new GameObject("LeftArmTarget");
        LeftArmTarget.transform.parent = HelpersParent.transform;
        LeftArmTarget.AddComponent<IKGizmoHelper>();

        RightLegTarget = new GameObject("RightLegTarget");
        RightLegTarget.transform.parent = HelpersParent.transform;
        RightLegTarget.AddComponent<IKGizmoHelper>();

        LeftLegTarget = new GameObject("LeftLegTarget");
        LeftLegTarget.transform.parent = HelpersParent.transform;
        LeftLegTarget.AddComponent<IKGizmoHelper>();

        RightArmKnee = new GameObject("RightArmKnee");
        RightArmKnee.transform.parent = HelpersParent.transform;
        RightArmKnee.AddComponent<IKGizmoHelper>();
        RightArmKnee.GetComponent<IKGizmoHelper>()._color = Color.green;


        LeftArmKnee = new GameObject("LeftArmKnee");
        LeftArmKnee.transform.parent = HelpersParent.transform;
        LeftArmKnee.AddComponent<IKGizmoHelper>();
        LeftArmKnee.GetComponent<IKGizmoHelper>()._color = Color.green;

        RightLegKnee = new GameObject("RightLegKnee");
        RightLegKnee.transform.parent = HelpersParent.transform;
        RightLegKnee.AddComponent<IKGizmoHelper>();
        RightLegKnee.GetComponent<IKGizmoHelper>()._color = Color.green;

        LeftLegKnee = new GameObject("LeftLegKnee");
        LeftLegKnee.transform.parent = HelpersParent.transform;
        LeftLegKnee.AddComponent<IKGizmoHelper>();
        LeftLegKnee.GetComponent<IKGizmoHelper>()._color = Color.green;

    }
}

[CustomEditor(typeof(IKSetupMixamo))]
class DecalMeshHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Setup"))
        {
            IKSetupMixamo targetScript = (IKSetupMixamo)target;
            targetScript.SetupIK();
        }
    }
}