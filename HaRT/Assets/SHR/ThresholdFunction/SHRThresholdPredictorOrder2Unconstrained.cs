using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHRThresholdPredictorOrder2Unconstrained : MonoBehaviour
{
    // these coefficients are the result of fitting a polynomial of order 2
    // to the thresholds obtained in the first experiment
    // fitting was done in R
    public double coefficient2 = 9.172646e-07;
    public double coefficient1 = -0.0000288204;
    public double coefficient0 = 0.0020174414;

    // for testing only
    public float debugAngle = 0;
    public bool clickHereToPrintThresholdInConsole = false;
    /// <summary>
    /// Called when the bool "clickHereToPrintThresholdInConsole" is clicked.
    /// Prints the predicted threshold for "debugAngle" in the console.
    /// </summary>
    public void OnValidate()
    {
        if (clickHereToPrintThresholdInConsole)
        {
            Debug.Log("[Debug] Approximated Detection Threshold of " + debugAngle + "° is " + ApproximateDetectionThreshold(debugAngle) + " meters.");
            clickHereToPrintThresholdInConsole = false;
        }
    }

    /// <summary>
    /// Returns an approximation of the detection threshold for hand jumps in meters.
    /// The result indicates how much the virtual hand can jump during a saccade without users noticing it.
    /// The function used to predict the threshold is a polynomial of order 2 that was fit (without 
    /// constraints on the derivatives) to the empirical threholds obtained in the first experiment.
    /// </summary>
    /// <param name="angle">angle (in degrees) between hand jump direction and saccade direction (in 2D view plane) [between 0 and 180]</param>
    /// <returns>maximum unnoticeable hand jump in meters</returns>
    public float ApproximateDetectionThreshold(float angle)
    {
        if (angle > 180)
            angle = 360 - angle;
        if (angle < 0)
            angle = angle * -1;

        return (float) (
            coefficient2 * Mathf.Pow(angle, 2) 
            + coefficient1 * Mathf.Pow(angle, 1) 
            + coefficient0 * Mathf.Pow(angle, 0));
    }
}