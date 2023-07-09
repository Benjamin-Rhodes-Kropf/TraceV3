using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendTraceManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static SendTraceManager instance;
    
    [Header("Trace Values")]
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
        Vector2 location = _onlineMapsLocationService.GetUserLocation();
        FbManager.instance.UploadTrace(fileLocation, radius, location, mediaType,usersToSendTo);
        foreach (var user in usersToSendTo)
        {
            BackgroundNotificationManager.Instance.SendNotificationUsingFirebaseUserId(user, FbManager.instance.thisUserModel.DisplayName, "sent you a trace!");
        }
    }
}

public enum MediaType
{
    PHOTO,
    VIDEO
}
