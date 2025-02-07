// Modified from https://github.com/qian256/ur5_unity
// ORIGINAL
// Author: Long Qian
// Email: lqian8@jhu.edu

using UnityEngine;
using System.Collections;

public class UR10ControllerSlave : MonoBehaviour
{

    public GameObject RobotBase;

    //public PincherController pincherController;
    public GripperController gripperController;

    public float[] bufferedValues = new float[7];
    public float[] jointValues = new float[7];
    private GameObject[] jointList = new GameObject[6];
    private float[] upperLimit = { 180f, 180f, 180f, 180f, 180f, 180f };
    private float[] lowerLimit = { -180f, -180f, -180f, -180f, -180f, -180f };
    // offset needed to be adjusted, robot zero position is {0, 90, 0, 90, 0, 0}, 90 in base joint is used to match the orientation
    [SerializeField]
    private float[] jointOffset = { 90f, 90f, 0f, 90f, 0f, 0f };
    //private float[] jointSign = { 1f, 1f, -1f, 1f, -1f, 1f }; // original
    [SerializeField, Range(-1f, 1f)]
    private float[] jointSign = { -1f, 1f, 1f, 1f, -1f, 1f };
    private bool useQuaternion = true;

    private bool lerpInProgress;

    // Use this for initialization
    void Start()
    {
        initializeJoints();
    }

    void Update()
    {
        //if (!lerpInProgress)
        //{
        //    for (int i = 0; i < jointList.Length; i++)
        //    {
        //        StartCoroutine(LerpFunction(i, bufferedValues[i], 1));
        //        lerpInProgress = true;
        //    }
        //}
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i < 6; i++)
        {

            if (useQuaternion)
            {
                float angleInDegrees = jointSign[i] * jointValues[i] + jointOffset[i];

                // Determine axis of rotation based on joint index
                Vector3 rotationAxis = (i == 0 || i == 4) ? Vector3.up : Vector3.right;

                // Convert to quaternion
                Quaternion targetRotation = Quaternion.AngleAxis(angleInDegrees, rotationAxis);

                // Apply the rotation to the joint
                jointList[i].transform.localRotation = targetRotation;
            }
            else // use euler angles
            {
                Vector3 currentRotation = jointList[i].transform.localEulerAngles;

                // Debug.Log(currentRotation);
                if ((i == 0) | (i == 4))
                    currentRotation.y = jointSign[i] * jointValues[i] + jointOffset[i];
                else
                    currentRotation.x = jointSign[i] * jointValues[i] + jointOffset[i];

                jointList[i].transform.localEulerAngles = currentRotation; // * Mathf.PI/180.0f;
            }
        }

        //pincherController.grip = jointValues[6];
        //gripperController.gripRatio = jointValues[6];
    }

    IEnumerator LerpFunction(int index, float endValue, float duration)
    {
        float time = 0;
        float startValue = jointValues[index];

        while (time < duration)
        {
            jointValues[index] = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        jointValues[index] = endValue;
        lerpInProgress = false;
    }

/*    void OnGUI()
    {
        int boundary = 22;

#if UNITY_EDITOR
        int labelHeight = 22;
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 20;
#else
        int labelHeight = 40;
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 40;
#endif
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        for (int i = 0; i < 6; i++)
        {
            switch (i)
            {
                // changed names to better represent the robot
                case 0:
                    GUI.Label(new Rect(boundary, boundary + (i * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Base: ");
                    break;
                case 1:
                    GUI.Label(new Rect(boundary, boundary + (i * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Shoulder: ");
                    break;
                case 2:
                    GUI.Label(new Rect(boundary, boundary + (i * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Elbow: ");
                    break;
                default:
                    GUI.Label(new Rect(boundary, boundary + (i * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Wrist " + (i - 2) + ": ");
                    break;
            }
            jointValues[i] = GUI.HorizontalSlider(new Rect(boundary + labelHeight * 4 , boundary + (i * 2 + 1) * labelHeight + labelHeight / 4, labelHeight * 5, labelHeight), jointValues[i], lowerLimit[i], upperLimit[i]);

            // Display degree values beneath slider
            GUI.Label(new Rect(boundary + labelHeight * 4, boundary + (i * 2 + 2) * labelHeight, labelHeight * 5, labelHeight), $"{jointValues[i]:F1}°"); // Display angle to 1 decimal place
        }

        // Gripper value
        GUI.Label(new Rect(boundary, boundary + (6 * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Gripper: ");
        jointValues[6] = GUI.HorizontalSlider(new Rect(boundary + labelHeight * 4, boundary + (6 * 2 + 1) * labelHeight + labelHeight / 4, labelHeight * 5, labelHeight), jointValues[6], 0, 1);
    }*/


    // Create the list of GameObjects that represent each joint of the robot
    void initializeJoints()
    {
        var RobotChildren = RobotBase.GetComponentsInChildren<Transform>();
        for (int i = 0; i < RobotChildren.Length; i++)
        {
            if (RobotChildren[i].name == "Joint_1")
            {
                jointList[0] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "Joint_2")
            {
                jointList[1] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "Joint_3")
            {
                jointList[2] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "Joint_4")
            {
                jointList[3] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "Joint_5")
            {
                jointList[4] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "Joint_6")
            {
                jointList[5] = RobotChildren[i].gameObject;
            }
        }
    }
}