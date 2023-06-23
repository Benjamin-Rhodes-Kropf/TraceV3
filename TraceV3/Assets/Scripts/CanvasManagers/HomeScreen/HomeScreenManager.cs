using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class HomeScreenManager : MonoBehaviour
{
    [Header("External")] 
    [SerializeField] private SlideToOpenManager slideToOpenManager;
    [SerializeField] private Animator homeScreenAnimator;
    
    

    public void OpenTrace(string traceID, string mediaType) //Todo: Make mediaType an Enum
    {
        Debug.Log("Open Trace");
        if (traceID == null)
        {
            Debug.Log("Open Trace");
            return;
        }
        Debug.Log("mediaType:" + mediaType);
        
        //determine what type of trace it is
        if (mediaType == MediaType.PHOTO.ToString())
        {
            Debug.Log("mediaType == MediaType.PHOTO.ToString()");
            StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceID, (texture) =>
            {
                if (texture != null)
                {
                    // ScreenManager.instance.OpenPopup("Trace");
                    slideToOpenManager.ActivatePhotoFormat();
                    slideToOpenManager.displayTrace.texture = texture;
                    // FbManager.instance.MarkTraceAsOpened(traceID);
                    // TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
                    // TraceManager.instance.UpdateTracesOnMap();
                }
                else
                {
                    Debug.LogError("LoadTraceImage Failed");
                }
            }));
            return;
        }
        if (mediaType == MediaType.VIDEO.ToString())
        {
            Debug.Log("mediaType == MediaType.Video.ToString()");
            StartCoroutine(FbManager.instance.GetTraceVideoByUrl(traceID, (path) =>
            {
                if (path != null)
                {
                    Debug.Log("Open Trace View");
                    slideToOpenManager.videoPlayer.url = path;
                    slideToOpenManager.ActivateVideoFormat();
                   
                    // FbManager.instance.MarkTraceAsOpened(traceID); 
                    // TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
                    // TraceManager.instance.UpdateTracesOnMap();
                }
                else
                {
                    Debug.LogError("LoadTraceImage Failed");
                }
            }));
            return;
        }
        
    }
}
