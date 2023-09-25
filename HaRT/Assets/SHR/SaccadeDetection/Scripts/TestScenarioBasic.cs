using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO:
// hängt manchmal / braucht mehrere / längere Fokussierungen.. Tracking vs Code?
// modify bool type --> saccade = true
// start / target methods in one method with parameters --> avoid duplicated code
// saccade detected löst bei reiner kopfbewegung aus --> values?


public class TestScenarioBasic : MonoBehaviour
{

    #region Variables
    // INSPECTOR
    [Header("Required")]
    [SerializeField]
    CubeManager cubeManager;
    [SerializeField]
    SaccadeDetection saccadeDetection;
    //[SerializeField]
    //DataLogger dataLogger;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 3000)]
    [Tooltip("duration in ms the cube needs to be focused")]
    int duration = 500;
    [Tooltip("path of input file for testMode. input file is a logfile.csv from earlier trials")]
    public string InputPath;
    [Range(0,8)]
    [Tooltip("allowed range of eyetracker frames which should still count as 'correct detected'")]
    public int AllowedRange = 3;


    // Help Variables
    int startCube = 0;
    int cubeIdCounter = 1;
    int modeCounter = 1;        // 0: focus start once, 1: focus start duration, 2: stop focusing start, 3: focus cube once, 4: focus cube duration, 5: stop focusing cube
    bool saccadeShouldAppear = false;
    bool simulateInput;
    bool trackingDataStart = false;


    // Measurements
    [SerializeField]
    [Tooltip("if true: prints the distance, duration of saccade between startpoint-cube ")]
    public bool DEBUG_Measurements = false;
    public bool DEBUG_Switch = false;
    string[] DEBUG_Messages = {"focusStartOnce", "focusStartDuration", "stopFocusingStart", "focusTargetOnce", "focusTargetDuration", "stopFocusingTarget" };
    int timeSinceSaccadeOnset;      // time since saccade started in ms
    float distanceStartTarget;      // distance from start to target
    float saccadeSize;              // size of saccade in degrees (angle between (start - head) & (target - head))
    int timestamp = 0;
    int oldTimestamp = 0;

    // start: after first startcondition, end: after last cube; used for benchmark logging
    // avoids too early / too long measurements that results in wrong analysis data
    bool experimentRunning = false;
    bool newDataAvailable = false;

    // Simulation
    bool DEBUG_Simulation = false;
    StreamReader reader;
    Queue<SimFrame> SimFrames = new Queue<SimFrame>();
    string[] dataValues = new string[20];       // check size


    #endregion

    // Start is called before the first frame update
    void Start()
    {
        simulateInput = saccadeDetection.SimulateInput;
        if (simulateInput)
        {
            // Check InputFile for correctness
            string sceneName = SceneManager.GetActiveScene().name;
            sceneName = sceneName.Replace("SaccadeDetection", ""); 
            if (!InputPath.Contains(sceneName))
            {
                Debug.Assert(InputPath.Contains(sceneName),
                "The given input file  '" + InputPath + "'  does not match the current scene  '" + sceneName + "' . \n " +
                "Please check the TestScene type and give a matching logging file as input. \n The current analysis is not correct!");
            }
            ReadInputFile();
            experimentRunning = true;
            saccadeDetection.IsExperimentRunning(true);
            //dataLogger.IsExperimentRunning(true);
            saccadeDetection.NewDataNeeded.AddListener(sendSimulationData);
            }
        if (saccadeDetection.TestMode || simulateInput)
        {
            cubeManager.SetCube(startCube);
        }
        saccadeDetection.NewDataAvailable.AddListener(newData);
    }

