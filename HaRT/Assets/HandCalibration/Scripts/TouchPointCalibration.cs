using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TouchPointCalibration : MonoBehaviour
{
    // This script should be attached to the tracker which moves the "ObjectToCalibrate"
    // You need another controller (with a touchpad) for the calibration

    // Object which needs to be calibrated
    // in our example: the hand model (ReferencePosition)
    [Tooltip("reference position/ object which needs to be calibrated")]
    [SerializeField]
    private GameObject ObjectToCalibrate;
    [Tooltip("'ghost' which follows calibrated object")]
    [SerializeField]
    private GameObject SimulatedHand;
    [Tooltip("Sphere Object at fingertip of model")]
    [SerializeField]
    private GameObject Sphere = null;
    [Tooltip("Name of touchpad as string")]
    [SerializeField]
    private string TouchPadName = "trackpad_touch";
    [Tooltip("Name of touch point as string")]
    [SerializeField]
    private string TouchPointName = "attach";
    [Tooltip("true: ObjectToCalibrate will first be visible when first touch was received")]
    [SerializeField]
    private bool RemoveObjectToCalibrate = false;
    [Tooltip("true: tracker will be invisible once calibration is fine")]
    [SerializeField]
    private bool RemoveTrackerAfterCalibr = false;
    [Tooltip("maximum allowed position offset as double in m, recommended range [0.003; 0.008]")]
    [SerializeField]
    private double PositionAccuracy = 0.004;
    [Tooltip("maximum allowed rotation angle offset as float, recommended range [2; 7]")]
    [SerializeField]
    private float RotationAccuracy = 5;


    private GameObject rightHand3DModel;

    // @already: true if the current touch was already calculated
    // @positionFine: true if positionOffset meets the Accuracy
    // @rotationOffset: true if rotationOffset meets the Accuracy
    bool already = false;
    bool positionFine = false;
    bool rotationFine = false;

    // @direction: goal direction for ObjectToCalibrate
    Vector3 direction;
    Vector3 positionOffset;
    Quaternion oldRotation;
    MeshRenderer sphereMesh;

    // Start is called before the first frame update
    void Start()
    {
        MakeObjectInvisible();
        rightHand3DModel = ObjectToCalibrate.transform.Find("RightHand3DModel").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

        // Find: GO only returned if active && only active if touched
        GameObject touchPad = GameObject.Find(TouchPadName);
        if (touchPad != null && !already && touchPad.transform.localPosition.magnitude >= 1e-9 )
        {
            Transform definitiveTouch = touchPad.transform.Find(TouchPointName);
            if (definitiveTouch != null)
            {
                // POSITION
                // in our example: the finger tip is the origin of our model,
                // therefore we can easily set its position to the automatically generated "attach" position

                Vector3 oldPosition = ObjectToCalibrate.transform.localPosition;
                Sphere.transform.position = definitiveTouch.position;
                ObjectToCalibrate.transform.position = definitiveTouch.position;
                SimulatedHand.transform.position = ObjectToCalibrate.transform.position;
                positionOffset = oldPosition - ObjectToCalibrate.transform.localPosition;


                // ROTATION
                // finger points from tracker to touch point
                // as "Upwards": forward from tracker
                oldRotation = ObjectToCalibrate.transform.localRotation;
                direction = definitiveTouch.position - this.transform.position;
                ObjectToCalibrate.transform.rotation = Quaternion.LookRotation(direction, this.transform.forward);
                SimulatedHand.transform.rotation = ObjectToCalibrate.transform.rotation;

                MakeObjectVisible();
                CheckCalibrationState();

                // touch processed
                already = true;
            }  
        }

        // touch ended
        if (touchPad == null)
        {
            already = false;
        }
    }


    void MakeObjectInvisible()
    {
        if (RemoveObjectToCalibrate)
        {
            ObjectToCalibrate.SetActive(false);
            SimulatedHand.SetActive(false);
        }
    }

    void MakeObjectVisible()
    {
        if (!ObjectToCalibrate.activeSelf)
        {
            ObjectToCalibrate.SetActive(true);
            SimulatedHand.SetActive(true);
        }
    }


    void CheckCalibrationState()
    {
        float positionOffMagnitude = positionOffset.magnitude;
        float rotationOffAngle = Quaternion.Angle(oldRotation, ObjectToCalibrate.transform.localRotation);
        positionFine = positionOffMagnitude <= PositionAccuracy;
        rotationFine = rotationOffAngle <= RotationAccuracy;

        if (positionFine && rotationFine)
        {
            MakeTrackerInVisible();
            Debug.Log("Calibration conditions are met." + Environment.NewLine +
                      "position offset: " + positionOffMagnitude + "     " +
                      "rotation offset: " + rotationOffAngle);  
        }
        else
        {
            MakeTrackerVisible();
            Debug.LogWarning("Calibration is not sufficient yet." + Environment.NewLine + 
                             "position offset: " + positionOffMagnitude + "     " + 
                             "rotation offset: " +  rotationOffAngle);
        }
    }


    void MakeTrackerInVisible()
    {
        if (RemoveTrackerAfterCalibr)
        {
            MeshRenderer trackerMesh = this.transform.Find("Model").Find("Model").Find("body").GetComponent<MeshRenderer>();
            trackerMesh.enabled = false;
        }
    }


    /// <summary>
    /// makes this tracker visible again
    /// </summary>
    public void MakeTrackerVisible()
    {
        try
        {
            MeshRenderer trackerMesh = this.transform.Find("Model").Find("Model").Find("body").GetComponent<MeshRenderer>();
            trackerMesh.enabled = true;
        }
        catch (NullReferenceException e)
        {

        }
        
    }

}



