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
    
    public void SendComment(string fileLocation, TraceObject traceObject)
    {
        Debug.Log("SEND TRACE!");
        SendLocalNotification("Sending Trace", "hang on while we upload it!", 1f);
        this.traceObject = traceObject;
        FbManager.instance.UploadComment(traceObject, fileLocation);
    }
    
    public void SendNotificationToUsersWhoRecivedTheComment()
    {
        //users
        foreach (var user in traceObject.people)
        {
            try //no clue why this makes it work
            {
                StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user.id, FbManager.instance.thisUserModel.name, "Commented on a Trace!", (float)traceObject.lng,(float)traceObject.lat));
            }
            catch (Exception e)
            {
                Debug.Log("Notification failed... trying again");
                StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user.id, FbManager.instance.thisUserModel.name, "Commented on a Trace!", (float)traceObject.lng,(float)traceObject.lat));
            }
        }
        SendLocalNotification("Comment Sent", "lets hope they find it!",1f);
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
