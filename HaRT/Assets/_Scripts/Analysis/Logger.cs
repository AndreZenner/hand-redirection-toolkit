using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HR;
using HR_Toolkit;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [HideInInspector]
    public MovementLogger realHandMovement;

    private RedirectionManager redirectionManager;
    
    // Stream Writer variables:
    private StreamWriter writer;
    private int frame = 0;
    private DateTime time;
    private Vector3 rHandPos;
    private Quaternion rHandRot;
    private Vector3 vHandPos;
    private Quaternion vHandRot;
    private HandRedirector redirectionTechnique;
    private RedirectionObject target;
    private Vector3 targetRealPos;
    private Quaternion targetRealRot;
    private Vector3 targetVirtualPos;
    private Quaternion targetVirtualRot;
    private Vector3 bodyPos;
    private Quaternion bodyRot;


    
    void Start()
    {
        redirectionManager = RedirectionManager.instance;
        //realHandMovement = gameObject.AddComponent<MovementLogger>();
        //realHandMovement.LoadPath();
        //realHandMovement.StartToLog(RedirectionManager.instance.realHand);
        
        writer = new StreamWriter(GetPath());
        // write the first line of the log file:
        writer.WriteLine("Frame;" +
                         "Timestamp;" +
                         "Real Hand Position;" +
                         "Real Hand Rotation;" +
                         "Virtual Hand Position;" +
                         "Virtual Hand Rotation;" +
                         "Target;" +
                         "Redirection Technique;" +
                         "Target Real Position;" +
                         "Target Real Rotation;" +
                         "Target Virtual Position;" +
                         "Target Virtual Rotation;" +
                         "Body Position;" +
                         "Body Rotation;");
    }
    
    private void OnApplicationQuit()
    {
        writer.Close();
    }

    private void Update()
    {
        time = DateTime.Now;
        rHandPos = redirectionManager.realHand.transform.position;
        rHandRot = redirectionManager.realHand.transform.rotation;
        vHandPos = redirectionManager.virtualHand.transform.position;
        vHandRot = redirectionManager.virtualHand.transform.rotation;
        
        target = redirectionManager.target;
        if (target != null)
        {
            redirectionTechnique = target.redirectionTechnique;
            targetRealPos = target.positions[0].realPosition.transform.position;
            targetRealRot = target.positions[0].realPosition.transform.rotation;
            targetVirtualPos = target.positions[0].virtualPosition.transform.position;
            targetVirtualRot = target.positions[0].virtualPosition.transform.rotation;
        }

        bodyPos = redirectionManager.body.transform.position;
        bodyRot = redirectionManager.body.transform.rotation;
        if (target != null)
        {
            writer.WriteLine(frame + ";" +
                             time + " " + time.Millisecond + ";" +
                             rHandPos + ";" +
                             rHandRot + ";" +
                             vHandPos + ";" +
                             vHandRot + ";" +
                             target + ";" +
                             redirectionTechnique.name + ";" +
                             targetRealPos + ";" +
                             targetRealRot + ";" +
                             targetVirtualPos + ";" +
                             targetVirtualRot + ";" +
                             bodyPos + ";" +
                             bodyRot + ";");
        }
        else
        {
            writer.WriteLine(frame + ";" +
                             time + " " + time.Millisecond + ";" +
                             rHandPos + ";" +
                             rHandRot + ";" +
                             vHandPos + ";" +
                             vHandRot + ";" +
                             "---" + ";" +
                             "---" + ";" +
                             "---" + ";" +
                             "---" + ";" +
                             "---" + ";" +
                             "---" + ";" +
                             bodyPos + ";" +
                             bodyRot + ";");
        }

        frame++;
    }

    public void Draw(int start, int end)
    {
        realHandMovement.DrawPath(Color.green, start, end);
    }
    
    private static string GetPath()
    {
        #if UNITY_EDITOR
        return Application.dataPath + "/log.csv";
        #endif
    }
    
    
    
}
