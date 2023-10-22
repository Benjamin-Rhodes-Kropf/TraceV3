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
    
    public Dictionary<string, TraceObject> receivedTraceObjects;
    public Dictionary<string, TraceObject> sentTraceObjects; 
    private List<TraceObject> recivedTraceObjectsByDistanceToUser; //todo: why tf isnt this getting used?

    [Header("Variables")] 
    [SerializeField] private float pinModeMultiplyer;
    [SerializeField] private AnimationCurve _clickRadiusAnimationCurve;
    [SerializeField] private bool enableLocationNotificationSchedulingInUnity;

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
        
        receivedTraceObjects = new Dictionary<string, TraceObject>();
        sentTraceObjects = new Dictionary<string, TraceObject>();
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
        
        if (enableLocationNotificationSchedulingInUnity)
        {
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            ScheduleNotifications(false);
        }
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
                if (trace.Value.marker.isShowingPrimaryTexture)
                {
                    distanceFromMouse = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.Value.lat, (float)trace.Value.lng, trace.Value.radius*1000f);
                }
                else
                {
                    distanceFromMouse = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.Value.lat, (float)trace.Value.lng, _clickRadiusAnimationCurve.Evaluate(onlineMaps.floatZoom)*pinModeMultiplyer);
                }
                
                if (distanceFromMouse < 0 && (trace.Value.HasBeenOpened || trace.Value.canBeOpened) && !trace.Value.isExpired)
                {
                    accessibleTraces.Add((trace.Value, distanceFromMouse));
                }
                else if (distanceFromMouse < 0 && !trace.Value.HasBeenOpened && !trace.Value.canBeOpened && !trace.Value.isExpired) 
                {
                    viewableAbleTraces.Add((trace.Value, distanceFromMouse));
                }
            }
        }
        else
        {
            foreach (var trace in sentTraceObjects)
            {
                double distance = 0;
                if (trace.Value.marker.isShowingPrimaryTexture)
                {
                    distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.Value.lat, (float)trace.Value.lng, trace.Value.radius*1000f);
                }
                else
                {
                    distance = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(mouseLatAndLong.y, mouseLatAndLong.x, (float)trace.Value.lat, (float)trace.Value.lng, _clickRadiusAnimationCurve.Evaluate(onlineMaps.floatZoom)*pinModeMultiplyer);
                }

                //open all sent traces
                if (distance < 0 && !trace.Value.isExpired)
                {
                    accessibleTraces.Add((trace.Value, distance));
                }
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
            
            
            
            //if(currentlyClickingTraceID == traceToOpen.Item1.id) 
            
            
            //convert trace image to hollow if clicked
            traceToOpen.Item1.marker.displayedTexture =  traceToOpen.Item1.marker.primaryHollowInTexture;

            HapticManager.instance.PlaySelectionHaptic();
            FbManager.instance.AnalyticsOnTracePressed(traceToOpen.Item1.senderName, traceToOpen.Item1.sendTime, "open");
            StartCoroutine(_dragAndZoomInertia.ZoomToObject(new Vector2((float)traceToOpen.Item1.lng, (float)traceToOpen.Item1.lat), -traceToOpen.Item1.radius, 0.1f));
            homeScreenManager.OpenTrace(traceToOpen.Item1);
            currentlyClickingTraceID = traceToOpen.Item1.id;
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
        foreach (var traceObjectPair in receivedTraceObjects)
        {
            var traceObject = traceObjectPair.Value; // This is your TraceObject.
            traceObject.distanceToUser = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates(
                userLocation.x, userLocation.y, 
                (float)traceObject.lat, 
                (float)traceObject.lng, 
                traceObject.radius);
        }

        // Order the values of the dictionary.
        var traceObjectsInOrderOfDistance = receivedTraceObjects.Values.OrderBy(f => f.distanceToUser);
        return traceObjectsInOrderOfDistance;
    }
    private List<TraceObject> ApplyTraceFilter(float userLat, float userLon)
    {
        var filtered = new List<(TraceObject, double)>();
        foreach (var trace in receivedTraceObjects)
        {
            var distance = ApproximateDistanceBetweenTwoLatLongsInM(userLat, userLon, trace.Value.lat,
                trace.Value.lng);
            
            if (!trace.Value.HasBeenOpened && !trace.Value.isExpired)
            {
                filtered.Add((trace.Value, distance));
            }
        }

        filtered.Sort((i1, i2) => i1.Item2.CompareTo(i2.Item2));
        return filtered.Select(i => i.Item1).ToList();
    }
    private void UpdateNotificationsForNext20Traces()
    {
        Debug.Log("Remove All Scheduled Notifications");
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        
        if (receivedTraceObjects.Count < 1)
        {
            Debug.Log("UpdateNotificationsForNextTraces: No Traces Available!");
            return;
        }

        List<TraceObject> filteredTraces = ApplyTraceFilter(_previousLatitude, _previousLongitude);

        
        for (var i = 0; i < filteredTraces.Count && i < 20; i++)
        {
            var trace = filteredTraces[i];
            if (!trace.HasBeenOpened && !trace.isExpired)
            {
                Debug.Log("placed trace notification:" + i);
                ScheduleNotificationOnEnterInARadius((float)trace.lat, (float)trace.lng, trace.radius, " ", trace.senderName);
            }
        }
        
        //todo: uncomment to place on exit notification
        //Schedule prompt to tell user to send a trace
        //Debug.Log("Placed Exit Trace: lat:" + onlineMapsLocationService.position.x + " long:" + onlineMapsLocationService.position.y);
        //if you move a significant distance
        //ScheduleNotificationOnExitInARadius(onlineMapsLocationService.position.x, onlineMapsLocationService.position.y, 1000);
    }
    private static void ScheduleNotificationOnEnterInARadius(float latitude, float longitude, float radius, string message, string SenderName)
    {
        var enterLocationTrigger = new iOSNotificationLocationTrigger
        {
            Center = new Vector2(latitude, longitude),
            Radius = radius*1.1f, //make radius a bit bigger for better experience
            NotifyOnEntry = true,
            NotifyOnExit = false,
            Repeats = true,
        };

        var entryBasedNotification = new iOSNotification
        {
            Title = "You Found a Trace!",
            Subtitle =  "Left Here By " + SenderName,
            Body = "",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = enterLocationTrigger
        };
        
        // Schedule notification for entry base
        iOSNotificationCenter.ScheduleNotification(entryBasedNotification);
        
        //count and debug number of IOS notifications
        int counter = 0;
        foreach (var iOsNotification in iOSNotificationCenter.GetScheduledNotifications()){
            counter++;
        }
        Debug.Log("number of ios notifications:" + counter);

    }
    private static void ScheduleNotificationOnExitInARadius(float latitude, float longitude, float radius)
    {
        var exitLocationTrigger = new iOSNotificationLocationTrigger
        {
            Latitude = latitude,
            Longitude = longitude,
            Radius = radius,
            NotifyOnEntry = false,
            NotifyOnExit = true,
            Repeats = true
        };
        
        var exitBasedNotification = new iOSNotification
        {
            
            Title = "Cool Spot?",
            Subtitle =  "Leave a Trace!",
            Body = "This might be a good spot to leave a trace",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = exitLocationTrigger
        };
        
        // Schedule notification for entry base
        iOSNotificationCenter.ScheduleNotification(exitBasedNotification);
    }
    public void StopLocationServices()
    {
        //If required then we can stop the location service by this
        Input.location.Stop();
    }
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

        if (!HomeScreenManager.isInSendTraceView)
        {
            foreach (var traceObject in receivedTraceObjects)
            {
                var dist = CalculateTheDistanceBetweenCoordinatesAndCurrentCoordinates((float)traceObject.Value.lat, (float)traceObject.Value.lng, currentLatitude, currentLongitude, (float)(traceObject.Value.radius*1000));
                if (!traceObject.Value.hasBeenAdded)
                {
                    traceObject.Value.marker = drawTraceOnMap.DrawCircle(traceObject.Value.lat, traceObject.Value.lng, (traceObject.Value.radius), GetTraceType(dist, traceObject.Value), traceObject.Value.id);
                }
            }
        }
        else
        {
            foreach (var traceObject in sentTraceObjects)
            {
                if (!traceObject.Value.hasBeenAdded)
                {
                    if (traceObject.Value.isExpired)
                    {
                        traceObject.Value.marker = drawTraceOnMap.DrawCircle(traceObject.Value.lat, traceObject.Value.lng, (traceObject.Value.radius), DrawTraceOnMap.TraceType.EXPIREDOLD, traceObject.Value.id);
                    }
                    else
                    {
                        if(traceObject.Value.hasUpdate)
                            traceObject.Value.marker = drawTraceOnMap.DrawCircle(traceObject.Value.lat, traceObject.Value.lng, (traceObject.Value.radius), DrawTraceOnMap.TraceType.SPECIALClOSED, traceObject.Value.id);
                        else
                            traceObject.Value.marker = drawTraceOnMap.DrawCircle(traceObject.Value.lat, traceObject.Value.lng, (traceObject.Value.radius), DrawTraceOnMap.TraceType.SENT, traceObject.Value.id);
                    }

                    traceObject.Value.hasBeenAdded = true;
                }
            }

            if (SendTraceManager.instance.isSendingTrace)
            {
                var loadingTraceObject = drawTraceOnMap.sendingTraceTraceLoadingObject;
                drawTraceOnMap.DrawCircle(loadingTraceObject.lat, loadingTraceObject.lng, loadingTraceObject.radius, DrawTraceOnMap.TraceType.SENDING, loadingTraceObject.id);
            }
        }
    }

    public void RefreshTrace(TraceObject traceObject)
    {
        homeScreenManager.RefreshTraceView(traceObject);
    }

    public int CheckExpiration(DateTime expirationDate)
    {
        DateTime currentDate = DateTime.Now;
        TimeSpan timeDifference = currentDate - expirationDate;

        if (timeDifference.TotalDays >= 7)
        {
            return 3; // Expired by a week or more
        }
        else if (timeDifference.TotalDays >= 1)
        {
            return 2; // Expired by a day or more
        }
        else
        {
            return 1; // Not expired or expired within the day
        }
    }
    
    private DrawTraceOnMap.TraceType GetTraceType(double dist, TraceObject traceObject)
    {
        if (traceObject.isExpired)
        {
            int expirationResult = CheckExpiration(traceObject.expiration);
            switch (expirationResult)
            {
                case 1:
                    return DrawTraceOnMap.TraceType.EXPIREDMOSTRECENT;
                case 2:
                    return DrawTraceOnMap.TraceType.EXPIREDRECENT;
                case 3:
                    return DrawTraceOnMap.TraceType.EXPIREDOLD;
                default:
                    throw new ArgumentOutOfRangeException("Invalid expiration result");
            }
        }
            
        
        if (!traceObject.HasBeenOpened)
        {
            if (currentlyClickingTraceID == traceObject.id)
            {
                if (dist < 0)
                {
                    //if they can open the trace draw trace opening color depending on friendship
                    traceObject.canBeOpened = true;
                    if(traceObject.hasUpdate)
                        return DrawTraceOnMap.TraceType.SPECIALOPENED;
                    if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).relationship == Relationship.BestFriend)
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
                    if(traceObject.hasUpdate)
                        return DrawTraceOnMap.TraceType.SPECIALClOSED;
                    
                    if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).relationship == Relationship.BestFriend)
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
                
                //has update for trace you cant get yet
                if(traceObject.hasUpdate)
                    return DrawTraceOnMap.TraceType.SPECIALClOSED;
                
                if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).relationship == Relationship.BestFriend)
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
            
            if(traceObject.hasUpdate)
                return DrawTraceOnMap.TraceType.SPECIALOPENED;
            
            if (FriendsModelManager.GetFriendModelByOtherFriendID(traceObject.senderID).relationship == Relationship.BestFriend)
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
                traceobject.Value.hasBeenAdded = false;
            }
        }
        else
        {
            foreach (var traceobject in sentTraceObjects)
            {
                traceobject.Value.hasBeenAdded = false;
            }
        }
        UpdateTracesOnMap();
        _scaleMapElements.UpdateAllTraceScale();
    }
    public void UpdateMap(Vector2 vector2)
    {
        ClearTracesOnMap();
        UpdateTracesOnMap();
        _scaleMapElements.UpdateAllTraceScale();
        //ScheduleNotifications(false); //todo: make run only when application quit
    }
    public void ClearTracesOnMap()
    {
        drawTraceOnMap.Clear();
        foreach (var traceobject in receivedTraceObjects)
        {
            traceobject.Value.hasBeenAdded = false;
        }
        foreach (var traceobject in sentTraceObjects)
        {
            traceobject.Value.hasBeenAdded = false;
        }
    }
    
    
    //when app is being exited
    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds: running ScheduleNotifications");
        if(enableLocationNotificationSchedulingInUnity)
            ScheduleNotifications(true); 
        else
            Debug.Log("Location notification scheduling in Unity Is turned Off");

        if (FbManager.instance.useBackgroundLocationTasks)
        {
            BackgroundTasksBridge.Instance.NativeMonitorMyLocation(_previousLatitude, _previousLongitude, 100);
        }
    }
    private void OnApplicationPaused()
    {
        Debug.Log("Application Paused after " + Time.time + " seconds: running ScheduleNotifications");
        if(enableLocationNotificationSchedulingInUnity)
            ScheduleNotifications(true); 
        else
            Debug.Log("Location notification scheduling in Unity Is turned Off");
        
        if (FbManager.instance.useBackgroundLocationTasks)
        {
            BackgroundTasksBridge.Instance.NativeMonitorMyLocation(_previousLatitude, _previousLongitude, 100);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Debug.Log("Application Will Resign Active after " + Time.time + " seconds");
            if(enableLocationNotificationSchedulingInUnity)
                ScheduleNotifications(true); 
            else
                Debug.Log("Location notification scheduling in Unity Is turned Off");
            
            if (FbManager.instance.useBackgroundLocationTasks)
            {
                BackgroundTasksBridge.Instance.NativeMonitorMyLocation(_previousLatitude, _previousLongitude, 100);
            }
        }
    }

    
    
    private void ScheduleNotifications(bool overideMaxDist)
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
        if (_distance > maxDist || overideMaxDist)
        {
            // Remove All Pending Notifications
            iOSNotificationCenter.RemoveAllScheduledNotifications();

            // Set current player's location
            _previousLatitude = currentLatitude;
            _previousLongitude = currentLongitude;

            // Add Notifications for the Next 10 Distance Filtered Traces
            UpdateNotificationsForNext20Traces();
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
    public string location;
    public float[] soundWave;
    public TraceCommentObject(string id, string time, string senderID, string senderName, float[] soundWave)
    {
        this.id = id;
        this.time = time;
        this.senderName = senderName;
        this.senderID = senderID;
        this.soundWave = soundWave;
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
    public string groupID;
    public double lat;
    public double lng;
    public float radius;
    public List<TraceReceiverObject> people; //replace this with recievers
    public Dictionary<string, TraceCommentObject> comments;
    public double distanceToUser;
    public string mediaType;
    public string senderID;
    public string senderName;
    public bool hasBeenAdded;
    public bool canBeOpened = false;
    private bool _hasBeenOpened = false;
    public string sendTime;
    public DateTime expiration;
    public bool hasUpdate;
    public string debugExpiration; //dont use
    public bool exirationExists;
    public bool isExpired;
    
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
    
    public TraceObject(double longitude, double latitude, float radius, List<TraceReceiverObject> people, Dictionary<string,TraceCommentObject> comments, string senderID, string senderName, string sendTime, DateTime expiration, bool exirationExists, string mediaType, string id, bool hasBeenOpened, bool isExpired, string groupID, bool hasUpdate)
    {
        lng = longitude;
        lat = latitude;
        this.radius = radius;
        this.senderID = senderID;
        this.groupID = groupID;
        this.people = people;
        this.comments = comments;
        this.senderName = senderName;
        this.sendTime = sendTime;
        this.expiration = expiration;
        this.exirationExists = exirationExists;
        this.debugExpiration = expiration.ToString(); //debug
        this.mediaType = mediaType;
        this.id = id;
        this.isExpired = isExpired;
        this.hasUpdate = hasUpdate;
        _hasBeenOpened = hasBeenOpened; //dont use setter because we dont want to destroy objects coming from memory
    }
}