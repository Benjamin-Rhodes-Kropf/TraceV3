using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenManager : MonoBehaviour
{
    public bool isInSentMode;
    public RawImage displayTrace;


    public void ToggleisInSentMode()
    {
        isInSentMode = !isInSentMode;
    }

    public void OpenTrace(string traceID, string mediaType) //Todo: Make mediaType an Enum
    {
        if(traceID == null)
            return;
        
        //determine what type of trace it is
        
        
        StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceID, (texture) =>
        {
            if (texture != null)
            {
                displayTrace.texture = texture;
                FbManager.instance.MarkTraceAsOpened(traceID);
                ScreenManager.instance.OpenPopup("Trace");
                TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
                TraceManager.instance.UpdateTracesOnMap();
            }
            else
            {
                Debug.LogError("LoadTraceImage Failed");
            }
        }));
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
