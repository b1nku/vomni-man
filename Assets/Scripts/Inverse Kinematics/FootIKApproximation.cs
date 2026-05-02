using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIKApproximation : MonoBehaviour
{
    //public PlayerStateMachine Ctx;

    void LeftFootIK()
    {
        /*Ctx.FootIK();
        Ctx.LeftIKConstraint.weight = 1;
        Debug.Log("Left foot weight: " + Ctx.LeftIKConstraint.weight);
        Ctx.RightIKConstraint.weight = 0;
        Debug.Log("Right foot weight: " + Ctx.RightIKConstraint.weight);
        Ctx.FootIK();*/
    }

    void RightFootIK()
    {
        /*Ctx.FootIK();
        Ctx.RightIKConstraint.weight = 1;
        Debug.Log("Right foot weight: " + Ctx.RightIKConstraint.weight);
        Ctx.LeftIKConstraint.weight = 0;
        Debug.Log("Left foot weight: " + Ctx.LeftIKConstraint.weight);
        Ctx.FootIK();*/
    }
}
