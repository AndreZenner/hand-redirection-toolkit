using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    /// Combination from the rotational and translational world warping approach. See more information in:
    ///     - WorldWarping_Translation.cs
    ///     - WorldWarping_Rotational.cs
    /// </summary>
    public class WorldWarping_Rotation_Translation : WorldWarping
    {
        private bool _initTranslationalWarping = false;

        private void Start ()
        {
            Debug.Log("Head Rotation: Q, E");
        }

        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
           // rotational:
            world = RedirectionManager.instance.virtualWorld.GetComponent<Transform>();
            this.head = head;
            currentHeadRotation = head.rotation.eulerAngles.y;
            currentHeadTranslation = head.position;
            SetUpWorldWarping(redirectionObject);
            
            // translational:
            
        }

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
            RedirectionObject target,
            Transform bodyTransform)
        {
            virtualHandPos.position = realHandPos.position;
            if (!RotationalWorldWarping(realHandPos, virtualHandPos, warpOrigin, target, bodyTransform)) return;

            if (!_initTranslationalWarping)
            { 
                currentHeadTranslation = head.position;
                _initTranslationalWarping = true;
            }
            
            if (!TranslationalWorldWarping(realHandPos, virtualHandPos, warpOrigin, target, bodyTransform)) return;
            
            //Debug.Log("World is aligned");
        }
    }


}
