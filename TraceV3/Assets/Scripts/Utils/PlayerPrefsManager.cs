using System.Collections.Generic;
using UnityEngine;
using  Newtonsoft.Json;

public class PlayerPrefsManager : MonoBehaviour
{
    private const string m_ReceivedTracesKey = "ReceivedTraces_Key";
    public static PlayerPrefsManager s_Instance;
    
    void Start()
    {
        s_Instance = this;
    }


    public void ReceivedTraces(List<TraceObject> traces)
    {
        string traceJSON = JsonConvert.SerializeObject(traces);
        Debug.Log("------ Trace JSON ------");
        Debug.Log(traceJSON);
        PlayerPrefs.SetString(m_ReceivedTracesKey,traceJSON);
        PlayerPrefs.Save();
    }


    public List<TraceObject> GetReceivedTraces()
    {
        var traces = new List<TraceObject>();
        var tracesString = PlayerPrefs.GetString(m_ReceivedTracesKey,"");
        traces = JsonConvert.DeserializeObject<List<TraceObject>>(tracesString);
        return traces;
    }








}
