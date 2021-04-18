using System.Collections;
using System.Collections.Generic;
using HR_Toolkit;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public GameObject objToFollow;
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = objToFollow.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
