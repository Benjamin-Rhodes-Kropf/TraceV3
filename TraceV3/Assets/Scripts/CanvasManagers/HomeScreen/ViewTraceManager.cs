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
    public int up_targetYVal = 0;
    public int down_targetYVal = -1000;


    private void Awake()
    {
        switch(ScreenSizeManager.instance.currentModel)
        {
            case iPhoneModel.iPhone7_8:
                up_targetYVal = 900;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone7Plus_8Plus:
                up_targetYVal = 1300;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhoneX_XS:
                up_targetYVal = 1350;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhoneXR:
                up_targetYVal = 1000;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhoneXSMax:
                up_targetYVal = 1470;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone11:
                up_targetYVal = 1000;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone11Pro: //not in simulator but I generalized
                up_targetYVal = 1350;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone11ProMax: //not in simulator but I generalized
                up_targetYVal = 1470;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhoneSE2: //not working at all????????
                up_targetYVal = 2000;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone12Mini:
                up_targetYVal = 1320;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone12_12Pro:
                up_targetYVal = 1400;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone12ProMax:
                up_targetYVal = 1550;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone14ProMax_14Plus_13ProMax_:
                up_targetYVal = 1550;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone13_13Pro_14_14Pro:
                up_targetYVal = 1450;
                down_targetYVal = -1000;
                return;
            case iPhoneModel.iPhone13Mini:
                up_targetYVal = 1320;
                down_targetYVal = -1000;
                return;
        }
    }
    private void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        canUsePhysics = false;
        m_transform.position = new Vector3(m_transform.position.x, startLocation, m_transform.position.z);
        Dy = 0;
        changeInYVal = 0;
        m_targetYVal = up_targetYVal;
        canUsePhysics = false;
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
