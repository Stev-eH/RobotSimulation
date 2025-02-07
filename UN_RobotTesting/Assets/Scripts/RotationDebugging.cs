using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationDebugging : MonoBehaviour
{
    public bool rotate;
    // Start is called before the first frame update
    void Start()
    {
        rotate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(rotate)
        {
            this.transform.localEulerAngles += Vector3.left * 1f;
        }
    }
}
