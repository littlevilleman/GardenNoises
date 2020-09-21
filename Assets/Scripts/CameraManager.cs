using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
   public  Animator cameraAnimator;

    public static CameraManager instance;

    void Awake()
    {
        //singleton
        if (instance != null)
            Destroy(this);

        instance = this;

        cameraAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
    }
    
}
