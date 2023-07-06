using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.OnlineMapsExamples;
using Unity.Notifications.iOS;
using UnityEngine;

public class TraceManager : MonoBehaviour
{
    // Required package "com.unity.mobile.notifications": "2.1.1",
    public static TraceManager instance;
    [SerializeField] private HomeScreenManager homeScreenManager;
    [SerializeField] private OnlineMapsControlBase onlineMapsControlBase;
    [SerializeField] private OnlineMapsMarkerManager markerManager;
    [SerializeField] private OnlineMapsLocationService onlineMapsLocationService;
    [SerializeField] private DrawTraceOnMap drawTraceOnMap;
    [SerializeField] private SendOrRecievedViewSelectorManager sentRecivedToggle;
    [SerializeField] private Vector2 userLocation;
    public string currentlyClickingTraceID;
    public List<TraceObject> recivedTraceObjects;
    public List<TraceObject> sentTraceObjects;
    public List<TraceObject> recivedTraceObjectsByDistanceToUser;

    [Header("Variables")] 
    [SerializeField] private float startingPointLatitude;
    [SerializeField] private float startingPointLongitude;
    
    [Header("Maximum Distance in meters")]
    [SerializeField] private double maxDist;
    
    private bool _areTracesInitialized;
    private double _distance;
    private float _previousLatitude, _previousLongitude;

    private void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        recivedTraceObjectsByDistanceToUser = new List<TraceObject>();
        _areTracesInitialized = false;
        Application.runInBackground = true;
        onlineMapsLocationService.updatePosition = true;
        
