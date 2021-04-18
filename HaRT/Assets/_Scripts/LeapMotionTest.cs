/*using Leap;
using Leap.Unity;
using UnityEngine;

namespace HR_Toolkit {

  public class LeapMotionTest : PostProcessProvider {
    
    [Header("Projection")]
    public Transform headTransform;

    public Vector3 testPos;
    
    public override void ProcessFrame(ref Frame inputFrame) {
      if (headTransform == null) { headTransform = Camera.main.transform; }
      
      foreach (var hand in inputFrame.Hands)
      { 
        ///hand.SetTransform(RedirectionManager.instance.virtualHand.transform.position, hand.Rotation.ToQuaternion());
        hand.SetTransform(testPos, hand.Rotation.ToQuaternion());
      }
    }
  }
}*/
