using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum iPhoneModel
{
    iPhone6s_7_8,
    iPhone6sPlus_6Plus_7Plus_8Plus,
    iPhoneX_XS,
    iPhoneXR,
    iPhoneXSMax,
    iPhone11,
    iPhone11Pro,
    iPhone11ProMax,
    iPhoneSE2,
    iPhone12Mini,
    iPhone12_12Pro,
    iPhone12ProMax,
    iPhone14ProMax_14Plus_13ProMax_,
    iPhone13_13Pro_14_14Pro,
    iPhone13Mini,
    iPhoneSE3
}

public class ScreenSizeManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static ScreenSizeManager instance;
    public iPhoneModel currentModel;
    private void Awake()
    {
        // Don't destroy on load
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Retrieve the device model using SystemInfo.deviceModel
        #if UNITY_EDITOR
        #else
                string deviceModel = SystemInfo.deviceModel;
                currentModel = GetiPhoneModel(deviceModel);
        #endif
                // Log the current iPhone model
                Debug.Log("Current iPhone Model: " + currentModel);
    }

    private iPhoneModel GetiPhoneModel(string deviceModel)
    {
        Debug.Log("Device Model:" + deviceModel);
        switch (deviceModel)
        {
            case "iPhone8,1":
            case "iPhone9,1":
            case "iPhone10,1":
                return iPhoneModel.iPhone6s_7_8;
            
            case "iPhone8,2":
            case "iPhone7,1":
            case "iPhone9,2":
            case "iPhone10,5":
                return iPhoneModel.iPhone6sPlus_6Plus_7Plus_8Plus;

            case "iPhone10,6":
            case "iPhone11,2":
                return iPhoneModel.iPhoneX_XS;

            case "iPhone11,8":
                return iPhoneModel.iPhoneXR;

            case "iPhone11,4":
            case "iPhone11,6":
                return iPhoneModel.iPhoneXSMax;

            case "iPhone12,1":
                return iPhoneModel.iPhone11;

            case "iPhone12,3":
                return iPhoneModel.iPhone11Pro;

            case "iPhone12,5":
                return iPhoneModel.iPhone11ProMax;

            case "iPhone12,8":
                return iPhoneModel.iPhoneSE2;

            case "iPhone13,1":
                return iPhoneModel.iPhone12Mini;

            case "iPhone13,2":
            case "iPhone13,3":
                return iPhoneModel.iPhone12_12Pro;

            case "iPhone13,4":
                return iPhoneModel.iPhone12ProMax;
            
            case "iPhone15,3":
            case "iPhone14,8":
            case "iPhone14,3":
                return iPhoneModel.iPhone14ProMax_14Plus_13ProMax_;
            
            case "iPhone14,5":
            case "iPhone14,2":
            case "iPhone14,7":
            case "iPhone15,2":
                return iPhoneModel.iPhone13_13Pro_14_14Pro;
            
            case "iPhone14,4":
                return iPhoneModel.iPhone13Mini;
            
            case "iPhone14,6":
                return iPhoneModel.iPhoneSE3;
            
            default:
                Debug.LogWarning("Unknown device model: " + deviceModel);
                return iPhoneModel.iPhoneXR; // Default to a common iPhone size
        }
    }
}