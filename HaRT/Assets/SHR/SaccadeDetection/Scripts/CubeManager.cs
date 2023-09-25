using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;


public class CubeManager : MonoBehaviour
{
    // INSPECTOR
    [Header("Required")]
    [Tooltip("Material for the higlighted cube")]
    [SerializeField] Material highlight;
    [Tooltip("Material for the standard cube")]
    [SerializeField] Material standard;
    [SerializeField] SaccadeDetection saccadeDetection;
    [Tooltip("LineRenderer from the scene needed. (f.e. GazeRaySample_v2 from SDK")]
    [SerializeField] LineRenderer gazeRayRenderer;
    [Tooltip("LineRenderer from the scene needed. Has to be different from the one above")]
    [SerializeField] LineRenderer gazeRayRendererSeparate;

    [Header("Settings")]
    [Tooltip("Number of frames the cube has to be focused until the next one is higlighted")]
    [SerializeField] [Range(0, 300)] int MinimumHitDuration = 10;
    [Tooltip("true: plots GazeRay, either combined or separate depending on seperateEye value")]
    [SerializeField] bool plotGazeRay = false;
    /* [Tooltip("true: plots GazeRay of left and right eye")]
     [SerializeField] bool plotSeparateGazeRay = false;*/

    [Header("Debug Settings")]
    public bool DEBUG_Cubes = false;


    // Internal Variables
    [HideInInspector] public int numCubes;      // total number of cubes
    int currentNum;                             // current cube number
    int nextNum;                                // next cube number
    int timestamp;                              // timestamp from eyeData
    int oldTimestamp;                           // timestamp from eyeData once before
    bool newDataAvailable = false;
    bool simulateInput = false;

    Vector3 eyeDirectionLocal;                  // local combined eye direction
    Vector3 eyeDirectionLeftLocal;              // local left
    Vector3 eyeDirectionRightLocal;             // local right
    Vector3 eyeDirectionGlobal;                       // world combined eye direction
    Vector3 eyeDirectionLeftGlobal;                   // world left
    Vector3 eyeDirectionRightGlobal;                  // world right

    Vector3 eyeOriginLocal;                     // local combined eye origin
    Vector3 eyeOriginLeftLocal;                 // local left
    Vector3 eyeOriginRightLocal;                // local right
    Vector3 eyeOriginGlobal;                          // world combined eye origin
    Vector3 eyeOriginLeftGlobal;                      // world left
    Vector3 eyeOriginRightGlobal;                     // world right
    
    RaycastHit hit;                             // collision information from combined eye gaze
    RaycastHit hitLeft;                         // "" left eye gaze
    RaycastHit hitRight;                        // "" right eye gaze
    int currentHitDuration = 0;                 // counts sequential eye gaze collision with current cube


