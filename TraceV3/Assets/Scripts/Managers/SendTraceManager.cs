using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendTraceManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendTraceManager instance;
    
    [Header("Trace Values")]
    public bool isSendingTrace;
    public bool isOpeningTrace;
    public Vector2 location;
    public string fileLocation;
    public MediaType mediaType;
    public OnlineMapsLocationService _onlineMapsLocationService;
    [SerializeField]private float maxRadius;
    [SerializeField]private float minRadius;
    public float radius;
    public List<String> usersToSendTo;
    
    
    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue){
 
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
 
        return(NewValue);
    }
    public void SetRadius(float radius)
    {
        this.radius = scale(0, 1, minRadius, maxRadius, radius);
    }
    
    public void SendTrace()
    {
        Debug.Log("SEND TRACE!");
        location = _onlineMapsLocationService.GetUserLocation();
        FbManager.instance.UploadTrace(fileLocation, radius, location, mediaType,usersToSendTo);
        // foreach (var user in usersToSendTo)
        // {
        //     Debug.Log("sending to:" + user);
        //     try //no clue why this makes it work
        //     {
        //         StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.name, "Sent You A Trace!", location.y,location.x));
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log("Notification failed... trying again");
        //         StartCoroutine(NotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.name, "Sent You A Trace!", location.y,location.x));
        //     }
        // }
    }

    public void TraceSentSuccsefuly()
    {
        foreach (var user in usersToSendTo)
        {
            Debug.Log("sending to:" + user);
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
