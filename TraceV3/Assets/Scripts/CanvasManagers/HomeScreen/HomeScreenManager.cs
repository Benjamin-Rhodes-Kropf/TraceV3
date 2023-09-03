using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class HomeScreenManager : MonoBehaviour
{

    [Header("External")]
    public static bool isInSendTraceView;
    [SerializeField] private OnlineMaps _onlineMaps;
    [SerializeField] private TraceManager _traceManager;
    [SerializeField] private OnlineMapsControlBase onlineMapsControlBase;
    [SerializeField] private OpenTraceManager openTraceManager;

    [Header("Internal")]
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private ViewTraceManager viewTraceManager;
    [SerializeField] private Animator _loadingAnimator;
    [SerializeField] private TMP_Text _locationNameDisplay;
    [SerializeField] private Animator _locationTextAnimator;
    [SerializeField] private string lastlocationText;
    [SerializeField] private string locationText;

    private void Start()
    {
        //when to update location name
        onlineMapsControlBase.OnMapDrag += ChangeLocationText;
        onlineMapsControlBase.OnMapPress += ChangeLocationText;
        onlineMapsControlBase.OnMapZoom += ChangeLocationText;
    }

    private void OnEnable()
    {
        if (SendTraceManager.instance.isSendingTrace)
        {
            StartCoroutine(PlayLoadingAnimationUntilTraceSends());
        }
        _locationNameDisplay.text = "";
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
    
    public void ViewTrace(string senderName, string sendDate, List<TraceReceiverObject> receivers)
    {
        CloseOpenTrace();
        viewTraceManager.ActivateView(senderName, sendDate, receivers.Count);   
    }

    public void ToggleTutorial()
    {
        _tutorialCanvas.SetActive(!_tutorialCanvas.gameObject.active);
    }

    public void ChangeLocationText()
    {
        StartCoroutine(ChangeLocationTextReduceApiCallSpeed(_onlineMaps.floatZoom));
        StartCoroutine(IsChangingLocation());
    }
    public void UpdateLocationText(float zoom)
    {
        StartCoroutine(ChangeLocationTextReduceApiCallSpeed(zoom));
        StartCoroutine(IsChangingLocation());
    }

    public void PlayChangingLocationAnim()
    {
        StartCoroutine(IsChangingLocation());
    }

    #region Location Text
    public bool isChangingLocation = false;
    public bool isLocationTextUpdateRunning = false;
    public float waitTime;
    public bool isChangeLocationAnimRunning = false;
    IEnumerator IsChangingLocation()
    {
        if (isChangingLocation)
        {
            waitTime += 1f;
            if (waitTime > 2)
            {
                waitTime = 2;
            }
            yield break;
        }
        waitTime = 2;
        isChangingLocation = true;
        while (waitTime > 1)
        {
            yield return new WaitForSeconds(waitTime/10);
            waitTime -= waitTime/10;
            if (waitTime < 0)
            {
                waitTime = 0;
            }
        }

        isChangingLocation = false;
        //Debug.Log("done waiting!");
        StartCoroutine(ChangeLocationTextAnim());
    }
    IEnumerator ChangeLocationTextReduceApiCallSpeed(float zoom)
    {
        if (!isLocationTextUpdateRunning)
        {
            isLocationTextUpdateRunning = true;
            _locationTextAnimator.Play("exit");
            yield return new WaitForSeconds(0.3f);
            Debug.Log("ChangeLocationTextSlowApiCalls: lng" + _onlineMaps.position.x);
            StartCoroutine(MapboxGeocoding.Instance.GetGeocodingData(_onlineMaps.position.x ,_onlineMaps.position.y, zoom, (result) => {
                if (result != "null")
                {
                    locationText = result;
                }
            }));
            isLocationTextUpdateRunning = false;
        }
        else
        {
            yield return null;
        }
    }
    IEnumerator ChangeLocationTextAnim()
    {
        // Check if the coroutine is already running
        if (isChangeLocationAnimRunning)
        {
            yield break; // Exit the coroutine without starting it again
        }
        // if (lastlocationText == locationText)
        // {
        //     yield break;
        // };
        while (isChangingLocation) //wait for map to stop updating
        {
            yield return new WaitForEndOfFrame();
        }
        // Set the flag to indicate that the coroutine is running
        isChangeLocationAnimRunning = true;
        yield return new WaitForSeconds(0.2f);
        _locationNameDisplay.text = locationText;
        lastlocationText = locationText;
        _locationTextAnimator.Play("enter");
        isChangeLocationAnimRunning = false;
    }
    #endregion
    
    public void OpenTrace(TraceObject trace) //Todo: Make mediaType an Enum
    {
        
        CloseViewTrace();
        Debug.Log("Opening Trace:" + trace.ToString());
        if (trace.id == null)
        {
            Debug.Log("TRACE ID IS NULL!");
            return;
        }
        Debug.Log("mediaType:" + trace.mediaType);
        
        //determine what type of trace it is
        if (trace.mediaType == MediaType.PHOTO.ToString())
        {
            Debug.Log("Open Trace Picture");
            StartCoroutine(GetTraceTexture(trace.id, (texture) =>
            {
                if (texture != null)
                {
                    openTraceManager.ActivatePhotoFormat(trace);
                    openTraceManager.displayTrace.texture = texture;
                }
                else
                {
                    Debug.LogError("LoadTraceImage Failed");
                }
            }));
            return;
        }
        if (trace.mediaType == MediaType.VIDEO.ToString())
        {
            Debug.Log("Open Trace Video");
            StartCoroutine(GetVideoPath(trace.id, (path) =>
            {
                if (path != null)
                {
                    Debug.Log("Open Trace View");
                    openTraceManager.videoPlayer.url = path;
                    StartCoroutine((openTraceManager.ActivateVideoFormat(trace)));
                    StartCoroutine(GetAudioFiles(trace));
                }
                else
                {
                    Debug.LogError("LoadTraceImage Failed");
                }
            }));
        }
        //todo: get trace comments
    }

    public void RefreshTraceView(TraceObject traceObject)
    {
        openTraceManager.RefreshTrace(traceObject);
        StartCoroutine(GetAudioFiles(traceObject));
    }
    
    public void UpdateMap()
    {
        TraceManager.instance.UpdateMap(new Vector2(0,0));
    }
    

    private IEnumerator GetVideoPath(string traceId, Action<string> callback)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "Traces/Videos/"+traceId+".mp4");
        if (File.Exists(filePath))
            callback(filePath);
        else
            StartCoroutine(FbManager.instance.GetTraceVideoByUrl(traceId, callback)); //get it from database now
        yield return null;
    }
    
    private IEnumerator GetAudioFiles(TraceObject traceObject)
    {
        foreach (var comment in traceObject.comments)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "/Comments/"+traceObject.id+"/"+comment.Key+".wav");
            if (File.Exists(filePath))
            {
                openTraceManager.trace.comments[comment.Key].location = filePath;
            }
            else
                StartCoroutine(FbManager.instance.GetTraceAudioByUrl(traceObject.id+"/"+comment.Value.id, commentPath => openTraceManager.PutAudioFileLocation(comment.Key, filePath))); //get it from database now
            yield return null;
        }
    }
    


    private IEnumerator GetTraceTexture(string traceId, Action<Texture> callback)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "Traces/Photos/"+traceId+".png");
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
