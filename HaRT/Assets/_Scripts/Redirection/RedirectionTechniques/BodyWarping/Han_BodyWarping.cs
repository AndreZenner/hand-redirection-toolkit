using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    ///  Han et al. created two remapping techniques for reaching in Virtual Reality:
    ///     - Translational Shift, which introduces a static offset between the virtual and physical hand before a hand reach
    ///     - Interpolated Reach, which dynamically interpolates the position of the virtual hand during a reaching motion
    ///
    ///  Translational Shift:
    ///     Translational shift is a remapping technique that involves relocating the virtual hand based on the positional
    ///     offset between the real world object and the virtual object.
    ///     The calculation for the virtual hand position, Pvh, using translation offset is given by:
    ///         Pvh=Pph+(Ppo−Pvo)
    ///     Here,Pph is the physical hand’s current world position and Ppo and Pvo are the world positions of the physical
    ///     object and virtual object respectively.
    ///
    ///  Interpolated Reach:
    /// 
    /// More information:
    /// 
    /// </summary>
    public class Han_BodyWarping : HandRedirector
    {
        public enum Han_Technique
        {
            TranslationalShift,
            InterpolatedReach
        }

        public Han_Technique han_RedirectionTechnique;
        /// <summary>
        /// Boundary offset that is used in the interpolation reach method
        /// </summary>
        public float c;

        public float b;
        
        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
            RedirectionObject target,
            Transform bodyTransform)
        {
            // set pPH to users physical hand position
            var pPH = realHandPos.position;
            // set pPO to physical (object) target position
            var pPO = target.GetRealTargetPos();
            // set pVO to virtual (object) target position
            var pVO = target.GetVirtualTargetPos();
            
            var warp = Vector3.zero;
            // apply the warp depending on the chosen technique
            switch (han_RedirectionTechnique)
            {
                case Han_Technique.TranslationalShift:
                    warp = (pVO - pPO); 
                    // apply warp to virtual hand
                    virtualHandPos.position = pPH + warp;
                    return;
                
                case Han_Technique.InterpolatedReach:
                    // distance between the physical obj and physical hand
                    var d = Vector3.Distance(pPO, pPH);
                    if (d >= b)
                    {
                        warp = Vector3.zero;
                    }
                    else
                    {
                        warp = (pVO - pPO) * (1 - (d / b));
                    }
                    // apply warp to virtual hand
                    virtualHandPos.position = pPH + warp;
                    break;
                
                default:
                    throw new Exception("No Han_Redirection Technique Set");
            }
            
        }

        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            var pPH = RedirectionManager.instance.realHand.transform.position;
            var pPO = redirectionObject.GetRealTargetPos();
            
            // interpolation boundary
            b = Vector3.Distance(pPO, pPH) + c;
        }
    }
}