using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firebase.Messaging;
using Helper;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Networking;

public class NotificationManager : UnitySingleton<NotificationManager>
{
    private static string url = "https://trace-notification-s5iopr6l5a-uc.a.run.app/sendNotification";
    private void Awake()
    {
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

        // Extract custom data from the message
        if (e.Message.Data != null)
        {
            foreach (var entry in e.Message.Data)
            {
                string key = entry.Key;
                string value = entry.Value;
                Debug.LogFormat("Custom Data - Key: {0}, Value: {1}", key, value);

                // Handle custom data here
                if (key == "customKey1")
                {
                    // Handle customKey1 data
                }
                else if (key == "customKey2")
                {
                    // Handle customKey2 data
                }
                // Add more conditions as needed for other custom keys
            }
        }
    }
    private void OnNotificationReceived(string payload)
    {
        Debug.Log("Received notification: " + payload);
        // Process the notification payload and perform desired actions in Unity
    }

    public async void SendNotificationUsingFirebaseUserId(string firebaseUserId, string title = "", string message = "")
    {
        var fcmToken = await FbManager.instance.GetDeviceTokenForUser(firebaseUserId);

        if (String.IsNullOrEmpty(fcmToken))
        {
            Debug.Log("user FCM token null or does not exist");
            return;
        }
        Debug.Log("SendNotificationUsingFirebaseUserID FCM TOKEN:" + fcmToken);
        StartCoroutine(SendNotification(fcmToken, title, message));
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