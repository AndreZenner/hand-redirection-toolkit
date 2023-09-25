using UnityEngine;

/// <summary>
/// This is an example script for reading the saccade detection data. 
/// Here we access the data via the saccade detection events.
/// In this case the output occurs for each saccade/blink onset/end.
/// </summary>

public class ExampleScript2 : MonoBehaviour
{
    // accessing the saccade values
    public void saccadeOccured()
    {
        Debug.Log("saccade occured");
    }
    
    public void saccadeIsOver()
    {
        Debug.Log("saccade is over");
    }
    
    // accessing the blink values
    public void blinkOccured()
    {
        Debug.Log("blink occured");
    }
    
    public void blinkIsOver()
    {
        Debug.Log("blink is over");
    }
}