    // Update is called once per frame
    // simpler:  // focusOnce(0), focusDuration(0), stopFocusing(0), focusOnce(ID), focusDuration(ID), stopFocusing(ID)
    void Update()
    {

        // only if in TestMode or Simulation
        if (saccadeDetection.TestMode || simulateInput)
        {
            if (cubeIdCounter < cubeManager.numCubes)
            {   // as long as there are cubes left

                switch (modeCounter)
                {
                    case 0:
                        focusStartOnce();
                        break;
                    case 1:
                        focusStartDuration();
                        break;
                    case 2:
                        stopFocusingStart();
                        break;
                    case 3:
                        focusTargetOnce();
                        break;
                    case 4:
                        focusTargetDuration();
                        break;
                    case 5:
                        stopFocusingTarget();
                        break;
                    default:
                        Debug.LogWarning("modeCounter has an invalid value: " + modeCounter);
                        break;
                }
                if (DEBUG_Switch)
                {
                    Debug.Log(DEBUG_Messages[modeCounter]);
                }

            }
            else
            {
                // experiment is over - all cubes done
                if (experimentRunning)
                {
                    experimentRunning = false;
                    saccadeDetection.IsExperimentRunning(false);
                    //dataLogger.IsExperimentRunning(false);
                    if(simulateInput)
                    {
                        Debug.Log("End of file. You can stop the play mode now");
                    }
                    else
                    {
                        Debug.Log("TestScenario finished. You can stop the play mode now");
                    }
                }
            }
        }
        if (newDataAvailable && experimentRunning)
        {
            //dataLogger.SaveDataToFile(timestamp, saccadeShouldAppear, saccadeDetection.GetSaccadeDetectionStatus(), saccadeSize, timeSinceSaccadeOnset);
            if (saccadeShouldAppear && DEBUG_Measurements) {
                Debug.Log("Saccade should appear");
            }
            newDataAvailable = false;
        }
    }

   

    #region TestScene Steps

    private void focusStartOnce()
    {
        // 0
        updateMeasurements();
        // condition
        if (cubeManager.CheckCollision(0))
        {
            stopMeasurements();
            modeCounter++;
            cubeIdCounter++;
        }
    }

    private void focusStartDuration()
    {
        // 1
        // condition
        if (cubeManager.CheckCollisionDuration(duration, 0, oldTimestamp))
        {
            setNextSetup();
            modeCounter++;
        }
    }

    private void stopFocusingStart()
    {
        // 2
        // condition
        if (!cubeManager.CheckCollision(0))
        {
            startMeasurements();
            modeCounter++;
        }
    }

    private void focusTargetOnce()
    {
        // 3
        updateMeasurements();
        // condition
        if (cubeManager.CheckCollision(cubeIdCounter))
        {
            stopMeasurements();
            modeCounter++;
        }
    }

    private void focusTargetDuration()
    {
        // 4
        // condition
        if (cubeManager.CheckCollisionDuration(duration, cubeIdCounter, oldTimestamp))
        {
            setStartSetup();
            modeCounter++;
        }
    }

    private void stopFocusingTarget()
    {
        // 5
        // condition
        if (!cubeManager.CheckCollision(cubeIdCounter))
        {
            startMeasurements();
            modeCounter = 0;
        }
    }

    #endregion

    #region Help Methods TestScene

    #region Manage Setup

    /// <summary>
    /// after start focused: reset startCube, set nextCube, start measurements
    /// </summary>
    private void setNextSetup()
    {
        cubeManager.SetCube(cubeIdCounter);
        cubeManager.ResetCube(startCube);
        experimentRunning = true;
        saccadeDetection.IsExperimentRunning(true);
        //dataLogger.IsExperimentRunning(true);
    }

    /// <summary>
    /// after target focused: reset currentCube, set startCube
    /// </summary>
    private void setStartSetup()
    {
        cubeManager.SetCube(startCube);
        cubeManager.ResetCube(cubeIdCounter);
    }

    #endregion

    #region Measurements

    /// <summary>
    /// starts when cube different from start point is highlighted && user stopped focusing start point
    /// </summary>
    private void startMeasurements()
    {
        saccadeShouldAppear = true;
        timeSinceSaccadeOnset = 0;
        calculateDistance();
        calculateSaccadeSize();
    }

    /// <summary>
    /// updated when cube different from start point is highlighted && users focus is neither on start point or on target cube
    /// here saccade should occur
    /// </summary>
    private void updateMeasurements()
    {
        timeSinceSaccadeOnset += timestamp - oldTimestamp;
    }

    /// <summary>
    /// stops while cube different from start point is highlighted but user already focused target cube
    /// </summary>
    private void stopMeasurements()
    {
        if (DEBUG_Measurements)
        {
            Debug.Log("Cube: " + cubeIdCounter + "\n" +
                 "Saccade Duration: " + timeSinceSaccadeOnset + "s       " +
                 "Distance Cubes: " + distanceStartTarget * 100 + "cm");
        }
        saccadeShouldAppear = false;
        timeSinceSaccadeOnset = 0;
        distanceStartTarget = 0f;
        saccadeSize = 0f;
    }

