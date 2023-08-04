using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class HomeScreenManager : MonoBehaviour
{

    [Header("External")]
    public static bool isInSendTraceView;
    [SerializeField] private TraceManager _traceManager;
    
    
    [Header("Internal")]
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private OpenTraceManager openTraceManager;
    [SerializeField] private ViewTraceManager viewTraceManager;
    [SerializeField] private Animator _loadingAnimator;

    private void OnEnable()
    {
        if (SendTraceManager.instance.isSendingTrace)
        {
            StartCoroutine(PlayLoadingAnimationUntilTraceSends());
        }
    }

    public IEnumerator PlayLoadingAnimationUntilTraceSends()
    {
        Debug.Log("Start Loading Animation");
        _loadingAnimator.Play("loading");
        yield return new WaitUntil(() => !SendTraceManager.instance.isSendingTrace);
    
        // Wait for the current animation to complete before starting the next one
        while (_loadingAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        Debug.Log("Start Done Loading Animation");
        _loadingAnimator.Play("doneloading");
    }

    public void CloseViewTrace()
    {
        viewTraceManager.ClosePreview();
    }
    public void CloseOpenTrace()
    {
        openTraceManager.CloseView();
    }

    public void RefreshMap()
    {
        Debug.Log("RefreshMap");
        _traceManager.UpdateMap(new Vector2(0,0));
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

    public static void SelectorPressed()
    {
        
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
            StartCoroutine(GetTraceTexture(traceID, (texture) =>
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
            #region Old Way
            // StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceID, (texture) =>
            // {
            //     if (texture != null)
            //     {
            //         openTraceManager.ActivatePhotoFormat(traceID, sendDate, senderName, senderID, numOfPeopleSent);
            //         openTraceManager.displayTrace.texture = texture;
            //     }
            //     else
            //     {
            //         Debug.LogError("LoadTraceImage Failed");
            //     }
            // }));
            // return;
            #endregion
        }
        if (mediaType == MediaType.VIDEO.ToString())
        {
            StartCoroutine(GetVideoPath(traceID, (path) =>
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
            #region OldCode
            // Debug.Log("mediaType == MediaType.Video.ToString()");
            // StartCoroutine(FbManager.instance.GetTraceVideoByUrl(traceID, (path) =>
            // {
            //     if (path != null)
            //     {
            //         Debug.Log("Open Trace View");
            //         openTraceManager.videoPlayer.url = path;
            //         StartCoroutine((openTraceManager.ActivateVideoFormat(traceID,sendDate,senderName, senderID, numOfPeopleSent)));
            //     }
            //     else
            //     {
            //         Debug.LogError("LoadTraceImage Failed");
            //     }
            // }));
            return;
            #endregion
        }
        
    }
    
    public void UpdateMap()
    {
        TraceManager.instance.UpdateMap(new Vector2(0,0));
    }

    private IEnumerator GetVideoPath(string traceId, Action<string> callback)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "ReceivedTraces/Videos/"+traceId+".mp4");
        if (File.Exists(filePath))
            callback(filePath);
        else
            StartCoroutine(FbManager.instance.GetTraceVideoByUrl(traceId, callback));
        yield return null;
    }


    private IEnumerator GetTraceTexture(string traceId, Action<Texture> callback)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "ReceivedTraces/Photos/"+traceId+".png");
        if (File.Exists(filePath))
        {
            byte[] textureData = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(128, 128,TextureFormat.RGB24,false); // Provide initial dimensions, it will be overridden by LoadImage.
            loadedTexture.LoadImage(textureData);
            callback(loadedTexture);
        }
        else
            StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceId, callback));
        yield return null;
    }
    
}
