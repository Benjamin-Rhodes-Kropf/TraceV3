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

    [SerializeField] private OnlineMaps _maps;
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private OpenTraceManager openTraceManager;
    [SerializeField] private ViewTraceManager viewTraceManager;
    [SerializeField] private Animator homeScreenAnimator; //Todo: add animation

    public void CloseViewTrace()
    {
        viewTraceManager.ClosePreview();
    }
    public void CloseOpenTrace()
    {
        openTraceManager.CloseView();
    }

    public void ViewTrace(string senderName, string sendDate, int numOfPeopleSent)
    {
        CloseOpenTrace();
        viewTraceManager.ActivateView(senderName, sendDate, numOfPeopleSent);   
    }

    public void ToggleTutorial()
    {
        _tutorialCanvas.SetActive(!_tutorialCanvas.gameObject.active);
    }
    
    public void OpenTrace(string traceID, string senderName, string senderID, string sendDate, string mediaType, int numOfPeopleSent) //Todo: Make mediaType an Enum
    {
        CloseViewTrace();
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
            StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceID, (texture) =>
            {
                if (texture != null)
                {
                    openTraceManager.ActivatePhotoFormat(traceID, sendDate, senderName, senderID, numOfPeopleSent);
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
                    StartCoroutine((openTraceManager.ActivateVideoFormat(traceID,sendDate,senderName, senderID, numOfPeopleSent)));
                }
                else
                {
                    Debug.LogError("LoadTraceImage Failed");
                }
            }));
            return;
        }
        
    }
    
    public void UpdateMap()
    {
        TraceManager.instance.UpdateMap(new Vector2(0,0));
    }
}
