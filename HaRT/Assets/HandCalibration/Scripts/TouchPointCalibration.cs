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
    [Tooltip("reference position/ object which needs to be calibrated. When using the HaRT, probably the child of the RealHand called RightHand")]
    [SerializeField]
    private GameObject ObjectToCalibrate;
    [Tooltip("'ghost' (virtual hand) which follows calibrated object (real hand). When using the HaRT, probably the child of the VirtualHand called SimulatedHand. Can also be null if none exists")]
    [SerializeField]
    private GameObject GhostOfCalibrationObject;
    [Tooltip("Sphere Object at fingertip of model. If there is no Sphere used this variable is not needed")]
    [SerializeField]
    private GameObject Sphere = null;
    [Tooltip("Name of touchpad as string. When using the ViveCameraRig the name should be fine.")]
    [SerializeField]
    private string TouchPadName = "trackpad_touch";
    [Tooltip("Name of touch point as string. When using the ViveCameraRig the name should be fine.")]
    [SerializeField]
    private string TouchPointName = "attach";
    [Tooltip("true: ObjectToCalibrate will first be visible when first touch was received. based on the realHand model")]
    [SerializeField]
    private bool RemoveObjectToCalibrate = false;
    [Tooltip("true: tracker and controller will be invisible once calibration is fine")]
    [SerializeField]
    private bool RemoveTrackerControllerAfterCalibr = false;
    [Tooltip("true: hand calibration automatically stops once calibration is fine. For new calibration the scene has to be restarted")]
    [SerializeField]
    private bool stopAutomatically = false;
    [Tooltip("maximum allowed position offset as double in m, recommended range [0.003; 0.008]")]
    [SerializeField]
    private double PositionAccuracy = 0.004;
    [Tooltip("maximum allowed rotation angle offset as float, recommended range [2; 7]")]
    [SerializeField]
    private float RotationAccuracy = 5;

    GameObject touchPad;

    // Visualization
    MeshRenderer[] trackerMeshes;
    MeshRenderer[] controllerMeshes;
    private Transform simHandModel;
    private Transform realHandModel;
    private Transform realHandSphere;
    string handModelName = "Hand3DModel";
    bool handAlreadyRemoved = false;

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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Calibration starts");
    }

    // Update is called once per frame
    void Update()
    {
        if (!handAlreadyRemoved)
        {
            MakeCalibrationObjectInvisible();
        }

        // Find: GO only returned if active && only active if touched
        touchPad = GameObject.Find(TouchPadName);

        if (touchPad != null && !already && touchPad.transform.localPosition.magnitude >= 1e-9)
        {
            Transform exactTouchPoint = touchPad.transform.Find(TouchPointName);
            if (exactTouchPoint != null)
            {
                // POSITION
                // in our example: the finger tip is the origin of our model,
                // therefore we can easily set its position to the automatically generated "attach" position

                Vector3 oldPosition = ObjectToCalibrate.transform.localPosition;
                ObjectToCalibrate.transform.position = exactTouchPoint.position;
                positionOffset = oldPosition - ObjectToCalibrate.transform.localPosition;
                if (Sphere != null)
                {
                    Sphere.transform.position = exactTouchPoint.position;
                }

                // ROTATION
                // finger points from tracker to touch point
                // as "Upwards": forward from tracker
                oldRotation = ObjectToCalibrate.transform.localRotation;
                direction = exactTouchPoint.position - this.transform.position;
                ObjectToCalibrate.transform.rotation = Quaternion.LookRotation(direction, this.transform.forward);

                adaptGhostObject();
                MakeCalibrationObjectVisible();
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

    void adaptGhostObject()
    {
        if (GhostOfCalibrationObject != null)
        {
            GhostOfCalibrationObject.transform.position = ObjectToCalibrate.transform.position;
            GhostOfCalibrationObject.transform.rotation = ObjectToCalibrate.transform.rotation;
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
            MakeTrackerInvisible();
            Debug.Log("Calibration conditions are met." + Environment.NewLine +
                      "position offset: " + positionOffMagnitude + "     " +
                      "rotation offset: " + rotationOffAngle);
            if (stopAutomatically)
            {
                Debug.Log("Hand is calibrated. Calibration is automatically stopped now");
                TouchPointCalibration script = this.gameObject.transform.GetComponent<TouchPointCalibration>();
                script.enabled = false;
            }
        }
        else
        {
            MakeTrackerVisible();
            Debug.LogWarning("Calibration is not sufficient yet." + Environment.NewLine +
                             "position offset: " + positionOffMagnitude + "     " +
                             "rotation offset: " + rotationOffAngle);
        }
    }

    #region additional Features

    void MakeCalibrationObjectInvisible()
    {
        if (RemoveObjectToCalibrate)
        {
            realHandModel = ObjectToCalibrate.transform.Find("Right" + handModelName);
            realHandSphere = ObjectToCalibrate.transform.parent.transform.Find("Sphere");
            
            if (realHandModel != null)
            {
                realHandModel.gameObject.SetActive(false);
            }

            if (realHandSphere != null)
            {
                realHandSphere.gameObject.SetActive(false);
            }

            if (GhostOfCalibrationObject != null)
            {
                simHandModel = GhostOfCalibrationObject.transform.Find("Simulated" + handModelName);
                if(simHandModel != null)
                {
                    simHandModel.gameObject.SetActive(false);
                }
            }
        }
        handAlreadyRemoved = true;
    }

    void MakeCalibrationObjectVisible()
    {
        if (!RemoveObjectToCalibrate)
        {
            realHandModel = ObjectToCalibrate.transform.Find("Right" + handModelName);
            realHandSphere = ObjectToCalibrate.transform.parent.transform.Find("Sphere");
        }
        if (!realHandModel.gameObject.activeSelf)
        {
            realHandModel.gameObject.SetActive(true);
            
            if (realHandSphere != null)
            {
                realHandSphere.gameObject.SetActive(true);
            }

            if (simHandModel != null)
            {
                simHandModel.gameObject.SetActive(true);
            }
        }
    }


    /// <summary>
    /// makes this tracker visible again
    /// </summary>
    public void MakeTrackerVisible()
    {
        try
        {
            // make tracker visible
            trackerMeshes = this.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer currMesh in trackerMeshes)
            {
                currMesh.enabled = true;
            }
            // make controller visible
            controllerMeshes = touchPad.transform.parent.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer currMesh in controllerMeshes)
            {
                currMesh.enabled = true;
            }
        }
        catch (NullReferenceException)
        {

        }
    }

    void MakeTrackerInvisible()
    {
        if (RemoveTrackerControllerAfterCalibr)
        {
            // make tracker invisible
            trackerMeshes = this.transform.Find("Model").GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer currMesh in trackerMeshes)
            {
                currMesh.enabled = false;
            }
            // make controller invisible
            controllerMeshes = touchPad.transform.parent.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer currMesh in controllerMeshes)
            {
                currMesh.enabled = false;
            }
        }
    }

    #endregion
}



