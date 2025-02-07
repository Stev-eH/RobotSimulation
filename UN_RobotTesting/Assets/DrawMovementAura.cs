using GK;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngineInternal;
using UnityEditor;

public class DrawMovementAura : MonoBehaviour
{
    public CCDIK solverHolder;
    public UR10Controller controller;
    private IKSolverCCD solver;
    private GameObject[] jointList = new GameObject[6];
    private Vector3[] tempValues = new Vector3[6];
    private float[] minAngle = new float[6], maxAngle = new float[6];
    public bool coroutineStarted = false;

    private List<Vector3> pointList; //needs to be private to reduce slowdown, will try to display it's values in the inspector
    private List<float> distances;
    private List<float> shortenedDistances;
    private List<float> jointDifference;
    private List<float> shortenedJointDifference;

    public float treshold;

    public float[] weights = new float[6];

    public GameObject TCP;
    private bool stopCoroutine;
    private Thread dumpPointCloud;
    private ConvexHullCalculator calc;
    private List<int> tris = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector3> verts = new List<Vector3>();

    public GameObject hull;
    public int stepSize;


    private bool CCDIKDone;
    private bool startListgeneration;

    public Gradient heatMap;

    private string outputPath;

    // Start is called before the first frame update
    void Start()
    {
        calc = new ConvexHullCalculator();
        SignalHandler.SolverDoneExecuting += CatchSignal;


        jointList = controller.GetJointList();
        CCDIKDone = false;
        stopCoroutine = false;
        startListgeneration = false;
        pointList = new List<Vector3>();
        distances = new List<float>();
        shortenedDistances = new List<float>();
        jointDifference = new List<float>();
        shortenedJointDifference = new List<float>();

        tris = new List<int>();
        normals = new List<Vector3>();
        verts = new List<Vector3>();

        outputPath = ".\\Resources\\Pointcloud.txt";
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (CCDIKDone)
        {

            if (!coroutineStarted)
            {
                if (startListgeneration)
                {
                    dumpPointCloud = new Thread(dumpPointList);
                    startListgeneration = false;
                    solverHolder.enabled = false;
                    pointList.Clear();
                    coroutineStarted = true;
                    StartCoroutine(DrawAura());
                    dumpPointCloud.Start();
                }
            }
        }
        else
        {
            solverHolder.enabled = true;
        }
    }

    void ItterateJoint(int iterator, int whichJoint)
    {
        Vector3 rotationVector;
        if (whichJoint == 0 || whichJoint == 4)
        {
            rotationVector = new Vector3(0, 1, 0);
        }
        else
            rotationVector = new Vector3(1, 0, 0);

        rotationVector *= iterator;
        jointList[whichJoint].transform.localEulerAngles = rotationVector;
    }


    void CatchSignal()
    {
        for (int k = 0; k < 6; k++)
        {
            tempValues[k] = jointList[k].transform.localEulerAngles;
            GetLimits(jointList);
        }
        CCDIKDone = true;
    }

    void GetLimits(GameObject[] jointList)
    {
        for (int i = 0; i < jointList.Length; i++)
        {
            RotationLimitHinge jointLimits = jointList[i].GetComponent<RotationLimitHinge>();
            minAngle[i] = jointLimits.min;
            maxAngle[i] = jointLimits.max;
            Debug.Log("Joint " + i + " MixValue: " + minAngle[i] + "MaxValue: " + maxAngle[i]);
        }
    }

    // the list that we pass to this function dictates the coloring of the generated mesh
    // the values to generate the mesh are passed globally
    public void GenerateMesh(List<float> toGenerate)
    {
        var genObj = Instantiate(hull);

        genObj.transform.SetParent(transform, false);
        genObj.transform.localPosition = Vector3.zero;
        genObj.transform.localRotation = Quaternion.identity;
        genObj.transform.localScale = Vector3.one;

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);

        UnityEngine.Color[] colors = new UnityEngine.Color[toGenerate.Count];

        float minValue, maxValue;

        minValue = toGenerate[0];
        maxValue = toGenerate[0];

        //sets up the boundaries for the Gradient evaluation
        for (int i = 0; i < toGenerate.Count; i++)
        {
            if (toGenerate[i] < minValue) minValue = toGenerate[i];
            if (toGenerate[i] > maxValue) maxValue = toGenerate[i];
        }

        for (int i = 0; i < toGenerate.Count; i++)
        {
            colors[i] = heatMap.Evaluate(Mathf.InverseLerp(minValue, maxValue, toGenerate[i]));
        }

        mesh.colors = colors;
        genObj.GetComponent<MeshFilter>().sharedMesh = mesh;
        genObj.GetComponent<MeshCollider>().sharedMesh = mesh;

