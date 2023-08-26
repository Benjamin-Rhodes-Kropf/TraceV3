using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.OnlineMapsExamples;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Unity.Notifications.iOS;
using UnityEngine;

public class TraceManager : MonoBehaviour
{
    // Required package "com.unity.mobile.notifications": "2.1.1",
    public static TraceManager instance;
    [SerializeField] private HomeScreenManager homeScreenManager;
    [SerializeField] private OnlineMapsControlBase onlineMapsControlBase;
    [SerializeField] private ScaleMapElements _scaleMapElements;
    [SerializeField] private OnlineMaps onlineMaps;
    [SerializeField] private OnlineMapsMarkerManager markerManager;
    [SerializeField] private OnlineMapsLocationService onlineMapsLocationService;
    [SerializeField] private DrawTraceOnMap drawTraceOnMap;
    [SerializeField] private SendOrRecievedViewSelectorManager sentRecivedToggle;
    [SerializeField] private Vector2 userLocation;
    [SerializeField] private DragAndZoomInertia _dragAndZoomInertia;
    public string currentlyClickingTraceID;
    public List<TraceObject> receivedTraceObjects;
    public List<TraceObject> sentTraceObjects; 
    private List<TraceObject> recivedTraceObjectsByDistanceToUser; //todo: why tf isnt this getting used?

