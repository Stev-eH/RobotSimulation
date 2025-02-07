using GK;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using B83.MeshTools;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

public class TextToMeshGenerator : MonoBehaviour
{
    public GameObject hullObject;

    private ConvexHullCalculator calc;
    private List<int> tris = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector3> verts = new List<Vector3>();

    private List<Vector3> pointList = new List<Vector3>();

    private StreamReader reader;
    private string inputName = "Pointcloud.txt";

    private BinaryWriter writer;
    private string outputName = "MeshSerializer.txt";

    public byte[] serializationData;

    private Mesh generatedMesh;

    private const string defaultPath = ".\\Assets\\Resources\\";

    private string uploadURL = "http://127.0.0.1:8000/upload/";


    // Start is called before the first frame update
    void Start()
    {
        calc = new ConvexHullCalculator();
        tris = new List<int>();
        normals = new List<Vector3>();
        verts = new List<Vector3>();

        inputName = defaultPath + inputName;
        outputName = defaultPath + outputName;
        reader = new StreamReader(inputName);

        InitPointList();
        reader.Close();

        InitMesh();

        UploadFile(serializationData, "Mesh");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Function to upload the file
    public void UploadFile(byte[] fileBytes, string fileName)
    {
        StartCoroutine(UploadFileCoroutine(fileBytes, fileName));
    }

    private IEnumerator UploadFileCoroutine(byte[] fileBytes, string fileName)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileBytes, fileName);

        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("File uploaded successfully");
            }
            else
            {
                Debug.LogError("File upload failed: " + www.error);
            }
        }
    }

    void InitPointList()
    {
        string line = "";
        char[] trimChars = { '(', ')', '\n' };
        char seperator = ',';

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim(trimChars);
            string[] values = line.Split(seperator);

            if (values.Length != 3)
                continue;

            else
            {
                //cast values to appropriate format
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);

                Vector3 toAdd = new Vector3(x, y, z);

                pointList.Add(toAdd);
            }
        }

        Debug.Log("PointList initialized");
    }

    void InitMesh()
    {
        calc.GenerateHull(pointList, false, ref verts, ref tris, ref normals);
        generatedMesh = GenerateMesh();
        //writer = new BinaryWriter(File.OpenWrite(outputPath));
        serializationData = MeshSerializer.SerializeMesh(generatedMesh);

        Debug.Log("Mesh serialized");
    }

    public Mesh GenerateMesh()
    {
        var hull = Instantiate(hullObject);

        hull.transform.SetParent(transform, false);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localRotation = Quaternion.identity;
        hull.transform.localScale = new Vector3(1f, 1f, 1f); // for some reason, on porting the mesh increases it's size by 10

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);

        hull.GetComponent<MeshFilter>().sharedMesh = mesh;
        hull.GetComponent<MeshCollider>().sharedMesh = mesh;

        Debug.Log("Generated mesh");
        return mesh;
    }
}
