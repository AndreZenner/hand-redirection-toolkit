using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace HR_Toolkit
{
    /// <summary>
    /// ToDo
    /// 
    /// does algorithm also work for retargeting or does it create mistakes
    /// 
    /// add description
    /// 
    /// </summary>
    /// 

    public class SaccadicHandRedirection : Cheng_BodyWarping
    {
        #region Inspector Variables

        [Header("Thresholds above are used for continuous HR by Cheng.")]
        [Space]


        [Header("Saccadic Threshold Function")]
        public bool UseSaccades = false;
        [SerializeField] SHRThresholdPredictorOrder2Unconstrained thresholdFunction;



        [Header("Blink Thresholds")]

        [Tooltip("true: instantaneous offsets will also be applied during blinks.")]
        public bool UseBlinks = false;

        [Tooltip("threshold for offset LENGTH (NOT for each axis!)")]
        [Range(0f, 1f)]
        public float blinkThreshold = 0.05f;

        [Header("Required fields")]
        [SerializeField]
        [Tooltip("SaccadeDetection Script (automatically includes blink detection too. If not attached press left/right shift for saccade/blink simulation")]
        SaccadeDetection saccadeDetection;

        UnityEvent saccadeOccuredEvent = new UnityEvent();
        UnityEvent blinkOccuredEvent = new UnityEvent();

        Vector3 saccadeDirection = Vector3.zero;
        float saccadeOffsetAngle = 0f;
        float threshold;

        Vector3 instantOffset;

        bool simulationActive;

       // [SerializeField] GameObject firstIntersection;



        #endregion

        #region Internal Variables
        /// <summary>
        /// applied warping amount
        /// </summary>
        Vector3 _W_warp;
        /// <summary>
        /// applied instantaneous warping amount
        /// </summary>
        Vector3 remainingOffset;

        /// <summary>
        /// offset between the virtual and real target
        /// </summary>
        Vector3 _T_offsetTargets;

        // decides which thresholds are needed
        enum Detection
        {
            saccade,
            blink
        }

        #endregion

     
        public void Start()
        {
            if (saccadeDetection == null)
            {
                // simulate saccades/ blinks manually   (if there is no saccadeDetection)

                saccadeOccuredEvent.AddListener(ApplySaccadicRedirection);
                blinkOccuredEvent.AddListener(ApplyBlinkRedirection);

                simulationActive = true;

                Debug.Log("LeftShift: Saccade \n" + "                           RightShift: Blink");
            }
            else
            {
                // use saccade/ blink detection script
                saccadeDetection.SaccadeOccured.AddListener(ApplySaccadicRedirection);
                saccadeDetection.BlinkOccured.AddListener(ApplyBlinkRedirection);

                if (saccadeDetection.SimulateEventsWithKeyboard)
                {
                    simulationActive = true;
                }
            }
            Debug.Log("simulationActive: " + simulationActive);
        }

      

        #region Redirection

        #region override: Init, ApplyRedirection, EndRedirection
        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            _T_offsetTargets = redirectionObject.GetVirtualTargetPos() - redirectionObject.GetRealTargetPos();
            base.Init(redirectionObject, head, warpOrigin);
        }

        public override void ApplyRedirection(Transform fingertipPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target, Transform playerTransform)
        {
            //Debug.Log("SHR ApplyRedirection");
            if (saccadeDetection == null)
            {
                simulateSaccadesAndBlinks(fingertipPos, virtualHandPos, warpOrigin, target, playerTransform);
            }

            // Cheng Redirection
            base.ApplyRedirection(fingertipPos, virtualHandPos, warpOrigin, target, playerTransform);
        }

        public override void EndRedirection()
        {
            //Debug.Log("ending instantaneous");
            base.EndRedirection();
        }

        #endregion

        #region instantaneous Redirection

        /// <summary>
        /// triggered by saccadeOccuredEvent
        /// </summary>
        void ApplySaccadicRedirection()
        {
            // only apply offset when using saccades and redirection is running
            if (!UseSaccades || RedirectionManager.instance.target == null) return;

            ApplyInstantaneousRedirection(Detection.saccade, RedirectionManager.instance.realHand.transform.Find("RightHand"), RedirectionManager.instance.virtualHand.transform);
        }

        /// <summary>
        /// triggered by blinkOccuredEvent
        /// </summary>
        void ApplyBlinkRedirection()
        {
            // only apply offset when using blinks and redirection running
            if (!UseBlinks || RedirectionManager.instance.target == null) return;

            ApplyInstantaneousRedirection(Detection.blink, RedirectionManager.instance.realHand.transform.Find("RightHand"), RedirectionManager.instance.virtualHand.transform);
        }

        /// <summary>
        /// performs an instantaneous offset _Ws_saccadicWarp within the thresholds once a saccade is detected
        /// 
        /// </summary>
        /// <param name="fingertipPos"></param>
        /// <param name="virtualHandPos"></param>
        /// <param name="target"></param>
        void ApplyInstantaneousRedirection(Detection detectionType, Transform fingertipPos, Transform virtualHandPos)
        {
            //Debug.Log("instantaneous offset");

            // get warp from last frame
            _W_warp = base.Getw();
            // instantaneous offset
            // check remaining offset amount
            remainingOffset = _T_offsetTargets - _W_warp;        // total distance - so far applied warp
            //Debug.DrawRay(RedirectionManager.instance.virtualHand.transform.position, _Wr_instWarp, Color.black, 50);
            //Debug.DrawRay(Camera.main.transform.position, saccadeDirection, Color.yellow, 50);

            CheckThresholds(detectionType);
            // apply the warp
            virtualHandPos.position = RedirectionManager.instance.realHand.transform.position + _W_warp + instantOffset;

            Debug.Log("Applied Offset: " + instantOffset.magnitude);

            // manage other classes
            base.Set_t0(_W_warp + instantOffset);
            RedirectionManager.instance.SetWarpOrigin(fingertipPos.position);
        }
        #endregion

        
        void simulateSaccadesAndBlinks(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target, Transform playerTransform)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                // saccade detected
                Debug.Log("saccade");
                saccadeOccuredEvent.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.RightShift))
            {
                // blink detected
                Debug.Log("blink");
                blinkOccuredEvent.Invoke();
            }
        }
        #endregion

        #region clamp instantaneous warp to threshold
        private void CheckThresholds(Detection detectionType)
        {
            switch (detectionType)
            {
                case Detection.saccade:
                    if (!simulationActive && IntersectionManager.instance != null)
                    {
                        // get real saccade offset angle
                        saccadeOffsetAngle = IntersectionManager.instance.GetSaccadeOffsetAngle(_T_offsetTargets);
                    }
                    else
                    {
                        // simulate random saccade offset angle
                        saccadeOffsetAngle = UnityEngine.Random.Range(0.0f, 180.0f);
                        Debug.Log("random SaccadeOffsetAngle (simulation) \n or missing IntersectionPlane");

                    }
                    Debug.Log("Saccade Offset Angle: " + saccadeOffsetAngle);
                    threshold = thresholdFunction.ApproximateDetectionThreshold(saccadeOffsetAngle);
                    // set instantaneous offset amount
                    instantOffset = remainingOffset.normalized * Math.Min(remainingOffset.magnitude, threshold);
                    break;

                case Detection.blink:
                    instantOffset = remainingOffset.normalized * Math.Min(remainingOffset.magnitude, blinkThreshold);
                    break;

                default:
                    Debug.LogWarning("This should not happen");
                    break;
            }
        }

    /*
        Vector3 WorldToCameraCoordinates(Vector3 worldCoordinates)
        {
            return Camera.main.transform.InverseTransformVector(worldCoordinates);
        }

        Vector3 CameraToWorldCoordinates(Vector3 relativeToCameraCoordinates)
        {
            return Camera.main.transform.TransformVector(relativeToCameraCoordinates);
        }


         float calculateSaccadeOffsetAngle()
          {
              Vector3 saccadeDirection_camera = WorldToCameraCoordinates(saccadeDirection);
              Vector3 totalOffset_camera = WorldToCameraCoordinates(_T_offsetTargets);


              //Debug.Log("saccadeDirection.z: " + saccadeDirection_camera.z);
              //Debug.Log("remainingOffset.z: " + remainingOffset_camera.z);

              // set z = 0
              saccadeDirection_camera.z = 0;
              totalOffset_camera.z = 0;

              Debug.DrawRay(firstIntersection.transform.position, CameraToWorldCoordinates(totalOffset_camera), Color.red, 2);
              Debug.DrawRay(firstIntersection.transform.position, CameraToWorldCoordinates(saccadeDirection_camera), Color.yellow, 2);

              return Vector3.Angle(saccadeDirection_camera, totalOffset_camera);
          }

          */

        #endregion

    }
}

