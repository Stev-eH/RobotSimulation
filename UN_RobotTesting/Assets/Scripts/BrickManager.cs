using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    public GameObject brickPrefab;
    public Transform mainBrick;

    private Vector3 mainBrickPos_;
    private Quaternion mainBrickRot_;
    private bool mainBrickUpdated_ = false;

    private string layoutInitialization_;
    private bool initialized_ = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mainBrickUpdated_)
        {
            mainBrick.position = mainBrickPos_;
            mainBrick.rotation = mainBrickRot_;
            mainBrickUpdated_ = false;
        }

        if (initialized_)
        {
            InitializeLayout(layoutInitialization_);
            initialized_ = false;
        }
    }

    public void UpdateMainBrickPose(Vector3 position, Quaternion rotation)
    {
        mainBrickPos_ = position;
        mainBrickRot_ = rotation;
        mainBrickUpdated_ = true;
    }

    public void InitializeBricks(string layoutInitialization)
    {
        layoutInitialization_ = layoutInitialization;
        initialized_ = true;
    }

    private void InitializeLayout(string layoutInitialization)
    {
        // Destroy any previous initialzation
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float heightOffset = 0.0f;

        string[] cols = layoutInitialization.Split(',');

        if (cols.Length >= 7)
        { 
            int n = int.Parse(cols[0]);
            Debug.Log(n + " bricks to place");

            int colOffset = 1;
            for (int i = 0; i < n; i++)
            {                
                Vector3 brickPos = new Vector3( float.Parse(cols[colOffset]),
                                                float.Parse(cols[colOffset + 1]), 
                                                heightOffset);

                Quaternion brickRot = new Quaternion(float.Parse(cols[colOffset + 2]),
                                                     float.Parse(cols[colOffset + 3]),
                                                     float.Parse(cols[colOffset + 4]),
                                                     float.Parse(cols[colOffset + 5]));

                colOffset += 6;

                GameObject.Instantiate(brickPrefab, brickPos, brickRot, transform);
            }
        }

    }
}
