using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ViewTraceManager :  MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text senderNameDisplay;
    [SerializeField] private TMP_Text senderDateDisplay;
    
    [Header("Swipe Physics Both")] 
    [SerializeField] private float startLocation;
    
    [Header("Swipe Physics Primary")]
    [SerializeField] RectTransform m_transform;
    [SerializeField] private float m_targetYVal;
    [SerializeField] private float changeInYVal;
    [SerializeField] private float Dy;
    [SerializeField] private float frictionWeight = 1f;
    [SerializeField] private float friction = 1f;
    [SerializeField] private float dyLimitForScreenExit;
    [SerializeField] private AnimationCurve slideFrictionCurve;
    [SerializeField] private AnimationCurve slideRestitutionCurve;
    
    [Header("Swipe Physics Limits")]
    [SerializeField] private float changeInYDvLimit;
    [SerializeField] private float changeInYvalExitLimit;
    
    [Header("State")]
    [SerializeField] private bool isDragging;
    [SerializeField] private bool hasBegunExit;
    [SerializeField] private bool canUsePhysics;
    
    private void OnEnable()
    {
        Reset();
    }
    
    switch
    
    
    public void Reset()
    {
        canUsePhysics = false;
        m_transform.position = new Vector3(m_transform.position.x, startLocation, m_transform.position.z);
        Dy = 0;
        changeInYVal = 0;
        m_targetYVal = 2000;
        canUsePhysics = false;

        if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone4_4S)
        {
            m_targetYVal = 2000;
            Debug.Log("Iphone 444444444444444444444");
        }
        if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone11)
        {
            m_targetYVal = 2000;
            Debug.Log("Iphone 11111111111111111111111");

        }
    }
    
    public void ActivateView(string senderName, string sendDate)
    {
        senderNameDisplay.text = senderName;
        senderDateDisplay.text = sendDate;
        canUsePhysics = true;
        hasBegunExit = false;
        
        
    }

    public void ClosePreview()
    {
        Debug.Log("ExitTrace");
        hasBegunExit = true;
        m_targetYVal = -1000;
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
        }
        
        if (changeInYVal < changeInYvalExitLimit && !isDragging && Dy < dyLimitForScreenExit)
        {
            Debug.Log("ExitTrace");
            m_targetYVal = 0;
            hasBegunExit = true;
        }
        Dy *= friction;
        
        if (hasBegunExit && Mathf.Abs(changeInYVal) < 30)
        {
            Reset(); 
        }
        
        
        if (changeInYVal > changeInYDvLimit)
        {
            Dy = 0;
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
