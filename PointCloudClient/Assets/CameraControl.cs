using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// based on https://answers.unity.com/questions/29741/mouse-look-script.html
public class CameraControl : MonoBehaviour
{
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    public bool InvertedY = true;
    public bool InvertedX = true;
    public bool OnlyOnPress = true;
    private Vector3 eulerRot;

    void Start ()
    {
        eulerRot = transform.localRotation.eulerAngles;
        eulerRot.z = 0;
    }

    void Update ()
    {
        if (OnlyOnPress && ! Input.GetMouseButton(0)) {
            // only rotate when pressing the mouse button
            return;
        }

        // Do not rotate if GUI elements are used (e.g. sliders)
        if (!(GUIUtility.hotControl == 0))
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        if (InvertedX)
        {
            mouseX *= -1;
        }

        float mouseY = Input.GetAxis("Mouse Y");
        if (InvertedY)
        {
            mouseY *= -1;
        }

        eulerRot.y += mouseX * mouseSensitivity * Time.deltaTime;
        
        eulerRot.x += mouseY * mouseSensitivity * Time.deltaTime;
        eulerRot.x = Mathf.Clamp(eulerRot.x, -clampAngle, clampAngle);

        transform.rotation = Quaternion.Euler(eulerRot);
    }
}
