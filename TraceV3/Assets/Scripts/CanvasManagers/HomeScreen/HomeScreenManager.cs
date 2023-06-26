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
    public static bool isInSendTraceView;
    [SerializeField] private OpenTraceManager openTraceManager;
    [SerializeField] private ViewTraceManager viewTraceManager;
    [SerializeField] private Animator homeScreenAnimator; //Todo: add animation

    
    
    public void ViewTrace(string senderName, string sendDate)
    {
        viewTraceManager.ActivateView(senderName, sendDate);   
    }
    
    public void OpenTrace(string traceID, string senderName, string sendDate, string mediaType) //Todo: Make mediaType an Enum
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
                    openTraceManager.ActivatePhotoFormat(traceID, sendDate, senderName);
                    openTraceManager.displayTrace.texture = texture;
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
                    openTraceManager.videoPlayer.url = path;
                    openTraceManager.ActivateVideoFormat(traceID,sendDate,senderName);
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
