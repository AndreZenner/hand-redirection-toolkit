using System;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    /// Hand Redirection aims to minimize the passive haptic feedback problems.  InHand Redirection exists no one-to-one
    /// mapping between virtual and physical objects, instead we want to warp the virtual space and fool the human
    /// hand-eye-coordination.
    ///
    /// There are three main conceptual approaches:
    ///     1. Body Warping- Manipulating the virtual hand movement by creating an offset between the virtual and the
    ///     physical hand, such that both hands touch an object in their corresponding environments - @class BodyWarping
    ///
    ///     2. World Warping- Remapping a virtual object by rotating the virtual world around the user’s head - @class WorldWarping
    ///
    ///    3. Distortions of the complete virtual space or parts of it- A discrepancy between the real and virtual
    ///     hand movement - @class
    ///
    /// To implement a new Redirection Technique, Inherit either from this class or if you use one of the conceptual approches
    /// from above, inherit from them.
    /// 
    /// </summary>
    public abstract class HandRedirector: MonoBehaviour
    {

        /// <summary>
        /// The ResetPosition is used between two redirections. Instead of redirecting from one target to another target,
        /// the user will be redirected to the reset position first. This prevents to huge redirections. 
        /// </summary>
       // public RedirectionObject resetPosition; // TODO
        /// <summary>
        /// The Init() method is neccessary for all Redirection Techniques. Init() gets called everytime a Redirected
        /// Prefab is set to next target.
        /// This method is used to set up the redirection technique. To trigger specif Unity Events use OnRedirectionStart()
        /// on each Redirected Prefab
        /// </summary>
        /// <param name="redirectionObject">The redirected Prefab that is set to Target in the RedirectionManager</param>
        /// <param name="head">Transform of the tracked head </param>
        public virtual void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            
        }

        /// <summary>
        /// ApplyRedirection() gets called every frame in RedirectionManager's Update. Each Redirection Technique computes
        /// the new offset/warp of the user's virtual hand and applies it
        /// </summary>
        /// <param name="realHandPos">Tracked real hand position</param>
        /// <param name="virtualHandPos">Transform of the virtual hand</param>
        /// <param name="warpOrigin">Warp origin that is set in the Redirection Manager on the redirection start</param>
        /// <param name="target">The next target, it contains the virtual and real target position</param>
        /// <param name="bodyTransform">Transform of the tracked head</param>
        /// <exception cref="NotImplementedException">Throws Exception, if there is not Redirection applied</exception>
        public virtual void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, 
            Transform warpOrigin, RedirectionObject target, Transform bodyTransform)
        {
            
        }

        public virtual void EndRedirection()
        {

        }

        public virtual bool HasThresholds()
        {
            return false;
        }

        public virtual bool IsInThreshold(RedirectionManager redirectionManager, RedirectionObject redirectionObject)
        {
            throw new NotImplementedException("There are no thresholds for this technique!");
        }
        
        public virtual RedirectionObject GetResetPosition()
        {
            return null;
        }
    }
}