    // Start is called before the first frame update
    void Start()
    {
        numCubes = transform.childCount;        // store number of cubes
        simulateInput = saccadeDetection.GetSimulateInput();
        if (!saccadeDetection.TestMode && !simulateInput)
        {
            nextNum = Random.Range(0, numCubes);
            SetCube(transform.GetChild(nextNum).gameObject);
            currentNum = nextNum;
        }

        if (saccadeDetection.GetSimulateInput())
        {
            saccadeDetection.NewDataAvailable.AddListener(newSimulationData);
        }
        else
        {
            saccadeDetection.NewDataAvailable.AddListener(newData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (newDataAvailable)
        {
            if (!simulateInput)
            {
                calculateNewGlobals();

                if (!saccadeDetection.TestMode)
                {
                    basicMode();
                }
            }
            manageGazeRayPlotting();
        }
    }


    // After focusing higlighted cube for the given duration, a next random cube is higlighted
    private void basicMode()
    {
        if (CheckCollisionDuration())
        {
            RandomNextCube();
            if (DEBUG_Cubes)
            {
                Debug.Log("Hit");
            }
        }
    }

    /// <summary>
    /// manages single combined gazeRay and also separate gazeRay consisting of two rays
    /// </summary>
    private void manageGazeRayPlotting()
    {
        if (plotGazeRay)
        {
            if (!saccadeDetection.separateEye)
            {
                // plots single combined gazeRay
                gazeRayRenderer.SetPosition(0, eyeOriginGlobal - Vector3.up * 0.02f);
                gazeRayRenderer.SetPosition(1, eyeOriginGlobal + eyeDirectionGlobal);
                gazeRayRendererSeparate.enabled = false;
            }
            else
            {
                // separateEye
                gazeRayRenderer.SetPosition(0, eyeOriginLeftGlobal - Vector3.up * 0.02f);
                gazeRayRenderer.SetPosition(1, eyeOriginLeftGlobal + eyeDirectionLeftGlobal);
                gazeRayRendererSeparate.SetPosition(0, eyeOriginRightGlobal - Vector3.up * 0.02f);
                gazeRayRendererSeparate.SetPosition(1, eyeOriginRightGlobal + eyeDirectionRightGlobal);
            }
        }
        else
        {
            gazeRayRenderer.enabled = false;
            gazeRayRendererSeparate.enabled = false;
        }
    }

    #region Cube Coloring

    /// <summary>
    /// resets old cube, and sets a randomly chosen new cube
    /// </summary>
    private void RandomNextCube()
    {
        while (currentNum == nextNum)
        {
            nextNum = Random.Range(0, numCubes);
        }
        SetCube(transform.GetChild(nextNum).gameObject);
        ResetCube(transform.GetChild(currentNum).gameObject);
        currentNum = nextNum;
    }

    public void SetCube(GameObject cube)
    {
        cube.GetComponent<Renderer>().material = highlight;
        // colour, on/ off etc
    }

    /// <summary>
    /// sets the cubeID GO to the highlight color 
    /// </summary>
    /// <param name="cubeID"></param>
    public void SetCube(int cubeID)
    {
        SetCube(transform.GetChild(cubeID).gameObject);
        if (DEBUG_Cubes)
        {
            Debug.Log(transform.GetChild(cubeID).gameObject.name);
        }
    }

    public void ResetCube(GameObject cube)
    {
        cube.GetComponent<Renderer>().material = standard;
    }

    /// <summary>
    /// resets the cubeID GO to the standard color again
    /// </summary>
    /// <param name="cubeID"></param>
    public void ResetCube(int cubeID)
    {
        ResetCube(transform.GetChild(cubeID).gameObject);
    }

    #endregion

    #region Collision

    /// <summary>
    /// checks EyeGaze collision with currentID
    /// </summary>
    /// <returns>true if collision with current cube found </returns>
    public bool CheckCollision(int currentID)
    {
        // (!saccadeDetection.SeparateEye && collisionBasicCondition()) || (saccadeDetection.SeparateEye && collisionSeparateEyeCondition()))
        if (collisionBasicCondition()) 
        {
            // (!saccadeDetection.SeparateEye && checkBasicCollision(currentID)) || (saccadeDetection.SeparateEye && checkSeparateEyeCollision(currentID))
            if (checkBasicCollision(currentID))
            {
                return true;
            }
        }
        return false;
    }

    private bool collisionBasicCondition()
    {
        return Physics.Raycast(eyeOriginGlobal, eyeDirectionGlobal, out hit);
    }
    private bool checkBasicCollision(int currentID)
    {
        return hit.transform.name.Equals(transform.GetChild(currentID).name);
    }

    private bool collisionSeparateEyeCondition()
    {
        return ((Physics.Raycast(eyeOriginLeftGlobal, eyeDirectionLeftGlobal, out hitLeft)) && Physics.Raycast(eyeOriginRightGlobal, eyeDirectionRightGlobal, out hitRight));
    }

    private bool checkSeparateEyeCollision(int currentID)
    {
        return ((hitLeft.transform.name.Equals(transform.GetChild(currentID).name)) && (hitRight.transform.name.Equals(transform.GetChild(currentID).name)));
    }


    /// <summary>
    /// checks whether EyeGaze collision with currentID is lasting 'minimumDuration' frames long
    /// </summary>
    /// <returns>true if collision lasted 'minimumDuration' frames</returns>
    public bool CheckCollisionDuration(int minimumDuration, int currentID, int currentTimestamp)
    {
        if (CheckCollision(currentID))
        {
            currentHitDuration += timestamp - oldTimestamp;
        }
        else
        {
            currentHitDuration = 0;
        }
        
        if (minimumDuration <= currentHitDuration)
        {
            currentHitDuration = 0;
            return true;
        }
        return false;
    }

    private bool CheckCollisionDuration()
    {
        return CheckCollisionDuration(MinimumHitDuration, currentNum, timestamp);
    }

    #endregion

    private void calculateNewGlobals()
    {
        // combined
        eyeDirectionGlobal = Camera.main.transform.TransformDirection(eyeDirectionLocal);
        eyeOriginGlobal = Camera.main.transform.TransformPoint(eyeOriginLocal);

        // left, right
        eyeDirectionLeftGlobal = Camera.main.transform.TransformDirection(eyeDirectionLeftLocal);
        eyeDirectionRightGlobal = Camera.main.transform.TransformDirection(eyeDirectionRightLocal);
        eyeOriginLeftGlobal = Camera.main.transform.TransformPoint(eyeOriginLeftLocal);
        eyeOriginRightGlobal = Camera.main.transform.TransformPoint(eyeOriginRightLocal);
    }

    #region Getter

    public Vector3 GetEyeDirectionGlobal()
    {
        return eyeDirectionGlobal;
    }

    public Vector3 GetEyeOriginGlobal()
    {
        return eyeOriginGlobal;
    }

    public Vector3 GetEyeDirectionLeftGlobal()
    {
        return eyeDirectionLeftGlobal;
    }

    public Vector3 GetEyeDirectionRightGlobal()
    {
        return eyeDirectionRightGlobal;
    }

    public Vector3 GetEyeOriginLeftGlobal()
    {
        return eyeOriginLeftGlobal;
    }

    public Vector3 GetEyeOriginRightGlobal()
    {
        return eyeOriginRightGlobal;
    }

    #endregion

    #region Receive Data

    void newSimulationData ()
    {
        oldTimestamp = timestamp;

        saccadeDetection.SendNewSimulationData(out timestamp, out eyeDirectionGlobal, out eyeOriginGlobal, out eyeDirectionLeftGlobal, 
            out eyeOriginLeftGlobal, out eyeDirectionRightGlobal, out eyeOriginRightGlobal);
        newDataAvailable = true;
    }

    // Simulation
    public void ReceiveSimulationData(int newTimestamp, Vector3 newEyeDirectionGlobal, Vector3 newEyeOriginGlobal, Vector3 newEyeDirectionLeftGlobal, Vector3 newEyeOriginLeftGlobal , Vector3 newEyeDirectionRightGlobal, Vector3 newEyeOriginRightGlobal)
    {
        oldTimestamp = timestamp;
        timestamp = newTimestamp;
        eyeDirectionGlobal = newEyeDirectionGlobal;
        eyeOriginGlobal = newEyeOriginGlobal;
        eyeDirectionLeftGlobal = newEyeDirectionLeftGlobal;
        eyeOriginLeftGlobal = newEyeOriginLeftGlobal;
        eyeDirectionRightGlobal = newEyeDirectionRightGlobal;
        eyeOriginRightGlobal = newEyeOriginRightGlobal;
        newDataAvailable = true;
    }

    void newData()
    {
        oldTimestamp = timestamp;
        saccadeDetection.SendNewData(out timestamp, out eyeDirectionLocal, out eyeOriginLocal, out eyeDirectionLeftLocal, out eyeOriginLeftLocal, out eyeDirectionRightLocal, out eyeOriginRightLocal);
        newDataAvailable = true;
    }

    // normal
    // local values used to calculate its global ones 
    public void ReceiveData(int newTimestamp, Vector3 newEyeDirectionLocal, Vector3 newEyeOriginLocal, Vector3 newEyeDirectionLeftLocal, Vector3 newEyeOriginLeftLocal, Vector3 newEyeDirectionRightLocal, Vector3 newEyeOriginRightLocal)
    {
        oldTimestamp = timestamp;
        timestamp = newTimestamp;
        eyeDirectionLocal = newEyeDirectionLocal;
        eyeOriginLocal = newEyeOriginLocal;
        eyeDirectionLeftLocal = newEyeDirectionLeftLocal;
        eyeOriginLeftLocal = newEyeOriginLeftLocal;
        eyeDirectionRightLocal = newEyeDirectionRightLocal;
        eyeOriginRightLocal = newEyeOriginRightLocal;
        newDataAvailable = true;
    }

    public void SendGlobalLoggingData(out Vector3 outEyeDirectionGlobal, out Vector3 outEyeOriginGlobal, out Vector3 outEyeDirectionLeftGlobal, out Vector3 outEyeDirectionRightGlobal, out Vector3 outEyeOriginLeftGlobal, out Vector3 outEyeOriginRightGlobal)
    {
        outEyeDirectionGlobal = eyeDirectionGlobal;
        outEyeOriginGlobal = eyeOriginGlobal;
        outEyeDirectionLeftGlobal = eyeDirectionLeftGlobal;
        outEyeOriginLeftGlobal = eyeOriginLeftGlobal;
        outEyeDirectionRightGlobal = eyeDirectionRightGlobal;
        outEyeOriginRightGlobal = eyeOriginRightGlobal;
    }


    #endregion
}