    #endregion

    void calculateDistance()
    {
        distanceStartTarget = Vector3.Distance(getCube(startCube).transform.position, getCube(cubeIdCounter).transform.position);
    }

    void calculateSaccadeSize()
    {
        // Angle(A-B, C-B)
        saccadeSize = Vector3.Angle(getCube(startCube).transform.position - cubeManager.GetEyeOriginGlobal(), getCube(cubeIdCounter).transform.position - cubeManager.GetEyeOriginGlobal());
    }

    private GameObject getCube(int cubeID)
    {
        return transform.GetChild(0).gameObject.transform.GetChild(cubeID).gameObject;
    }

    #endregion

    #region Manage Simulation Input

    

    public void ReadInputFile()
    {
        // 0: timestamp
        // 1: saccade (truth)
        // 2: speed
        // 3: acceleration
        // 4: saccadeSize
        // 5: openessLeft
        // 6: openessRight

        // 7-9: eyeDirectionLocal
        // 10-12: eyeOriginLocal
        // 13-15: eyeDirectionGlobal
        // 16-18: eyeOriginGlobal

        // 19-21: eyeDirectionLeftLocal
        // 22-24: eyeOriginLeftLocal
        // 25-27: eyeDirectionRightLocal
        // 28-30: eyeOriginRightLocal
        // 31-33: eyeDirectionLeftGlobal
        // 34-36: eyeOriginLeftGlobal
        // 37-39: eyeDirectionRightGlobal
        // 40-42: eyeOriginRightGlobal

        reader = new StreamReader(File.OpenRead(InputPath));
        string dataLines;

        while (!reader.EndOfStream)
        {
            dataLines = reader.ReadLine();
            dataValues = dataLines.Split(',');


            // erste Zeile überspringen
            if (!dataValues[0].Contains("timestamp"))
            {

                if (!trackingDataStart)
                {
                    // find start
                    foreach (char charac in dataValues[0])
                    {
                        if (char.IsDigit(charac))
                        {
                            trackingDataStart = true;
                            break;
                        }
                    }
                }
                else
                {

                int timestamp = int.Parse(dataValues[0]);
                bool saccadeTruth = bool.Parse(dataValues[1]);
                float eyeOpenessLeft = float.Parse(dataValues[5]);
                float eyeOpenessRight = float.Parse(dataValues[6]);

                Vector3 eyeDirectionLocal = readVector(7);
                Vector3 eyeOriginLocal = readVector(10);
                Vector3 eyeDirectionGlobal = readVector(13);
                Vector3 eyeOriginGlobal = readVector(16);

                Vector3 eyeDirectionLeftLocal = readVector(19);
                Vector3 eyeOriginLeftLocal = readVector(22);
                Vector3 eyeDirectionRightLocal = readVector(25);
                Vector3 eyeOriginRightLocal = readVector(28);

                Vector3 eyeDirectionLeftGlobal = readVector(31);
                Vector3 eyeOriginLeftGlobal = readVector(34);
                Vector3 eyeDirectionRightGlobal = readVector(37);
                Vector3 eyeOriginRightGlobal = readVector(40);

                SimFrame currentSimFrame = new SimFrame(timestamp, saccadeTruth, eyeOpenessLeft, eyeOpenessRight, eyeDirectionLocal, eyeOriginLocal, eyeDirectionGlobal, eyeOriginGlobal, eyeDirectionLeftLocal, eyeOriginLeftLocal, eyeDirectionRightLocal, eyeOriginRightLocal, eyeDirectionLeftGlobal, eyeOriginLeftGlobal, eyeDirectionRightGlobal, eyeOriginRightGlobal);
                SimFrames.Enqueue(currentSimFrame);

                if (DEBUG_Simulation)
                {
                    Debug.Log("timestamp: " + timestamp + "      eyeDirectionRightGlobal: " + eyeDirectionRightGlobal.x + "," + eyeDirectionRightGlobal.y + "," + eyeDirectionRightGlobal.z);
                }
                }

            }
        }
        if (DEBUG_Simulation)
        {
            Debug.Log("Reading Input File Complete");
        }
    }



    private Vector3 readVector(int start)
    {
        return new Vector3(float.Parse(dataValues[start]), float.Parse(dataValues[start + 1]), float.Parse(dataValues[start + 2]));
    }


