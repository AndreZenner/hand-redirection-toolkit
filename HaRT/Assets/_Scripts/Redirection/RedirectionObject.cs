using System;
using System.Collections.Generic;
using System.Linq;
using HR_Toolkit.Thresholds;
using HR_Toolkit.Redirection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;

namespace HR_Toolkit
{
    public class RedirectionObject : MonoBehaviour
    {
        public List<VirtualToRealConnection> positions;
        [Space]
        [Header("Optional Settings:")]
        [Tooltip("Optional - If no redirection technique is selected, the default technique that is set in the Redirection Manager will be used. Otherwise the selected technique will only be used on this object.")]
        public HandRedirector redirectionTechnique;
        [Tooltip("Optional - Can be used if you want to use a specif warp origin just for this redirection object that differs from the default warp origin")]
        public GameObject warpOrigin;

        public RedirectionObject resetPosition;
        public bool useResetPosition;
        public bool thisIsAResetPosition;
        
        [Space]
        [Header("Redirection Events:")]
        public UnityEvent onRedirectionActivated;
        public UnityEvent onRedirectionDeactivated;

        private Color _startColor;

        void Start()
        {
            if (redirectionTechnique == null)
            {
                redirectionTechnique = RedirectionManager.instance.GetDefaultRedirectionTechnique();
            }

            if (warpOrigin == null)
            {
                warpOrigin = RedirectionManager.instance.GetDefaultWarpOrigin();
            }

            this.tag = "virtualTarget";
            this.gameObject.layer = LayerMask.NameToLayer("Virtual/Object");

            foreach (var prefabCorrespondent in gameObject.GetComponentsInChildren<VirtualToRealConnection>())
            {
                if(positions.Contains(prefabCorrespondent)) continue;
                
                positions.Add(prefabCorrespondent);
            }

        }

        public void Redirect()
        {
            redirectionTechnique.ApplyRedirection(RedirectionManager.instance.realHand.transform, 
                RedirectionManager.instance.virtualHand.transform, RedirectionManager.instance.warpOrigin.transform, 
                this, RedirectionManager.instance.body.transform);
        }

        public void StartRedirection()
        {
            onRedirectionActivated.Invoke();
            redirectionTechnique.Init(this, RedirectionManager.instance.body.transform,RedirectionManager.instance.warpOrigin.transform.position);
            HighlightOn();
        }

        public void EndRedirection()
        {
            onRedirectionDeactivated.Invoke();
            HighlightOff();
            redirectionTechnique.EndRedirection();
            if (useResetPosition)
            {
                resetPosition.HighlightOff();
            }
        }
        
        
        #region Private helpers
        private void OnHandEnter()
        {
            if (!useResetPosition || RedirectionManager.instance.target != this) return;

            this.HighlightOff();
            resetPosition.HighlightOn();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("virtualHand") || thisIsAResetPosition) return;
            OnHandEnter();
        }
        private void HighlightOn()
        {
            _startColor = this.GetComponent<Renderer>().material.color;
            GetComponent<Renderer>().material.color = Color.yellow;   
        }

        private void HighlightOff()
        {
            GetComponent<Renderer>().material.color = _startColor;
        }


        #endregion

        #region Getter & Setter

        public HandRedirector GetRedirectionTechnique()
        {
            return redirectionTechnique;
        }

        public GameObject GetWarpOrigin()
        {
            return warpOrigin;
        }
        public Vector3 GetVirtualTargetPos()
        {
            if (positions[0] == null) if (positions[0] != null) Debug.LogWarning("The RedirectionObject " + gameObject.name + "is missing a VirtualToRealConnection. Make sure one is placed as a child object and it is assigned in the positions array!", transform);

            return positions[0].virtualPosition.position;
        }

        public Vector3 GetRealTargetPos()
        {
            if (positions[0] == null) Debug.LogWarning("The RedirectionObject " + gameObject.name + "is missing a VirtualToRealConnection. Make sure one is placed as a child object and it is assigned in the positions array!", transform);
            
            return positions[0].realPosition.position;
        }

        public Quaternion GetVirtualRot()
        {
            return positions[0].virtualPosition.rotation;
        }

        public Quaternion GetRealRot()
        {
            return positions[0].realPosition.rotation;
        }

        public List<VirtualToRealConnection> GetAllPositions()
        {
            return positions;
        }

        public Vector3 GetRealTargetForwardVector()
        {
            return positions[0].realPosition.forward;
        }

        public Vector3 GetVirtualTargetForwardVector()
        {
            return positions[0].virtualPosition.forward;
        }

        public bool UseResetPosition()
        {
            return useResetPosition;
        }

        public RedirectionObject GetResetPosition()
        {
            return resetPosition;
        }

        public GameObject GetRealTargetObject()
        {
            return positions[0].realPosition.gameObject;
        }

        public GameObject GetVirtualTargetObject()
        {
            return positions[0].virtualPosition.gameObject;
        }

        #endregion


    }
}
