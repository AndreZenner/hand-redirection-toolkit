using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace HR_Toolkit
{
    /// <summary>
    /// 
    /// In Zenner & Krüger's approach we can choose from different redirection dimensions: horizontal, vertical, gain-based,
    /// and custom defined. 
    ///   - Horizontal and vertical methods offset the virtual hand to thr right/left or up/down, respectively,
    ///   - Gain-based method scales the distance of the hand from the warp origin
    ///   - Custom defined uses a custom defined plane -> Needs the 'Custom Warp Settings'
    ///
    /// To compute the warped position (excepted from the gain based method), the hand is projected on a horizontal/vertical/custom
    /// plane, its angle relative to a forward direction and the warp origin is incremented by α (the Redirection Angle) and the
    /// projected back in 3D space.
    /// For this, they defined a general rotational warp algorithm (see RotationalWarp()) allowing for displacements in
    /// arbitrary planes defined by a unit forward vector f and an orthogonal unit redirection vector r indicating the
    /// direction of positive displacement. Further inputs are the location of the warp origin o and the redirection angle α.
    ///
    /// The algorithm for the gain-based method (GainWarp()) scaled the distance of the hand from warp origin. It computes
    /// the distance vector dR to the unwarped position of the real hand and applies a gain factor g, effectively decreasing
    /// (if 0 < g < 1) or increseang (if g > 1) the speed of the hand moving away from o
    ///
    /// Custom Warp Settings:
    /// For the custom dimension we use a public Transform that can be set in the Unity Editor. Afterwards we use its unit forward vector f and
    /// its orthogonal unit right vector r, these define the new redirection plane.
    ///
    /// More information:
    /// A. Zenner and A. Krüger, Estimating Detection Thresholds for Desktop-Scale Hand Redirection in Virtual Reality.
    /// In 2019 IEEE Conference on Virtual Reality and 3D User Interfaces (VR). 47–55.
    /// DOI:http://dx.doi.org/10.1109/VR.2019.8798143
    /// </summary>
    public class Zenner_BodyWarping : BodyWarping
    {
        /// <summary>
        /// The warp angle α that is used in the Warping Mode
        /// </summary>
        public float redirectionAngleAlpha;
       
        /// <summary>
        /// Select one 'Warping Mode' in the editor. It controls the used redirection
        /// </summary>
        public WarpingMode selectedWarpingMode;
        
        /// <summary>
        /// The different redirection dimensions that are used by Zenner & Krüger
        /// </summary>
        public enum WarpingMode
        {
            Horizontal, 
            Vertical, 
            GainBased,
            Custom
        }

        /// <summary>
        /// Its forward and right vector define the new custom redirection plane
        /// </summary>
        [Header("Custom Warp Settings:")]
        public Transform customTransformCoordinates;
        
        /// <summary>
        /// The unit redirection vector r that defines the custom redirection plane
        /// </summary>
        private Vector3 redirection;
        /// <summary>
        /// The unit forward vector f that defines the custom redirection plane
        /// </summary>
        private Vector3 forward;

        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            base.Init(redirectionObject, head, warpOrigin);
        }

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
            Transform bodyTransform)
        {
            switch (selectedWarpingMode)
            {
                case WarpingMode.Horizontal:
                    forward = Vector3.forward.normalized;
                    redirection = Vector3.right.normalized;
                    var horizontalWarp = RotationalWarp(realHandPos.position, warpOrigin.position, forward, redirection, redirectionAngleAlpha);
                    virtualHandPos.position = horizontalWarp;
                    break;
                
                case WarpingMode.Vertical:
                    forward = Vector3.forward.normalized;
                    redirection = Vector3.up.normalized;
                    var verticalWarp = RotationalWarp(realHandPos.position, warpOrigin.position,forward, redirection, redirectionAngleAlpha);
                    virtualHandPos.position = verticalWarp;
                    break;
                
                case WarpingMode.GainBased:
                    var gainBasedWarp = GainWarp(realHandPos.position, warpOrigin.position, redirectionAngleAlpha);
                    virtualHandPos.position = gainBasedWarp;
                    break;
                
                case WarpingMode.Custom:
                    forward = customTransformCoordinates.forward.normalized;
                    redirection = customTransformCoordinates.right.normalized;
                    var customWarp = RotationalWarp(realHandPos.position, warpOrigin.position,forward, redirection, redirectionAngleAlpha);
                    virtualHandPos.position = customWarp;
                    break;
               
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public static Vector3 RotationalWarp(Vector3 handPosReal, Vector3 o, Vector3 f, Vector3 r, float redirectionAngle) 
        {
            // compute unit height vector
            var h = Vector3.Cross(f, r).normalized;
            // save heightDebug.Log("av: " + aV);
            var height = Vector3.Dot(handPosReal - o, h);
            // project on redirection plane
            var pProj = handPosReal - height * h;
            // unwarped offset in plane
            var dProjR = pProj - o;
            // angle rel. to f & o
            var aR = Mathf.Atan2(Vector3.Dot(dProjR, r), Vector3.Dot(dProjR, f)) ;

            // adding angular offset
            var aV = aR + redirectionAngle * Mathf.Deg2Rad; 

            // warped offset in plane
            var dProjV = Mathf.Sin(aV) * Vector3.Magnitude(dProjR) * r +
                     Mathf.Cos(aV) * Vector3.Magnitude(dProjR) * f;
            
            // final warped position
            var pos = o + dProjV + height * h;
            return pos;
        }

        public static Vector3 GainWarp(Vector3 handPosReal, Vector3 o, float gainFactor)
        {
            // unwarped offset from origin
            var dR = handPosReal - o;
            // warped offset from origin
            var dV = gainFactor * dR;
            // final warped position
            var pos = o + dV;
            return pos;

        }
    }
}
