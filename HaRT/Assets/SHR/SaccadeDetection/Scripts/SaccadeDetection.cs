//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using ViveSR.anipal.Eye;



public class SaccadeDetection : MonoBehaviour
{
    public static SaccadeDetection instance;

    #region Inspector
    [Header("Shown Data Settings")]
    [Tooltip("true: writes angle, speed and acceleration values for current retrieved data into the console.")]
    public bool Show_PhysicalCalculations = false;
    [Tooltip("true: writes the current Update and EyeTracker frequence for each second into the console.")]
    public bool Show_Framerate = false;
    [Tooltip("true: writes 'Eyes closed' into the console whenever the EyeValue undercuts the closedEyeThreshold.")]
    public bool Show_Eye = false;

    [Header("TestMode")]
    public bool TestMode = false;
    [Tooltip("true: the .csv inputFile in !TestScenario! is used for every algorithm analysis, resulting in better comparisons.")]
    public bool SimulateInput = false;      // static bool for EyeCallback
    [Tooltip("Invokes events (saccades, blinks) with the keyboard")]
    public bool SimulateEventsWithKeyboard = false;

    [Tooltip("Saccade Detection Modes, additional to basic: Speed, Sample Threshold,..")]
    [Header("Saccade Detection Mode")]
    public bool separateEye = false;

    // Saccade Detection

    // Threshold variables in Inspector
    [Header("Saccade Detection Thresholds")]
    [SerializeField]
    [Tooltip("Speed Threshold for Saccade Detection [degrees/ second]. If eye rotation exceeds threshold then it might be a saccade")]
    [Range(50, 400)]
    int speedThreshold = 80;
    [SerializeField]
    [Tooltip("Speed Threshold for Saccade Detection [degrees/ second] which only needs to be exceeded ONCE in 3 frames.")]
    [Range(0, 500)]
    int speedThresholdOnce = 130;
    [SerializeField]
    [Tooltip("Speed Threshold above which considered measured speed as noise [degrees/ second]. If eye rotation exceeds threshold then the current sample does not increase the sample counter.")]
    [Range(400, 1000)]
    int speedNoiseThreshold = 800;
    [SerializeField]
    [Tooltip("Acceleration Threshold for Saccade Detection [degrees/ second^2]. If one of the last 3 eye rotation values exceed this threshold then it might be a saccade.")]
    [Range(0, 30000)]
    int accelerationThresholdOnce = 1000;
    [SerializeField]
    [Tooltip("How many of the most recent of all speed samples must exceed the defined speedThreshold.")]
    [Range(0, 5)]
    int minimumSamples = 2;
    [SerializeField]
    [Tooltip("For 'breakThreshold seconds' after a blink no saccades will be detected.")]
    [Range(0.0f, 0.2f)]
    float breakTimer = 0.007f;
    [SerializeField]
    [Tooltip("Threshold which determines whether the eye is interpreted as closed (if eyeOpeness < closedEyeThreshold) or not. " +
             "Eye Openess values are in the range from 0.0 (closed) to 1.0 (open)")]
    [Range(0.1f, 0.5f)]
    float closedEyeThreshold = 0.2f;


    // Visualisierung & Audiofeedback
    [Header("Visualization & Audio")]
    [SerializeField]
    [Tooltip("true: there will be no sound when a saccade/blink is detected")]
    bool muted;
    [SerializeField]
    [Tooltip("Sound is played when a blink is detected")]
    AudioClip blinkSound;
    [SerializeField]
    [Tooltip("Sound is played when a saccade is detected")]
    AudioClip saccadeSound;
    [SerializeField]
    [Tooltip("AudioSource to play sound from")]
    AudioSource myAudioSource;

    #endregion
    #region helpers

    // Analysis helpers
    Settings currentSettings;
    int onceFrameRange = 3;
    float[] lastAccelerations;
    float[] lastSpeedValues;
    int currAccIndex = 0;

    // Eye Data
    bool eye_callback_registered = false;                   // ?
    static EyeData_v2 eyeData = new EyeData_v2();           // current eye data
    
