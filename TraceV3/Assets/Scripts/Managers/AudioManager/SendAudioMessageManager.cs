using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAudioMessageManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendAudioMessageManager Instance;

    [Header("Audio Values")] 
    public TraceObject trace;
    public string fileLocation;
    public MediaType mediaType;
}
