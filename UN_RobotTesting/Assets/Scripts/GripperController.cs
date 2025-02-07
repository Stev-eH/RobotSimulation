using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripperController : MonoBehaviour
{
    public float gripRatio = 1.0f;
    private float dMaxGrip_ = 0;
    private float minTheta_ = -45.0f;
    private float dGripTol_ = 0.005f;

    private HingeJoint lHingeJoint_, rHingeJoint_;


    // Start is called before the first frame update
    void Start()
    {
        dMaxGrip_ = GripSpacing();

        rHingeJoint_ = transform.Find("RHS/RightOuterKnuckleJoint").GetComponent<HingeJoint>();
        lHingeJoint_ = transform.Find("LHS/RightOuterKnuckleJoint").GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        float thetaTarget = (1 - gripRatio) * minTheta_;
        float dCur = GripSpacing();

        JointSpring lSpring = lHingeJoint_.spring;
        lSpring.targetPosition = thetaTarget;
        lHingeJoint_.spring = lSpring;

        JointSpring rSpring = rHingeJoint_.spring;
        rSpring.targetPosition = thetaTarget;
        rHingeJoint_.spring = rSpring;

        //if (dTarget < dCur - dGripTol_)
        //{
        //    //lHingeJoint_.GetComponent<Rigidbody>().isKinematic = true;
        //    //rHingeJoint_.GetComponent<Rigidbody>().isKinematic = true;

        //    // Set motor properties
        //    var lMotor = lHingeJoint_.motor;
        //    lMotor.targetVelocity = -velMotor;
        //    lHingeJoint_.motor = lMotor;

        //    // Set motor properties
        //    var rMotor = lHingeJoint_.motor;
        //    rMotor.targetVelocity = -velMotor;
        //    rHingeJoint_.motor = rMotor;

        //    lHingeJoint_.useMotor = true;
        //    rHingeJoint_.useMotor = true;
        //}
        //else if (dTarget > dCur + dGripTol_)
        //{
        //    //lHingeJoint_.GetComponent<Rigidbody>().isKinematic = true;
        //    //rHingeJoint_.GetComponent<Rigidbody>().isKinematic = true;

        //    // Set motor properties
        //    var lMotor = lHingeJoint_.motor;
        //    lMotor.targetVelocity = velMotor;
        //    lHingeJoint_.motor = lMotor;

        //    // Set motor properties
        //    var rMotor = lHingeJoint_.motor;
        //    rMotor.targetVelocity = velMotor;
        //    rHingeJoint_.motor = rMotor;

        //    lHingeJoint_.useMotor = true;
        //    rHingeJoint_.useMotor = true;
        //}
        //else
        //{            
        //    lHingeJoint_.useMotor = false;
        //    rHingeJoint_.useMotor = false;

        //    //lHingeJoint_.GetComponent<Rigidbody>().isKinematic = false;
        //    //rHingeJoint_.GetComponent<Rigidbody>().isKinematic = false;
        //}
    }

    private float GripSpacing()
    {
        Vector3 rFingerTip = transform.Find("RHS/RightInnerFingerJoint/FingerTip").position;
        Vector3 lFingerTip = transform.Find("LHS/RightInnerFingerJoint/FingerTip").position;

        return Vector3.Distance(rFingerTip, lFingerTip);
    }
}
