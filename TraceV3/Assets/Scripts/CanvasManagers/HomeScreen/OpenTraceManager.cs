using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class OpenTraceManager : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("External")] 
    [SerializeField] private HomeScreenManager _homeScreenManager;
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocation;
    [SerializeField] private CommentAudioManager _commentAudioManager;
    [SerializeField] private CommentDisplayManager _commentDisplayManager;

    [Header("Trace Stuff")]
    public TraceObject trace;
    [SerializeField] private GameObject imageObject;
    [SerializeField] private GameObject videoObject;
    [SerializeField] private RectTransform _videoRectTransform;
    [SerializeField] private float videoScaleConstant;
    [SerializeField] private string senderID;
    [SerializeField] private TMP_Text senderNameDisplay;
    [SerializeField] private TMP_Text senderDateDisplay;
   
    public VideoPlayer videoPlayer;
    public  RawImage displayTrace;

    [Header("Swipe Physics Both")] 
    [SerializeField] private float startLocation;
    [SerializeField] private GameObject openTraceBackground;
    
    [Header("Swipe Physics Primary")]
    [SerializeField] RectTransform m_transform;
    [SerializeField] private float m_targetYVal;
    [SerializeField] private float changeInYVal;
    [SerializeField] private float Dy;
    [SerializeField] private float friction = 0.95f;
    [SerializeField] private float frictionWeight = 1f;
    [SerializeField] private float dyLimitForScreenExit;
    [SerializeField] private AnimationCurve slideFrictionCurve;
    [SerializeField] private AnimationCurve slideRestitutionCurve;
    
    [Header("Swipe Physics Limits")]
    [SerializeField] private float changeInYvalGoLimit;
    [SerializeField] private float changeInYDvLimit;
    [SerializeField] private float m_YResetLimit;
    [SerializeField] private float changeInYvalEnterCommentsLimit;
    [SerializeField] private float changeInYvalExitCommentsLimit;
    [SerializeField] private float changeInYvalSlidUpExitLimit;
    [SerializeField] private float changeInYvalMediaEnterLimit;
    [SerializeField] private float changeInYvalMediaExitLimit;
    [SerializeField] private float changeInYvalCommentEnterLimit;
    [SerializeField] private float changeInYvalCloseLimit;
    [SerializeField] private float dyForScreenSwitchLimit;
    [SerializeField] private float stopAtScreenTopLimit;
    [SerializeField] private float hugeCloseLimit;
    [SerializeField] private int openUp_targetYVal = 0;
    [SerializeField] private float openCommentViewWhileOpeningMiedaLimit = 6000;
    [SerializeField] private int viewImageHeightTarget;
    [SerializeField] private int commentImageHeightTarget;

    [Header("State")] 
    [SerializeField] private bool isPhoto;
    [SerializeField] private bool isDragging;
    [SerializeField] private State currentState;
    [SerializeField] private enum State
    {
        Closed,
        Closing,
        OpeningSlideUpToView,
        SlideUpToView,
        OpeningMediaView,
        MediaView,
        OpeningCommentView,
        CommentView,
        ClosingCommentView,
    }
    
    [Header("Swipe Physics Secondary")]
    [SerializeField] private GameObject g_gameObject;
    [SerializeField] private RectTransform g_transform;
    [SerializeField] private float springStiffness = 0.1f;
    [SerializeField] private float springDamping = 0.8f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float g_offset;
    [SerializeField] private float bobOffset;
    [SerializeField] private float gTransformVelocity;
    
    [Header("Swipe Arrow")] 
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Image arrowImage;
    [SerializeField] private AnimationCurve arrowScale;
    [SerializeField] private AnimationCurve colorScale;
    [SerializeField] private Gradient gradient;

    private void OnEnable()
    {
        SetScreenSize();
        Reset();
        videoPlayer.loopPointReached += OnVideoEnded;
    }
    private void SetScreenSize()
    {
        switch(ScreenSizeManager.instance.currentModel)
        {
            //g_offset is just openUp_targetYVal * 0.75
            case iPhoneModel.iPhone7_8: //working
                openUp_targetYVal = 780;
                g_offset = -585;
                viewImageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhone7Plus_8Plus: //working
                openUp_targetYVal = 1100;
                g_offset = -825;
                viewImageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhoneX_XS: //working
                openUp_targetYVal = 1150;
                g_offset = -862;
                viewImageHeightTarget = 3650;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhoneXR: //working
                openUp_targetYVal = 850;
                g_offset = -610;
                viewImageHeightTarget = 2710;
                return;
            case iPhoneModel.iPhoneXSMax: //working
                openUp_targetYVal = 1250;
                g_offset = -937;
                viewImageHeightTarget = 4000;
                videoScaleConstant = 0.76f;
                return;
            case iPhoneModel.iPhone11: //working
                openUp_targetYVal = 835;
                g_offset = -615;
                viewImageHeightTarget = 2680;
                videoScaleConstant = 1.13f;
                return;
            case iPhoneModel.iPhone11Pro: //working
                openUp_targetYVal = 1150;
                g_offset = -862;
                viewImageHeightTarget = 3650;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone11ProMax: //working
                openUp_targetYVal = 1250;
                g_offset = -937;
                viewImageHeightTarget = 4000;
                return;
            case iPhoneModel.iPhoneSE2: //not working at all????????
                openUp_targetYVal = 2000;
                g_offset = -1500;
                viewImageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhone12Mini: //Working
                openUp_targetYVal = 1120;
                g_offset = -830;
                viewImageHeightTarget = 3490;
                videoScaleConstant = 0.86f;
                return;
            case iPhoneModel.iPhone12_12Pro: //Working
                openUp_targetYVal = 1200;
                g_offset = -900;
                viewImageHeightTarget = 3800;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone12ProMax: //working
                openUp_targetYVal = 1350;
                g_offset = -991;
                viewImageHeightTarget = 4160;
                videoScaleConstant = 0.80f;
                return;
            case iPhoneModel.iPhone14ProMax_14Plus_13ProMax_: //working
                openUp_targetYVal = 1350;
                g_offset = -991;
                viewImageHeightTarget = 4160;
                videoScaleConstant = 0.82f;
                return;
            case iPhoneModel.iPhone13_13Pro_14_14Pro: //working
                openUp_targetYVal = 1200;
                g_offset = -906;
                viewImageHeightTarget = 3800;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone13Mini:
                openUp_targetYVal = 1120;
                g_offset = -830;
                viewImageHeightTarget = 3500;
                videoScaleConstant = 0.86f;
                return;
        }
    }
    public void Reset()
    {
        videoPlayer.enabled = false;
        displayTrace.texture = null;
        imageObject.SetActive(false);
        videoObject.SetActive(false);
        currentState = State.Closed;
        
        m_transform.position = new Vector3(m_transform.position.x, startLocation, m_transform.position.z);
        g_transform.position = new Vector3(g_transform.position.x, startLocation, g_transform.position.z);
        openTraceBackground.SetActive(true);
        Dy = 0;
        changeInYVal = 0;
        gTransformVelocity = 0;
        m_targetYVal = openUp_targetYVal;
        _homeScreenManager.RefreshMap();
    }
    
    public void ActivatePhotoFormat(TraceObject trace)
    {
        Reset();
        currentState = State.OpeningSlideUpToView;
        this.trace = trace;
        senderNameDisplay.text = trace.senderName;
        senderDateDisplay.text = "Left " + HelperMethods.ReformatDate(trace.sendTime) + HelperMethods.ReformatRecipients(trace.people.Count);
        isPhoto = true;
        imageObject.SetActive(true);
        videoObject.SetActive(false);
    }
    public IEnumerator ActivateVideoFormat(TraceObject trace)
    {
        Reset();
        currentState = State.OpeningSlideUpToView;
        this.trace = trace;
        senderNameDisplay.text = trace.senderName;
        senderDateDisplay.text = "Left " + HelperMethods.ReformatDate(trace.sendTime) + HelperMethods.ReformatRecipients(trace.people.Count);

        isPhoto = false;
        imageObject.SetActive(false);
        videoObject.SetActive(true); 
        videoPlayer.enabled = true;
        videoPlayer.Prepare();
        Debug.Log("video player preparing");
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Video Player Prepared");
        videoPlayer.Play();
        videoPlayer.Pause(); 
        ScaleVideoAspectRatio();
        
        _commentDisplayManager.DisplayComments(trace.comments);
    }

    public void PutAudioFileLocation(string commentId, string location)
    {
        Debug.Log("Putting Audio File:" +location);
        _commentDisplayManager.comments[commentId].GetComponent<AudioView>().location = location;
    }

    public void RefreshTrace(TraceObject trace)
    {
        if(trace.id != this.trace.id) //make sure we are refreshing the correct one
            return;
        this.trace = trace;
        _commentDisplayManager.DisplayComments(trace.comments);
    }

    public void ScaleVideoAspectRatio()
    {
        float videoAspectRatio = (float)videoPlayer.width / videoPlayer.height;
        float screenAspectRatio = (float)Screen.width / Screen.height;

        if (videoAspectRatio > screenAspectRatio)
        {
            _videoRectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / videoAspectRatio)*videoScaleConstant;
        }
        else
        {
            _videoRectTransform.sizeDelta = new Vector2(Screen.height * videoAspectRatio, Screen.height)*videoScaleConstant;
        }
    }

    public void CloseView()
    {
        m_targetYVal = -1000;
        videoPlayer.Pause();
        currentState = State.Closing;
    }

    void OnVideoEnded(VideoPlayer vp) //slide the window up to show comments
    {
        if (currentState == State.MediaView) 
            OpenCommentViewTransition();
    }
    
    public void Update()
    {
        switch (currentState)
        {
            case State.Closed:
                break;//do nothing
            case State.Closing:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();

                //state junctions
                if (ClosedView())
                    ClosedTransition();
                break;
            case State.OpeningSlideUpToView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();
                //state junctions
                if (DoneOpeningSlideUpToView())
                    currentState = State.SlideUpToView;
                break;
            case State.SlideUpToView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();
                //state junctions
                if (OpenMediaView())
                    OpenMediaViewTransition();
                else if (CloseSlideUpToView())
                    CloseSlideUpToViewTransition();
                break;
            case State.OpeningMediaView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();

                //if its over shot the target
                if (changeInYVal > -50)
                {
                    Dy *= 0.98f;
                    Debug.Log("Big Friction");
                }
                
                //state junctions
                if(HugeClose())
                    CloseSlideUpToViewTransition();
                if (OpenCommentViewWhileOpeningMedia())
                    OpenCommentViewTransition();
                if (DoneOpeningMediaView())
                    DoneOpeningMediaTransition();
                break;
            case State.MediaView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();
                
                if(OpenCommentView())
                    OpenCommentViewTransition();
                else if (CloseMediaView())
                    CloseSlideUpToViewTransition();
                break;
            case State.OpeningCommentView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();
                
                //if its over shot the target
                // if (changeInYVal > -60 && changeInYVal < -50)
                // {
                //     Dy *= 0.8f;
                //     Debug.Log("Big Friction");
                // }
                
                //state junctions
                
                if(DoneOpeningCommentView())
                    DoneOpeningCommentTransition();
                break;
            case State.CommentView:
                //ApplyPhysics();
                // AnimateSecondaryMotions();
                CommentViewPhyisics();
                if (CloseComments())
                    CloseCommentViewTransition();
                break;
            case State.ClosingCommentView:
                //state actions
                ApplyPhysics();
                AnimateSecondaryMotions();
                
                if (changeInYVal > 10 && changeInYVal < 40)
                {
                    Dy *= 0.9f;
                    Debug.Log("Big Friction");
                }

                Debug.Log("closing comment view: " + changeInYVal);
                //state junctions
                if (HugeCloseOutOfCommentView())
                    CloseSlideUpToViewTransition();
                if (DoneOpeningMediaView())
                    DoneOpeningMediaTransition();
                break;
        }
    }

    #region State Junctions
    bool DoneOpeningSlideUpToView()
    {
        return (changeInYVal > changeInYvalSlidUpExitLimit);
    }
    bool DoneOpeningMediaView()
    {
        return (changeInYVal > changeInYvalMediaEnterLimit && changeInYVal < changeInYvalEnterCommentsLimit && Mathf.Abs(changeInYVal) < 50);
    }

    bool DoneOpeningCommentView()
    {
        return (changeInYVal > changeInYvalCommentEnterLimit);
    }
    bool OpenMediaView()
    {
        return (changeInYVal > changeInYvalGoLimit && !isDragging && Dy > dyForScreenSwitchLimit);
    }

    bool HugeClose()
    {
        return (changeInYVal < hugeCloseLimit && Dy < 0);
    }
    
    bool HugeCloseOutOfCommentView()
    {
        return (changeInYVal < hugeCloseLimit && Dy < 0);
    }
    
    bool OpenCommentViewWhileOpeningMedia()
    {
        return (m_transform.position.y > openCommentViewWhileOpeningMiedaLimit);
    }

    bool OpenCommentView()
    {
        return (changeInYVal > changeInYvalEnterCommentsLimit);
    }
    bool CloseSlideUpToView()
    {
        return (changeInYVal < changeInYvalSlidUpExitLimit);
    }
    bool CloseMediaView()
    {
        return (changeInYVal < changeInYvalMediaExitLimit);
    }
    
    bool CloseComments()
    {
        return (changeInYVal < changeInYvalExitCommentsLimit);
    }
    
    bool ClosedView()
    {
        return (changeInYVal < m_YResetLimit);
    }
    #endregion
    
    #region State Transitions
    void OpenMediaViewTransition()
    {
        Debug.Log("OpeningMediaView");
        m_targetYVal = viewImageHeightTarget;
        currentState = State.OpeningMediaView;
        
        //start playing video early
        if (!isPhoto)
        {
            videoPlayer.Play();
        }
    }
    
    void CloseCommentViewTransition()
    {
        Debug.Log("CloseCommentViewTransition");
        m_targetYVal = viewImageHeightTarget;
        currentState = State.ClosingCommentView;
        _commentAudioManager.StopPlayingRecording();
        TraceManager.instance.UpdateMap(new Vector2(0,0)); //vector don't matter just redraw
        //start playing video early
    }
    
    void OpenCommentViewTransition()
    {
        Debug.Log("OpenCommentViewTransition");
        m_targetYVal = commentImageHeightTarget;
        currentState = State.OpeningCommentView;
    }
    

    void CloseSlideUpToViewTransition()
    {
        Debug.Log("ClosingSlideUpToView");
        g_transform.position = new Vector3(g_transform.position.x, startLocation, g_transform.position.z);
        m_targetYVal = -1000; //close quickly!
        Dy *= 2; //close quickly!
        // videoPlayer.Pause();
        videoPlayer.Stop();
        currentState = State.Closing;
    }

    void DoneOpeningMediaTransition()
    {
        Dy = 0;
        var pos = m_transform.position;
        m_transform.position = new Vector3(pos.x, m_targetYVal, pos.z);
        HapticManager.instance.PlaySelectionHaptic();
        PlayVideo();
        
        //TraceManager.instance.ClearTracesOnMap(); //todo: maybe do this more seamlessly it causes traces on map to dip for a second unitl it repaints
        
        currentState = State.MediaView;

        if (!trace.HasBeenOpened && trace.senderID != FbManager.instance.thisUserModel.userID)
        {
            FbManager.instance.MarkTraceAsOpened(trace);
            Vector2 _location = _onlineMapsLocation.GetUserLocation();
            StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(senderID, FbManager.instance.thisUserModel.name , "opened your trace!", _location.y, _location.x));
        }
    }

    public void MuteVideoAudio()
    {
        videoPlayer.Pause();
    }

    public void PlayVideo()
    {
        videoPlayer.Play();
    }

    void DoneOpeningCommentTransition()
    {
        Dy = 0;
        var pos = m_transform.position;
        m_transform.position = new Vector3(pos.x, m_targetYVal, pos.z);
        currentState = State.CommentView;
    }
    
    void ClosedTransition()
    {
        Debug.Log("ClosedTransition");
        currentState = State.Closed;
    }
    #endregion
    
    #region State Actions
    public void ApplyPhysics()
    {
        if (isDragging)
        {
            return;
        }
        m_transform.position = new Vector3(m_transform.position.x, m_transform.position.y + Dy + slideRestitutionCurve.Evaluate(changeInYVal)*100f);
        Dy *= frictionWeight;
        bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight; 
        changeInYVal =  m_transform.position.y-m_targetYVal;
    }

    public void CommentViewPhyisics()
    {
        changeInYVal =  m_transform.position.y-m_targetYVal;
        Dy *= frictionWeight;
        if (!isDragging)
        {
            if (changeInYVal > trace.comments.Count * 210 + 200)
            {
                Dy -= 5;
                m_transform.position = new Vector3(m_transform.position.x, m_transform.position.y + Dy - Math.Abs((changeInYVal- trace.comments.Count * 210 + 200))*0.02f);
                if (changeInYVal > trace.comments.Count * 210 + 250)
                {
                    Dy *= 0.8f;
                    Dy -= 10;
                }
            }
            else
                m_transform.position = new Vector3(m_transform.position.x, m_transform.position.y + Dy);
        }
    }

    public void AnimateSecondaryMotions()
    {
        var scaleArrow = arrowScale.Evaluate(changeInYVal);
        arrow.transform.localScale = new Vector3(0.25f,scaleArrow, 0.25f);
        arrowImage.color = gradient.Evaluate(colorScale.Evaluate(changeInYVal));

        // Apply spring-like behavior to g_transform
        float displacement = g_transform.position.y-m_transform.position.y-g_offset-bobOffset;
        float springForce = -springStiffness * displacement;
        float dampingForce = -springDamping * gTransformVelocity;

        float acceleration = (springForce + dampingForce) / frictionWeight;
        gTransformVelocity += acceleration * Time.deltaTime;
        g_transform.position += new Vector3(0,gTransformVelocity, 0) * Time.deltaTime;
    }
    #endregion

    #region State Inputs
        public void OnDrag(PointerEventData eventData)
        {
            isDragging = true;
            Dy += eventData.delta.y;
            m_transform.position += new Vector3(0, eventData.delta.y * slideFrictionCurve.Evaluate(changeInYVal));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    

    #endregion
    
}
