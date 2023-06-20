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
    public TracePopup tracePopupManager;
    public Animator homeScreenAnimator;
    public GameObject openTraceBackground;
    public GameObject openTraceForeground;
    

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
                    FbManager.instance.MarkTraceAsOpened(traceID);
                    ScreenManager.instance.OpenPopup("Trace");
                    tracePopupManager.ActivatePhotoFormat();
                    tracePopupManager.displayTrace.texture = texture;
                    TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
                    TraceManager.instance.UpdateTracesOnMap();
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
                    //do somthing
                    // displayTrace.texture = texture;
                    ScreenManager.instance.OpenPopup("Trace");
                    tracePopupManager.ActivateVideoFormat();
                    tracePopupManager.videoPlayer.url = path;
                    tracePopupManager.videoPlayer.Play();
                    FbManager.instance.MarkTraceAsOpened(traceID); 
                    TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
                    TraceManager.instance.UpdateTracesOnMap();
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
