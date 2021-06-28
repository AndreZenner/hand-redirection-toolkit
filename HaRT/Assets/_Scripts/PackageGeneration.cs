using System;
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
                    path.Contains("Blink"))
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
            //exportedPackageAssetList.Add("Assets/Materials");
            
            
            // models: get all
            foreach (var guid in AssetDatabase.FindAssets("", new []{"Assets/Models"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            //exportedPackageAssetList.Add("Assets/Models");
            
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Packages/Test_HaRT_core.unitypackage");
            
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
            // TODO leap rig is currently missing!!
            /*foreach (var guid in AssetDatabase.FindAssets("Leap", new []{"Assets/Prefabs"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }*/

            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Test_HaRT_Leap.unitypackage",
                ExportPackageOptions.Recurse);
            
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
            
            exportedPackageAssetList.Add("Assets/_Scripts/RedirectionTechniques/BodyWarping/Zenner_Regitz_Krueger_BodyWarping_BSHR");
            
            // example scenes: get only BSHR scenes
            foreach (var guid in AssetDatabase.FindAssets("Blink", new []{"Assets/ExampleScenes"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }
            
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/../../Test_HaRT_BSHR.unitypackage",
                ExportPackageOptions.Recurse);
            
            Debug.Log("Finished BSHR Exporter");
        }
    }
}
