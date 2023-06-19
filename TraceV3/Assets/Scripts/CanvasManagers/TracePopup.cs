using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TracePopup : MonoBehaviour
{
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
        imageObject.SetActive(true);
        videoObject.SetActive(false);
    }
    public void ActivateVideoFormat()
    {
        imageObject.SetActive(false);
        videoObject.SetActive(true);
        videoPlayer.enabled = true;
    }
}
