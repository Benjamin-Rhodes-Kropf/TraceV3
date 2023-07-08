using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firebase.Messaging;
using Helper;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Networking;

public class BackgroundNotificationManager : UnitySingleton<BackgroundNotificationManager>
{
    private const string BaseUrl = "https://api-unity-notification.vercel.app/";
    
    // public async void SendNotificationUsingFriendId(string friendId, string title = "", string message = "")
    // {
    //     // Fetch Firebase user Id against friendId
    //     var firebaseUserId = friendId; // TODO: Confirm is friendId a firebaseUserId?
    //     
    //     // Fetch Fcm Token From Firebase
    //     var fcmToken = await FbManager.instance.GetDeviceTokenForUser(firebaseUserId);
    //     
    //     StartCoroutine(SendNotificationUsingFcmTokenEnumerator(fcmToken, title, message));
    // }

    public void Start()
    {
        Application.runInBackground = true;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        // iOSNotificationCenter.OnRemoteNotificationReceived += remoteNotification =>
        // {
        //     Debug.Log("OnRemoteNotificationReceived!");
        //     var enterLocationTrigger = new iOSNotificationLocationTrigger
        //     {
        //         Center = new Vector2(0, 0),
        //         Radius = 100,
        //         NotifyOnEntry = true,
        //         NotifyOnExit = false
        //     };
        //     Debug.Log("Push Notification is set for a radius of " + enterLocationTrigger.Radius + "Meters"
        //               + " When user enters in " + "Latitude = " + 0 + "===" + "Longitude = " + 0);
        //
        //     var entryBasedNotification = new iOSNotification
        //     {
        //         Title = "SenderName",
        //         Subtitle =  "Left You A Trace Here",
        //         Body = "",
        //         //Body = message == "" ? "Radius latitude was > " + latitude + " and longitude was > " + longitude : message,
        //         ShowInForeground = true,
        //         ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
        //         Trigger = enterLocationTrigger
        //     };
        //     // Schedule notification for entry base
        //     iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
        // };
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
    public void OnRemoteNotificationReceived(string notification)
    {
        Debug.Log("Received remote notification from iOS: " + notification);

        // Parse the notification data and handle it as needed
    }
    
    public async void SendNotificationUsingFirebaseUserId(string firebaseUserId, string title = "", string message = "")
    {
        Debug.Log("SendNotificationUsingFirebaseUserId firebaseUserId:" + firebaseUserId);
        var fcmToken = await FbManager.instance.GetDeviceTokenForUser(firebaseUserId);
        if (fcmToken == null || fcmToken == "null")
        {
            Debug.Log("user FCM token null or does not exist");
            return;
        }
        Debug.Log("SendNotificationUsingFirebaseUserId FCM TOKEN:" + fcmToken);
        StartCoroutine(SendNotification(fcmToken, title, message));
    }
    
    public IEnumerator SendNotification(string token, string title, string body)
    {
        Debug.Log("Send Notification");
        Debug.Log("token:" + token);
        Debug.Log("title:" + title);
        Debug.Log("body:" + body);
        var url = "https://trace-notification-s5iopr6l5a-uc.a.run.app/sendNotification";
        // var requestData = new Dictionary<string, string>
        // {
        //     {"token", token},
        //     {"title", title},
        //     {"body", body}
        // };
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