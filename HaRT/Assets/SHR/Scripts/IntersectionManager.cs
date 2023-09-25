using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;           // eye tracking
using ViveSR.anipal.Eye;
using HR_Toolkit;
using UnityEditor;

public class IntersectionManager : MonoBehaviour
{
    public static IntersectionManager instance;

    [SerializeField] LayerMask intersectionLayer;
    [SerializeField] GameObject previousEyeIntersection;
    Vector3 firstEyeIntersection = Vector3.zero;
    [SerializeField] GameObject secondEyeIntersection;
   

    // EYE DATA
    bool eye_callback_registered = false;
    static EyeData_v2 eyeData = new EyeData_v2();        // current eye data
    static bool processed = false;

    Vector3 localEyeOrigin;
    Vector3 localEyeDirection;

    Vector3 globalEyeOrigin;
    Vector3 globalEyeDirection;

    // Vector3 intersection = Vector3.zero;         // ONLY FOR TESTING


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        // UseVRHeadsetTracking
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // happens automatically as child of camera
        //setIntersectionPlaneAngle();

        // only if active Redirection
        if (RedirectionManager.instance.target == null) return;

        // new data available --> new intersection, store old one
        if (!processed)
        {
            previousEyeIntersection.transform.position = firstEyeIntersection;
        }

        readEyeData();
                
        // calculate new intersection
        // if (SceneHandler.instance.StartConditionMet)          UNDO??
        {
            firstEyeIntersection = getIntersection();
        }
    }

  

    public float GetSaccadeOffsetAngle(Vector3 targetOffset)
    {
        Vector3 saccadeDirection;

        if (globalEyeDirection == Vector3.zero && globalEyeOrigin == Vector3.zero)
        {
            Debug.LogWarning("Eye direction and origin was zero! This should not happen! A default SaccadeOffsetAngle of 0 is returned.");
            return 0;
        }

        secondEyeIntersection.transform.position = getIntersection();

        // ONLY FOR TESTING
        // Debug.Log("Calculating");
        // Vector3 saccadeDirection = secondEyeIntersection.transform.position - previousEyeIntersection.transform.position;
        // Debug.DrawRay(previousEyeIntersection.transform.position, saccadeDirection, Color.yellow, 2);
        // EditorApplication.isPaused = true;
        // Debug.Log(previousEyeIntersection.transform.position.x + ", " + previousEyeIntersection.transform.position.y + ", " + previousEyeIntersection.transform.position.z);
        // Debug.Log(firstEyeIntersection.x + ", " + firstEyeIntersection.y + ", " + firstEyeIntersection.z);
        // Debug.Log(secondEyeIntersection.transform.position.x + ", " + secondEyeIntersection.transform.position.y + ", " + secondEyeIntersection.transform.position.z);

        saccadeDirection = secondEyeIntersection.transform.position - previousEyeIntersection.transform.position;

        Vector3 saccadeDirection_camera = WorldToCameraCoordinates(saccadeDirection);
        Vector3 totalOffset_camera = WorldToCameraCoordinates(targetOffset);

        // set z = 0
        saccadeDirection_camera.z = 0;
        totalOffset_camera.z = 0;

        Debug.DrawRay(previousEyeIntersection.transform.position, CameraToWorldCoordinates(totalOffset_camera), Color.red, 2);
        Debug.DrawRay(previousEyeIntersection.transform.position, CameraToWorldCoordinates(saccadeDirection_camera), Color.yellow, 2);


        return Vector3.Angle(saccadeDirection_camera, totalOffset_camera);
    }


    #region intersection

    void setIntersectionPlaneAngle()
    {
        this.transform.rotation = Camera.main.transform.rotation;
        this.transform.Rotate(0, 90, -90, Space.Self);
    }

    Vector3 getIntersection()
    {
        // Only for testing mode without VR
        if (globalEyeDirection == Vector3.zero && globalEyeOrigin == Vector3.zero)
        {
            Debug.LogWarning("Eye direction and origin was zero! This should not happen!");
            return Vector3.zero;
        }
        RaycastHit intersection;

        if (Physics.Raycast(globalEyeOrigin, globalEyeDirection, out intersection, Mathf.Infinity, intersectionLayer))
        {
            // ONLY FOR TESTING
            // Debug.DrawRay(globalEyeOrigin, globalEyeDirection.normalized * intersection.distance, Color.yellow);
            // Debug.Log("distance between intersection and plane = " + Vector3.Distance(intersection.collider.ClosestPoint(intersection.point), intersection.point));
            // closestPoint.transform.position = planeHitPoint.collider.ClosestPoint(planeHitPoint.point);

            return intersection.point;
        }
        Debug.LogWarning("There was no intersection. This should not happen.");
        return Vector3.zero;
    }

    #endregion


    #region eye tracking
    void readEyeData()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        if (!processed)
        {
            if (eye_callback_registered)
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out localEyeOrigin, out localEyeDirection, eyeData))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out localEyeOrigin, out localEyeDirection, eyeData))
                {
                    Debug.Log("LEFT instead of combined");
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out localEyeOrigin, out localEyeDirection, eyeData))
                {
                    Debug.Log("RIGHT instead of combined");
                }
            }
            else
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out localEyeOrigin, out localEyeDirection))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out localEyeOrigin, out localEyeDirection))
                {
                    Debug.Log("LEFT instead of combined");
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out localEyeOrigin, out localEyeDirection))
                {
                    Debug.Log("RIGHT instead of combined");
                }
            }

            // calculate globals
            globalEyeOrigin = Camera.main.transform.TransformPoint(localEyeOrigin);
            globalEyeDirection = Camera.main.transform.TransformDirection(localEyeDirection);

            processed = true;
        }
    }

    static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
        processed = false;
    }



    #endregion

    #region help methods

    Vector3 WorldToCameraCoordinates(Vector3 worldCoordinates)
    {
        return Camera.main.transform.InverseTransformVector(worldCoordinates);
    }

    Vector3 CameraToWorldCoordinates(Vector3 relativeToCameraCoordinates)
    {
        return Camera.main.transform.TransformVector(relativeToCameraCoordinates);
    }

    #endregion
}
