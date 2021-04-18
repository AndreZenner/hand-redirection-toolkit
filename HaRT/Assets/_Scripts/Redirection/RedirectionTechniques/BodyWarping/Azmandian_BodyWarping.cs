using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    /// Azmandian et al. created a very simple and illustrative example for a body warping scenario, see AzmandianExampleScene.unity.
    /// A real cube A is positioned in front of the user on a desk and a virtual cube A’ is shifted slightly to the right.
    /// In this case, as the user reaches for the cube, to ensure that the virtual hand meets the real cube, a translation
    /// to the right must be applied. A straightforward way of achieving this is to shift the entire rendering of the body
    /// to the right, effectively translating the user’s hands and arms to the right.
    ///
    /// Because an instantaneous shift would be directly noticeable and could disturb a user, Azmandian applied the warp
    /// incrementally. The warp depends on the hand’s progression towards the real object.
    ///
    /// To define an incremental warp, they first measure the position pH of the user's hand when body warping is
    /// activated and define it as the warping origin w0. The warping end wT is then set to target position at the time
    /// of body warping activation. With this they computed the warping ration @para a
    ///
    /// Note:
    /// The computation of the warping ratio was wrong in the paper, we swapped the min and max part
    ///
    /// More information:
    /// Mahdi Azmandian et al., Haptic Retargeting: Dynamic Repurposing of Passive Hap-tics for Enhanced Virtual Reality Experiences.
    /// InProceedings of the 2016CHI Conference on Human Factors in Computing Systems (CHI ’16). ACM,New York, NY, USA, 1968–1979.
    /// DOI: https://dl.acm.org/doi/10.1145/2858036.2858226
    /// </summary>
    public class Azmandian_BodyWarping : BodyWarping
    {
        /// <summary>
        /// position of users real hand
        /// </summary>
        private Vector3 pH; 
        /// <summary>
        /// warping origin
        /// </summary>
        private Vector3 w0; 
        /// <summary>
        /// warping end
        /// </summary>
        private Vector3 wT; 
        /// <summary>
        /// hand position at the time of body warping activation
        /// </summary>
        private Vector3 pA; 
        /// <summary>
        /// warping ratio. IT quantifies the hand's progression towars the target and determines the amount of warp applied to the
        /// hand's position.
        /// </summary>
        private float a; 
        /// <summary>
        /// distance between the virtual/physical target
        /// </summary>
        private Vector3 _t; 
        /// <summary>
        /// Vector3.zero
        /// </summary>
        private Vector3 _t0;
        /// <summary>
        /// To define a 
        /// </summary>
        /// <param name="realHandPos"></param>
        /// <param name="virtualHandPos"></param>
        /// <param name="warpOrigin"></param>
        /// <param name="target"></param>
        /// <param name="bodyTransform"></param>
        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
            Transform bodyTransform)
        {
            // set pH to users hand position
            pH = realHandPos.position;
            // define warping origin
            w0 = warpOrigin.position;
            // define the warping end
            wT = target.GetRealTargetPos();
            // compute the warping ratio
            a = Mathf.Max(0, Mathf.Min(1, (Vector3.Dot((wT - w0), (pH - w0))) / Vector3.Dot(wT - w0, wT - w0)));
            // compute the new position
            var w = a * _t;
            // apply the warp to the virtual hand
            virtualHandPos.position = pH + w;
        }
        
        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            _t = redirectionObject.GetVirtualTargetPos() - redirectionObject.GetRealTargetPos();
        }
    }
}