    float eyeOpenessLeft = 0.0f;                        // current eyeOpeness ranging from 0 (closed) to 1 (open)
    float eyeOpenessRight = 0.0f;                        // current eyeOpeness ranging from 0 (closed) to 1 (open)

    // combined
    Vector3 eyeOriginLocal;                                // current combined gazeOrigin in local coordinates
    Vector3 eyeDirectionLocal;                             // current combined gazeDirection in local coordinates
    Vector3 oldGazeDirectionCombinedLocal = new Vector3(0, 0, 0);   // combined gazeDirection in local coordinates from one sample before

    // left            
    Vector3 eyeOriginLeftLocal;                                // current combined gazeOrigin in local coordinates
    Vector3 eyeOriginLeftGlobal;
    Vector3 eyeDirectionLeftLocal;                             // current combined gazeDirection in local coordinates
    Vector3 eyeDirectionLeftGlobal;
    Vector3 oldEyeDirectionLeftLocal = new Vector3(0, 0, 0);   // combined gazeDirection in local coordinates from one sample before
    float speedLeft;
    float oldSpeedLeft = 0;
    float accelerationLeft;

    // right
    Vector3 eyeOriginRightLocal;                                // current combined gazeOrigin in local coordinates
    Vector3 eyeOriginRightGlobal;
    Vector3 eyeDirectionRightLocal;                             // current combined gazeDirection in local coordinates
    Vector3 eyeDirectionRightGlobal;
    Vector3 oldEyeDirectionRightLocal = new Vector3(0, 0, 0);   // combined gazeDirection in local coordinates from one sample before
    float speedRight;
    float oldSpeedRight = 0;
    float accelerationRight;


    // Frequency Data. only used for debugging
    static long callBackCounter = 0;        // counts how often the 'CallBack' method is called per second. This equals the eye tracker frequency
    long frameRateCounter = 0;              // counts how often the Update method is called per second. This equals the frame frequency
    float timer = 0;                        // resets itself every second


   

    // internal calculations
    int timestamp;              // timestamp from eyeData
    int oldTimeStamp = 0;       // timestamp from oldEyeData
    float deltaTime = 0;        // timestamp - oldTimeStamp / 1000              time difference between adjacent samples
    float angle;                // angle between oldGazeDirectionCombinedLocal and gazeDirectionCombinedLocal
    float speed = 0.0f;         // angle movement / deltaTime             unit: degrees/s       for correct unit
    float oldSpeed = 0;         // speed from one sample before
    float acceleration;         // (speed - oldSpeed) / deltaTime         unit: degrees/s^2     for correct unit
    [HideInInspector]
    public bool saccade = false;        // true when saccade is detected. false when over again. Avoids detecting still ongoing but already detected saccade.
    static int sampleCounter = 0;       // counts the number of adjacent samples which fulfill the saccade criterion. if sampleCounter > sampleThreshold --> saccade
                                        // ToDo: duplicate if left, right eye seperate !!
    static bool processed = true;       // true if current eyeData already has been checked/ processed. false if new data available. Avoids checking same samples multiple times
    float blockedTimer = 0;             // over timestamps??        // blocks SaccadeDetection after blink for breakThreshold seconds. blockedTimer reset after blink 
    [HideInInspector]
    public bool blink = false;            // true if eyes closed. if true and eye open (means eye is opening again) --> reset blockedTimer. Also avoids detecting closed eye multiple times



    // SIMULATION & EVENTS
    static bool dataNeeded = false;      // static bool for EyeCallback
    bool saccadeTruth = false;
    public UnityEvent NewDataAvailable = new UnityEvent();
    public UnityEvent SaccadeDetectionDataAvailable = new UnityEvent();
    public UnityEvent NewDataNeeded = new UnityEvent();
    public UnityEvent SaccadeOccured = new UnityEvent();
    public UnityEvent SaccadeIsOver = new UnityEvent();
    public UnityEvent BlinkOccured = new UnityEvent();
    public UnityEvent BlinkIsOver = new UnityEvent();
    bool isExperimentRunning = false;

    #endregion


