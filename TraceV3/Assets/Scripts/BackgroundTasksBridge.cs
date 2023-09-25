using System.Runtime.InteropServices;
using UnityEngine;

public class BackgroundTasksBridge : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SetNativePlayerPrefs(string key, string value);

    [DllImport("__Internal")]
    private static extern void SetLocationToMonitor(float latitude, float longitude);

    public static BackgroundTasksBridge Instance { get; set; }



    private int counter = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        //LT 31.5096497
        //LN 74.3459482

        // InvokeRepeating(nameof(TestNativeCode),2f,2f);
    }


    private void TestNativeCode()
    {
        // UpdateLocations();
        SendDataToiOS("TraceData_Native","This  will  be the value number ::"+ counter);
    }

    public void SendLocationToMonitor(float latitude, float longitude)
    {
#if UNITY_EDITOR
        Debug.Log("Please Switch To IOS Device To get this work");
#elif UNITY_IOS
        SetLocationToMonitor(latitude,longitude);
#endif
    }
    
    
    private void SendDataToiOS(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
#if UNITY_EDITOR
        Debug.Log("Please Switch To IOS Device To get this work");
#elif UNITY_IOS
        SetNativePlayerPrefs(key,value);
        SetLocationToMonitor(31.5096497f,74.3459482f );
#endif
    }


    public void UpdateLocations( )
    {
// #if UNITY_EDITOR
//         Debug.Log("Please Switch To IOS Device To get this work");
// #elif UNITY_IOS
//         DesiredLocationToMonitor(31.5096497f,74.3459482f );
// #endif  
    }
}