        Debug.Log("Generated mesh");
    }

    bool IsCollisionDetected()
    {
        // Iterate through each link of the robot arm to check for collisions
        for (int i = 0; i < jointList.Length - 1; i++)
        {
            // Get the start and end positions of the current link
            Vector3 linkStart = jointList[i].transform.position;
            Vector3 linkEnd = jointList[i + 1].transform.position;

            // Check if this link collides with any obstacles
            if (CheckLinkCollision(linkStart, linkEnd))
            {
                return true; // Collision detected
            }
        }

        return false; // No collision detected
    }

    bool CheckLinkCollision(Vector3 start, Vector3 end)
    {
        // Cast a ray to simulate the link and check for intersections with obstacles
        RaycastHit[] hits = Physics.RaycastAll(start, end - start, Vector3.Distance(start, end));

        // Iterate through all hit objects to check for collisions
        foreach (var hit in hits)
        {
            // If it hits an obstacle, consider it a collision
            if (hit.collider.CompareTag("Obstacle"))
            {
                return true; // Collision detected
            }
        }

        return false; // No collision
    }

    public IEnumerator DrawAura()
    {
        float tick = Time.realtimeSinceStartup;

        // these two variables hold the initial values before simulationg rotation
        Vector3 TCPInitPosition = TCP.transform.position;
        float[] jointAngles = GetJointAngles();

        // We want to flush our lists before filling them
        distances.Clear();
        jointDifference.Clear();

        for (int j1 = (int)minAngle[0]; j1 <= maxAngle[0]; j1 += stepSize)
        {
            for (int j2 = (int)minAngle[1]; j2 <= maxAngle[1]; j2 += stepSize)
            {
                for (int j3 = (int)minAngle[2]; j3 <= maxAngle[2]; j3 += stepSize)
                {
                    for (int j4 = (int)minAngle[3]; j4 <= maxAngle[3]; j4 += stepSize)
                    {
                        for (int j5 = (int)minAngle[4]; j5 <= maxAngle[4]; j5 += stepSize)
                        {
                            ItterateJoint(j5, 4);

                            // Check for collisions in the current configuration
                            if (IsCollisionDetected())
                            {
                                // Further logic can be implemented here to handle collision cases
                                //collisionList.Add(TCP.transform.position);


                                continue; // Skip this configuration if there is a collision
                            }

                            float distance = Mathf.Sqrt(Mathf.Pow(TCP.transform.position.x - TCPInitPosition.x, 2) + Mathf.Pow(TCP.transform.position.y - TCPInitPosition.y, 2) + Mathf.Pow(TCP.transform.position.z - TCPInitPosition.z, 2));
                            //float div = CalcJointDifference(jointAngles);
                            pointList.Add(TCP.transform.position);
                            distances.Add(distance);
                            //jointDifference.Add(div);
                        }
                        ItterateJoint(j4, 3);

                    }
                    ItterateJoint(j3, 2);
                }
                ItterateJoint(j2, 1);
            }
            ItterateJoint(j1, 0);
        }
        float tock = Time.realtimeSinceStartup;
        Debug.Log("Calculation time for pointcloud at Stepsize " + stepSize + ": " + (tock - tick) + "s");

        CCDIKDone = false;
        coroutineStarted = false;

        //calc is an Object of ConvexHullGenerator, it takes a pointcloud (in Vector3s) and passes the required mesh data to create a convex hull from the pointcloud
        calc.GenerateHull(pointList, true, ref verts, ref tris, ref normals);
        ShortLists(distances, shortenedDistances);
        // GenerateMesh relies on the mesh data created by calc.GenerateHull, the order is important here
        GenerateMesh(shortenedDistances);


        /*        ShortLists(jointDifference, shortenedJointDifference);
                GenerateMesh();*/
        yield break;
    }

    public void dumpPointList()
    {
        Debug.Log("Begin dumping...");
        StreamWriter outputWriter = new StreamWriter(outputPath, false);
        outputWriter.AutoFlush = true;

        foreach (Vector3 point in pointList)
        {
            outputWriter.WriteLine(point.ToString());
        }

        Debug.Log("Dump complete!");
        outputWriter.Close();
    }

    public float CalcJointDifference(float[] referencePoint)
    {
        float[] currJointAngles = GetJointAngles();
        float sum = 0;

        for (int i = 0; i < jointList.Length; i++)
        {
            sum += weights[i] * Mathf.Sqrt(Mathf.Pow(currJointAngles[i] - referencePoint[i], 2));
        }

        return sum;
    }



    public float[] GetJointAngles()
    {
        float[] output = new float[jointList.Length];

        for (int i = 0; i < jointList.Length; i++)
        {
            if (i == 0 || i == 4)
                output[i] = jointList[i].transform.rotation.y;
            else
                output[i] = jointList[i].transform.rotation.x;
        }

        return output;
    }

    // Since our convex hull generator shortenes the list available points, we also need to shorten the list that we pass for coloring the mesh
    public void ShortLists(List<float> original, List<float> shortened)
    {
        shortened.Clear();
        foreach (var shortenedPoint in verts)
        {
            int index = pointList.IndexOf(shortenedPoint);

            if (index >= 0)
            {
                shortened.Add(original[index]);
            }
        }
    }

// Adds a custom button to the inspector window to start the hull generation
    [CustomEditor(typeof(DrawMovementAura))]
    public class DrawMovementAuraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Generate Hull"))
            {
                DrawMovementAura dMA = (DrawMovementAura)target;
                dMA.startListgeneration = true;
            }
        }
    }
}