    private void Start()
    {
        instance = this;

        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
        // needed for static EyeCallback
        dataNeeded = !SimulateInput;

        currentSettings = new Settings(separateEye, speedThreshold, speedThresholdOnce, speedNoiseThreshold, accelerationThresholdOnce, minimumSamples, breakTimer, closedEyeThreshold);
        lastAccelerations = new float[onceFrameRange];
        lastSpeedValues = new float[onceFrameRange];

        if (SimulateEventsWithKeyboard)
        {
            Debug.Log("leftShift saccade \n rightShift blink");
        }
    }

    private void Update()
    {
        if (SimulateEventsWithKeyboard)
        {
            checkKeyboardEvents();
            return;
        }

        manageDebugTimeFrames();
        blockedTimer += deltaTime;
        currentToOldValues();
        manageReadEyeData();

        if (!processed)
        {
            calculateEyeSpeedAndAcceleration();
            eyeStatus();
            checkSaccade();

            // only stored if logging activated or during TestMode --> press 'l'
            SaccadeDetectionDataAvailable.Invoke();
            processed = true;
        }

       
    }

    void checkKeyboardEvents()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // saccade detected
            Debug.Log("saccade");
            SaccadeOccured.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            // blink detected
            Debug.Log("blink");
            BlinkOccured.Invoke();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            // saccade detected
            //Debug.Log("saccadeIsOver");
            SaccadeIsOver.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.RightShift))
        {
            // blink detected
            //Debug.Log("blinkIsOver");
            BlinkIsOver.Invoke();
        }
    }


    #region New Eye Data
    void manageReadEyeData()
    {
        if (SimulateInput)
        {
            if (processed)
            {
                simulateReadEyeData();
            }
            if (!processed)
            {
                NewDataAvailable.Invoke();
            }
        }
        else
        {
            readEyeData();
            if (!processed)
            {
                NewDataAvailable.Invoke();
            }
        }
    }

    void readEyeData()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        if (!processed)
        {
            if (eye_callback_registered)
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out eyeOriginLocal, out eyeDirectionLocal, eyeData))
                {
                    //SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out eyeRay, eyeData);
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out eyeOriginLocal, out eyeDirectionLocal, eyeData))
                {
                    Debug.Log("LEFT instead of combined");
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out eyeOriginLocal, out eyeDirectionLocal, eyeData))
                {
                    Debug.Log("RIGHT instead of combined");
                }
            }
            else
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out eyeOriginLocal, out eyeDirectionLocal))
                {
                    //SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out eyeRay, eyeData);
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out eyeOriginLocal, out eyeDirectionLocal))
                {
                    Debug.Log("LEFT instead of combined");
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out eyeOriginLocal, out eyeDirectionLocal))
                {
                    Debug.Log("RIGHT instead of combined");
                }
            }
            // needed for SeparateEyeDetection and PlotSeparateGazeRay
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out eyeOriginLeftLocal, out eyeDirectionLeftLocal, eyeData);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out eyeOriginRightLocal, out eyeDirectionRightLocal, eyeData);

            timestamp = eyeData.timestamp;
        }
    }

    Vector3 eyeDirectionGlobal;
    Vector3 eyeOriginGlobal;
    void simulateReadEyeData()
    {
        if (processed)
        {
            NewDataNeeded.Invoke();
        }
        else
        {
            Debug.LogWarning("unprocessed data available. This should not happen!");
        }
    }


    static void EyeCallback(ref EyeData_v2 eye_data)
    {
        if (dataNeeded)
        {
            eyeData = eye_data;
            callBackCounter++;
            processed = false;
        }
    }

    #endregion

    #region Speed, Acceleration

    /// <summary>
    /// manages different kinds of calculations
    /// (Basic, Seperated Eye)
    /// </summary>
    void calculateEyeSpeedAndAcceleration()
    {
        if (!separateEye)
        {
            calculateEyeSpeedAndAcceleration(eyeDirectionLocal, oldGazeDirectionCombinedLocal, oldSpeed);
            storeSpeed(speed);
            storeAcceleration(acceleration);
        }
        else
        {
            calculateEyeSpeedAndAcceleration(eyeDirectionLeftLocal, oldEyeDirectionLeftLocal, oldSpeedLeft);
            speedLeft = speed;
            accelerationLeft = acceleration;
            calculateEyeSpeedAndAcceleration(eyeDirectionRightLocal, oldEyeDirectionRightLocal, oldSpeedRight);
            speedRight = speed;
            accelerationRight = acceleration;
            storeSpeed(speedLeft, speedRight);
            storeAcceleration(accelerationLeft, accelerationRight);
        }
    }

    // look @post regarding eye_data saccade calculation --> methods
    // 'normal' saccade speed: 300-400°/s, acceleration > 1000°/s^2
    // occur every 300-400ms, last about: 20-200ms
    void calculateEyeSpeedAndAcceleration(Vector3 gazeDirectionLocal, Vector3 oldGazeDirectionLocal, float oldSpeed)
    {
        deltaTime = timestamp - oldTimeStamp;
        deltaTime /= 1000.0f;    // ms in s
        if (deltaTime == 0)
        {
            Debug.LogWarning("timestamp == oldTimestamp, this should not happen!");
        } 

        angle = Vector3.Angle(gazeDirectionLocal, oldGazeDirectionLocal);           // returns angle between 0° and 180°
        speed = angle / deltaTime;                           // in degrees/ s

        acceleration = (speed - oldSpeed) / deltaTime;       // in degrees/ s^2
        // sign(speed) == sign(acc) --> speed up, else slow down 
        
        if (Show_PhysicalCalculations)
        {
            Debug.Log("Timestamp: " + timestamp + "\n" +
                      "Angle: " + angle + "\n" +
                      "Speed: " + speed + "\n" +
                      "Acceleration: " + acceleration);
        }
    }

    #endregion

    #region Eye

    // SRanipal in ReadEyeData oder if case
    void eyeStatus()
    {
        if (!SimulateInput)
        {
            // read EyeOpeness
            SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.LEFT, out eyeOpenessLeft, eyeData);
            SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.RIGHT, out eyeOpenessRight, eyeData);
        }
       
        // eye closes
        if (areBothEyesClosed() && !blink)
        {
            if (TestMode && isExperimentRunning || !TestMode || SimulateInput)
            {
                Debug.Log("Blink detected");
            }
            blink = true;
            // end saccade first
            if (saccade)
            {
                SaccadeIsOver.Invoke();
                saccade = false;
            }
            // trigger blink
            BlinkOccured.Invoke();

            if (!muted && blinkSound != null)
            {
                // UNDO ONLY FOR TESTING
                playAudio(blinkSound);
            }
        }
        // eye opens
        else if (!areBothEyesClosed() && blink)
        {
            blink = false;
            BlinkIsOver.Invoke();
        }

        if (!areBothEyesOpen())
        {
            // pause saccadeDetection
            blockedTimer = 0;
            sampleCounter = 0;
        }
    }

    bool areBothEyesOpen()
    {
        return (eyeOpenessLeft >= closedEyeThreshold && eyeOpenessRight >= closedEyeThreshold);
    }

    bool areBothEyesClosed()
    {
        return (eyeOpenessLeft < closedEyeThreshold && eyeOpenessRight < closedEyeThreshold);
    }

    #endregion

    #region Saccade Detection

    /// <summary>
    /// Returns true if conditions are met (such as: speed, acceleration > 0, samples)
    /// handles basic and separateEye conditions
    /// </summary>
    /// <param name="speedVal"></param>
    /// <param name="accVal"></param>
    /// <returns></returns>
    void checkSaccade()
    {
        // blink: SaccadeDetection blocked until eyes open again + breakThreshold

        if (areBothEyesOpen() && blockedTimer > breakTimer)
        {
            
            // BasicCondition or SeparateCondition
            if ((!separateEye && basicSaccadeCondition()) || (separateEye && separateSaccadeCondition()))
            {
                // might be a Saccade because of eye movement
                sampleCounter++;
                // sample duration?
                // check onceSpeed and onceAcc
                if (sampleCounter >= minimumSamples && onceAccSaccadeCondition() && onceSpeedSaccadeCondition())
                {
                    saccadeOccurred();
                }
            }
            // no Saccade/ Saccade is over
            else if ((!separateEye && basicNoSaccadeCondition()) || (separateEye && separateNoSaccadeCondition()))
            {
                saccadeEnded();
            }
        }
        // no Saccade/ Saccade is over
        else
        {
            saccadeEnded();
        }
    }


    #region Help Methods Saccade Detection
    bool basicSaccadeCondition()
    {
        return ((speed >= speedThreshold) && (speed < speedNoiseThreshold));
    }

    // overthink again if there is a better NoSaccadeCondition
    bool basicNoSaccadeCondition()
    {
        return (speed < speedThreshold);
    }

    bool separateSaccadeCondition()
    {
        return (((speedLeft >= speedThreshold) && (speedRight >= speedThreshold)) && ((speedLeft < speedNoiseThreshold) && (speedRight < speedNoiseThreshold)));
    }

    // overthink again if there is a better NoSaccadeCondition
    bool separateNoSaccadeCondition()
    {
        return ((speedLeft < speedThreshold) && (speedRight < speedThreshold));
    }

    /// <summary>
    /// returns true if one of the last accelerations exceeded the accThreshold
    /// separateEye is handled in storeAcc
    /// </summary>
    /// <returns></returns>
    bool onceAccSaccadeCondition()
    {
        foreach (float oneAcc in lastAccelerations)
        {
            if (oneAcc > accelerationThresholdOnce) return true;
        }
        return false;
    }

    /// <summary>
    /// returns true if one of the last speedValues exceeded the onceSpeedThreshold
    /// separateEye is handled in storeSpeed
    /// </summary>
    /// <returns></returns>
    bool onceSpeedSaccadeCondition()
    {
        foreach (float oneSpeed in lastSpeedValues)
        {
            if (oneSpeed > speedThresholdOnce) return true;
        }
        return false;
    }

       
    void saccadeOccurred()
    {
        if (!saccade)
        {
            if (TestMode && isExperimentRunning || !TestMode || SimulateInput)
            {
                Debug.Log("Saccade detected" + "\n" +
                                timestamp);
            }
            saccade = true;
            if (!muted && saccadeSound != null)
            {
                playAudio(saccadeSound);
            }
            SaccadeOccured.Invoke();
        }
    }

    void saccadeEnded()
    {
        if (saccade)
        {
            SaccadeIsOver.Invoke();
        }
        saccade = false;
        sampleCounter = 0;
    }


    #endregion
    #endregion

    #region Current To Old

    /// <summary>
    /// current values to old values (Gaze, timestamp, ...)
    /// </summary>
    void currentToOldValues()
    {
        if (oldTimeStamp == timestamp)
        {
            //processed = true;
        }
        oldTimeStamp = timestamp;

        if (separateEye)
        {
            oldEyeDirectionLeftLocal = eyeDirectionLeftLocal;
            oldSpeedLeft = speedLeft;
            oldEyeDirectionRightLocal = eyeDirectionRightLocal;
            oldSpeedRight = speedRight;
        }
        else
        {
            oldGazeDirectionCombinedLocal = eyeDirectionLocal;
            oldSpeed = speed;
            oldGazeDirectionCombinedLocal = eyeDirectionLocal;
        }
    }
    #endregion

    #region Small Help Methods

    void manageDebugTimeFrames()
    {
        if (Show_Framerate)
        {
            frameRateCounter++;
            checkTime();
        }
        else
        {
            // reset variable regulary when not needed to avoid overflow
            // can not be prevented in EyeCallBack since static function & non-static Debug_Framerate variable
            callBackCounter = 0;
        }
    }

    void checkTime()
    {
        timer += Time.deltaTime;
        if (timer >= 1.0)
        {
            Debug.Log("Eye Tracker: " + callBackCounter);
            Debug.Log("Update: " + frameRateCounter);
            callBackCounter = 0;
            timer = 0;
            frameRateCounter = 0;
        }
    }

    public void playAudio(AudioClip clip = null)
    {
        if (myAudioSource != null)
        {
            if (clip == null)
            {
                myAudioSource.PlayOneShot(saccadeSound);
            }
            else
            {
                myAudioSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning("If you want to play a detection sound make sure the 'Visualization&Header' category has everything thats needed");
        }
    }

    #region store values for once in a range criteria

    void storeAcceleration(float currAcc)
    {
        if (currAccIndex >= onceFrameRange)
        {
            currAccIndex = 0;
        }
        lastAccelerations[currAccIndex] = currAcc;
        currAccIndex++;
    }

    void storeAcceleration(float leftAcc, float rightAcc)
    {
        if (leftAcc <= rightAcc) storeAcceleration(leftAcc);
        else storeAcceleration(rightAcc);
    }


    void storeSpeed(float currSpeed)
    {
        if (currAccIndex >= onceFrameRange)
        {
            currAccIndex = 0;
        }
        lastSpeedValues[currAccIndex] = currSpeed;
    }

    void storeSpeed(float leftSpeed, float rightSpeed)
    {
        if (leftSpeed <= rightSpeed) storeSpeed(leftSpeed);
        else storeSpeed(rightSpeed);
    }

    #endregion
    #endregion

    #region Public Methods

    public bool GetSaccadeDetectionStatus()
    {
        return saccade;
    }

    public bool GetSimulateInput()
    {
        return SimulateInput;
    }

    public int GetTimestamp()
    {
        return timestamp;
    }

    public bool GetProcessed()
    {
        return processed;
    }

    public void SendNewSimulationData(out int outTimestamp, out Vector3 outEyeDirectionGlobal, out Vector3 outEyeOriginGlobal, out Vector3 outEyeDirectionLeftGlobal,
           out Vector3 outEyeOriginLeftGlobal, out Vector3 outEyeDirectionRightGlobal, out Vector3 outEyeOriginRightGlobal)
    {
        outTimestamp = timestamp;
        outEyeDirectionGlobal = eyeDirectionGlobal;
        outEyeOriginGlobal = eyeOriginGlobal;
        outEyeDirectionLeftGlobal = eyeDirectionLeftGlobal;
        outEyeOriginLeftGlobal = eyeOriginLeftGlobal;
        outEyeDirectionRightGlobal = eyeDirectionRightGlobal;
        outEyeOriginRightGlobal = eyeOriginRightGlobal;
    }

    public void SendNewData(out int outTimestamp, out Vector3 outEyeDirectionCombinedLocal, out Vector3 outEyeOriginCombinedLocal, out Vector3 outEyeDirectionLeftLocal, out Vector3 outEyeOriginLeftLocal, out Vector3 outEyeDirectionRightLocal, out Vector3 outEyeOriginRightLocal)
    {
        outTimestamp = timestamp;
        outEyeDirectionCombinedLocal = eyeDirectionLocal;
        outEyeOriginCombinedLocal = eyeOriginLocal;
        outEyeDirectionLeftLocal = eyeDirectionLeftLocal;
        outEyeOriginLeftLocal = eyeOriginLeftLocal;
        outEyeDirectionRightLocal = eyeDirectionRightLocal;
        outEyeOriginRightLocal = eyeOriginRightLocal;
    }

    public void GetSimFrame(int newTimestamp, bool newSaccadeTruth, float newEyeOpenessLeft, float newEyeOpenessRight,
                Vector3 newEyeDirectionLocal, Vector3 newEyeOriginLocal, Vector3 newEyeDirectionGlobal, Vector3 newEyeOriginGlobal,
                Vector3 newEyeDirectionLeftLocal, Vector3 newEyeOriginLeftLocal, Vector3 newEyeDirectionRightLocal, Vector3 newEyeOriginRightLocal,
                Vector3 newEyeDirectionLeftGlobal, Vector3 newEyeOriginLeftGlobal, Vector3 newEyeDirectionRightGlobal, Vector3 newEyeOriginRightGlobal)
    {
        timestamp = newTimestamp;
        saccadeTruth = newSaccadeTruth;
        eyeDirectionLocal = newEyeDirectionLocal;
        eyeOriginLocal = newEyeOriginLocal;
        eyeDirectionGlobal = newEyeDirectionGlobal;
        eyeOriginGlobal = newEyeOriginGlobal;
        eyeOpenessLeft = newEyeOpenessLeft;
        eyeOpenessRight = newEyeOpenessRight;
        eyeDirectionLeftLocal = newEyeDirectionLeftLocal;
        eyeOriginLeftLocal = newEyeOriginLeftLocal;
        eyeDirectionRightLocal = newEyeDirectionRightLocal;
        eyeOriginRightLocal = newEyeOriginRightLocal;
        eyeDirectionLeftGlobal = newEyeDirectionLeftGlobal;
        eyeOriginLeftGlobal = newEyeOriginLeftGlobal;
        eyeDirectionRightGlobal = newEyeDirectionRightGlobal;
        eyeOriginRightGlobal = newEyeOriginRightGlobal;
        // new data available
        processed = false;
    }

    public void IsExperimentRunning (bool truefalse)
    {
        isExperimentRunning = truefalse;
    }

    public string SendSettings(string NEXT_ROW, string DELIMITER)
    {
        return ("SETTINGS:" + NEXT_ROW +
        "Saccade Detection Mode:" + DELIMITER + "Separate Eye" + DELIMITER + separateEye + NEXT_ROW +
        "Saccade Detection Thresholds:" + DELIMITER + "Speed" + DELIMITER + speedThreshold + DELIMITER + "Noise" + DELIMITER + speedNoiseThreshold + DELIMITER + "Acceleration" + DELIMITER + accelerationThresholdOnce + DELIMITER + "Sample" + DELIMITER + minimumSamples + DELIMITER + "Break" + DELIMITER + breakTimer + DELIMITER + "Closed Eye" + DELIMITER + closedEyeThreshold + NEXT_ROW
        + "Simulation: " + DELIMITER + SimulateInput + NEXT_ROW
        + "AllowedRange: " + DELIMITER);
    }

    public Settings GetSettings()
    {
        return currentSettings;
    }

    public void SendLoggingData(out int outTimestamp, out float outSpeed, out float outAcceleration, out float outEyeOpenessLeft, out float outEyeOpenessRight, out Vector3 outEyeDirectionLocal, out Vector3 outEyeOriginLocal, out Vector3 outEyeDirectionLeftLocal, out Vector3 outEyeOriginLeftLocal, out Vector3 outEyeDirectionRightLocal, out Vector3 outEyeOriginRightLocal)
    {
        outTimestamp = timestamp;
        outSpeed = speed;
        outAcceleration = acceleration;
        outEyeOpenessLeft = eyeOpenessLeft;
        outEyeOpenessRight = eyeOpenessRight;
        outEyeDirectionLocal = eyeDirectionLocal;
        outEyeOriginLocal = eyeOriginLocal;
        outEyeDirectionLeftLocal = eyeDirectionLeftLocal;
        outEyeOriginLeftLocal = eyeOriginLeftLocal;
        outEyeDirectionRightLocal = eyeDirectionRightLocal;
        outEyeOriginRightLocal = eyeOriginRightLocal;
    }

    #endregion

    public struct Settings {
    
        public Settings(bool separateEyeMode, int speedThreshold, int onceSpeedThreshold, int speedNoiseThreshold, int accThreshold, int sampleThreshold, float breakThreshold, float closedEyeThreshold)
        {
            SeparateEye = separateEyeMode;
            Speed = speedThreshold;
            OnceSpeed = onceSpeedThreshold;
            SpeedNoise = speedNoiseThreshold;
            Acceleration = accThreshold;
            Sample = sampleThreshold;
            Break = breakThreshold;
            ClosedEye = closedEyeThreshold;
        }

        public bool SeparateEye;
        public int Speed;
        public int OnceSpeed;
        public int SpeedNoise;
        public int Acceleration;
        public int Sample;
        public float Break;
        public float ClosedEye;
    }


}
