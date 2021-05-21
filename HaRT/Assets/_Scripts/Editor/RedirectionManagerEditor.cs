using System;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RedirectionManager))]
[CanEditMultipleObjects]
public class RedirectionManagerEditor : Editor
{
    
    private SerializedProperty _movement;
    private RedirectionManager _redirectionManager;

    private SerializedProperty _virtualWorld;
    private SerializedProperty _realHand;
    private SerializedProperty _virtualHand;
    private SerializedProperty _warpOrigin;
    private SerializedProperty _body;
    private SerializedProperty _allRedirectedPrefabs;

    private SerializedProperty _redirectionTechnique;

    private SerializedProperty _target;
    private SerializedProperty _lastTarget;

    private SerializedProperty _testController;
    private SerializedProperty _pathGenerator;
    private SerializedProperty _logFile;
    

    private void OnEnable()
    {
        _movement= serializedObject.FindProperty("movement");
        _virtualWorld = serializedObject.FindProperty("virtualWorld");
        _realHand = serializedObject.FindProperty("realHand");
        _virtualHand = serializedObject.FindProperty("virtualHand");
        _warpOrigin = serializedObject.FindProperty("warpOrigin");
        _body = serializedObject.FindProperty("body");
        _allRedirectedPrefabs = serializedObject.FindProperty("allRedirectedPrefabs");

        _redirectionTechnique = serializedObject.FindProperty("redirectionTechnique");

        _target = serializedObject.FindProperty("target");
        _lastTarget = serializedObject.FindProperty("lastTarget");

        _testController = serializedObject.FindProperty("testControllers");
        _pathGenerator = serializedObject.FindProperty("pathGenerator");
        _logFile = serializedObject.FindProperty("logFile");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _redirectionManager = (RedirectionManager) target;

        EditorGUILayout.PropertyField(_virtualWorld, new GUIContent("Virtual World"));
        EditorGUILayout.PropertyField(_realHand, new GUIContent("Real Hand"));
        EditorGUILayout.PropertyField(_virtualHand, new GUIContent("Virtual Hand"));
        EditorGUILayout.PropertyField(_warpOrigin, new GUIContent("Warp Origin"));
        EditorGUILayout.PropertyField(_body, new GUIContent("Body"));
        EditorGUILayout.LabelField("Movement Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_movement, new GUIContent("Movement"));

        DefineRedirectedPrefabSection();
        DefineRedirectionSection();
        //DefineThresholdControllerSection();

        //EditorGUILayout.PropertyField(_target, new GUIContent("Current Target"));
        //EditorGUILayout.PropertyField(_lastTarget, new GUIContent("Last Target"));
        
        
        //DefineAnalysisSection();
        
        /*if (GUILayout.Button("Test"))
        {
            Debug.Log("We pressed a button!");
        }*/

        serializedObject.ApplyModifiedProperties();
        DefineMovement();
    }
    
    private void DefineMovement()
    {
        _redirectionManager.movementController = _redirectionManager.GetComponent<MovementController>();
        if (_redirectionManager.movementController == null)
        {
            _redirectionManager.gameObject.AddComponent<MovementController>();
            _redirectionManager.movementController = _redirectionManager.GetComponent<MovementController>();
        }

        // Mouse Movement
        if (_redirectionManager.movement == MovementController.Movement.Mouse)
        {
            _redirectionManager.speed = EditorGUILayout.Slider("Speed", _redirectionManager.speed, 1f, 20f);
            _redirectionManager.mouseWheelSpeed = EditorGUILayout.Slider("Mouse Wheel Speed", _redirectionManager.mouseWheelSpeed, 1f, 20f);
        }
    }

    private void DefineRedirectedPrefabSection()
    {
        EditorGUILayout.LabelField("Redirected Prefab Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_allRedirectedPrefabs, new GUIContent("All Redirected Prefabs"));
    }

    private void DefineRedirectionSection()
    {
        EditorGUILayout.LabelField("Redirection Technique Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_redirectionTechnique, new GUIContent("Default Redirection Technique"));
        
    }

    /*private void DefineThresholdControllerSection()
    {
        EditorGUILayout.LabelField("Threshold Controller Options (WIP)", EditorStyles.boldLabel);
    }

    private void DefineAnalysisSection()
    {
        EditorGUILayout.LabelField("Test & Analysis Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_testController, new GUIContent("Test Controllers (WIP)"));
        EditorGUILayout.PropertyField(_pathGenerator, new GUIContent("Path Generator (WIP)"));
        EditorGUILayout.PropertyField(_logFile, new GUIContent("Log File"));
    }*/
}
