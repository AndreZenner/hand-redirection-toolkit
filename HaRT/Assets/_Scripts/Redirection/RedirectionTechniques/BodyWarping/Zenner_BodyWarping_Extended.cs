using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;
/// <summary>
/// Redirection to a specific target by using Zenner & Krüger's hand redirection approach.
/// See more information in Zenner_BodyWarping.cs
///
/// 1. Creating a plane from warping origin, real target and virtual target
/// 2. Calculate redirection angle (angle between real and virtual target)
/// 3. Calculate gain factor (relation between origin - real target and origin- virtual target)
///
/// And apply those warps.
/// 
/// </summary>
public class Zenner_BodyWarping_Extended : BodyWarping
{
    /// <summary>
    /// The warp angle α that is used in the Warping Mode
    /// </summary>
    public float redirectionAngleAlpha;
    /// <summary>
    /// The gain factor
    /// </summary>
    public float gainFactor;
    /// <summary>
    /// The unit forward vector f that defines the custom redirection plane
    /// </summary>
    private Vector3 forward;
    /// <summary>
    /// The unit forward vector r that defines the custom redirection plane
    /// </summary>
    private Vector3 redirection;
    /// <summary>
    /// Custom redirection plane, gets defined by warping origin, real target and virtual target
    /// </summary>
    private Plane _plane;
    
    public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
    {
        var targetRealPos = redirectionObject.GetRealTargetPos();
        var targetVirtualPos = redirectionObject.GetVirtualTargetPos();
        var warpingOrigin = RedirectionManager.instance.warpOrigin.transform.position;

        var originToRealTarget = targetRealPos - warpingOrigin;
        var originToVirtualTarget = targetVirtualPos - warpingOrigin;
        
        // create custom plane
        _plane.Set3Points(warpingOrigin, targetRealPos, targetVirtualPos);
        // set forward vector 
        forward = (originToVirtualTarget).normalized;
        redirection = Vector3.Cross(forward, -_plane.normal).normalized;
        // compute redirection angle alpha
        redirectionAngleAlpha = Vector3.Angle(originToRealTarget, originToVirtualTarget); 
        // compute gain factor
        gainFactor = (originToVirtualTarget.magnitude / originToRealTarget.magnitude);
    }
    
    public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
        Transform bodyTransform)
    {
        // compute rotational warp
        var warp = Zenner_BodyWarping.RotationalWarp(realHandPos.position, warpOrigin.position,forward, redirection, redirectionAngleAlpha);
        // compute gain factor
        var gain = Zenner_BodyWarping.GainWarp(warp, warpOrigin.position, this.gainFactor);
        // apply it to virtual hand
        virtualHandPos.position = gain;
    }
}
