using System;
using System.Collections;
using System.Collections.Generic;
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
    private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
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
