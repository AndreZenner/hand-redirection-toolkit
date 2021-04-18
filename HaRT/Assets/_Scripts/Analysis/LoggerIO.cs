using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using HR;

namespace HR_Toolkit
{
    public struct Log
    {
        public List<Vector3> handPos;
        public List<Quaternion> handRot;
    }
    public static class LoggerIO
    {
        public static void SaveMovement(List<Vector3> handPos, List<Quaternion> handRot, string handName)
        {
            // need to turn vector & qutarnion into a serializable entity first
            // TODO maybe creat struct that is saved here instead of using multiple list that get saved indiviually
            /*List<SerializableVector3> handPosSer = new List<SerializableVector3>();
            List<SerializableQuaternion> handRotSer = new List<SerializableQuaternion>();
            for (var i = 0; i < handPos.Count -1; i++)
            {
                SerializableVector3 serializableVector3 = new SerializableVector3(handPos[i].x, handPos[i].y, handPos[i].z);
                SerializableQuaternion serializableQuaternion = new SerializableQuaternion(handRot[i].x, handRot[i].y, handRot[i].z, handRot[i].w);
                handPosSer.Add(serializableVector3);
                handRotSer.Add(serializableQuaternion);
            }*/

            //Debug.Log("Positions: " + handPos.Count);
            //Debug.Log("Try to save");
            
            //StreamWriter writer = new StreamWriter(GetPath());
            
            //writer.WriteLine("Position,Rotation");
            for (int i = 0; i < handPos.Count-1; i++)
            {
                //writer.WriteLine(handPos[i].ToString() + "&" + handRot[i].ToString());
            }
            
            //writer.Close();
            
            Debug.Log("saved at " + GetPath());
            //BinaryFormatter formatter = new BinaryFormatter();
            //string path = Application.persistentDataPath + "/" + handName + "handMovement.fun";
            //FileStream stream = new FileStream(path, FileMode.Create);

            //formatter.Serialize(stream, handPosSer);
            //formatter.Serialize(stream, handRotSer);
            //stream.Close();
        }

        public static IEnumerator LoadFile(Log log)
        {
            StreamReader reader = new StreamReader(GetPath());
            var line = reader.ReadLine(); // first line contains only header
            line = reader.ReadLine();

            while (line != null)
            {
                // split line into substrings
                var lineInputs = line.Split('&');
                // hand position = Vector 3
                log.handPos.Add(StringToVector3(lineInputs[0]));
                // hand rotation = quaternion
                log.handRot.Add(StringToQuaternion(lineInputs[1]));
                
                line = reader.ReadLine();
            }
            
            yield return log;
        }

        
        public static List<Vector3> LoadMovement(string handName)
        {
            /*string path = Application.persistentDataPath + "/" + handName + "handMovement.fun";
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                List<SerializableVector3> handPosSer = formatter.Deserialize(stream) as List<SerializableVector3>;
                stream.Close();

                List<Vector3> handPos = new List<Vector3>();
                foreach (var serializableVector3 in handPosSer.ToList())
                {
                    Vector3 vector3 = new Vector3(serializableVector3.x, serializableVector3.y, serializableVector3.z);
                    handPos.Add(vector3);
                }

                return handPos;

            }
            else
            {
                Debug.Log("Save file not found in " + path);
                return null;
            }*/
            return null;
        }

        private static string GetPath()
        {
            #if UNITY_EDITOR
            return Application.dataPath + "/log.csv";
            #endif
        }

        #region String To Type helpers
        
        private static Quaternion StringToQuaternion(string sQuaternion)
        {
            // Remove the parentheses
            if (sQuaternion.StartsWith ("(") && sQuaternion.EndsWith (")")) {
                sQuaternion = sQuaternion.Substring(1, sQuaternion.Length-2);
            }
 
            // split the items
            string[] sArray = sQuaternion.Split(',');
 
            // store as a Quaternion
            Quaternion result = new Quaternion(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]),
                float.Parse(sArray[3]));
 
            return result;
        }

        private static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
                sVector = sVector.Substring(1, sVector.Length-2);
            }
 
            // split the items
            string[] sArray = sVector.Split(',');
 
            // store as a Vector3
            Vector3 result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            return result;
        }
        

        #endregion

    }
}
