#define VR
using System;
using HR_Toolkit;
#if Leap
using Leap.Unity;
#endif
using UnityEngine;


public class MovementController : MonoBehaviour
{
    public enum Movement
    {
        Mouse, Leap,
        VR
    }

    public GameObject trackedHand;

    [HideInInspector]
    public Movement currentMovement;
    [HideInInspector]
    public float speed = 10.0f;
    [HideInInspector]
    public float mouseWheelSpeed = 10.0f;
    [HideInInspector]
    public float rotationSpeed = 100.0f;

    private GameObject virtualHand;
    private GameObject realHand;
    private GameObject obj;
    private GameObject body;
    private Rigidbody rb;

    public void Init(GameObject obj)
    {
        this.obj = obj;
        this.virtualHand = RedirectionManager.instance.virtualHand;
        this.realHand = RedirectionManager.instance.realHand;
        this.body = RedirectionManager.instance.body;

        if (body.GetComponent<Rigidbody>() != null) rb = body.GetComponent<Rigidbody>();

    }

    public void MoveHand()
    {
        this.currentMovement = RedirectionManager.instance.movement;
        switch (currentMovement)
        {
            case Movement.Mouse:
                if (!Input.GetMouseButton(0)) return;

                this.speed = RedirectionManager.instance.speed;
                this.mouseWheelSpeed = RedirectionManager.instance.mouseWheelSpeed;

                var translationZ = Input.GetAxis("Mouse Y") * speed;
                var translationX = Input.GetAxis("Mouse X") * speed;
                var translationY = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSpeed;

                translationZ *= Time.deltaTime;
                translationX *= Time.deltaTime;
                translationY *= Time.deltaTime;
        
                obj.transform.Translate(translationX, translationY, translationZ);
                break;
            //case Movement.AutoPilot:
                //virtualHand.transform.rotation = RedirectionManager.instance.realHand.transform.rotation;
                //break;
            case Movement.VR:
                // activate movement of vr hand
                //realHand.transform.parent = trackedHand.transform;
                //realHand.transform.position = Vector3.zero;
                realHand.transform.position = trackedHand.transform.position;
                realHand.transform.rotation = trackedHand.transform.rotation;
                break;
            case Movement.Leap:
                // works only if the Leap Motion Unity Package is installed!
                // if you want to use the Leap Motion Setup, please uncomment the first line where leap is defined. If you are aware
                // of a more elegant way to include this, feel free to send us a mail 
#if Leap
                if (trackedHand.GetComponent<CapsuleHand>().GetLeapHand().GetPalmPose() == null) break;
                obj.transform.position = trackedHand.GetComponent<CapsuleHand>().GetLeapHand().GetPalmPose().position;
#endif
                break;
        }
    }

    public void MoveBody()
    {
        var bodySpeed = .5f;
        var rotSpeed = .5f;
        Vector3 pos = body.transform.position;
        Transform rot = body.transform;
 
        // movement
        if (Input.GetKey ("w")) {
            pos.z += bodySpeed * Time.deltaTime;
        }
        if (Input.GetKey ("s")) {
            pos.z -= bodySpeed * Time.deltaTime;
        }
        if (Input.GetKey ("d")) {
            pos.x += bodySpeed * Time.deltaTime;
        }
        if (Input.GetKey ("a")) {
            pos.x -= bodySpeed * Time.deltaTime;
        }
        
        // rotation
        if (Input.GetKey("q"))
        {
            rot.RotateAround(pos, Vector3.up, -rotSpeed);
        }
        if (Input.GetKey("e"))
        {
            rot.RotateAround(pos, Vector3.up, rotSpeed);
        }
        
             
 
        body.transform.position = pos;
    }
    

    public void GenerateRandomPath()
    {
        // TODO 
        throw new NotImplementedException();
    }
    
}
