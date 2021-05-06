using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


    [CustomEditor(typeof(Logger))]
    [CanEditMultipleObjects]
    public class LoggerEditor : Editor
    {
        private Logger _logger;
        //float minVal   = 10;
        //float minLimit = 0;
        //float maxVal   =  30;
        //float maxLimit =  100;

        /*public override void OnInspectorGUI()
        
        {
            base.OnInspectorGUI();
            _logger = (Logger) target;
            EditorGUILayout.LabelField("Min Val:",minVal.ToString());
            EditorGUILayout.LabelField("Max Val:", maxVal.ToString());
            EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
            if (GUILayout.Button("Move!"))
                _logger.Draw(Mathf.FloorToInt(minVal), Mathf.FloorToInt(maxVal));

            serializedObject.ApplyModifiedProperties();
        }*/

    }

