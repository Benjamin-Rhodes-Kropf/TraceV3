using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendOrRecievedViewSelectorManager : MonoBehaviour
{
    [Header("External")] 
    [SerializeField] private Animator selectorAnimator;
    public void OnEnable()
    {
        if (HomeScreenManager.isInSendTraceView)
        {
            selectorAnimator.Play("SetToDown");
        }
        else
        {
            selectorAnimator.Play("SetToUp");
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
