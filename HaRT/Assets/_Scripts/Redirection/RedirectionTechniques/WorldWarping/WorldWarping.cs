using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
    public class WorldWarping : HandRedirector
    {
        
        /// Thresholds from Azmandian: (from RW reference)
        /// Translations: scaled up by 26% or down by 14%
        /// Rotations: scaled up by 49% and down by 20% (MAYBE DIFFERENT IN SEATED ENVIRONMENT!) 

        protected Transform world;
        
        [Tooltip("The amount that is needed to classify a head rotation as real rotation")]
        private static float HEAD_ROTATION_TRIGGER_THRESHOLD = 20f;
        private static float MAX_POSSIBLE_HEAD_ROTATION = 180f;
        private static float OBJECTS_ARE_ALIGNED_THRESHOLD = 0.1f;

        private static float HEAD_TRANSLATION_TRIGGER_THRESHOLD = 0.0005f;
        private static float MAX_POSSIBLE_HEAD_TRANSLATION = 0.1f;
        private static float OBJECTS_ARE_TRANSLATIONAL_ALIGNED = 0.01f;

        #region Fields: World Warping Thresholds
        [Header("Azmandian's Gain Factors")]
        [Tooltip("Scaling factor for a rotation towards a target (Azmandian: up by 49%)")]
        [Range(1f,1.49f)]
        public float rotationScalingFactorUpwards = 1;
        [Tooltip("Scaling factor for a rotation away from a target (Azmandian: down by 20%)")]
        [Range(0.8f,1)]
        public float rotationScalingFactorDownwards = 1;
        
        [Tooltip("Scaling factor for a translation towards a target (Azmandian: up by 26%)")]
        [Range(1f,1.26f)]
        public float translationScalingFactorUpwards = 1;
        [Tooltip("Scaling factor for a translation away from a target (Azmandian: down by 14%)")]
        [Range(0.86f,1f)]
        public float translationScalingFactorDownwards = 1;
        #endregion

        #region Fields: Rotational World Warping 
        
        private float lastHeadRotation;
        protected float currentHeadRotation;
        private float rotationDif;
        private float rFrame;
        protected Transform head;

        private float d;
        private float dNeeded;
        
        private bool rotating = false;
        private bool rotatingClockwise = false;
        
        #endregion

        #region Fields: Translational World Warping
        private Vector3 lastHeadTranslation;
        protected Vector3 currentHeadTranslation;
        private Vector3 translationDifVector;
        protected float translationDif;

        protected bool moving = false;
        protected bool movingTowardsTarget = false;
        
        private float lastHeadTargetDistance;
        private float currentHeadTargetDistance;
        
        #endregion

        #region Rotational Computations
        /// <summary>
        // check rotation direction
        // it is not important, where the target is in the world, relative to us. instead we have to check, if our 
        // rotation is the same as the rotation we have to apply to the virtual world. to do so, we check if the
        // world has to rotate clockwise or not. then we check if we rotated clockwise or not. 
        /// </summary>
        protected virtual void ComputeHeadRotationAngle() 
        {
            #region update head rotation and position information
            lastHeadRotation = currentHeadRotation;
            currentHeadRotation = head.rotation.eulerAngles.y;
            lastHeadTranslation = currentHeadTranslation;
            currentHeadTranslation = head.forward;
            var currentVirtualTargetPosition = RedirectionManager.instance.target.GetVirtualTargetPos();
            #endregion
            
            // get head rotation
            var rotInDeg = Vector3.SignedAngle(lastHeadTranslation, currentHeadTranslation, Vector3.up);
            var frameDuration = Time.deltaTime;
            var rSeconds = rotInDeg / frameDuration;
            
            if (Mathf.Abs(rSeconds) > MAX_POSSIBLE_HEAD_ROTATION)
            {
                rFrame = 0;
                return;
            }
            
            rFrame = rotInDeg;

            // is rotating?
            rotating = Mathf.Abs(rSeconds) > HEAD_ROTATION_TRIGGER_THRESHOLD;
            
            // Check if we rotate clockwise
            rotatingClockwise = RotationIsClockwise(lastHeadTranslation, currentHeadTranslation);
        }
        
        /// <summary>
        /// returns true, if the angle between these two vectors is smaller in a clockwise direction
        /// </summary>
        private bool RotationIsClockwise(Vector3 from, Vector3 to)
        {
            if (Vector3.SignedAngle(from,to,Vector3.up) > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// the angle between the real targets forward vector and the virtual targets forward vector. to achieve
        /// an alignment, we have add this angle to the rotation of the virtual world!
        /// </summary>
        /// <param name="target"></param>
        protected void SetUpWorldWarping(RedirectionObject target)
        {
            d = Vector3.SignedAngle(target.GetVirtualTargetForwardVector(), target.GetRealTargetForwardVector(), Vector3.up);
            dNeeded = d;
        }
        
        /// <summary>
        /// Returns true, if the real world and the virtual world are aligned
        /// </summary>
        protected bool RotationalWorldWarping(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
            RedirectionObject target, Transform bodyTransform)
        {
            # region Debug Rays
            Debug.DrawRay(target.GetRealTargetPos(), -target.positions[0].realPosition.forward,
                Color.blue);
            Debug.DrawRay(target.GetVirtualTargetPos(), -target.positions[0].virtualPosition.forward,
                Color.green);
            #endregion
            
            #region Get current prams
            var vrtRot = target.GetVirtualRot();
            var relRot = target.GetRealRot();
            var difRot = relRot * Quaternion.Inverse(vrtRot);
            var endRot = world.rotation * difRot;
            #endregion
            
            dNeeded = Vector3.SignedAngle(target.GetVirtualTargetForwardVector(), target.GetRealTargetForwardVector(), Vector3.up);

            // world is aligned, return
            if (Math.Abs(dNeeded) < OBJECTS_ARE_ALIGNED_THRESHOLD) return true;
            
            ComputeHeadRotationAngle();
            
            // if there are no head rotations, return
            if (!rotating) return false;
            
            var rAngle = GetMaxApplicableRotation(target);
            var rWorld = dNeeded < 0 ? Mathf.Max(rAngle, dNeeded) : Mathf.Min(rAngle, dNeeded);

            // Apply rotation
            var worldParent = world.parent;
            world.parent = head;
            world.RotateAround(world.position, Vector3.up, rWorld);
            world.parent = worldParent;

            return false;
        }

        private float GetMaxApplicableRotation(RedirectionObject target)
        {
            // do we rotate towards the target?
            var worldHasToRotateClockwise = RotationIsClockwise(target.GetVirtualTargetForwardVector(),
                target.GetRealTargetForwardVector());
            var rotatingTowardsTarget = worldHasToRotateClockwise == rotatingClockwise;
            
            // apply scaling factors
            var rScaled = 1f;
            if (rotatingTowardsTarget)
            {
                rScaled = rFrame * rotationScalingFactorUpwards;
                
            }
            else rScaled = rFrame * rotationScalingFactorDownwards;
            
            return (rScaled - rFrame);
        }

        #endregion

        #region Translational Computations
        protected virtual Vector3 ComputeHeadTranslationDistance(Vector3 currentVirtualTargetPosition)
        {
            #region update prams
            lastHeadTranslation = currentHeadTranslation;
            currentHeadTranslation = head.position;
            lastHeadTargetDistance = currentHeadTargetDistance;
            currentHeadTargetDistance = Vector3.Distance(head.position,currentVirtualTargetPosition);
            #endregion
            
            translationDifVector.x = Mathf.Abs(currentHeadTranslation.x - lastHeadTranslation.x);
            translationDifVector.y = Mathf.Abs(currentHeadTranslation.y - lastHeadTranslation.y);
            translationDifVector.z = Mathf.Abs(currentHeadTranslation.z - lastHeadTranslation.z);

            //translationDif = Math.Abs(lastHeadTargetDistance - currentHeadTargetDistance);
            translationDif = Mathf.Abs(currentHeadTargetDistance - lastHeadTargetDistance);
            //Debug.Log("translational dif: " + translationDif);
            
            // can't be true, too larget
            if (translationDif > MAX_POSSIBLE_HEAD_TRANSLATION)
            {
                moving = false;
                movingTowardsTarget = false;
                return Vector3.zero;
            }
            
            // check if moving at all
            if (translationDif < HEAD_TRANSLATION_TRIGGER_THRESHOLD)
            {
                moving = false;
                movingTowardsTarget = false;
                return translationDifVector;
            }
            
            //Debug.Log("transl dif: " + translationDif);
            moving = true;
            return translationDifVector;
        }

        protected bool TranslationalWorldWarping(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
            RedirectionObject target,
            Transform bodyTransform)
        {
            # region Debug Rays
            Debug.DrawRay(target.GetRealTargetPos(), -target.positions[0].realPosition.forward,
                Color.blue);
            Debug.DrawRay(target.GetVirtualTargetPos(), -target.positions[0].virtualPosition.forward,
                Color.green);
            # endregion

            var vecDist = target.GetRealTargetPos() - target.GetVirtualTargetPos();
            // the vector from the virtual target to the real target
            var pv = target.GetRealTargetPos() - target.GetVirtualTargetPos();
            var remainingDistance = pv.magnitude;
            
            // objects are aligned, return
            if (remainingDistance < OBJECTS_ARE_TRANSLATIONAL_ALIGNED) return true;

            // Check and set translation variables
            ComputeHeadTranslationDistance(target.GetVirtualTargetPos());

            if(!moving) return false;

            // the users moving vector
            var b = currentHeadTranslation - lastHeadTranslation;

            // moving in same direction?
            var dot = Vector3.Dot(pv.normalized, b.normalized);
            movingTowardsTarget = dot > 0;

            // get the matching gain factor
            var gainFactor = movingTowardsTarget ? translationScalingFactorUpwards : translationScalingFactorDownwards;
            
            // the users moving vector multiplied with the gain factor
            var bg = b * gainFactor;
            
            // project the b and bg on pv
            var proj = Vector3.Project(b, pv);
            var projWithGain = Vector3.Project(bg, pv);

            // compute the amount of how much the world need to be shifted along/against pv
            var x = Mathf.Abs(projWithGain.magnitude - proj.magnitude);
            var a = pv.normalized * x;

            world.transform.position += a;

            return false;
        }

        #endregion
    }
}
