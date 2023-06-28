using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendOrRecievedViewSelectorManager : MonoBehaviour
{
    [Header("External")] 
    [SerializeField] private DrawTraceOnMap _drawTraceOnMap;
    
    [SerializeField] private Animator selectorAnimator;
    public void OnEnable()
    {
        if (HomeScreenManager.isInSendTraceView)
        {
            selectorAnimator.Play("MoveDown");
        }
        else
        {
            selectorAnimator.Play("MoveUp");
        }
    }

    public void SelectorPressed()
    {
        HomeScreenManager.isInSendTraceView = HomeScreenManager.isInSendTraceView ? false : true;
        if (HomeScreenManager.isInSendTraceView)
        {
            selectorAnimator.Play("MoveDown");
        }
        else
        {
            selectorAnimator.Play("MoveUp");
        }
    }
}
