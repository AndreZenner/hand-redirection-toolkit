using UnityEngine;

/// <summary>
/// This is an example script for reading the saccade detection data. 
/// Here we access the data via the two public variables saccade and blink.
/// In this case, the output occurs as long as the saccade/ blink is lasting.
/// </summary>

public class ExampleScript1 : MonoBehaviour
{
    [Tooltip("The currently used Saccade Detection")]
    public SaccadeDetection saccadeDetection;
    

    // Update is called once per frame
    void Update()
    {
        if (saccadeDetection.saccade)
        {
            Debug.Log("saccade");
        }
        
        if (saccadeDetection.blink)
        {
            Debug.Log("blink");
        }
    }
}
