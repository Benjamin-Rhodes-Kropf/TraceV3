using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerPrefsBridge : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SetNativePlayerPrefs(string key, string value);

    // [DllImport("__Internal")]
    // private static extern string GetStringData(string key);

    private static PlayerPrefsBridge Instance { get; set; }



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
        
        
        
        InvokeRepeating(nameof(TestNativeCode),2f,3f);
    }



    public void TestNativeCode()
    {
        SendDataToiOS("TraceData_Native","This  will  be the value number ::"+ counter);
        counter++;
    }

    
    public void SendDataToiOS(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
#if UNITY_EDITOR
        Debug.Log("Please Switch To IOS Device To get this work");
#elif UNITY_IOS
        SetNativePlayerPrefs(key,value);
#endif
    }
}