    [Header("Variables")] 
    [SerializeField] private float pinModeMultiplyer;
    [SerializeField] private AnimationCurve _clickRadiusAnimationCurve;

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
            foreach (var trace in receivedTraceObjects)
            {
                double distanceFromMouse = 0;
                if (trace.marker.isShowingPrimaryTexture)
                {
                    distanceFromMouse = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, trace.radius*1000f);
                }
                else
                {
                    distanceFromMouse = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, _clickRadiusAnimationCurve.Evaluate(onlineMaps.floatZoom)*pinModeMultiplyer);
                }
                
                if (distanceFromMouse < 0 && (trace.HasBeenOpened || trace.canBeOpened))
                {
                    accessibleTraces.Add((trace, distanceFromMouse));
                }
                else if (distanceFromMouse < 0 && !trace.HasBeenOpened && !trace.canBeOpened) 
                {
                    viewableAbleTraces.Add((trace, distanceFromMouse));
                }
            }
        }
        else
        {
            foreach (var trace in sentTraceObjects)
            {
                double distance = 0;
                if (trace.marker.isShowingPrimaryTexture)
                {
                    distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, trace.radius*1000f);
                }
                else
                {
                    distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.lat, (float)trace.lng, _clickRadiusAnimationCurve.Evaluate(onlineMaps.floatZoom)*pinModeMultiplyer);
                }

                //open all sent traces
                if (distance < 0)
                {
                    accessibleTraces.Add((trace, distance));
                }
                // accessibleTraces.Add((trace, distance));
                
                // if (distance < 0 && !trace.hasBeenOpened && !trace.canBeOpened)
                // {
                //     viewableAbleTraces.Add((trace, distance));
                // }
            }   
        }
        
        //open based on sort
        if (accessibleTraces.Count > 0)
        {
            accessibleTraces.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
            var traceToOpen = accessibleTraces[accessibleTraces.Count - 1];
            
            Debug.Log("OPEN TRACE:" + traceToOpen.Item1.id);
            Debug.Log("OPEN TRACE: can be opened:" + traceToOpen.Item1.canBeOpened);
            Debug.Log("OPEN TRACE: has been opened:" + traceToOpen.Item1.HasBeenOpened);
            
            //keep track of which trace should be hollow for if we redraw the map after action taken
            currentlyClickingTraceID = traceToOpen.Item1.id;
            
            //convert trace image to hollow if clicked
            traceToOpen.Item1.marker.displayedTexture =  traceToOpen.Item1.marker.primaryHollowInTexture;

            HapticManager.instance.PlaySelectionHaptic();
            FbManager.instance.AnalyticsOnTracePressed(traceToOpen.Item1.senderName, traceToOpen.Item1.sendTime, "open");
            StartCoroutine(_dragAndZoomInertia.ZoomToObject(new Vector2((float)traceToOpen.Item1.lng, (float)traceToOpen.Item1.lat), -traceToOpen.Item1.radius, 0.1f));
            homeScreenManager.OpenTrace(traceToOpen.Item1);
            homeScreenManager.UpdateLocationText(17);
        }
        else if(viewableAbleTraces.Count > 0)
        {
            viewableAbleTraces.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
            var traceToView = viewableAbleTraces[viewableAbleTraces.Count - 1];
            
            Debug.Log("VIEW TRACE:" + traceToView.Item1.id);
            Debug.Log("VIEW TRACE: can be opened:" + traceToView.Item1.canBeOpened);
            Debug.Log("VIEW TRACE: has been opened:" + traceToView.Item1.HasBeenOpened);
            
            HapticManager.instance.PlaySelectionHaptic();
            StartCoroutine(_dragAndZoomInertia.ZoomToObject(new Vector2((float)traceToView.Item1.lng, (float)traceToView.Item1.lat), -traceToView.Item1.radius, 0.1f));
            homeScreenManager.ViewTrace( traceToView.Item1.senderName,traceToView.Item1.sendTime, traceToView.Item1.people);
            homeScreenManager.UpdateLocationText(17);
            FbManager.instance.AnalyticsOnTracePressed(traceToView.Item1.senderName, traceToView.Item1.sendTime, "view");
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
        distance = distance * 1000f; //meters
        distance -= radiusOfTraceInMeters; //account for trace radius
        return distance;
    }
    public IOrderedEnumerable<TraceObject> OrderTracesByDistanceToUser()
    {
        foreach (var traceObject in receivedTraceObjects)
        {
            traceObject.distanceToUser = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(
                userLocation.x,userLocation.y,(float)traceObject.lat, (float)traceObject.lng, traceObject.radius);
        }
        var traceObjectsInOrderOfDistance = receivedTraceObjects.OrderBy(f => f.distanceToUser);
        return traceObjectsInOrderOfDistance;
    }
    private List<TraceObject> ApplyDistanceFilterTraces(float userLat, float userLon)
    {
        var filtered = new List<(TraceObject, double)>();
        foreach (var trace in receivedTraceObjects)
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
        if (receivedTraceObjects.Count < 1)
        {
            Debug.Log("UpdateNotificationsForNextTraces: No Traces Available!");
            return;
        }

        receivedTraceObjects = ApplyDistanceFilterTraces(_previousLatitude, _previousLongitude);

        for (var i = 0; i < receivedTraceObjects.Count && i < 50; i++)
        {
            var trace = receivedTraceObjects[i];
            if (!trace.HasBeenOpened)
            {
                ScheduleNotificationOnEnterInARadius((float)trace.lat, (float)trace.lng,trace.radius, " ", trace.senderName);
            }
        }
        
        //Schedule prompt to tell user to send a trace
        ScheduleNotificationOnExitInARadius(onlineMapsLocationService.position.x, onlineMapsLocationService.position.y, 1000);
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
        // Debug.Log("Push Notification is set for a radius of " + enterLocationTrigger.Radius + "Meters"
        //           + " When user enters in " + "Latitude = " + latitude + "===" + "Longitude = " + longitude);

        var entryBasedNotification = new iOSNotification
        {
            Title = "You Found Trace!",
            Subtitle =  "Left Here By " + SenderName,
            Body = "",
            //Body = message == "" ? "Radius latitude was > " + latitude + " and longitude was > " + longitude : message,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = enterLocationTrigger
        };
        
        // Schedule notification for entry base
        iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
    }
    private static void ScheduleNotificationOnExitInARadius(float latitude, float longitude, float radius)
    {
        var exitLocationTrigger = new iOSNotificationLocationTrigger
        {
            Center = new Vector2(latitude, longitude),
            Radius = radius,
            NotifyOnEntry = false,
            NotifyOnExit = true
        };
        
        var entryBasedNotification = new iOSNotification
        {
            Title = "Cool Spot?",
            Subtitle =  "Leave a Trace!",
            Body = "",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = exitLocationTrigger
        };
        
        // Schedule notification for entry base
        iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
    }
    
    public void StopLocationServices()
    {
        //If required then we can stop the location service by this
        Input.location.Stop();
    }
    public int GetReceivedTraceIndexByID(string traceID)
    {
        int index = receivedTraceObjects.FindIndex(p => p.id == traceID);
        if (index == -1) // FindIndex returns -1 if no match is found.
        {
            Debug.LogError("Failed to GetReceivedTraceIndexByID:" + traceID);
        }
        return index;
    }



    // Update is called once per frame //todo: move out of update
    void UpdateTracesOnMap()
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
            foreach (var traceObject in receivedTraceObjects)
            {
                var dist = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates((float)traceObject.lat, (float)traceObject.lng, currentLatitude, currentLongitude, (float)(traceObject.radius*1000));
                if (!traceObject.hasBeenAdded)
                    traceObject.marker = drawTraceOnMap.DrawCircle(traceObject.lat, traceObject.lng, (traceObject.radius), GetTraceType(dist, traceObject), traceObject.id);
            }
        }
        else
        {
            foreach (var traceobject in sentTraceObjects)
            {
                if (!traceobject.hasBeenAdded)
                {
                    traceobject.marker = drawTraceOnMap.DrawCircle(traceobject.lat, traceobject.lng, (traceobject.radius), DrawTraceOnMap.TraceType.SENT, traceobject.id);
                    traceobject.hasBeenAdded = true;
                }
            }

            if (SendTraceManager.instance.isSendingTrace)
            {
                Debug.Log("Drawing Sending Trace Loading On Map");
                var loadingTraceObject = drawTraceOnMap.sendingTraceTraceLoadingObject;
                drawTraceOnMap.DrawCircle(loadingTraceObject.lat, loadingTraceObject.lng, loadingTraceObject.radius, DrawTraceOnMap.TraceType.SENDING, loadingTraceObject.id);
            }
        }
    }

    private DrawTraceOnMap.TraceType GetTraceType(double dist, TraceObject traceObject)
    {
        if (!traceObject.HasBeenOpened)
        {
            if (currentlyClickingTraceID == traceObject.id)
            {
                if (dist < 0)
                {
                    //if they can open the trace draw trace opening color depending on friendship
                    traceObject.canBeOpened = true;
                    if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).isBestFriend)
                    {
                        return DrawTraceOnMap.TraceType.OPENINGBESTFRIEND;
                    }
                    else
                    {
                        return DrawTraceOnMap.TraceType.OPENING;
                    }
                }
                else
                {
                    //if they cant open the trace draw trace recieved color depending on friendship
                    traceObject.canBeOpened = false;
                    if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).isBestFriend)
                    {
                        return DrawTraceOnMap.TraceType.RECEIVEDBESTFRIEND;
                    }
                    else
                    {
                        return DrawTraceOnMap.TraceType.RECEIVED;
                    }
                }
            } //if they are not clicking on the trace
            else
            {
                if (dist < 0)
                {
                    //if they can open the trace draw trace opening color depending on friendship
                    traceObject.canBeOpened = true;
                }
                else
                {
                    //if they cant open the trace draw trace color depending on friendship
                    traceObject.canBeOpened = false;
                }
                if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).isBestFriend)
                {
                    return DrawTraceOnMap.TraceType.RECEIVEDBESTFRIEND;
                }
                else
                {
                    return DrawTraceOnMap.TraceType.RECEIVED;
                }
            }
        }
        else //trace object has been opened
        {
            traceObject.canBeOpened = true; //all trace that have been opened can be opened regardless of distance
            if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).isBestFriend)
            {
                return DrawTraceOnMap.TraceType.OPENEDBESTFRIEND;
            }
            else
            {
                return DrawTraceOnMap.TraceType.OPENED;
            }
        }
    }

    
    
    
    public void TraceViewSwitched()
    {
        ClearTracesOnMap();
        if (!HomeScreenManager.isInSendTraceView)
        {
            foreach (var traceobject in receivedTraceObjects)
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
        UpdateTracesOnMap();
    }

    
    public void UpdateMap(Vector2 vector2)
    {
        // Debug.Log("Map Update");
        ClearTracesOnMap();
        UpdateTracesOnMap();
        _scaleMapElements.UpdateAllTraceScale();
    }
    public void ClearTracesOnMap()
    {
        drawTraceOnMap.Clear();
        foreach (var traceobject in receivedTraceObjects)
        {
            traceobject.hasBeenAdded = false;
        }
        foreach (var traceobject in sentTraceObjects)
        {
            traceobject.hasBeenAdded = false;
        }
    }
    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        ScheduleNotifications();
    }
    
    private void OnApplicationPaused()
    {
        Debug.Log("Application Paused after " + Time.time + " seconds");
        ScheduleNotifications();
    }

    private void ScheduleNotifications()
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
    }
}

