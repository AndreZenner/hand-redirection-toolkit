using System;
using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

namespace HR
{
    [Serializable]
    public class MovementLogger : MonoBehaviour
    {
        //private List<Vector3> _handPos = new List<Vector3>();
        //private List<Quaternion> _handRot = new List<Quaternion>();
        [HideInInspector]
        public Log logLastSession = new Log {handPos = new List<Vector3>(), handRot = new List<Quaternion>()};
        Log logThisSession = new Log {handPos = new List<Vector3>(), handRot = new List<Quaternion>()};
        [HideInInspector]
        public GameObject _hand;

        private void Start()
        {
            //StartCoroutine(LoggerIO.LoadFile(logLastSession));
        }

        public void StartToLog(GameObject hand)
        {
            _hand = hand;
            InvokeRepeating(nameof(LogPosition), 1f, 0.2f); // TODO call every frame instead
        }

        public void LoadPath()
        {
            Debug.Log("Try to load");
            StartCoroutine(LoggerIO.LoadFile(logLastSession));
            
        }

        void LogPosition()
        {
            logThisSession.handPos.Add(_hand.transform.position);
            logThisSession.handRot.Add(_hand.transform.rotation);
        }

        public void DrawPath(Color color)
        {
            Debug.Log("Draw Line from " + logLastSession.handPos.Count + " positions");
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;

            lineRenderer.positionCount = logLastSession.handPos.Count;

            for (int i = 0; i < logLastSession.handPos.Count; i++)
            {
                lineRenderer.SetPosition(i, logLastSession.handPos[i]);
            }
        }

        public void DrawPath(Color color, int start, int end)
        {
            Debug.Log("Draw Line from " + (end-start) + " positions");
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
            }
            else
            {
                lineRenderer = gameObject.GetComponent<LineRenderer>();
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                lineRenderer.positionCount = 0;
            }

            lineRenderer.positionCount = logLastSession.handPos.Count;

            // compute start & end; need to multiplay with sampling rate // TODO introduce variable sampling rate
            var _start = start * 5;
            var _end = end * 5;
            for (int i = _start; i < _end; i++)
            {
                lineRenderer.SetPosition(i, logLastSession.handPos[i]);
            }
        }

        private void OnApplicationQuit()
        {
            CancelInvoke();
            LoggerIO.SaveMovement(logThisSession.handPos, logThisSession.handRot, _hand.name);
        }
        
    }
}
