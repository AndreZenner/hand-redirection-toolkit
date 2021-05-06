using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace HR_Toolkit
{
    /// <summary>
    /// The world warping translational approaches follows the concept of redirected walking, but instead of applying a
    /// rotation and translation to the head, we only apply a translation to the world.
    ///
    /// The goal is to translate the world, so that the real and virtual target have the same position. To achieve this
    /// we scale the users head motions such that the virtual target and the real target align. 
    ///
    /// This does not imply that both object are physically aligned. Their orientation can still differ.
    /// To achieve a complete alignment, use the world warping translation approach in addition. 
    /// 
    /// </summary>
    public class WorldWarping_Translation : WorldWarping
    {
        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            world = RedirectionManager.instance.virtualWorld.GetComponent<Transform>();
            this.head = head;
            currentHeadTranslation = head.position;
        }

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
            RedirectionObject target,
            Transform bodyTransform)
        {
            TranslationalWorldWarping(realHandPos, virtualHandPos, warpOrigin, target, bodyTransform);
            virtualHandPos.position = realHandPos.position;
        }
    }
}
