using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.iOS;
using UnityEngine;

public class SendCommentManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendCommentManager instance;

    [Header("Comment Values")] 
    public TraceObject traceObject;
    
    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void SendComment(string fileLocation, TraceObject traceObject, float[] extractedValues)
    {
        Debug.Log("Sending Comment!");
        //SendLocalNotification("Sending Comment", "hang on while we upload it!", 1f);
        this.traceObject = traceObject;
        FbManager.instance.UploadComment(traceObject, fileLocation, extractedValues);
    }
    
    public void SendNotificationToUsersWhoRecivedTheComment()
    {
        string displayName = FbManager.instance.thisUserModel.name;
        //users
        string message = "commented on " + traceObject.senderName + "'s " + "trace!";
        foreach (var user in traceObject.people)
        {
            if(user.id != traceObject.senderID && user.id != FbManager.instance.thisUserModel.userID)
                NotificationManager.Instance.StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user.id, displayName, message, (float)traceObject.lng,(float)traceObject.lat));
        }
        
        if(FriendsModelManager.Instance.GetRelationship(traceObject.senderID) != Relationship.SuperUser && FbManager.instance.thisUserModel.userID != traceObject.senderID)
            NotificationManager.Instance.StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(traceObject.senderID, displayName, "commented on your trace!", (float)traceObject.lng,(float)traceObject.lat));
    }
    
    public void SendLocalNotification(string title, string message, float delayInSeconds)
    {
        Debug.Log("Sending Notification");
        TimeSpan delay = TimeSpan.FromSeconds(delayInSeconds);

        iOSNotification notification = new iOSNotification
        {
            Title = title,
            Body = message,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            Trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = delay, // Use TimeSpan instead of double
                Repeats = false
            }
        };
        iOSNotificationCenter.ScheduleNotification(notification);
    }
}
