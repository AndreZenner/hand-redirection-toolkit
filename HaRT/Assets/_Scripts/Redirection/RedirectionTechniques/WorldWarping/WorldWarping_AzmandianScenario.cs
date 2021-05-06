using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR_Toolkit
{
    /// <summary>
    /// A representation of Azmandian et al.'s ring of cubes illusion.
    /// 
    /// In Azmandian et al.’s hand redirection scenario they concentrated on the head rotation only, because they used it
    /// in a seated environment where head rotations are much more common as in a standing environment. For this they
    /// calculated the user’s current change in the rotation and scaled the desired rotation proportionally. To ensure
    /// alignment, they developed a mechanism to calculate the required scale factor for alignment. This approach was
    /// demonstrated and tested afterwards in the ring of cubes illusion.
    ///
    /// The Ring of Cubes Illusion:
    /// A single physical cube is mapped with world warping to different virtual cubes that are arranged around the
    /// physical one on a circle.
    ///
    /// This approach works only with the setup of the ring of cubes illusion -> Assets/Scenes/RingOfIllusion
    ///
    /// More information:
    /// Mahdi Azmandian et al., Haptic Retargeting: Dynamic Repurposing of Passive Hap-tics for Enhanced Virtual Reality Experiences.
    /// InProceedings of the 2016CHI Conference on Human Factors in Computing Systems (CHI ’16). ACM,New York, NY, USA, 1968–1979.
    /// DOI: https://dl.acm.org/doi/10.1145/2858036.2858226
    /// </summary>
    public class WorldWarping_AzmandianScenario : WorldWarping
    {
        /// <summary>
        /// The deviation when two objects are on the same position.
        /// </summary>
        public float deviation;

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, 
            RedirectionObject target, Transform bodyTransform)
        {
            // calculate the vector between the player and the virtual target
            var vecPlayerVrtTarget = target.positions[0].virtualPosition.transform.position - bodyTransform.position;
            // calculate the vector between the player and the real target
            var vecPlayerRealTarget = target.positions[0].realPosition.transform.position - bodyTransform.position;
            
            // TODO y must be zero

            Debug.DrawLine(target.positions[0].virtualPosition.transform.position,bodyTransform.position, Color.blue);
            Debug.DrawLine(target.positions[0].realPosition.transform.position,bodyTransform.position, Color.green);
            
            var angle = Vector3.SignedAngle(vecPlayerRealTarget, vecPlayerVrtTarget, Vector3.up);
            if (Mathf.Abs(angle) < deviation)
            {
                Debug.Log("world is aligned");
                return;
            }
            
            // TODO use world warping rotation here!
            world.transform.Rotate(0, -angle * Time.deltaTime, 0);
            
        }
    }
}
