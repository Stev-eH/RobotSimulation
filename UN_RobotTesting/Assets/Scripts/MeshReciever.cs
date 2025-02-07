using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MeshTools;
using System.IO;

public class MeshReciever : MonoBehaviour
{
    private string inputPath;

    private Mesh deserializedMesh;

    public GameObject meshReciever;

    // Start is called before the first frame update
    void Start()
    {
        inputPath = "C:\\Users\\Work\\Documents\\GitHub\\XR4Robotics\\UN_RobotTesting\\Assets\\Resources\\MeshSerializer.txt";
        FileStream fs = File.Open(inputPath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        deserializedMesh = MeshSerializer.DeserializeMesh(br);

        meshReciever.GetComponent<MeshFilter>().sharedMesh = deserializedMesh;
        meshReciever.GetComponent<MeshCollider>().sharedMesh = deserializedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
