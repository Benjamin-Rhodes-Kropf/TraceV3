using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Helper;
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

    public async void SendNotificationUsingFirebaseUserId(string firebaseUserId, string title = "", string message = "")
    {
        var fcmToken = await FbManager.instance.GetDeviceTokenForUser(firebaseUserId);
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
