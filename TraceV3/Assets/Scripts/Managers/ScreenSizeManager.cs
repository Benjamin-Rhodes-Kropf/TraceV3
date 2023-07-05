using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum iPhoneModel
{
    iPhone4_4S,
    iPhone5_5S_SE,
    iPhone6_6S_7_8,
    iPhone6Plus_6SPlus_7Plus_8Plus,
    iPhoneX_XS,
    iPhoneXR,
    iPhoneXSMax,
    iPhone11,
    iPhone11Pro,
    iPhone11ProMax,
    iPhoneSE2,
    iPhone12Mini,
    iPhone12_12Pro,
    iPhone12ProMax
}

public class ScreenSizeManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static ScreenSizeManager instance;
    public iPhoneModel currentModel;
    public iPhoneModel simPhoneModel;
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
        string deviceModel = SystemInfo.deviceModel;
        currentModel = GetiPhoneModel(deviceModel);

        // Log the current iPhone model
        Debug.Log("Current iPhone Model: " + currentModel);
    }

    private iPhoneModel GetiPhoneModel(string deviceModel)
    {
        switch (deviceModel)
        {
            case "iPhone 4":
            case "iPhone 4S":
                return iPhoneModel.iPhone4_4S;

            case "iPhone 5":
            case "iPhone 5S":
            case "iPhone SE":
                return iPhoneModel.iPhone5_5S_SE;

            case "iPhone 6":
            case "iPhone 6S":
            case "iPhone 7":
            case "iPhone 8":
                return iPhoneModel.iPhone6_6S_7_8;

            case "iPhone 6 Plus":
            case "iPhone 6S Plus":
            case "iPhone 7 Plus":
            case "iPhone 8 Plus":
                return iPhoneModel.iPhone6Plus_6SPlus_7Plus_8Plus;

            case "iPhone X":
            case "iPhone XS":
                return iPhoneModel.iPhoneX_XS;

            case "iPhone XR":
                return iPhoneModel.iPhoneXR;

            case "iPhone XS Max":
                return iPhoneModel.iPhoneXSMax;

            case "iPhone 11":
                return iPhoneModel.iPhone11;

            case "iPhone 11 Pro":
                return iPhoneModel.iPhone11Pro;

            case "iPhone 11 Pro Max":
                return iPhoneModel.iPhone11ProMax;

            case "iPhone SE 2":
                return iPhoneModel.iPhoneSE2;

            case "iPhone 12 Mini":
                return iPhoneModel.iPhone12Mini;

            case "iPhone 12":
            case "iPhone 12 Pro":
                return iPhoneModel.iPhone12_12Pro;

            case "iPhone 12 Pro Max":
                return iPhoneModel.iPhone12ProMax;

            default:
                Debug.LogWarning("Unknown device model: " + deviceModel);
                return iPhoneModel.iPhoneXR; // Default to a common iPhone size
        }
    }

    // Start is called before the first frame update
    void Start()
        
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}