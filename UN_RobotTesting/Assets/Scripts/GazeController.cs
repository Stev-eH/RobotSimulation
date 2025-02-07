using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject gazeCursor;

    private List<Vector3> pointBuffer_;
    private Vector3 gazePos_;
    private int bufferSize_ = 10;

    // Start is called before the first frame update
    void Start()
    {
        pointBuffer_ = new List<Vector3>();
        gazeCursor.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Ray leRay = new Ray(leftEye.transform.position, leftEye.transform.forward);
        Ray reRay = new Ray(rightEye.transform.position, rightEye.transform.forward);

        RaycastHit leHitInfo;
        RaycastHit reHitInfo;

        bool leIsHit = Physics.Raycast(leRay, out leHitInfo);
        bool reIsHit = Physics.Raycast(reRay, out reHitInfo);

        if (leIsHit && reIsHit)
        {
            gazeCursor.SetActive(true);

            Vector3 leHitPoint = leHitInfo.point;
            Vector3 reHitPoint = reHitInfo.point;

            Vector3 midPoint = Vector3.Lerp(leHitPoint, reHitPoint, 0.5f);

            pointBuffer_.Add(midPoint);

            if (pointBuffer_.Count > bufferSize_)
            {
                pointBuffer_.RemoveAt(0);
            }

            Vector3 meanPoint = Vector3.zero;

            foreach (Vector3 point in pointBuffer_)
            {
                meanPoint += point;
            }

            meanPoint = meanPoint / bufferSize_;

            gazePos_ = meanPoint;

            gazeCursor.transform.position = meanPoint;
            gazeCursor.transform.up = reHitInfo.normal;
        }
        else
        {
            pointBuffer_.Clear();
            gazeCursor.SetActive(false);
        }
    }

    public Vector3 GazePosition()
    {
        return gazePos_;
    }
}
