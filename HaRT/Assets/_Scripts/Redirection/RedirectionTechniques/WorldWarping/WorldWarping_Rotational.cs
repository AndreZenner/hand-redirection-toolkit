using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
   /// <summary>
   /// The world warping rotational approaches follows the concept of redirected walking, but instead of applying a
   /// rotation and translation to the head, we only apply a rotation to the world.
   ///
   /// The goal is to rotate the world, so that the real and virtual target have the same orientation. To achieve this
   /// we scale the users head motions such that the virtual targets unit forward vector and the real target unit
   /// forward vector align. 
   ///
   /// This does not imply that both object are physically aligned. To achieve a complete alignment, use the world warping
   /// translation approach in addition. 
   /// 
   /// </summary>
   public class WorldWarping_Rotational : WorldWarping
   {
      public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
      {
         world = RedirectionManager.instance.virtualWorld.GetComponent<Transform>();
         this.head = head;
         currentHeadRotation = head.rotation.eulerAngles.y;
         currentHeadTranslation = head.position;
         SetUpWorldWarping(redirectionObject);
      }

      public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin,
         RedirectionObject target,
         Transform bodyTransform)
      {
         RotationalWorldWarping(realHandPos, virtualHandPos, warpOrigin, target, bodyTransform);
         virtualHandPos.position = realHandPos.position;
      }

      public override void EndRedirection()
      {
         Debug.Log("Cancel Invoke");
         //CancelInvoke("ComputeHeadRotationAngle");
      }
   }
}
