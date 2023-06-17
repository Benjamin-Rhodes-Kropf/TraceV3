using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileDebug : MonoBehaviour
{

    public static MobileDebug Instance;

    private void Awake()
    {
        Instance = this;
    }

    //public LayerMask layerMaskForMapDetection;
}
