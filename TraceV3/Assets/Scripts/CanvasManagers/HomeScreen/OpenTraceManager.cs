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
    [Header("Trace Stuff")]
    [SerializeField] private GameObject imageObject;
    [SerializeField] private GameObject videoObject;
    [SerializeField] private RectTransform _videoRectTransform;
    [SerializeField] private float videoScaleConstant;
    [SerializeField] private string traceID;
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
    [SerializeField] private float changeInYvalExitLimit;
    [SerializeField] private float changeInYvalCloseLimit;
    [SerializeField] private float dyForScreenSwitchLimit;
    [SerializeField] private float stopAtScreenTopLimit;
    [SerializeField] private int openUp_targetYVal = 0;
    [SerializeField] private int imageHeightTarget;

    [Header("State")] 
    [SerializeField] private bool isPhoto;
    [SerializeField] private bool isDragging;
    [SerializeField] private bool canUsePhysics;
    [SerializeField] private bool canCloseTrace;
    [SerializeField] private bool hasBegunOpenTrace;
    [SerializeField] private bool hasBegunCloseTrace;
    
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
    //public int down_targetYVal = -1000;

    private void OnEnable()
    {
        SetScreenSize();
        Reset();
    }
    private void SetScreenSize()
    {
        switch(ScreenSizeManager.instance.currentModel)
        {
            //g_offset is just openUp_targetYVal * 0.75
            case iPhoneModel.iPhone7_8: //working
                openUp_targetYVal = 780;
                g_offset = -585;
                imageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhone7Plus_8Plus: //working
                openUp_targetYVal = 1100;
                g_offset = -825;
                imageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhoneX_XS: //working
                openUp_targetYVal = 1150;
                g_offset = -862;
                imageHeightTarget = 3650;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhoneXR: //working
                openUp_targetYVal = 850;
                g_offset = -610;
                imageHeightTarget = 2710;
                return;
            case iPhoneModel.iPhoneXSMax: //working
                openUp_targetYVal = 1250;
                g_offset = -937;
                imageHeightTarget = 4000;
                videoScaleConstant = 0.76f;
                return;
            case iPhoneModel.iPhone11: //working
                openUp_targetYVal = 835;
                g_offset = -615;
                imageHeightTarget = 2680;
                videoScaleConstant = 1.13f;
                return;
            case iPhoneModel.iPhone11Pro: //working
                openUp_targetYVal = 1150;
                g_offset = -862;
                imageHeightTarget = 3650;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone11ProMax: //working
                openUp_targetYVal = 1250;
                g_offset = -937;
                imageHeightTarget = 4000;
                return;
            case iPhoneModel.iPhoneSE2: //not working at all????????
                openUp_targetYVal = 2000;
                g_offset = -1500;
                imageHeightTarget = 3800;
                return;
            case iPhoneModel.iPhone12Mini: //Working
                openUp_targetYVal = 1120;
                g_offset = -830;
                imageHeightTarget = 3490;
                videoScaleConstant = 0.86f;
                return;
            case iPhoneModel.iPhone12_12Pro: //Working
                openUp_targetYVal = 1200;
                g_offset = -900;
                imageHeightTarget = 3800;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone12ProMax: //working
                openUp_targetYVal = 1350;
                g_offset = -991;
                imageHeightTarget = 4160;
                videoScaleConstant = 0.80f;
                return;
            case iPhoneModel.iPhone14ProMax_14Plus_13ProMax_: //working
                openUp_targetYVal = 1350;
                g_offset = -991;
                imageHeightTarget = 4160;
                videoScaleConstant = 0.82f;
                return;
            case iPhoneModel.iPhone13_13Pro_14_14Pro: //working
                openUp_targetYVal = 1200;
                g_offset = -906;
                imageHeightTarget = 3823;
                videoScaleConstant = 0.84f;
                return;
            case iPhoneModel.iPhone13Mini:
                openUp_targetYVal = 1120;
                g_offset = -830;
                imageHeightTarget = 3500;
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
        hasBegunOpenTrace = false;
        hasBegunOpenTrace = false;
        hasBegunCloseTrace = false;
        canCloseTrace = false;
        canUsePhysics = false;
        
        m_transform.position = new Vector3(m_transform.position.x, startLocation, m_transform.position.z);
        g_transform.position = new Vector3(g_transform.position.x, startLocation, g_transform.position.z);
        openTraceBackground.SetActive(true);
        Dy = 0;
        changeInYVal = 0;
        gTransformVelocity = 0;
        m_targetYVal = openUp_targetYVal;
    }

    
    
    public void ActivatePhotoFormat(string traceID, string sendDate, string senderName, string senderID, int numOfPeopleSent)
    {
        this.traceID = traceID;
        senderNameDisplay.text = senderName;
        senderDateDisplay.text = "Left " + HelperMethods.ReformatDate(sendDate) + HelperMethods.ReformatRecipients(numOfPeopleSent);
        this.senderID = senderID;
        canUsePhysics = true;
        isPhoto = true;
        imageObject.SetActive(true);
        videoObject.SetActive(false);
    }
    public IEnumerator ActivateVideoFormat(string traceID, string sendDate, string senderName, string senderID, int numOfPeopleSent)
    {
        this.traceID = traceID;
        this.senderID = senderID;
        senderNameDisplay.text = senderName;
        senderDateDisplay.text = "Left " + HelperMethods.ReformatDate(sendDate) + HelperMethods.ReformatRecipients(numOfPeopleSent);

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
        canUsePhysics = true;
        // _videoRectTransform.sizeDelta = new Vector2(videoPlayer.width, videoPlayer.height);
        // Debug.Log("Video Height: " + videoPlayer.height + " Video Width: " + videoPlayer.width);
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
        hasBegunCloseTrace = true;
        canCloseTrace = true;
        m_targetYVal = 0;
        videoPlayer.Pause();
    }

    public void Update()
    {
        if (!canUsePhysics)
        {
            return;
        }
        changeInYVal =  m_transform.position.y-m_targetYVal;
        if (!isDragging)
        {
            m_transform.position = new Vector3(m_transform.position.x, m_transform.position.y + Dy*frictionWeight + slideRestitutionCurve.Evaluate(changeInYVal)*100f); 
            bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        }
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

        //only apply friction before screen switch
        if (!hasBegunOpenTrace)
        {
            Dy *= friction;
        }
        
        if (changeInYVal > changeInYDvLimit)
        {
            Dy = 0;
        }
        
        if (changeInYVal > changeInYvalGoLimit && !hasBegunOpenTrace  && !isDragging && Dy > dyForScreenSwitchLimit)
        {
            Debug.Log("OpenTrace");
            hasBegunOpenTrace = true;
            m_targetYVal = imageHeightTarget;
        }
        
        if (changeInYVal < changeInYvalExitLimit && !isDragging && Dy < dyLimitForScreenExit)
        {
            Debug.Log("ExitTrace");
            hasBegunCloseTrace = true;
            canCloseTrace = true;
            m_targetYVal = 0;
            videoPlayer.Pause();
        }

        //Slow down window as it hits the top Of the screen
        if (hasBegunOpenTrace && m_transform.localPosition.y > stopAtScreenTopLimit && Dy > 0)
        {
            Dy *= 0.5f;
            g_gameObject.SetActive(false);
        }
        
        if (hasBegunOpenTrace && !canCloseTrace && g_transform.localPosition.y > 1000)
        {
            //State
            canCloseTrace = true;
            
            //Play Video
            if (!isPhoto)
            {
                videoPlayer.Play();
            }
            //Update Map and Database
            FbManager.instance.MarkTraceAsOpened(traceID);
            TraceManager.instance.UpdateTracesOnMap();
            NotificationManager.Instance.SendNotificationUsingFirebaseUserId(senderID, FbManager.instance.thisUserModel.DisplayName , "opened your trace!");
        }
        
        if (hasBegunOpenTrace && changeInYVal < changeInYvalCloseLimit && !isDragging && canCloseTrace)
        {
            hasBegunCloseTrace = true;
            m_targetYVal = -1000;
        }
        
        if (hasBegunCloseTrace && m_transform.localPosition.y < m_YResetLimit && canCloseTrace)
        {
            Debug.Log("RESET OPEN TRACE");
            Reset();
        }
    }

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
}
