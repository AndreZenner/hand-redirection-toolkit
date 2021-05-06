using UnityEngine;

namespace HR_Toolkit
{
    public class InverseDistanceWeightingRedirection : InterpolationRedirection
    {

        /// <summary>
        /// Power
        /// </summary>
        public float p;
        

        public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
            Transform bodyTransform)
        {
            virtualHandPos.position = IDW(realHandPos.position, target);
        }

        private Vector3 IDW(Vector3 x, RedirectionObject target)
        {
            var u = Vector3.zero;
            var points = target.GetAllPositions();

            var topSum = Vector3.zero;
            var bottomSum = 0f;
            
            foreach (var point in points)
            {
                var d = Vector3.Distance(x, point.GetRealPosition());
                if (d == 0f)
                {
                    return x + point.GetVirtualPosition() - point.GetRealPosition();
                }
                var w = Mathf.Pow(1 / d, p);
                
                
                topSum += w * (point.GetVirtualPosition() - point.GetRealPosition());
                bottomSum += w;
                
            }

            u = topSum / bottomSum;
            return x + u;
        }
        
        
    }
}
