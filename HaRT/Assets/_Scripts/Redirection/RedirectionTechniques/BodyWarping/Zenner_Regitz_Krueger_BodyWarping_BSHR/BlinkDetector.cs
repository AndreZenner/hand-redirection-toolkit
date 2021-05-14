using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using ViveSR.anipal.Eye;


public class BlinkDetector : MonoBehaviour
{
    public Camera camera;
    public GameObject virtualHand;

    [Header("Additional Settings")]
    [Tooltip("Eye Tracker Measurement Options: \n - Pupil Diameter in mm (-1 = 'eyes closed') \n - Eye Openenss (default) (0='eyes closed', 1='eyes open')")]
    public bool PupilDiameter = false;

    [Tooltip("A blink is counted as soon as the value falls below the specified threshold.")]
    [Range(-1, 3.0f)]
    public float threshold = 0.5f;

    [HideInInspector]
    public bool blinked = false;
    [HideInInspector]
    public bool valid = false;
    [HideInInspector]
    public bool running = false;
    

    private float eye_openness_L, eye_openness_R;
    private static EyeData eye_data;

    private Zenner_Regitz_Krueger_BodyWarping_BSHR redirectionClass;


    void Start()
    {
        redirectionClass = GetComponent<Zenner_Regitz_Krueger_BodyWarping_BSHR>();
    }

    void Update()
    {
        if (running){

            if (PupilDiameter)
            {
                SRanipal_Eye_API.GetEyeData(ref eye_data);
                eye_openness_L = eye_data.verbose_data.left.pupil_diameter_mm;
                eye_openness_R = eye_data.verbose_data.right.pupil_diameter_mm;
            }
            else
            {
                SRanipal_Eye.GetEyeOpenness(EyeIndex.LEFT, out eye_openness_L);
                SRanipal_Eye.GetEyeOpenness(EyeIndex.RIGHT, out eye_openness_R);
            }


            if (eye_openness_L <= threshold && eye_openness_R <= threshold)
            {
                if (valid) 
                {
                    blinked = true;
                }
            }

            else
            {
                valid = IsBlinkValid(virtualHand, 10);
            }
        }
    }

    private bool IsBlinkValid(GameObject virtualHand, float angle)
     {
         Transform[] list = virtualHand.GetComponentsInChildren<Transform>();

         Vector3 middle, gazeray;
         SRanipal_Eye_API.GetEyeData(ref eye_data);
         SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out middle, out gazeray, eye_data);

         middle = camera.transform.TransformPoint(middle);
         gazeray = camera.transform.TransformDirection(gazeray);
 
         foreach (Transform o in list)
         { 
             bool objectWithinFocus = Vector3.Angle(gazeray, o.position - middle) <= angle || (Vector3.Angle(gazeray, (o.position + redirectionClass.GetVectorVP_()) - middle) <= angle);
             if (objectWithinFocus) return false;
         }
         return true;
     }
}


