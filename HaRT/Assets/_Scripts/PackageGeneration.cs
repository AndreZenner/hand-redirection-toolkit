﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HR_Toolkit
{
    public class PackageGeneration : MonoBehaviour
    {
        /// <summary>
        /// Export all assets for the HaRT_core package automatically
        /// </summary>
        [MenuItem("HaRT/HaRT_core Export")]
        public static void Export_core()
        {
            Debug.Log("Start Core Exporter");
            var exportedPackageAssetList = new List<string>();
            
            // scripts: get all except LeapMotionHandProjector, and BlinkSuppressedHR
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/_Scripts"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Leap") 
                    || path.Contains("BlinkDetector")
                    || path.Contains("Math3D")
                    || path.Contains("BSHR")
                    || path.Contains("PackageGeneration"))
                {
                    continue;
                }
                exportedPackageAssetList.Add(path);
            }
            
            // example scenes: get all except BSHR or Leap Examples
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/ExampleScenes"}))
            {
                
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Leap")||
                    path.Contains("Blink")||
                    path.Contains("HandCalibration"))
                {
                    continue;
                }
                exportedPackageAssetList.Add(path);
            }
            
            // prefabs: get all except HaRT_Leap and BSHR
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/Prefabs"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Leap")
                    || path.Contains("BSHR"))
                {
                    continue;
                }
                exportedPackageAssetList.Add(path);
            }
            
            // materials: get all
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/Materials"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            // models: get all
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/Models"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Packages/HaRT_core.unitypackage");
            
            Debug.Log("Finished core Exporter");
        }
        
        /// <summary>
        /// Export all assets for the HaRT_Leap package automatically
        /// </summary>
        [MenuItem("HaRT/HaRT_Leap Export")]
        public static void Export_Leap()
        {
            Debug.Log("Start Leap Exporter");
            var exportedPackageAssetList = new List<string>();
            
            // scripts: get only leap package
            foreach (var guid in AssetDatabase.FindAssets("Leap", new []{"Assets/_Scripts"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            // example scenes: get only leap scenes
            foreach (var guid in AssetDatabase.FindAssets("Leap", new []{"Assets/ExampleScenes"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            // prefabs: get only leap scenes
            foreach (var guid in AssetDatabase.FindAssets("Leap", new []{"Assets/Prefabs"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            // The hart_leap package adds more option to some hart_core scripts, like for example to the movementcontroller.
            // These options will only be used if the custom global "Leap" preprocessor is enabled. The csc.rsp file
            // enables it and is therefore added with the hart_leap package.
            exportedPackageAssetList.Add("Assets/csc.rsp");

            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Packages/HaRT_Leap.unitypackage");
            
            Debug.Log("Finished Leap Exporter");
        }
        
        /// <summary>
        /// Export all assets for the HaRT_BSHR package automatically
        /// </summary>
        [MenuItem("HaRT/HaRT_BSHR Export")]
        public static void Export_BSHR()
        {
            Debug.Log("Start BSHR Exporter");
            var exportedPackageAssetList = new List<string>();
            
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/_Scripts/Redirection/RedirectionTechniques/BodyWarping/Zenner_Regitz_Krueger_BodyWarping_BSHR"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            // example scenes: get only BSHR scenes
            foreach (var guid in AssetDatabase.FindAssets("Blink", new []{"Assets/ExampleScenes"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }

            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Packages/HaRT_BSHR.unitypackage");
            
            Debug.Log("Finished BSHR Exporter");
        }

        /// <summary>
        /// Export all assets for the HaRT_TrackerHandCalibration package automatically
        /// </summary>
        [MenuItem("HaRT/HaRT_TrackerHandCalibration Export")]
        public static void Export_TrackerHandCalibration()
        {
            Debug.Log("Start TrackerHandCalibration Exporter");
            var exportedPackageAssetList = new List<string>();

            foreach (var guid in AssetDatabase.FindAssets("", new[] { "Assets/HandCalibration" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }

            // example scenes: get only TrackerHandCalibration scenes
            foreach (var guid in AssetDatabase.FindAssets("HandCalibration", new[] { "Assets/ExampleScenes" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }

            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Packages/HaRT_TrackerHandCalibration.unitypackage");

            Debug.Log("Finished TrackerHandCalibration Exporter");
        }
    }
}
