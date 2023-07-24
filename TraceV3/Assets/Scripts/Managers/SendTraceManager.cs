using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendTraceManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendTraceManager instance;
    
    [Header("external values")]
    [SerializeField]private OnlineMapsLocationService _onlineMapsLocationService;

    [Header("Trace Values")]
    [SerializeField]private const float maxRadius = 0.8f;
    [SerializeField]private const float minRadius = 0.02f;
    public float selectedRadius;
    public bool isSendingTrace;
    public Vector2 location;
    public string fileLocation;
    public MediaType mediaType;
    public List<String> usersToSendTrace;
    
    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void SetRadius(float radius)
    {
        this.selectedRadius = scale(0, 1, minRadius, maxRadius, radius);
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
        FbManager.instance.UploadTrace(fileLocation, selectedRadius, location, mediaType,usersToSendTrace);
    }

    public void SendNotificationToUsersWhoRecivedTheTrace()
    {
        foreach (var user in usersToSendTrace)
        {
            try //no clue why this makes it work
            {
                StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.name, "Sent You A Trace!", location.y,location.x));
            }
            catch (Exception e)
            {
                Debug.Log("Notification failed... trying again");
                StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.name, "Sent You A Trace!", location.y,location.x));
            }
        }
    }
}

public enum MediaType
{
    PHOTO,
    VIDEO
}
