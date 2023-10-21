using System.Runtime.InteropServices;
using UnityEngine;

public class BackgroundTasksBridge : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SetNativePlayerPrefs(string key, string value);

    [DllImport("__Internal")]
    private static extern void SetDesiredLocationToMonitor(float latitude, float longitude, float radius);
    
    [DllImport("__Internal")]
    private static extern void SetLocationToMonitor(float latitude, float longitude, float radius);

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
    }
    
    public void SendLocationToMonitor(float latitude, float longitude, float radius)
    {
        Debug.Log("Monitor Location!");
        #if UNITY_EDITOR
                Debug.Log("Please Switch To IOS Device To get this work");
        #elif UNITY_IOS
                SetDesiredLocationToMonitor(latitude, longitude, radius);
        #endif
    }

    public void SetNativeLocationToMonitor(float lat, float lng, float rad)
    {
        #if UNITY_EDITOR
                Debug.Log("Please Switch To IOS Device To get this work");
        #elif UNITY_IOS
                SetLocationToMonitor(lat,lng, rad);
        #endif
    }
    
    
    
    // private void TestNativeCode()
    // {
    //     // UpdateLocations();
    //     //SendDataToiOS("TraceData_Native","This  will  be the value number ::"+ counter);
    // }
    
    //test code
//     private void SendDataToiOS(string key, string value)
//     {
//         PlayerPrefs.SetString(key, value);
// #if UNITY_EDITOR
//         Debug.Log("Please Switch To IOS Device To get this work");
// #elif UNITY_IOS
//         SetNativePlayerPrefs(key,value);
//         SetLocationToMonitor(31.5096497f,74.3459482f, 1000f);
// #endif
//     }
    //uhh not sure
//     public void UpdateLocations( )
//     {
// // #if UNITY_EDITOR
// //         Debug.Log("Please Switch To IOS Device To get this work");
// // #elif UNITY_IOS
// //         DesiredLocationToMonitor(31.5096497f,74.3459482f );
// // #endif  
//     }
}