    public struct SimFrame
    {
        public SimFrame(int fileTimestamp, bool fileSaccadeTruth, float fileEyeOpenessLeft, float fileEyeOpenessRight, Vector3 fileEyeDirectionLocal, Vector3 fileEyeOriginLocal, Vector3 fileEyeDirectionGlobal, Vector3 fileEyeOriginGlobal, Vector3 fileEyeDirectionLeftLocal, Vector3 fileEyeOriginLeftLocal, Vector3 fileEyeDirectionRightLocal, Vector3 fileEyeOriginRightLocal, Vector3 fileEyeDirectionLeftGlobal, Vector3 fileEyeOriginLeftGlobal, Vector3 fileEyeDirectionRightGlobal, Vector3 fileEyeOriginRightGlobal)
        {
            timestamp = fileTimestamp;
            saccadeTruth = fileSaccadeTruth;
            eyeOpenessLeft = fileEyeOpenessLeft;
            eyeOpenessRight = fileEyeOpenessRight;

            // combined
            eyeDirectionLocal = fileEyeDirectionLocal;
            eyeOriginLocal = fileEyeOriginLocal;
            eyeDirectionGlobal = fileEyeDirectionGlobal;
            eyeOriginGlobal = fileEyeOriginGlobal;

            // left, right
            eyeDirectionLeftLocal = fileEyeDirectionLeftLocal;
            eyeOriginLeftLocal = fileEyeOriginLeftLocal;
            eyeDirectionRightLocal = fileEyeDirectionRightLocal;
            eyeOriginRightLocal = fileEyeOriginRightLocal;

            eyeDirectionLeftGlobal = fileEyeDirectionLeftGlobal;
            eyeOriginLeftGlobal = fileEyeOriginLeftGlobal;
            eyeDirectionRightGlobal = fileEyeDirectionRightGlobal;
            eyeOriginRightGlobal = fileEyeOriginRightGlobal;
        }

        public int timestamp { get; set; }
        public bool saccadeTruth { get; set; }
        public float eyeOpenessLeft { get; set; }
        public float eyeOpenessRight { get; set; }

        public Vector3 eyeDirectionLocal { get; set; }
        public Vector3 eyeOriginLocal { get; set; }
        public Vector3 eyeDirectionGlobal { get; set; }
        public Vector3 eyeOriginGlobal { get; set; }

        public Vector3 eyeDirectionLeftLocal { get; set; }
        public Vector3 eyeOriginLeftLocal { get; set; }
        public Vector3 eyeDirectionRightLocal { get; set; }
        public Vector3 eyeOriginRightLocal { get; set; }
        public Vector3 eyeDirectionLeftGlobal { get; set; }
        public Vector3 eyeOriginLeftGlobal { get; set; }
        public Vector3 eyeDirectionRightGlobal { get; set; }
        public Vector3 eyeOriginRightGlobal { get; set; }


        public override string ToString() => $"({timestamp}, {eyeDirectionLocal}, {eyeOriginLocal}, {eyeOpenessLeft}, {eyeOpenessRight})";
    }

    #endregion


    #region Public Methods

    void sendSimulationData()
    {
        if (SimFrames.Count > 0)
        {
            saccadeDetection.GetSimFrame(SimFrames.Peek().timestamp, SimFrames.Peek().saccadeTruth, SimFrames.Peek().eyeOpenessLeft, SimFrames.Peek().eyeOpenessRight, 
                SimFrames.Peek().eyeDirectionLocal, SimFrames.Peek().eyeOriginLocal, SimFrames.Peek().eyeDirectionGlobal, SimFrames.Peek().eyeOriginGlobal,
                SimFrames.Peek().eyeDirectionLeftLocal, SimFrames.Peek().eyeOriginLeftLocal, SimFrames.Peek().eyeDirectionRightLocal, SimFrames.Peek().eyeOriginRightLocal,
                SimFrames.Peek().eyeDirectionLeftGlobal, SimFrames.Peek().eyeOriginLeftGlobal, SimFrames.Peek().eyeDirectionRightGlobal, SimFrames.Peek().eyeOriginRightGlobal);
            //dataLogger.GetCurrentGroundTruth(SimFrames.Dequeue().saccadeTruth);
        }
        else if (experimentRunning)
        {
            Debug.Log("reached end of file. Simulation can be stopped.");
            experimentRunning = false;
            saccadeDetection.IsExperimentRunning(false);
            //dataLogger.IsExperimentRunning(false);
        }
    }

