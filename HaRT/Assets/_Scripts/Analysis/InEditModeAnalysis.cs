using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit.Redirection;
using UnityEditor;
using UnityEngine;

namespace HR_Toolkit
{
    [ExecuteAlways]
    public class InEditModeAnalysis : MonoBehaviour
    {
        public RedirectionManager redirectionManager;
        private GameObject selectedObj;

        private VirtualToRealConnection lastHighlightedPrefab = null;

        private bool activatedThreshold;
        private bool activatedConnectionView;

        public void ActivateThresholdView(bool b)
        {
            activatedThreshold = b;

            gameObject.GetComponent<LineRenderer>().enabled = b;
        }

        public void ActivateVirtualToRealObjectConnection(bool b)
        {
            activatedConnectionView = b; 
            foreach (var redirectionObject in redirectionManager.allRedirectedPrefabs)
            {
                if (redirectionObject == null)
                {
                    return;
                }
                foreach (var virtualToRealConnection in redirectionObject.GetComponentsInChildren<VirtualToRealConnection>())
                {
                    virtualToRealConnection.GetComponent<LineRenderer>().enabled = b;
                }
            }
        }

        void Update()
        {
            if (redirectionManager == null) return;
            
            if (!activatedThreshold) return;
            
            selectedObj = GetActiveTarget();

            if (GetSelectedObject() != null) selectedObj = GetSelectedObject();

            if (selectedObj == null)
            {
                gameObject.GetComponent<LineRenderer>().enabled = false;
                return;
            }
            
            VirtualToRealConnection virtualToRealConnection = selectedObj.GetComponentInChildren<VirtualToRealConnection>();
            if (virtualToRealConnection == null )
                //|| virtualToRealConnection == lastHighlightedPrefab) return;
            
                HighlightConnection(lastHighlightedPrefab, false);
            //ShowThresholds(lastHighlightedPrefab, false);
           
            lastHighlightedPrefab = virtualToRealConnection;
            HighlightConnection(virtualToRealConnection, true);

            ShowThresholds(virtualToRealConnection, true);
           
        }

        private GameObject GetActiveTarget()
        {
            return redirectionManager.target == null ? null : redirectionManager.target.gameObject;
        }

        private GameObject GetSelectedObject()
        {
            if (Selection.activeTransform == null)
            {
                return null;
            }
            return Selection.activeTransform.gameObject.GetComponent<RedirectionObject>() == null ? null 
                : Selection.activeTransform.gameObject.GetComponent<RedirectionObject>().gameObject;
        }

        private void HighlightConnection(VirtualToRealConnection virtualToRealConnection, bool b)
        {
            if (virtualToRealConnection == null) return;
            if (b)
                virtualToRealConnection.ChangeMaterialToHighlighted();
            else
                virtualToRealConnection.ChangeMaterialToDefault();
        }

        private void ShowThresholds(VirtualToRealConnection virtualToRealConnection, bool b)
        {
           if (virtualToRealConnection == null) return;
            var redirectionTechnique = 
                virtualToRealConnection.gameObject.GetComponentInParent<RedirectionObject>().redirectionTechnique;

            if (redirectionTechnique == null)
            {
                redirectionTechnique = redirectionManager.redirectionTechnique;
            }

            var realTargetPos = virtualToRealConnection.realPosition.position;
            var virtualTargetPos = virtualToRealConnection.virtualPosition.position;
            var warpOrigin = redirectionManager.warpOrigin.transform.position;

            var redirectedPrefab =
                virtualToRealConnection.transform.parent.gameObject.GetComponent<RedirectionObject>();

            var color = CheckThresholds(redirectedPrefab);
            
            DrawThreshold(realTargetPos, virtualTargetPos, warpOrigin, color);
        }

        private void DrawThreshold(Vector3 realTargetPos, Vector3 virtualTargetPos, Vector3 warpOrigin, Color color)
        {
            var lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                throw new Exception("No line renderer is set to display thresholds");
            }
            lineRenderer.enabled = true;
            lineRenderer.materials[0].color = color;
           
            
            var pos = new Vector3[] {realTargetPos, warpOrigin, virtualTargetPos};
            
            lineRenderer.SetPosition(0, realTargetPos);
            lineRenderer.SetPosition(1, warpOrigin);
            lineRenderer.SetPosition(2, virtualTargetPos); 
        }

        private Color CheckThresholds(RedirectionObject redirectionObject)
        {
            var technique = redirectionManager.GetDefaultRedirectionTechnique();
            if (redirectionObject.redirectionTechnique != null) technique = redirectionObject.GetRedirectionTechnique();
            
            // if there are no thresholds, return color blue
            if (!technique.HasThresholds()) return Color.blue;
            
            // check for thresholds. If they are fine, return color green, else red
            if (technique.IsInThreshold(redirectionManager, redirectionObject)) return Color.green;
            
            return Color.red;

        }
    }
}