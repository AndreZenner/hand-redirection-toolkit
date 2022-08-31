using HR_Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;


public class TargetCollisionManager : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider collision)
    {
        if (RedirectionManager.instance.target == null) return;
        if (RedirectionManager.instance.reachedTarget) return;      // target already reached previously - nothing to do

        // check whether this is the current target
        if (collision.gameObject == RedirectionManager.instance.target.GetRealTargetObject())
        {
            RedirectionManager.instance.reachedTarget = true;           // tell RedirectionManager that we reached the target
            RedirectionManager.instance.target.HandReachedCube();
        }
    }
}
