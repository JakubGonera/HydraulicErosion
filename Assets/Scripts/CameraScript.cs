using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float rotationSpeed = 2f;
    public float radius = 10f;
    public float height = 5f;
    public float focusHeight = 300f;

    // Update is called once per frame
    void Update()
    {
        //Use parametric equation of a circle to move the camera in a circle
        float x = radius * Mathf.Cos(Time.time * rotationSpeed);
        float z = radius * Mathf.Sin(Time.time * rotationSpeed);
        transform.position = new Vector3(x, height, z);

        //Rotate the camera so it looks at the center
        transform.LookAt(new Vector3(0, focusHeight, 0));
    }
}
