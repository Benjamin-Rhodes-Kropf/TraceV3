using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    //Todo: Make this Work with remote notifications
    public void SubscribeToRemoteNotifications()
    {
        iOSNotificationCenter.OnRemoteNotificationReceived += remoteNotification =>
        {
            var enterLocationTrigger = new iOSNotificationLocationTrigger
            {
                Center = new Vector2(0, 0),
                Radius = 100,
                NotifyOnEntry = true,
                NotifyOnExit = false
            };
            Debug.Log("Push Notification is set for a radius of " + enterLocationTrigger.Radius + "Meters"
                      + " When user enters in " + "Latitude = " + 0 + "===" + "Longitude = " + 0);

            var entryBasedNotification = new iOSNotification
            {
                Title = "SenderName",
                Subtitle =  "Left You A Trace Here",
                Body = "",
                //Body = message == "" ? "Radius latitude was > " + latitude + " and longitude was > " + longitude : message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Trigger = enterLocationTrigger
            };
            // Schedule notification for entry base
            iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
        };
    }

    public async void SendNotificationUsingFirebaseUserId(string firebaseUserId, string title = "", string message = "")
    {
        var fcmToken = await FbManager.instance.GetDeviceTokenForUser(firebaseUserId);
        if (fcmToken == null || fcmToken == "null")
        {
            Debug.Log("user FCM token null or does not exist");
            return;
        }
        Debug.Log("SendNotificationUsingFirebaseUserId FCM TOKEN:" + fcmToken);
        StartCoroutine(SendNotificationUsingFcmTokenEnumerator(fcmToken, title, message));
    }
    
    private static IEnumerator SendNotificationUsingFcmTokenEnumerator(string deviceToken, string title, string message)
    {
        // Check Android Permissions
        Permissions.CheckAndroidPermissions();
   
        Debug.Log("Sending Notification to: " + deviceToken);

        // Create the data to send in the notification
        var bodyObject = new BackgroundNotification.Body
        {
            registrationToken = deviceToken
        };

        // Serialize the data to JSON
        var body = JsonUtility.ToJson(bodyObject);

        // Create a POST request to the Firebase Cloud Messaging API
        const string url = BaseUrl + "sendNotification";
        using var www = new UnityWebRequest(url, "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(body);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log("SendNotificationUsingFcmTokenEnumerator (" + www.error + "): " + www.downloadHandler?.text);
        else
            Debug.Log("SendNotificationUsingFcmTokenEnumerator (Success):" + www.downloadHandler?.text);
    }
}
