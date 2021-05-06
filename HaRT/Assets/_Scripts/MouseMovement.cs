using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float speed = 10.0f;
    public float mouseWheelSpeed = 10.0f;
    public float rotationSpeed = 100.0f;

    void Update()
    {
        if (!Input.GetMouseButton(0)) return;
        var translationZ = Input.GetAxis("Mouse Y") * speed;
        var translationX = Input.GetAxis("Mouse X") * speed;
        var translationY = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSpeed;
        //float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        translationZ *= Time.deltaTime;
        translationX *= Time.deltaTime;
        translationY *= Time.deltaTime;
        //rotation *= Time.deltaTime;
        
        transform.Translate(translationX, translationY, translationZ);

        //transform.Rotate(0, rotation, 0);
    }
}
