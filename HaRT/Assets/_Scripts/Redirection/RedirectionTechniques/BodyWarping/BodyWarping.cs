using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    /// Body Warping- Manipulating the virtual hand movement by creating an offset between the virtual and the physical
    /// hand, such that both hands touch an object in their corresponding environments
    /// </summary>
    public class BodyWarping : HandRedirector
    {
        [Header("Thresholds")] 
        [Range(0f, 40.5f)]
        public float horizontalThreshold = 1.0f;
        [Range(0f, 40.5f)]
        public float verticalThreshold = 1.0f;
        [Range(1f, 3f)]
        public float gainForwards = 1.0f;
        [Range(0.01f, 1f)]
        public float gainDownwards = 0.01f;

        [Tooltip("Accuracy [m] required to reach a target")]
        public float epsilon = 0.03f;

        [Header("Additional")]
        [Tooltip("Choose the distance calculation method for the current BW algorithm. Initially the standard method")]
        DistanceCalculationMode distanceCalculation = DistanceCalculationMode.standard;
        

        public enum ThresholdDirection
        {
            Horizontal, Vertical, Gain
        }

        public enum DistanceCalculationMode
        {
            standard
        }

        public override void EndRedirection()
        {
            RedirectionManager.instance.target = null;
        }

        #region thresholds
        public override bool HasThresholds()
        {
            return true;
        }

        public override bool IsInThreshold(RedirectionManager redirectionManager, RedirectionObject redirectionObject)
        {
            var warpOrigin = redirectionManager.GetDefaultWarpOrigin();
            if (redirectionObject.GetWarpOrigin() != null) warpOrigin = redirectionObject.GetWarpOrigin();

            if (!CheckThreshold(ThresholdDirection.Horizontal, warpOrigin.transform.position,
                redirectionObject.GetRealTargetPos(),
                redirectionObject.GetVirtualTargetPos())) return false;
            if (!CheckThreshold(ThresholdDirection.Vertical, warpOrigin.transform.position,
                redirectionObject.GetRealTargetPos(),
                redirectionObject.GetVirtualTargetPos())) return false;
            if (!CheckThreshold(ThresholdDirection.Gain, warpOrigin.transform.position,
                redirectionObject.GetRealTargetPos(),
                redirectionObject.GetVirtualTargetPos())) return false;
            
            return true;
        }

        public bool CheckThreshold(ThresholdDirection thresholdDirection, Vector3 warpOrigin, Vector3 realTarget, Vector3 virtualTarget)
        {
            var virtualTargetDir = virtualTarget - warpOrigin;
            var realTargetDir = realTarget - warpOrigin;
            var angle = 0f;

            switch (thresholdDirection)
            {
                case ThresholdDirection.Horizontal:
                    angle = ComputeRedirectionAngle(Vector3.forward, Vector3.right, realTarget, virtualTarget, Vector3.up, warpOrigin);
                    if (angle <= horizontalThreshold) return true;
                    break;
                case ThresholdDirection.Vertical:
                    angle = ComputeRedirectionAngle(Vector3.forward, Vector3.up, realTarget, virtualTarget, Vector3.up, warpOrigin);
                    if (angle <= verticalThreshold) return true;
                    return true;
                case ThresholdDirection.Gain:
                    var v = virtualTargetDir.magnitude;
                    var r = realTargetDir.magnitude;
                    var gain = v/r;
                    
                    if (gain < gainDownwards || gain > gainForwards) return true;
                    
                    break;
            }
            return false;
        }

        #endregion

        #region help methods
        public static float GetVerticalAngle(RedirectionObject redirectionObject)
        {
            if (redirectionObject == null)
            {
                return 0;
            }
            var angle = ComputeRedirectionAngle(Vector3.forward.normalized, Vector3.up.normalized, 
                redirectionObject.GetRealTargetPos(), redirectionObject.GetVirtualTargetPos(), Vector3.up, 
                redirectionObject.GetWarpOrigin().transform.position);

            return angle;
        }

        public static float GetHorizontalAngle(RedirectionObject redirectionObject)
        {
            if (redirectionObject == null)
            {
                return 0;
            }

            var angle = ComputeRedirectionAngle(Vector3.forward, Vector3.right, redirectionObject.GetRealTargetPos(),
                redirectionObject.GetVirtualTargetPos(), Vector3.up, redirectionObject.GetWarpOrigin().transform.position);
            
            return angle;
        }
        
        
        private static Vector3 ComputeProjection(Vector3 f, Vector3 r, Vector3 o, Vector3 t)
        {
            // compute unit height vector
            var h = Vector3.Cross(f, r).normalized;
            // save height 
            var height = Vector3.Dot(t - o, h);
            // project on redirection plane
            var pProj = t - height * h;
            // unwarped offset in plane
            return  pProj - o; 
        }
        

        private static float ComputeRedirectionAngle(Vector3 f, Vector3 r, Vector3 tR, Vector3 tV, Vector3 planeNormal, Vector3 o)
        {
            var posReal = ComputeProjection(f, r, o, tR);
            var posVirtual = ComputeProjection(f, r, o, tV);

            return Vector3.Angle(posVirtual, posReal);
        }

        public static float GetGainFactor(RedirectionObject redirectionObject)
        {
            if (redirectionObject == null)
            {
                return 0;
            }
            var virtualTargetDir = redirectionObject.GetVirtualTargetPos() - redirectionObject.GetWarpOrigin().transform.position;
            var realTargetDir = redirectionObject.GetRealTargetPos() - redirectionObject.GetWarpOrigin().transform.position;
            var v = virtualTargetDir.magnitude;
            var r = realTargetDir.magnitude;
            return (v / r);
        }

        #endregion

        #region distance calculation

        public void calculateDp(Transform realHandPos, RedirectionObject target, out float dp)
        {
            switch (distanceCalculation)
            {
                case DistanceCalculationMode.standard:
                    // dp = (realHandPos.position - target.GetRealTargetPos()).magnitude;
                    dp = Mathf.Max((realHandPos.position - target.GetRealTargetPos()).magnitude - epsilon, 0);
                    break;
                    
                default:
                    Debug.LogWarning("There was no matching distanceCalculationMode. This should not happen");
                    //dp = (realHandPos.position - target.GetRealTargetPos()).magnitude;
                    dp = Mathf.Max((realHandPos.position - target.GetRealTargetPos()).magnitude - epsilon, 0);
                    distanceCalculation = DistanceCalculationMode.standard;
                    break;
            }
        }

        #endregion
    }
}
