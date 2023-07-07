using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class MobileDebug : MonoBehaviour
{
    public static MobileDebug Instance;
    [SerializeField]private static bool debuggerEnabled = false;
    private void Awake()
    {
        if (debuggerEnabled)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Debug Disabled");
        }
    }

    //public LayerMask layerMaskForMapDetection;
}
