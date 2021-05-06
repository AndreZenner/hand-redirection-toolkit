using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit.Redirection;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace HR_Toolkit
{
    [ExecuteAlways]
    public class VisualizationManager : MonoBehaviour
    {
        public bool handTrajectories;
        
        public bool realVirtualObjectConnection;
        
        public bool redirectionThresholds;

        public bool infoBox;
        
        #region window parameters

        private HandRedirector redirectionTechnique;
        private bool handAlignment;
        private bool handDistance;
        private RedirectionObject target;

        private const int textWidth = 235;
        #endregion
        
        Rect windowRect = new Rect(20, 20, 240, 190);
        
        void OnGUI()
        {
            if (infoBox)
            {
                useGUILayout = true;
                windowRect = GUILayout.Window(0, windowRect, InfoBox, "Info Box"); //, GUILayout.Width(100));
                GUI.BringWindowToFront(0);
            }

            UpdateVisualizations();
        }

        // Make the contents of the window
        private void InfoBox(int windowID)
        {
            GUILayout.FlexibleSpace();
            if (RedirectionManager.instance == null) return;
            
            GUI.Label(new Rect(5, 10, textWidth, 35), "Redirection Technique: \n     " + GetRedirectionTechnique());
            
            // Body Warping
            if (RedirectionManager.instance.redirectionTechnique.GetType().IsSubclassOf(typeof(BodyWarping)))
            {
                GUI.Label(new Rect(5, 40, textWidth, 20), "Hands aligned: " + GetHandAlignment());
                GUI.Label(new Rect(5, 60, textWidth, 20), "Virtual target: " + GetActiveVirtualTargetName());
                GUI.Label(new Rect(5, 80, textWidth, 20), "Real target: " + GetActiveRealTargetName());
                GUI.Label(new Rect(5, 100, textWidth, 20), "Redirection Angles: ");
                GUI.Label(new Rect(5, 120, textWidth, 20),
                    "Horizontal: " + BodyWarping.GetHorizontalAngle(GetActiveTarget()).ToString("F1"));
                GUI.Label(new Rect(5, 140, textWidth, 20),
                    "Vertical: " + BodyWarping.GetVerticalAngle(GetActiveTarget()).ToString("F1"));
                GUI.Label(new Rect(5, 160, textWidth, 20),
                    "Gain: " + BodyWarping.GetGainFactor(GetActiveTarget()).ToString("F1"));
            }

            // World Warping
            if (RedirectionManager.instance.redirectionTechnique.GetType().IsSubclassOf(typeof(WorldWarping)))
            {
                GUI.Label(new Rect(5, 40, textWidth, 20), "WIP");
            }
            
            // Interpolation
            if (RedirectionManager.instance.redirectionTechnique.GetType().IsSubclassOf(typeof(Interpolation)))
            {
                GUI.Label(new Rect(5, 40, textWidth, 20), "WIP");
            }
        }

        private void Update()
        {
            UpdateVisualizations();
        }

        private void UpdateVisualizations()
        {
            ActivateRedirectionAngle(redirectionThresholds);
            ActivateRealVirtualObjectConnection(realVirtualObjectConnection);
            if (RedirectionManager.instance == null)
            {
                return;
            }
            ActivateHandTrajectories(handTrajectories);
        }

        private void ActivateHandTrajectories(bool b)
        {
            if (RedirectionManager.instance == null)
            {
                return;
            }
            
            var trailRendererRealHand = RedirectionManager.instance.realHand.GetComponentsInChildren<TrailRenderer>();
            foreach (var tr in trailRendererRealHand)
            {
                if (tr.gameObject.name != "RealHand")
                {
                    tr.enabled = b;
                }
            }
            var trailRendererVirtualHand = RedirectionManager.instance.virtualHand.GetComponentsInChildren<TrailRenderer>();
            foreach (var tr in trailRendererVirtualHand)
            {
                if (tr.gameObject.name != "VirtualHand")
                {
                    tr.enabled = b;
                }
            }
        }

        private void ActivateRealVirtualObjectConnection(bool b)
        {
            GetComponentInChildren<InEditModeAnalysis>().ActivateVirtualToRealObjectConnection(b);
        }

        private void ActivateRedirectionAngle(bool b)
        {
            GetComponentInChildren<InEditModeAnalysis>().ActivateThresholdView(b);
        }
        
        #region Update Gui Window helper

        private bool GetHandAlignment()
        {
            return RedirectionManager.instance == null ? false : RedirectionManager.instance.HandsAreAligned();
        }

        private RedirectionObject GetActiveTarget()
        {
            return RedirectionManager.instance == null ? null : RedirectionManager.instance.GetActiveTarget();
        }

        private string GetActiveVirtualTargetName()
        {
            if (GetActiveTarget() == null)
            {
                return "None";
            }
            return GetActiveTarget().GetVirtualTargetObject().name;
        }

        private string GetActiveRealTargetName()
        {
            if (GetActiveTarget() == null)
            {
                return "None";
            }
            return GetActiveTarget().GetRealTargetObject().name;
        
        }

        private string GetRedirectionTechnique()
        {
            if (GetActiveTarget() == null)
            {
                return "None";
            }

            return GetActiveTarget().GetRedirectionTechnique().ToString();
        }

        #endregion
    }
}
