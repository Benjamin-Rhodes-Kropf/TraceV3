using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TracePopup : MonoBehaviour
{
    [SerializeField] private GameObject _slideToOpenManager;
    public GameObject imageObject;
    public RawImage displayTrace;
    public GameObject videoObject;
    public VideoPlayer videoPlayer;
    
    private void OnDisable()
    {
        imageObject.SetActive(false);
        videoObject.SetActive(false);
        videoPlayer.enabled = false;
        displayTrace.texture = null;
    }
    
    public void ActivatePhotoFormat()
    {
        _slideToOpenManager.SetActive(true);
        imageObject.SetActive(true);
        videoObject.SetActive(false);
    }
    public void ActivateVideoFormat()
    {
        _slideToOpenManager.SetActive(true);
        imageObject.SetActive(false);
        videoObject.SetActive(true);
        videoPlayer.enabled = true;
    }
}
