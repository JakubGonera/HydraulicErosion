using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float rpm = 2f;
    public float radius = 10f;
    public float height = 5f;
    public float speed = 0.01f;

    // Update is called once per frame
    void Update()
    {
        float x = radius * Mathf.Cos(speed * Time.time * rpm);
        float z = radius * Mathf.Sin(speed * Time.time * rpm);
        transform.position = new Vector3(x, height, z);

        transform.LookAt(new Vector3(0, 300, 0));
    }
}
