using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager instance;

    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void CameraHaptic()
    {
        MMVibrationManager.Haptic(HapticTypes.SoftImpact); //Todo: on input on release button
    }
    public void SelectionHaptic()
    {
        MMVibrationManager.Haptic(HapticTypes.LightImpact);
    }
    
    public void SuccessHaptic()
    {
        MMVibrationManager.Haptic(HapticTypes.Success);
    }
    public void ErrorHaptic()
    {
        MMVibrationManager.Haptic(HapticTypes.Failure);
    }
}
