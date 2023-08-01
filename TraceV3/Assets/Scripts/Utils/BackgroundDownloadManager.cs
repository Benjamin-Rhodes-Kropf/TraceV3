using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Networking;
using Unity.VisualScripting;
using UnityEngine;

public class BackgroundDownloadManager: MonoBehaviour
{

    public static BackgroundDownloadManager s_Instance;

    private void Awake()
    {
        if (s_Instance != this && s_Instance != null)
            DestroyImmediate(this);
        
        s_Instance = this;
    }


    public void DownloadMediaInBackground(string traceId, string mediaType)
    {
        var downloadPath = "";
        if (mediaType == MediaType.PHOTO.ToString())
            downloadPath = "ReceivedTraces/Photos/"+traceId+".png";
        else
            downloadPath = "ReceivedTraces/Videos/"+traceId+".mp4";

        
        var filePath = Path.Combine(Application.persistentDataPath, downloadPath);

        if (File.Exists(filePath))
            return;

        FbManager.instance.GetTraceMediaDownloadURL(traceId, 
            (downloadUrl) =>
            {
                StartCoroutine(StartDownload(downloadUrl, downloadPath));
            }, 
            () =>
            {
                Debug.LogError("Unable to Get Trace Media Path");
            });
    }

    private IEnumerator StartDownload(string url, string filePath)
    {
        using var download = BackgroundDownload.Start(new Uri(url), filePath);
        yield return download;
        Debug.Log(download.status == BackgroundDownloadStatus.Failed ? download.error : "Done Downloading ::"+filePath);
    }
    
}