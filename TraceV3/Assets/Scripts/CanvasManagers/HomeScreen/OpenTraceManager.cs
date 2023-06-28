using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private string traceID;
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
    private void OnEnable()
    {
        Reset();
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
        m_targetYVal = 1200;
    }
    
    public void ActivatePhotoFormat(string traceID, string sendDate, string senderName)
    {
        this.traceID = traceID;
        senderNameDisplay.text = senderName;
        senderDateDisplay.text = sendDate;
        canUsePhysics = true;
        isPhoto = true;
        imageObject.SetActive(true);
        videoObject.SetActive(false);
    }
    public void ActivateVideoFormat(string traceID, string sendDate, string senderName)
    {
        this.traceID = traceID;
        senderNameDisplay.text = senderName;
        senderDateDisplay.text = sendDate;
        canUsePhysics = true;
        isPhoto = false;
        imageObject.SetActive(false);
        videoObject.SetActive(true); 
        videoPlayer.enabled = true;
        videoPlayer.Play();
        videoPlayer.Pause();
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
        
        if (changeInYVal < changeInYvalExitLimit && !isDragging && Dy < dyLimitForScreenExit)
        {
            Debug.Log("ExitTrace");
            hasBegunCloseTrace = true;
            canCloseTrace = true;
            m_targetYVal = 0;
            videoPlayer.Pause();
        }
        
        if (changeInYVal > changeInYvalGoLimit && !hasBegunOpenTrace  && !isDragging && Dy > dyForScreenSwitchLimit)
        {
            Debug.Log("OpenTrace");
            hasBegunOpenTrace = true;
            m_targetYVal = 3800;
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
        }
        if (hasBegunOpenTrace && changeInYVal < changeInYvalCloseLimit && !isDragging && canCloseTrace)
        {
            hasBegunCloseTrace = true;
            m_targetYVal = 0;
        }

        //changed from g to m
        Debug.Log("m_transform.localPosition.y:" + m_transform.localPosition.y);
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