    public bool ReadSimFrame(out int outTimestamp, out bool outSaccadeTruth, out Vector3 outEyeDirectionLocal, out Vector3 outEyeOriginLocal, out Vector3 outEyeDirectionGlobal, out Vector3 outEyeOriginGlobal, out float outEyeOpenessLeft, out float outEyeOpenessRight, out Vector3 outEyeDirectionLeftLocal, out Vector3 outEyeOriginLeftLocal, out Vector3 outEyeDirectionRightLocal, out Vector3 outEyeOriginRightLocal, out Vector3 outEyeDirectionLeftGlobal, out Vector3 outEyeOriginLeftGlobal, out Vector3 outEyeDirectionRightGlobal, out Vector3 outEyeOriginRightGlobal)
    {
        if (SimFrames.Count > 0)
        {
            outTimestamp = SimFrames.Peek().timestamp;
            outSaccadeTruth = SimFrames.Peek().saccadeTruth;
            outEyeOpenessLeft = SimFrames.Peek().eyeOpenessLeft;
            outEyeOpenessRight = SimFrames.Peek().eyeOpenessRight;

            // combined
            outEyeDirectionLocal = SimFrames.Peek().eyeDirectionLocal;
            outEyeOriginLocal = SimFrames.Peek().eyeOriginLocal;
            outEyeDirectionGlobal = SimFrames.Peek().eyeDirectionGlobal;
            outEyeOriginGlobal = SimFrames.Peek().eyeOriginGlobal;

            // left, right
            outEyeDirectionLeftLocal = SimFrames.Peek().eyeDirectionLeftLocal;
            outEyeOriginLeftLocal = SimFrames.Peek().eyeOriginLeftLocal;
            outEyeDirectionRightLocal = SimFrames.Peek().eyeDirectionRightLocal;
            outEyeOriginRightLocal = SimFrames.Peek().eyeOriginRightLocal;

            outEyeDirectionLeftGlobal = SimFrames.Peek().eyeDirectionLeftGlobal;
            outEyeOriginLeftGlobal = SimFrames.Peek().eyeOriginLeftGlobal;
            outEyeDirectionRightGlobal = SimFrames.Peek().eyeDirectionRightGlobal;
            outEyeOriginRightGlobal = SimFrames.Dequeue().eyeOriginRightGlobal;
            
            return true;
        }
        else
        {
            // any assignments, not used anyway
            outTimestamp = -1;
            outSaccadeTruth = false;
            outEyeOpenessLeft = -1.0f;
            outEyeOpenessRight = -1.0f;

            outEyeDirectionLocal = new Vector3(0,0,0);
            outEyeOriginLocal = new Vector3(0, 0, 0);
            outEyeDirectionGlobal = new Vector3(0,0,0);
            outEyeOriginGlobal = new Vector3(0, 0, 0);

            outEyeDirectionLeftLocal = new Vector3(0, 0, 0);
            outEyeOriginLeftLocal = new Vector3(0, 0, 0);
            outEyeDirectionRightLocal = new Vector3(0, 0, 0);
            outEyeOriginRightLocal = new Vector3(0, 0, 0);

            outEyeDirectionLeftGlobal = new Vector3(0, 0, 0);
            outEyeOriginLeftGlobal = new Vector3(0, 0, 0);
            outEyeDirectionRightGlobal = new Vector3(0, 0, 0);
            outEyeOriginRightGlobal = new Vector3(0, 0, 0);

            if (experimentRunning)
            {
                Debug.Log("reached end of file. Simulation can be stopped.");
                experimentRunning = false;
                saccadeDetection.IsExperimentRunning(false);
                //dataLogger.IsExperimentRunning(false);
            }
            
            return false;
        }
    }

    // event invoked when saccadeDetection receives newData
    void newData ()
    {
        ReceiveData(saccadeDetection.GetTimestamp());
    }

    public void ReceiveData(int newTimestamp)
    {
        oldTimestamp = timestamp;
        timestamp = newTimestamp;
        newDataAvailable = true;
    }

    public bool IsExperimentRunning()
    {
        return experimentRunning;
    }
    #endregion

}
