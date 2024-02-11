using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float DegreesPerSecond = 1.0f;
    public Vector3 RotationAxis = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(RotationAxis * Time.deltaTime * DegreesPerSecond);
    }
}