[Serializable]
public class TraceCommentObject
{
    public string id;
    public string time;
    public string senderName;
    public string senderID;
    public string audio;
    public TraceCommentObject(string id, string time, string senderID, string senderName,  string audio)
    {
        this.id = id;
        this.time = time;
        this.senderName = senderName;
        this.senderID = senderID;
        this.audio = audio;
    }
}

[Serializable]
public class TraceReceiverObject
{
    public string id;
    public string photo;
    public bool hasBeenOpened;
    public TraceReceiverObject(string id, bool hasBeenOpened, string photo = null)
    {
        this.id = id;
        this.hasBeenOpened = hasBeenOpened;
        this.photo = photo;
    }
}

[Serializable]
public class TraceObject
{
    [JsonIgnore]
    public OnlineMapsMarker marker;
    
    public string id;
    public double lat;
    public double lng;
    public float radius;
    public List<TraceReceiverObject> people; //replace this with recievers
    public List<TraceCommentObject> comments;
    public double distanceToUser;
    public string mediaType;
    public string senderID;
    public string senderName;
    public bool hasBeenAdded;
    public bool canBeOpened = false;
    private bool _hasBeenOpened = false;
    public string sendTime;
    public double endTimeStamp;
    
    // Getter and Setter for hasBeenOpened
    public bool HasBeenOpened
    {
        get { return _hasBeenOpened; }
        set
        {
            _hasBeenOpened = value;
            if (value == true)
            {
                marker.Dispose(); //destroy marker as it is now rendering the wrong thing //todo: set value when trace opened
            }
        }
    }
    
    public TraceObject(double longitude, double latitude, float radius, List<TraceReceiverObject> people, string senderID, string senderName, string sendTime, double endTimeStamp, string mediaType, string id, bool hasBeenOpened)
    {
        lng = longitude;
        lat = latitude;
        this.radius = radius;
        this.senderID = senderID;
        this.people = people;
        this.senderName = senderName;
        this.sendTime = sendTime;
        this.endTimeStamp = endTimeStamp;
        this.mediaType = mediaType;
        this.id = id;
        _hasBeenOpened = hasBeenOpened; //dont use setter because we dont want to destroy objects coming from memory
    }
}