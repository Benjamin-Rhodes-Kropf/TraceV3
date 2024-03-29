using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.Notifications.iOS;


[Serializable]
public class DateTimeOption
{
    public string text;
    public int hoursFromNow;

    public DateTimeOption(string text, int hoursFromNow)
    {
        this.text = text;
        this.hoursFromNow = hoursFromNow;
    }
}

public class SendTraceManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendTraceManager instance;
    
    [Header("external values")]
    [SerializeField]private OnlineMapsLocationService _onlineMapsLocationService;
    
    [Header("Trace Values")]
    [SerializeField]private const float maxRadius = 0.8f;
    [SerializeField]private const float minRadius = 0.02f;
    public List<DateTimeOption> TraceExpirationOptions;
    public float selectedRadius;
    public bool isSendingTrace;
    public Vector2 location;
    public string fileLocation;
    public MediaType mediaType;
    public List<String> usersToSendTrace;
    public List<String> phonesToSendTrace;
    public DateTime expiration;
    [SerializeField] private string debugExpiration;
    public bool sendToFollowers;


    [Header("Analytics Values")] 
    public float videoLength;
    public int camFlippedCount;
    
    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        sendToFollowers = false;
    }

    public void SetInitExpiration()
    {
        SetExpiration(DateTime.Now.AddHours(TraceExpirationOptions[0].hoursFromNow));
    }
    
    public void SetRadius(float radius)
    {
        this.selectedRadius = scale(0, 1, minRadius, maxRadius, radius);
    }

    public void SetExpiration(DateTime expiration)
    {
        this.expiration = expiration;
        debugExpiration = expiration.ToString();
    }
    
    public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue){
 
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
 
        return(NewValue);
    }
    
    
    public void SendTrace()
    {
        Debug.Log("SEND TRACE!");
        location = _onlineMapsLocationService.GetUserLocation();
        if (selectedRadius == 0)
        {
            Debug.Log("Selected Radius Set Wrong");
            selectedRadius = 0.4f; //todo: this is jank
        }

        //NotificationManager.Instance.SendLocalNotification("Sending Trace", "hang on while we upload it!", 1f);
        FbManager.instance.UploadTrace(usersToSendTrace, phonesToSendTrace, fileLocation, selectedRadius, location, mediaType, sendToFollowers, expiration);
        SendBulkSMS.Instance.SendTraceSMS(phonesToSendTrace, new Vector2(location.y, location.x));
        FbManager.instance.AnalyticsOnSendTrace(usersToSendTrace.Count, videoLength, camFlippedCount);
    }
    
    public void SendNotificationToUsersWhoRecivedTheTrace()
    {
        //Dont Send Notification... it is annoying
        // foreach (var user in usersToSendTrace)
        // {
        //     if(FriendsModelManager.Instance.GetRelationship(user) != Relationship.SuperUser)
        //         NotificationManager.Instance.StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.name, "Left You A Trace!", location.y,location.x));
        // }
    }
}

public enum MediaType
{
    PHOTO,
    VIDEO
}
