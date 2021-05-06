using System;
using UnityEngine;

namespace HR_Toolkit.Redirection
{
    [ExecuteAlways]
    public class VirtualToRealConnection : MonoBehaviour
    {
        public Transform realPosition;
        public Transform virtualPosition;
        
        public Material defaultConnectionMaterial;
        public Material highlightedConnectionMaterial;

        private LineRenderer _lineRenderer;

        private void Start()
        {
            if (GetComponent<LineRenderer>() == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
            
            _lineRenderer.startWidth = 0.005f;
            _lineRenderer.endWidth = 0.005f;

            _lineRenderer.startColor = Color.green;
            _lineRenderer.endColor = Color.green;
            _lineRenderer.material = defaultConnectionMaterial;
            
            gameObject.layer = LayerMask.NameToLayer("Visualization");
            
            ChangeMaterialToDefault();
        }

        private void Update()
        {
            _lineRenderer.SetPosition(0, realPosition.position);
            _lineRenderer.SetPosition(1, virtualPosition.position);
        }

        public void ChangeMaterialToDefault()
        {
            _lineRenderer.material = defaultConnectionMaterial;
        }

        public void ChangeMaterialToHighlighted()
        {
            try
            {
                _lineRenderer.material = highlightedConnectionMaterial;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Vector3 GetRealPosition()
        {
            return realPosition.position;
        }

        public Vector3 GetVirtualPosition()
        {
            return virtualPosition.position;
        }

        void OnDrawGizmos()
        {
            if (realPosition == null) return;

            if (GetRealPosition() == GetVirtualPosition())
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(GetRealPosition(), .007f);
            }
            else
            {
                // real positions
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(GetRealPosition(), .007f);
                
                // virtual positions
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(GetVirtualPosition(), .007f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(GetRealPosition(), GetVirtualPosition());
            }
        }
    }
}