        //handle map updates
        onlineMapsControlBase.OnMapClick += HandleMapClick;
        onlineMapsLocationService.OnLocationChanged += UpdateMap;
    }
    
    private void HandleMapClick()
    {
        Vector2 mouseLatAndLong = markerManager.GetMouseLatAndLong();
        var accessibleTraces = new List<(TraceObject, double)>();
        var viewableAbleTraces = new List<(TraceObject, double)>();
        
        //sort traces based on location of click and which view the user is in
        if (!HomeScreenManager.isInSendTraceView)
        {
            foreach (var trace in recivedTraceObjects)
            {
                var distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, trace.radius*1000f);
                Debug.Log( "Trace:"+trace.id +" Dist: " + distance);
                if (distance < 0 && !trace.hasBeenOpened && trace.canBeOpened)
                {
                    accessibleTraces.Add((trace, distance));
                }else if (distance < 0 && !trace.hasBeenOpened && !trace.canBeOpened)
                {
                    viewableAbleTraces.Add((trace, distance));
                }
            }
        }
        else
        {
            foreach (var trace in sentTraceObjects)
            {
                var distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, trace.radius*1000f);
                Debug.Log( "Trace:"+trace.id +" Dist: " + distance);
                if (distance < 0 && !trace.hasBeenOpened && !trace.canBeOpened)
                {
                    viewableAbleTraces.Add((trace, distance));
                }
            }   
        }
        
        //open based on sort
        if (accessibleTraces.Count > 0)
        {
            accessibleTraces.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
            var traceToOpen = accessibleTraces[accessibleTraces.Count - 1];
            currentlyClickingTraceID = traceToOpen.Item1.id;
            
            //get marker currently being opened and change its texture to hollow
            foreach (var marker in markerManager.items)
            {
                if (marker.traceID == traceToOpen.Item1.id)
                {
                    marker.displayedTexture = marker.primaryHollowInTexture;
                }
            }
            
            homeScreenManager.OpenTrace(traceToOpen.Item1.id,  traceToOpen.Item1.senderName,traceToOpen.Item1.senderID,traceToOpen.Item1.sendTime, traceToOpen.Item1.mediaType);
        }
        else if(viewableAbleTraces.Count > 0)
        {
            viewableAbleTraces.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
            var traceToView = viewableAbleTraces[viewableAbleTraces.Count - 1];
            homeScreenManager.ViewTrace( traceToView.Item1.senderName,traceToView.Item1.sendTime);
        }
    }
    
    
    
    private static double ApproximateDistanceBetweenTwoLatLongsInM(double lat1, double lon1, double lat2, double lon2)
    {
        var ldRadians = lat1 / 57.3 * 0.017453292519943295769236907684886;
        var ldCosR = Math.Cos(ldRadians);
        var x = 69.1 * (lat2 - lat1);
        var y = 69.1 * (lon2 - lon1) * ldCosR;

        return Math.Sqrt(x * x + y * y) * 1.609344 * 1000; /* Converts mi to km to m. */
    }
    private double CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(float lat1, float lon1, float lat2, float lon2, float radiusOfTraceInMeters)
    {
        double distance;
        var R = 6378.137; // Radius of earth in KM
        var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
        var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        distance = R * c;
        distance = distance * 1000f; // meters
        distance -= radiusOfTraceInMeters; //account for trace radius
        return distance;
    }
    public IOrderedEnumerable<TraceObject> OrderTracesByDistanceToUser()
    {
        foreach (var traceObject in recivedTraceObjects)
        {
            traceObject.distanceToUser = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(
                userLocation.x,userLocation.y,(float)traceObject.lat, (float)traceObject.lng, traceObject.radius);
        }
        var traceObjectsInOrderOfDistance = recivedTraceObjects.OrderBy(f => f.distanceToUser);
        return traceObjectsInOrderOfDistance;
    }
    
    private List<TraceObject> ApplyDistanceFilterTraces(float userLat, float userLon)
    {
        var filtered = new List<(TraceObject, double)>();
        foreach (var trace in recivedTraceObjects)
        {
            var distance = ApproximateDistanceBetweenTwoLatLongsInM(userLat, userLon, trace.lat,
                trace.lng);
            filtered.Add((trace, distance));
        }

        filtered.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
        return filtered.Select(i => i.Item1).ToList();
    }
    
    private void UpdateNotificationsForNext50Traces()
    {
        if (recivedTraceObjects.Count < 1)
        {
            Debug.Log("UpdateNotificationsForNextTraces: No Traces Available!");
            return;
        }

        recivedTraceObjects = ApplyDistanceFilterTraces(_previousLatitude, _previousLongitude);

        for (var i = 0; i < recivedTraceObjects.Count && i < 50; i++)
        {
            var trace = recivedTraceObjects[i];
            ScheduleNotificationOnEnterInARadius((float)trace.lat, (float)trace.lng,trace.radius, " ", trace.senderName);
        }
    }
    
    private static void ScheduleNotificationOnEnterInARadius(float latitude, float longitude, float radius, string message, string SenderName)
    {
        var enterLocationTrigger = new iOSNotificationLocationTrigger
        {
            Center = new Vector2(latitude, longitude),
            Radius = radius,
            NotifyOnEntry = true,
            NotifyOnExit = false
        };
        Debug.Log("Push Notification is set for a radius of " + enterLocationTrigger.Radius + "Meters"
                  + " When user enters in " + "Latitude = " + latitude + "===" + "Longitude = " + longitude);

        var entryBasedNotification = new iOSNotification
        {
            Title = SenderName,
            Subtitle =  "Left You A Trace Here",
            Body = "",
            //Body = message == "" ? "Radius latitude was > " + latitude + " and longitude was > " + longitude : message,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = enterLocationTrigger
        };
        
        // Schedule notification for entry base
        iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
    }

    private void UpdateNotificationForNextTrace()
    {
        if (recivedTraceObjects.Count < 1)
        {
            Debug.Log("UpdateNotificationForNextTrace: No Traces Available!");
            return;
        }

        var trace = recivedTraceObjects[0];
        ScheduleNotificationOnEnterInARadius((float)trace.lat, (float)trace.lng, trace.radius, trace.senderName + " Left You a Trace Here", trace.senderName);
        //ScheduleNotificationOnExitFromARadius(trace.lat, trace.lng, trace.text);
    }
    public void StopLocationServices()
    {
        //If required then we can stop the location service by this
        Input.location.Stop();
    }
    public int GetRecivedTraceIndexByID(string traceID)
    {
        int counter = 0;
        foreach (var trace in recivedTraceObjects)
        {
            if (trace.id == traceID)
            {
                return counter;
            }
            counter++;
        }
        Debug.LogError("Failed to find" + traceID);
        return -1;
    }



    // Update is called once per frame //todo: move out of update
    void Update()
    {
        Vector2 previousUserLocation = userLocation;
        if (onlineMapsLocationService.IsLocationServiceRunning())
        {
            userLocation = onlineMapsLocationService.position;
        }
        else
        {
            userLocation = onlineMapsLocationService.emulatorPosition;
        }
        if (previousUserLocation != userLocation)
        {
            recivedTraceObjectsByDistanceToUser = OrderTracesByDistanceToUser().ToList();
        }
        
        var currentLatitude = userLocation.y;
        var currentLongitude = userLocation.x;

        // Showing current updated coordinates
        _distance = ApproximateDistanceBetweenTwoLatLongsInM(_previousLatitude, _previousLongitude, currentLatitude, currentLongitude);

        // Detecting the Significant Location Change
        if (_distance > maxDist)
        {
            // Remove All Pending Notifications
            iOSNotificationCenter.RemoveAllScheduledNotifications();

            // Set current player's location
            _previousLatitude = currentLatitude;
            _previousLongitude = currentLongitude;

            // Add Notifications for the Next 10 Distance Filtered Traces
            UpdateNotificationsForNext50Traces();
        }
        
        if (!HomeScreenManager.isInSendTraceView)
        {
            foreach (var traceobject in recivedTraceObjects)
            {
                var dist = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates((float)traceobject.lat, (float)traceobject.lng, currentLatitude, currentLongitude, (float)(traceobject.radius*1000));
                if (!traceobject.hasBeenAdded && !traceobject.hasBeenOpened)
                {
                    if (currentlyClickingTraceID == traceobject.id)
                    {
                        if (dist < 0)
                        {
                            drawTraceOnMap.DrawCirlce(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.OPENING, traceobject.id);
                            traceobject.canBeOpened = true;
                        }
                        else
                        {
                            drawTraceOnMap.DrawCirlce(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.RECEIVED, traceobject.id);
                            traceobject.canBeOpened = false;
                        }
                    }
                    else
                    {
                        if (dist < 0)
                        {
                            drawTraceOnMap.DrawCirlce(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.RECEIVED, traceobject.id);
                            traceobject.canBeOpened = true;
                        }
                        else
                        {
                            drawTraceOnMap.DrawCirlce(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.RECEIVED, traceobject.id);
                            traceobject.canBeOpened = false;
                        }
                    }
                    traceobject.hasBeenAdded = true;
                }
            }
        }
        else
        {
            foreach (var traceobject in sentTraceObjects)
            {
                if (!traceobject.hasBeenAdded)
                {
                    //new Color(71,255,214)
                    drawTraceOnMap.DrawCirlce(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.SENT, traceobject.id);
                    traceobject.hasBeenAdded = true;
                }
            }
        }
    }

    public void TraceViewSwitched()
    {
        if (!HomeScreenManager.isInSendTraceView)
        {
            foreach (var traceobject in recivedTraceObjects)
            {
                traceobject.hasBeenAdded = false;
            }
        }
        else
        {
            foreach (var traceobject in sentTraceObjects)
            {
                traceobject.hasBeenAdded = false;
            }
        }
    }

    public void UpdateMap(Vector2 vector2)
    {
        Debug.Log("Map Update");
        UpdateTracesOnMap();
    }
    public void UpdateTracesOnMap()
    {
        drawTraceOnMap.Clear();
        foreach (var traceobject in recivedTraceObjects)
        {
            traceobject.hasBeenAdded = false;
        }
        foreach (var traceobject in sentTraceObjects)
        {
            traceobject.hasBeenAdded = false;
        }
    }
}

[Serializable]
public class TraceObject
{
    public string id;
    public double lat;
    public double lng;
    public float radius;
    public double distanceToUser;
    public string mediaType;
    public string text;
    public string senderID;
    public string senderName;
    public bool hasBeenAdded;
    public bool canBeOpened = false;
    public bool hasBeenOpened = false;
    public string sendTime;
    public double endTimeStamp;
    
    public TraceObject(double longitude, double latitude, float radius, string senderID, string senderName, string sendTime, double endTimeStamp, string mediaType, string id)
    {
        lng = longitude;
        lat = latitude;
        this.radius = radius;
        this.senderID = senderID;
        this.senderName = senderName;
        this.sendTime = sendTime;
        this.endTimeStamp = endTimeStamp;
        this.mediaType = mediaType;
        this.id = id;
    }
}