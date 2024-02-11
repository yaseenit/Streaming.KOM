using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotate the parent object around the target object when dragging the mouse.
/// </summary>
public class OrbitCameraControl : MonoBehaviour
{
    public Transform Target = null;
    public float Speed = 1.0f;
    public bool InvertAxis = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            float direction = 1.0f;
            if(InvertAxis)
            {
                direction = -1.0f;
            }

            transform.RotateAround(
                Target.transform.position,
                transform.up,
                direction*Input.GetAxis("Mouse X") * Speed
            );

            transform.RotateAround(
                Target.transform.position,
                transform.right,
                direction*Input.GetAxis("Mouse Y") * Speed
            );
        }
    }
}
