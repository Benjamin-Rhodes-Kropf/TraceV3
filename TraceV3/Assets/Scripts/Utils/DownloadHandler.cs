using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Helper;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Networking
{
    public class DownloadHandler : UnitySingleton<DownloadHandler>
    {
        private List<UnityWebRequest> _enquedRequestsList = new List<UnityWebRequest>();

        private static AssetBundle currentLoadedBundle = null;

        public void DownloadImage(string url,Material _material, bool enqueueRequest = false, bool cancelPreviousDownloading = false)
        {
            Debug.Log("********* Downloading Image");
            StartCoroutine(DownloadImageCoroutine(url,enqueueRequest, cancelPreviousDownloading, (downloadedTexture) =>
            {
                Debug.Log("********* Image downloaded, applying now");
                _material.SetTexture("_MainTex", downloadedTexture);
                _material.SetTexture("_EmissionMap", downloadedTexture);
                
            }, () =>
            {
                Debug.Log("Image downloading failed");
            }));
        }
        public void DownloadImage(string url, Action<Texture> onCompleted = null, Action onFailed = null)
        {
                if (string.IsNullOrEmpty(url))
                {
                    onFailed?.Invoke();
                    return;
                }
                StartCoroutine(DownloadImageCoroutine(url,false,false,onCompleted, onFailed));
        }

        public void DownloadTextFile(string url, Action<string> onCompleted = null, Action onFailed = null)
        {
            StartCoroutine(DownloadFileCoroutine(url, onCompleted, onFailed));
        }

        IEnumerator DownloadImageCoroutine(string url, bool enqueueRequest, bool dequeueAll, Action<Texture> onCompleted = null, Action onFailed = null)
        {
            if (dequeueAll)
                ClearDownloadingQueue();
            
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            
            
            if (enqueueRequest)
                _enquedRequestsList.Add(www);
            
            yield return www.SendWebRequest();

            if (www != null && (www.isNetworkError || www.isHttpError))
            {
                Debug.LogError($"Downloading Image Error : {www.responseCode} -- {www.error}\nURL: {url}");
                onFailed?.Invoke();
            }
            else if (www != null)
            {
                Texture tempTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
                
                if (tempTexture == null)
                    onFailed?.Invoke();
                else
                {
                
                    // float hToWRatio = (float) tempTexture.height / (float) tempTexture.width;
                    
                    if (enqueueRequest)
                        _enquedRequestsList.Remove(www);

                    www.Dispose();
                    
                    // Texture2D reducedTexture = new Texture2D(128, 128);
                    // // Copy the content of the original texture to the new one with resizing
                    // RenderTexture rt = new RenderTexture(128, 128, 24);
                    // Graphics.Blit((Texture2D)tempTexture, rt);
                    // RenderTexture.active = rt;
                    // reducedTexture.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                    // reducedTexture.Apply();
                    // RenderTexture.active = null;
                    // rt.Release();
                    
                    
                    onCompleted?.Invoke(tempTexture);
                    // DestroyImmediate(tempTexture);
                }
            }
        }
        
        public void ClearDownloadingQueue(Action onComplete = null)
        { 
            foreach (var request in _enquedRequestsList)
            {
                request.Abort();
            }
            
            _enquedRequestsList.Clear();
            
            onComplete?.Invoke();
        }
        
        IEnumerator DownloadFileCoroutine(string url, Action<string> onCompleted = null, Action onFailed = null)
        {
            Debug.Log("Download File Coroutine started");

            UnityWebRequest www = UnityWebRequest.Get(url);


            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Downloading TextFile Error : " + www.error);
                onFailed?.Invoke();
            }
            else
            {
                Debug.Log("File downloaded: " + www.downloadHandler.text);


                var textFile = www.downloadHandler.data;

                string json = Encoding.Default.GetString(textFile);

                Debug.Log("Json: " + json);

                onCompleted?.Invoke(json);
            }

            www.Dispose();
        }


        public void HTTP_GetRequest(string url, Action<string> onCompleted = null, Action onFailed = null)
        {
            Debug.Log("Get: " + url);
            StartCoroutine(HTTP_GetRequestCoroutine(url, onCompleted, onFailed));
        }

        IEnumerator HTTP_GetRequestCoroutine(string url, Action<string> onCompleted = null, Action onFailed = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("appApiKey", "4PS8X8R18NCMDPPT0FPQCD6YD9VKKHVV");
                www.SetRequestHeader("Authorization",
                    "Bearer OGY5MGFjNGIyZWE5ZDNlYjU2NjdjNjNlODFhOWZkZGE4N2RhNzY1MTI2NmNmNmIzMDkwNmRhNWYyNmExOTc1Zg");

                yield return www.SendWebRequest();

                if (www.isNetworkError)
                {
                    Debug.Log(www.error);
                    onFailed?.Invoke();
                }
                else
                {
                    if (www.isDone)
                    {
                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                        Debug.Log("~~~~~ GetResponse: " + JsonConvert.SerializeObject(jsonResult));
                        onCompleted.Invoke(jsonResult);
                    }
                }
            }
        }
    }
}
