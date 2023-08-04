using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Firebase;
using Firebase.Messaging;
using UnityEngine;

public partial class FbManager
{
    private bool IsApplicationFirstTimeOpened {
        get
        {
            return System.Convert.ToBoolean(PlayerPrefs.GetInt("IsApplicationFirstTimeOpened", 1));
        }
        set
        {
            PlayerPrefs.SetInt("IsApplicationFirstTimeOpened", Convert.ToInt32(value));
            PlayerPrefs.Save();
        }
    }
    
    private void InitializeFCMService()
    {
        Debug.Log("Initializing FCMService");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(async task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                await Firebase.Messaging.FirebaseMessaging.SubscribeAsync("all");
                var fcmToken = await FirebaseMessaging.GetTokenAsync();
                OnTokenReceived(null, new TokenReceivedEventArgs(fcmToken));
            }
            else
            {
                Debug.LogError($"Firebase dependencies not available: {dependencyStatus}");
            }
        });
    }
    private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
        Debug.Log("Received Registration Token: " + token.Token);
        // if (!IsApplicationFirstTimeOpened) 
        //     return; //todo not sure if this is needed seems like its blocking fcm cloud registration
        StartCoroutine(SetFCMDeviceToken(token.Token));
        IsApplicationFirstTimeOpened = false;
    }
    private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        //todo: switch screen to reciever screen
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
        Debug.Log("Received a new message!");
        Debug.Log("Message Raw Data:" + e.Message.RawData);
        Vector2 target = new Vector2();
        // Extract custom data from the message
        foreach (var entry in e.Message.Data)
        {
            string key = entry.Key;
            string value = entry.Value;
            Debug.LogFormat("Custom Data - Key: {0}, Value: {1}", key, value);

            // Handle custom data here
            if (key == "lat" && double.TryParse(value, out double latValue))
            {
                Debug.Log("Notification Lat: " + value);
                target.y = (float)latValue;
                Debug.Log("SetTargetY");
            }
            else if (key == "lng" && double.TryParse(value, out double lngValue))
            {
                Debug.Log("Notification Lng: " + value);
                target.x = (float)lngValue;
                Debug.Log("SetTargetX");
            }
        }
        
        Debug.LogFormat("Moving to Point ({0}, {1})", target.x, target.y);
        //_map.position = target;
        if (target != new Vector2(0, 0))
        {
            StartCoroutine(MoveMap(target));
        }
    }
    
    private IEnumerator MoveMap(Vector2 target)
    {
        yield return new WaitForSeconds(1.5f);
        if (!_dragAndZoomInertia.isZooming)
        {
            Debug.LogFormat("Zooming to Point ({0}, {1})", target.x, target.y);
            Debug.Log("Zoom to User");
            _map.zoom = 17;
            _map.position = target;
            // Note: You might need to modify the values in the ZoomToObject method as needed
            //StartCoroutine(_dragAndZoomInertia.ZoomToObject(target, 10, 5f));
        }
    }
    
    IEnumerator SetFCMDeviceToken(string token)
    {
        Debug.Log("Setting FCMDeviceToken()");
        var DBTaskSetUserFriends = _databaseReference.Child("FcmTokens").Child(_firebaseUser.UserId).SetValueAsync(token);
        while (DBTaskSetUserFriends.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        Debug.Log("SET FCM TOKEN!");
    }
    IEnumerator RemoveFCMDeviceToken()
    {
        var DBTaskSetUserFriends = _databaseReference.Child("FcmTokens").Child(_firebaseUser.UserId).SetValueAsync("null");
        while (DBTaskSetUserFriends.IsCompleted is false)
            yield return new WaitForEndOfFrame();
    }
    public async Task<string> GetDeviceTokenForUser(string firebaseUserId)
    {
        Debug.Log("Getting Device Token For:" + firebaseUserId);
        string deviceToken = null;

        await _databaseReference
            .Child("FcmTokens")
            .Child(firebaseUserId)
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("GetDeviceTokenForUser: Fetching device token was canceled.");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"GetDeviceTokenForUser: Failed to fetch device token. Error: {task.Exception}");
                }
                else if (task.IsCompleted)
                {
                    var snapshot = task.Result;
                    deviceToken = snapshot.Value as string;
                }
            });

        Debug.Log("Fetched FcmToken: " + deviceToken);
        return deviceToken;
    }
}
