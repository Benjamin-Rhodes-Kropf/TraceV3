using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firebase.Messaging;
using Helper;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.iOS;

using UnityEngine.Networking;

public class NotificationManager : UnitySingleton<NotificationManager>
{
    private static string url = "https://trace-notification-s5iopr6l5a-uc.a.run.app/sendNotification";
    private void Awake()
    {
        Debug.Log("Setting Up Notifications");
        Application.runInBackground = true;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Received a new Token!");
        // Handle token received event, if needed
    }
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        // Handle incoming message
        Debug.Log("Received a new message!");
        Debug.Log("Mesage Raw Data:" + e.Message.RawData);
        // Extract custom data from the message
        if (e.Message.Data != null)
        {
            foreach (var entry in e.Message.Data)
            {
                string key = entry.Key;
                string value = entry.Value;
                Debug.LogFormat("Custom Data - Key: {0}, Value: {1}", key, value);

                // Handle custom data here
                if (key == "Lat")
                {
                    Debug.Log("Notification Lat:" + value);
                }
                else if (key == "Lng")
                {
                    Debug.Log("Notification Lng:" + value);

                }
                // Add more conditions as needed for other custom keys
            }
        }
    }

    public IEnumerator SendNotificationUsingFirebaseUserId(string firebaseUserId, string title = "", string message = "")
    {
        Debug.Log("Getting Device token from:" + firebaseUserId);
        var task = FbManager.instance.GetDeviceTokenForUser(firebaseUserId);
        yield return new WaitUntil(() => task.IsCompleted);
        var fcmToken = task.Result;
        Debug.Log("Device token:" + fcmToken);

        if (string.IsNullOrEmpty(fcmToken))
        {
            Debug.Log("User FCM token null or does not exist");
            yield break;
        }

        Debug.Log("SendNotificationUsingFirebaseUserID FCM TOKEN:" + fcmToken);
        yield return StartCoroutine(SendNotification(fcmToken, title, message));

    }
    public IEnumerator SendNotification(string token, string title, string body)
    {
        var requestData = new RequestData();
        requestData.token = token;
        requestData.title = title;
        requestData.body = body;
        
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
        Debug.Log(JsonUtility.ToJson(requestData));
        request.uploadHandler = new UploadHandlerRaw(bodyData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Error sending notification: {request.error}");
        }
        else
        {
            Debug.Log("Notification sent successfully!");
        }
    }
}

[Serializable]
public class RequestData
{
    public string token;
    public string title;
    public string body;
}