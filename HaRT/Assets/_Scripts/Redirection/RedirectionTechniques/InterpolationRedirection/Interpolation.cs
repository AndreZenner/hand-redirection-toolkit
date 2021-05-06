using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR_Toolkit
{
    
    public class Interpolation : HandRedirector
    {
        public override void Init(RedirectionObject redirectionObject, Transform head, Vector3 warpOrigin)
        {
            
            
            
        }

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
            Transform bodyTransform)
        {
            virtualHandPos.position = GetWarpedCoordinates(realHandPos.position);
        }

        private void ComputeWarpedSpace(RedirectionObject[] redirectedPrefabs)
        {
            
            
        }

        private Vector3 GetWarpedCoordinates(Vector3 realHandPosition)
        {
            var virtualHandPosition = Vector3.zero;



            return virtualHandPosition;
        }
        
        
    }
}
