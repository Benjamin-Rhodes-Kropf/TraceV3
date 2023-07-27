using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;
using Unity.VisualScripting;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.UI;
using DownloadHandler = Networking.DownloadHandler;
using Object = System.Object;


[System.Serializable]
public partial class FbManager : MonoBehaviour
{
    [Header("Dont Destroy")]
    public static FbManager instance;
    
    [Header("Firebase References")]
    [SerializeField] private DependencyStatus dependencyStatus;
    [SerializeField] private String firebaseStorageReferenceUrl;
    [SerializeField] private FirebaseAuth _firebaseAuth;    
    [SerializeField] private FirebaseUser _firebaseUser;
    [SerializeField] private DatabaseReference _databaseReference;
    [SerializeField] private FirebaseStorage _firebaseStorage;
    [SerializeField] private StorageReference _firebaseStorageReference;
    [SerializeField] private FirebaseFirestore _firebaseFirestore;

    [Header("Login Settings")] 
    [SerializeField] private bool autoLogin;
    [SerializeField] private bool resetPlayerPrefs;

    [Header("Maps References")]
    [SerializeField] private DrawTraceOnMap _drawTraceOnMap;
    [SerializeField] private DragAndZoomInertia _dragAndZoomInertia;
    [SerializeField] private OnlineMaps _map;


    [Header("User Data")] 
    public Texture userImageTexture;
    public UserModel thisUserModel;
    public List<UserModel> users;

    private Dictionary<string, object> _firestoreData;
    
    public bool IsFirebaseUserInitialised
    {
        get;
        private set;
    }
    
    
   void Awake()
   {
       if (resetPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
        }

        IsFirebaseUserInitialised = false;
        
        PlayerPrefs.SetInt("NumberOfTimesLoggedIn", PlayerPrefs.GetInt("NumberOfTimesLoggedIn")+1);
        if (PlayerPrefs.GetInt("NumberOfTimesLoggedIn") == 1)
        {
            Debug.Log("FbManager: First Time Logging In!");
        }
        
        //makes sure nothing can use the db until its enabled
        dependencyStatus = DependencyStatus.UnavailableUpdating;
        
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        _firebaseStorage = FirebaseStorage.DefaultInstance;
        _firebaseStorageReference = _firebaseStorage.GetReferenceFromUrl(firebaseStorageReferenceUrl);

        _firebaseFirestore = FirebaseFirestore.DefaultInstance;
        
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                if (!autoLogin)
                {
                    //clear cache
                    PlayerPrefs.SetString("Username", null);
                    PlayerPrefs.SetString("Password", null);
                }
                Debug.Log("Auto Logging in with username:" + PlayerPrefs.GetString("Username"));
                Debug.Log("Auto Logging in with password:" + PlayerPrefs.GetString("Password"));
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

    } 
   private void InitializeFirebase()
    {
        Debug.Log("initializing firebase");
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        _allReceivedRequests = new List<FriendRequests>();
        _allSentRequests = new List<FriendRequests>();
        _allFriends = new List<FriendModel>();
    } 
   private void Start()
    {
        StartCoroutine(AutoLogin());
    }

   #region This User
    #region -User Login/Logout
    public IEnumerator AutoLogin()
    {
        while (dependencyStatus != DependencyStatus.Available)
        {
            yield return null;
        }
        String savedUsername = PlayerPrefs.GetString("Username");
        String savedPassword = PlayerPrefs.GetString("Password");
        
        Debug.Log("saved user:" +  PlayerPrefs.GetString("Username"));
        if (savedUsername != "null" && savedPassword != "null")
        {
            StartCoroutine(FbManager.instance.Login(savedUsername, savedPassword, (myReturnValue) => {
                if (myReturnValue.callbackEnum == CallbackEnum.SUCCESS)
                {
                    if(PlayerPrefs.GetInt("IsInQueue") == 0)
                        ScreenManager.instance.ChangeScreenFade("HomeScreen");
                    else {
                        ScreenManager.instance.ChangeScreenFade("UserInQue");
                    }
                }
                else
                {
                    if (myReturnValue.callbackEnum == CallbackEnum.CONNECTIONERROR)
                    {
                        ScreenManager.instance.ChangeScreenForwards("ConnectionError");
                        return;
                    }
                    Debug.LogError("FbManager: failed to auto login");
                    Logout(LoginStatus.LoggedOut);
                }
            }));
        }
        else
        {
            ScreenManager.instance.WelcomeScreen();
        }
    }
    
    public IEnumerator Login(string _email, string _password,  System.Action<CallbackObject> callback)
    {
        //Fb Login
        Debug.Log("logging in");
        CallbackObject callbackObject = new CallbackObject();
        var LoginTask = _firebaseAuth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        
        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            string message = "Login Failed!";
            callbackObject.callbackEnum = CallbackEnum.FAILED;
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    callbackObject.message = message;
                    callbackObject.callbackEnum = CallbackEnum.FAILED;                    
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    callbackObject.message = message;
                    callbackObject.callbackEnum = CallbackEnum.FAILED;
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    callbackObject.message = message;
                    callbackObject.callbackEnum = CallbackEnum.FAILED;
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    callbackObject.message = message;
                    callbackObject.callbackEnum = CallbackEnum.FAILED;
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    callbackObject.message = message;
                    callbackObject.callbackEnum = CallbackEnum.FAILED;
                    break;
                case AuthError.NetworkRequestFailed:
                    message = "ConnectionError";
                    callbackObject.message = message;
                    Debug.Log("Trace Network Request Failed");
                    callbackObject.callbackEnum = CallbackEnum.CONNECTIONERROR;
                    break;
            }
            Debug.Log("FBManager: failed to log in because " + errorCode.ToString());
            callbackObject.callbackEnum = CallbackEnum.FAILED;
            callbackObject.message = message;
            callback(callbackObject);
            yield break;
        }

        _firebaseUser = LoginTask.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})", _firebaseUser.DisplayName, _firebaseUser.Email);
        Debug.Log("logged In: user profile photo is: " + _firebaseUser.PhotoUrl);
        callbackObject.callbackEnum = CallbackEnum.SUCCESS;

        //stay logged in
        PlayerPrefs.SetString("Username", _email);
        PlayerPrefs.SetString("Password", _password);
        PlayerPrefs.Save();

        ContinuesListners();
        InitializeFCMService();
        GetCurrentUserData(_password);
        
        //set login status
        if (callbackObject.callbackEnum == CallbackEnum.SUCCESS)
        {
            StartCoroutine(SetUserLoginStatus(true, isSusscess =>
            {
                if (isSusscess)
                {
                    Debug.Log("FbManager: SetUserLoginStatus: Done!");
                }
            }));
            StartCoroutine(SetUserQueueStatus(Convert.ToBoolean(PlayerPrefs.GetInt("IsInQueue")), isSusscess =>
            {
                if (isSusscess)
                {
                    Debug.Log("FbManager: SetUserQueueStatus:" + Convert.ToBoolean(PlayerPrefs.GetInt("IsInQueue")));
                }
            }));
        }
        callback(callbackObject);
    }

    
    
    private void ContinuesListners()
    {
        //this is bad... this is called any time any friend request occurs globally in the database
        _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildAdded += HandleReceivedFriendRequest;
        _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildRemoved += HandleReceivedRemovedRequests;
        
        _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildAdded += HandleSentFriendRequest;
        _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildRemoved += HandleSentRemovedRequests;
        
        _databaseReference.Child("Friends").Child(_firebaseUser.UserId).ChildAdded += HandleFriends;
        _databaseReference.Child("Friends").Child(_firebaseUser.UserId).ChildRemoved += HandleRemovedFriends;
        
        _databaseReference.Child("users").ChildChanged += HandleUserChanged;
        _databaseReference.Child("users").ChildRemoved += HandleRemoveUser;
        
        SubscribeOrUnSubscribeToReceivingTraces(true);
        SubscribeOrUnsubscribeToSentTraces(true);
    }
    private void UnsubscribeFromListiners()
    { 
        _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildAdded -= HandleReceivedFriendRequest;
        _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildRemoved -= HandleReceivedRemovedRequests;

        _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildAdded -= HandleSentFriendRequest;
        _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildRemoved -= HandleSentRemovedRequests;
        
        _databaseReference.Child("Friends").Child(_firebaseUser.UserId).ChildAdded -= HandleFriends; 
        _databaseReference.Child("Friends").Child(_firebaseUser.UserId).ChildRemoved -= HandleRemovedFriends;
        
        _databaseReference.Child("users").Child(_firebaseUser.UserId).ChildAdded -= HandleUserAdded;
        _databaseReference.Child("users").Child(_firebaseUser.UserId).ChildRemoved -= HandleRemoveUser;
        _databaseReference.Child("users").ChildChanged += HandleUserChanged;
        
        SubscribeOrUnSubscribeToReceivingTraces(false);
        SubscribeOrUnsubscribeToSentTraces(false);
    }
    
    private void GetCurrentUserData(string password)
    {
        // Get a reference to the "users" node in the database
        DatabaseReference usersRef = _databaseReference.Child("users");
        
        // Attach a listener to the "users" node
        usersRef.Child(_firebaseUser.UserId).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot snapshot = null;
            if (task.IsCompleted)
            {
                // Iterate through the children of the "users" node and add each username to the list
                snapshot = task.Result;
                    string email = snapshot.Child("email").Value.ToString();
                    string displayName = snapshot.Child("name").Value.ToString();
                    string username = snapshot.Child("username").Value.ToString();
                    string phone = snapshot.Child("phone").Value.ToString();
                    string photoURL = snapshot.Child("photo").Value.ToString();
                    thisUserModel = new UserModel(_firebaseUser.UserId,email,displayName,username,phone,photoURL, password);
                    IsFirebaseUserInitialised = true;
            }
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
        });
        
    }
    public void Logout(LoginStatus loginStatus)
    {
        Debug.Log("FBManager: logging out");
       
        //Reset User Settings
        userImageTexture = null;
        IsApplicationFirstTimeOpened = false;
        
        //DB Tasks
        if (loginStatus == LoginStatus.LoggedIn)
        {
            UnsubscribeFromListiners();
            HandleFriendsManagerClearData();
            StartCoroutine(RemoveFCMDeviceToken());
            _drawTraceOnMap.Clear();
            StartCoroutine(SetUserLoginStatus(false, isSusscess =>
            {
                if (isSusscess) print("Updated Login Status");
            }));
        }
        TraceManager.instance.recivedTraceObjects.Clear();
        TraceManager.instance.sentTraceObjects.Clear();
        HomeScreenManager.isInSendTraceView = false;
        thisUserModel = new UserModel();
        _firebaseAuth.SignOut();
        PlayerPrefs.SetString("Username", "null");
        PlayerPrefs.SetString("Password", "null");
        ScreenManager.instance.ChangeScreenForwards("Welcome");
    }
    #endregion
    
    #region -User Registration
    private string GenerateUserProfileJson(string username, string name, string userPhotoLink, string email, string phone, string createdDate) {
        TraceUserInfoStructure traceUserInfoStructure = new TraceUserInfoStructure(username, name, userPhotoLink, email, phone, createdDate);
        string json = JsonUtility.ToJson(traceUserInfoStructure);
        return json;
    }
    public IEnumerator RegisterNewUser(string _email, string _password, string _username, string _phoneNumber,  System.Action<String,AuthError> callback)
    {
        RestTutorial();
        if (_username == "")
        {
            callback("Missing Username", AuthError.None); //having a blank nickname is not really a DB error so I return a error here
            yield break;
        }
        Task<FirebaseUser> RegisterTask  = null;
        string message = "";
        AuthError errorCode =  AuthError.None;
        var creationTask =  _firebaseAuth.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
        {
                RegisterTask = task;
            
            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");        
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                errorCode = (AuthError)firebaseEx.ErrorCode;
                Debug.LogError("Error Code :: " + errorCode);
                message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                Debug.LogWarning(message);
            }
           
            // Firebase user has been created.
            _firebaseUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                _firebaseUser.DisplayName, _firebaseUser.UserId);
        });


        while (!creationTask.IsCompleted)
            yield return new WaitForEndOfFrame();

        if (RegisterTask.Exception != null)
        {
            callback(message,errorCode);
            yield break; 
        }
        
        if (_firebaseUser == null)
        {
            Debug.LogError("User Null");
            yield break;
        }
        else
        {
            print("User Email :: "+_firebaseUser.Email);
        }
        
        var json = GenerateUserProfileJson( _username, "null", "null",_email, _phoneNumber, DateTime.UtcNow.ToString());
        _databaseReference.Child("users").Child(_firebaseUser.UserId.ToString()).SetRawJsonValueAsync(json);
        _firestoreData = new Dictionary<string, object>
        {
            {"email",_email},
        };

        
        var DBTaskSetUserFriends = _databaseReference.Child("Friends").Child(_firebaseUser.UserId).Child("Don't Delete This Child").SetValueAsync("*");
        while (DBTaskSetUserFriends.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        //if nothing has gone wrong try logging in with new users information
        StartCoroutine(Login(_email, _password, (myReturnValue) => {
            if (myReturnValue != null)
            {
                Debug.LogWarning("failed to login");
            }
            else
            {
                Debug.Log("Logged In!");
            }
        }));
        callback(null, errorCode);
    }

    #endregion
    #region -User Edit Information
    public IEnumerator SetUsername(string _username, System.Action<bool> callback)
    {
        Debug.Log("Db SetUsername to :" + _username);
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("username").SetValueAsync(_username);
        
        // yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            callback(false);
        }
        else
        {
            _firestoreData.Add("username",_username);
            callback(true);
        }
    }
    public IEnumerator SetUserProfilePhotoUrl(string _photoUrl, System.Action<bool> callback)
    {
        Debug.Log("Db update photoUrl to :" + _photoUrl);
        //Set the currently logged in user nickName in the database
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("photo").SetValueAsync(_photoUrl);
        
        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            callback(false);
        }
        else
        {
            GetCurrentUserData("**********");
            callback(true);
        }
    }
    public IEnumerator SetUserNickName(string _nickName, System.Action<bool> callback)
    {
        //Set the currently logged in user nickName in the database
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("name").SetValueAsync(_nickName);
        
        //yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        //todo: which of these is better?
        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        if (DBTask.Exception != null)
        {
            callback(false);
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            _firestoreData.Add("name",_nickName);
            callback(true);
        }
    }
    public IEnumerator SetUserPhoneNumber(string _phoneNumber, System.Action<bool> callback)
    {
        Debug.LogError("Is Database Reference is Null  ? "+ _databaseReference == null);
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("phone").SetValueAsync(_phoneNumber);

        Debug.LogError("Is Database Completion is Null  ? "+ DBTask == null);

        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            callback(false);
        }
        else
        {
            //prevent dict bug
            if (_firestoreData.ContainsKey("phone"))
            {
                _firestoreData["phone"] = _phoneNumber;
            }
            else
            {
                _firestoreData.Add("phone",_phoneNumber);
            }
            callback(true);
        }
    }
    
    public IEnumerator SetUserLoginStatus(bool _isOnline, System.Action<bool> callback)
    {
        if (_firebaseUser != null)
        {
            var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("isOnline").SetValueAsync(_isOnline);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                callback(false);
            }
            else
            {
                callback(true);
            }
        }
        else
            callback(false);
    }
    public IEnumerator SetUserQueueStatus(bool _isInQueue, System.Action<bool> callback)
    {
        if (_firebaseUser != null)
        {
            var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("isInQueue").SetValueAsync(_isInQueue);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                callback(false);
            }
            else
            {
                callback(true);
            }
        }
        else
            callback(false);
    }

    public IEnumerator UploadProfilePhoto(byte[] _picBytes, System.Action<bool,string> callback)
    {
        StorageReference imageRef = _firebaseStorage.GetReference("ProfilePhoto/"+_firebaseUser.UserId+".png");

        var task = imageRef.PutBytesAsync(_picBytes);

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Task Faulted Due To :: "+ task.Exception.ToString());
            callback(false,"");
        }
        else
        {
            Debug.LogError("Image Uploaded Successfully");
            var url = task.Result.Path + "";
            callback(true,url);
        }
    }
    public void UploadProfilePicture(Texture raw)
    {
// Cast the original texture to Texture2D
        Texture2D convertedTexture  = (Texture2D)raw;
        
// Apply the changes to make them visible
        convertedTexture.Apply();

        StartCoroutine(FbManager.instance.UploadProfilePhoto(convertedTexture.EncodeToPNG(), (isUploaded, url) =>
        {
            if (isUploaded)
            {
                StartCoroutine(FbManager.instance.SetUserProfilePhotoUrl(url,
                    (isUrlSet) =>
                    {
                        if (isUrlSet)
                        {
                            Debug.Log("SetUserProfilePhotoUrl");
                            _firestoreData.Add("photo",url);
                        }
                        else
                        {
                            Debug.Log("Failed To SetUserProfilePhotoUrl");
                        }
                    }));
            }
        }));
    }
    #endregion
    #region -User Info
    public void GetProfilePhotoFromFirebaseStorage(string userId, Action<Texture> onSuccess, Action<string> onFailed, ref Coroutine _coroutine) {
        _coroutine = StartCoroutine(GetProfilePhotoFromFirebaseStorageRoutine(userId, (myReturnValue) => {
            if (myReturnValue != null)
            {
                //compress to reduce mem load
                Texture2D reducedTexture = new Texture2D(128, 128);
                RenderTexture rt = new RenderTexture(128, 128, 24);
                Graphics.Blit((Texture2D)myReturnValue, rt);
                RenderTexture.active = rt;
                reducedTexture.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                reducedTexture.Apply();
                RenderTexture.active = null;
                rt.Release();
                onSuccess?.Invoke(reducedTexture);
                DestroyImmediate(myReturnValue);
            }
            {
                onFailed?.Invoke("Image not Found");
            }
        }));
    }
    public IEnumerator GetProfilePhotoFromFirebaseStorageRoutine(string userId, System.Action<Texture> callback)
    {
        var url = "";
        StorageReference pathReference = _firebaseStorage.GetReference("ProfilePhoto/"+userId+".png");
        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false) yield return new WaitForEndOfFrame();

        if (!task.IsFaulted && !task.IsCanceled) {
            url = task.Result + "";
        }
        else
        {
            Debug.Log("could not get image from:" + "ProfilePhoto/"+userId+".png setting profile photo to blank");
            url = "https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/ProfilePhoto%2FNullProfile.jpeg?alt=media&token=ad5a55e4-351e-4df5-976f-cdfbf18c80d2";
        }

        DownloadHandler.Instance.DownloadImage(url, callback, () =>
        {
            callback(null);
        });
    }
    public IEnumerator GetThisUserNickName(System.Action<String> callback, string firebaseUserId = "")
    {
        var DBTask = _databaseReference.Child("users").Child(firebaseUserId == ""? _firebaseUser.UserId: firebaseUserId).Child("Friends").Child("nickName").GetValueAsync();
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback(DBTask.Result.ToString());
        }
    }
    
    //Todo: Do this in the cloud... we cant store all users locally
    private void GetAllUsers()
    {
        // Create a list to store the usernames
        users = new List<UserModel>();
        //return;
        Debug.Log("Getting users");
        // Get a reference to the "users" node in the database
        DatabaseReference usersRef = _databaseReference.Child("users");
        
        // Attach a listener to the "users" node
        usersRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
             {
                 // Iterate through the children of the "users" node and add each username to the list
                 DataSnapshot snapshot = task.Result;
                 var  allUsersSnapshots = snapshot.Children.ToArrayPooled();

                 foreach (var snap in allUsersSnapshots)
                 {
                     //Debug.Log("SNAP :" + snap.Key);
                     try
                     {
                         UserModel userData = new UserModel();
                         userData.userID =  snap.Key.ToString(); 
                         if (snap.Child("email").Value.ToString() == "" || snap.Child("email").Value.ToString() == null)
                         {
                             Debug.Log("Caught and Error");
                             continue;
                         }
                         userData.email = snap.Child("email").Value.ToString();
                         
                         if (snap.Child("name").Value.ToString() == "" || snap.Child("name").Value.ToString() == null)
                         {
                             Debug.Log("Caught and Error");
                             continue;
                         }
                         userData.name =snap.Child("name").Value.ToString();
                         
                         if (snap.Child("username").Value.ToString() == "" || snap.Child("username").Value.ToString() == null)
                         {
                             Debug.Log("Caught and Error");
                             continue;
                         }
                         userData.username = snap.Child("username").Value.ToString();
                         
                         if (snap.Child("phone").Value.ToString() == "" || snap.Child("phone").Value.ToString() == null)
                         {
                             Debug.Log("Caught and Error");
                             continue;
                         }
                         userData.phone =  snap.Child("phone").Value.ToString();
                         
                         if (snap.Child("photo").Value.ToString() == "" || snap.Child("photo").Value.ToString() == null)
                         {
                             Debug.Log("Caught and Error");
                             continue;
                         }
                         userData.photo = snap.Child("photo").Value.ToString();
                         
                         
                         users.Add(userData); 
                     }
                     catch (Exception e)
                     {
                         Debug.Log("failed to get user");
                     }
                 }
                 return;
             }
            if (task.IsFaulted)
             {
                 Debug.LogError(task.Exception);
                 // Handle the error
                 return;
             }
        });
    }
    private void HandleUserAdded(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleUserAdded");
        try
        {
            if (args.Snapshot == null || args.Snapshot.Value == null) return;
            var userID = args.Snapshot.Key.ToString();
            if (string.IsNullOrEmpty(userID)) return;
            
            UserModel userData = new UserModel();
            userData.userID =  args.Snapshot.Key.ToString(); 
            Debug.Log("HandleUserAdded UserID:" + userData.userID);
            userData.email = args.Snapshot.Child("email").Value.ToString();
            Debug.Log("HandleUserAdded email:" + userData.email);
            userData.name = args.Snapshot.Child("name").Value.ToString();
            Debug.Log("HandleUserAdded name:" + userData.name);
            userData.username = args.Snapshot.Child("username").Value.ToString();
            if (userData.username == "null" || string.IsNullOrEmpty(userData.username))
            {
                Debug.Log("User Is Not Setup Correctly");
                return;
            }
            Debug.Log("HandleUserAdded username:" + userData.username);
            userData.phone =  args.Snapshot.Child("phone").Value.ToString();
            if (userData.phone == "") //todo add more
            {
                return;
            }
            Debug.Log("HandleUserAdded phone:" + userData.phone);
            userData.photo = args.Snapshot.Child("photo").Value.ToString();
            Debug.Log("HandleUserAdded photo:" + userData.photo);
            users.Add(userData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private void HandleUserChanged(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleUserChanged");
        try
        {
            if (args.Snapshot == null || args.Snapshot.Value == null) return;
            var userID = args.Snapshot.Key.ToString();
            if (string.IsNullOrEmpty(userID)) return;
            var userToChange = GetLocalUserByID(userID); //may not always work because its local db
            if (userToChange == null)
            {
                //create a new user
                try
                {
                    userToChange = new UserModel();
                    userToChange.userID = userID;
                    userToChange.email = args.Snapshot.Child("email").Value.ToString();
                    Debug.Log("HandleUserAdded email:" + userToChange.email);
                    userToChange.name = args.Snapshot.Child("name").Value.ToString();
                    Debug.Log("HandleUserAdded name:" + userToChange.name);
                    userToChange.username = args.Snapshot.Child("username").Value.ToString();
                    if (userToChange.username == "null" || string.IsNullOrEmpty(userToChange.username))
                    {
                        Debug.Log("User Is Not Setup Correctly");
                        return;
                    }
                    Debug.Log("HandleUserAdded username:" + userToChange.username);
                    userToChange.phone = args.Snapshot.Child("phone").Value.ToString();
                    Debug.Log("HandleUserAdded phone:" + userToChange.phone);
                    userToChange.photo = args.Snapshot.Child("photo").Value.ToString();
                    Debug.Log("HandleUserAdded photo:" + userToChange.photo);
                    users.Add(userToChange);
                }
                catch
                {
                    Debug.Log("Friend Malformed");
                }
                return;
            }
            
            //add to existing user
            userToChange.email = args.Snapshot.Child("email").Value.ToString();
            Debug.Log("HandleUserAdded email:" + userToChange.email);
            userToChange.name =args.Snapshot.Child("name").Value.ToString();
            Debug.Log("HandleUserAdded name:" + userToChange.name);
            userToChange.username = args.Snapshot.Child("username").Value.ToString();
            if (userToChange.username == "null" || string.IsNullOrEmpty(userToChange.username))
            {
                Debug.Log("User Is Not Setup Correctly");
                return;
            }
            Debug.Log("HandleUserAdded username:" + userToChange.username);
            userToChange.phone =  args.Snapshot.Child("phone").Value.ToString();
            if (userToChange.phone == "") //todo add more
            {
                return;
            }
            Debug.Log("HandleUserAdded phone:" + userToChange.phone);
            userToChange.photo = args.Snapshot.Child("photo").Value.ToString();
            Debug.Log("HandleUserAdded photo:" + userToChange.photo);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private UserModel GetLocalUserByID(string userToGetID)
    {
        foreach (var userObject in users)
        {
            if (userObject.userID == userToGetID)
            {
                return userObject;
            }
        }
        return null;
    }
    public void AddUserToLocalDbByID(string userToGetID)
    {
        Debug.Log("Adding:" + userToGetID);

        if (userToGetID == "Don't Delete This Child")
            return;
        //check if user already in local DB
        foreach (var obj in users)
        {
            if (obj.userID == userToGetID)
            {
                Debug.Log("users already added:" + obj.username);
                return;
            }
        }
        
        //if not retrive the user from the database
        UserModel user = new UserModel();
        user.userID = userToGetID;
        
        Debug.Log("creating document refrence");

        // Reference to the specific document you want to read from
        DocumentReference docRef = _firebaseFirestore.Collection("users").Document(userToGetID);
        
        Debug.Log("GetSnapshotAsync");
        // Read the document
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to read data from Firestore: {task.Exception}");
                return;
            }

            Debug.Log("adding snapshot" + user.userID);
            
            // Get the document snapshot
            DocumentSnapshot snapshot = task.Result;

            // Check if the document exists
            if (snapshot.Exists)
            {
                // Access the data from the snapshot
                Dictionary<string, object> data = snapshot.ToDictionary();

                // Access specific fields from the data dictionary
                if (data.ContainsKey("email"))
                {
                    object fieldValue = data["email"];
                    Debug.Log("adding email" + fieldValue);
                    user.email = fieldValue.ToString();
                }
                if (data.ContainsKey("name"))
                {
                    object fieldValue = data["name"];
                    Debug.Log("adding name" + fieldValue);
                    user.name = fieldValue.ToString();
                }
                if (data.ContainsKey("phone"))
                {
                    object fieldValue = data["phone"];
                    Debug.Log("adding phone" + fieldValue);
                    user.phone = fieldValue.ToString();
                }
                if (data.ContainsKey("photo"))
                {
                    object fieldValue = data["photo"];
                    Debug.Log("adding photo" + fieldValue);
                    user.photo = fieldValue.ToString();
                }
                if (data.ContainsKey("username"))
                {
                    object fieldValue = data["username"];
                    Debug.Log("adding username" + fieldValue);
                    user.username = fieldValue.ToString();
                }
                FbManager.instance.users.Add(user);
            }
            else
            {
                Debug.LogWarning("Document does not exist in firestore users:" + userToGetID);
            }
        });
    }

    private void HandleRemoveUser(object sender, ChildChangedEventArgs args)
    {
        //todo: handle remove user
    }
    #endregion
    #region -Is User Invited

    public IEnumerator SendInvite(string phoneNumber)
    {
        var DBTaskAddInvite = _databaseReference.Child("invited").Child(phoneNumber).Child(FbManager.instance.thisUserModel.userID).SetValueAsync(DateTime.UtcNow.ToString());
        while (DBTaskAddInvite.IsCompleted is false)
            yield return new WaitForEndOfFrame();
    }
    
    public IEnumerator IsUserInvited(string _phoneNumber, System.Action<bool> callback)
    {
        string cleanedPhoneNumber = Regex.Replace(_phoneNumber, @"[^0-9]", "");
        Debug.Log("Checking If User Invited:" + cleanedPhoneNumber);
        var DBTask = _databaseReference.Child("invited").Child(cleanedPhoneNumber).GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        
        if (DBTask.Exception != null)
        {
            // Error occurred while retrieving data (user is not invited)
            Debug.Log("user NOT Invited");
            callback(false);
        }
        else if (DBTask.Result.Exists)
        {
            // User is invited (data exists in the database)
            Debug.Log("user Invited");
            callback(true);
        }
        else
        {
            // User is not invited (data doesn't exist in the database)
            Debug.Log("user NOT Invited");
            callback(false);
        }
    }


    #endregion
    #region -User Tracking
    //Todo Write tracking functions to manage in app use
    

    #endregion
    #region -User Subscriptions
    public void SubscribeOrUnSubscribeToReceivingTraces(bool subscribe)
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("TracesRecived").Child(_firebaseUser.UserId);
        if (subscribe)
        {
            refrence.ChildAdded += HandleChildAdded;
            //refrence.ChildChanged += HandleChildChanged;
            //refrence.ChildRemoved += HandleChildRemoved;
            //refrence.ChildMoved += HandleChildMoved;
        }
        else
        {
            refrence.ChildAdded -= HandleChildAdded;
            //refrence.ChildChanged -= HandleChildChanged;
            //refrence.ChildRemoved -= HandleChildRemoved;
            //refrence.ChildMoved -= HandleChildMoved;
        }
        
        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            StartCoroutine(GetRecievedTrace(args.Snapshot.Key));
        }

        // void HandleChildChanged(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child changed:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }

        // void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child removed:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }

        // void HandleChildMoved(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child moved:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }
    }
    public void SubscribeOrUnsubscribeToSentTraces(bool subscribe)
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("TracesSent").Child(_firebaseUser.UserId);
        if (subscribe)
        {
            refrence.ChildAdded += HandleChildAdded;
            //refrence.ChildChanged += HandleChildChanged;
            //refrence.ChildRemoved += HandleChildRemoved;
            //refrence.ChildMoved += HandleChildMoved;
        }
        else
        {
            refrence.ChildAdded -= HandleChildAdded;
            //refrence.ChildChanged -= HandleChildChanged;
            //refrence.ChildRemoved -= HandleChildRemoved;
            //refrence.ChildMoved -= HandleChildMoved;
        }

        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            StartCoroutine(GetSentTrace(args.Snapshot.Key));
            //Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        }

        // void HandleChildChanged(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.Log("HandleChildAdded Error");
        //         //return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child changed:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }

        // void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child removed:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }

        // void HandleChildMoved(object sender, ChildChangedEventArgs args) {
        //     if (args.DatabaseError != null) {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     // Do something with the data in args.Snapshot
        //     Debug.Log("child moved:" +args.Snapshot);
        //     Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        // }
    }
    public void SubscribeToFriendShipRequests()
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("friendRequests").Child(_firebaseUser.UserId);
        refrence.ChildAdded += HandleChildAdded;
        refrence.ChildChanged += HandleChildChanged;
        refrence.ChildRemoved += HandleChildRemoved;
        refrence.ChildMoved += HandleChildMoved;

        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            // Do something with the data in args.Snapshot
            Debug.Log("child added:" +args.Snapshot);
            Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        }

        void HandleChildChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            // Do something with the data in args.Snapshot
            Debug.Log("child changed:" +args.Snapshot);
            Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        }

        void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            // Do something with the data in args.Snapshot
            Debug.Log("child removed:" +args.Snapshot);
            Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        }

        void HandleChildMoved(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            // Do something with the data in args.Snapshot
            Debug.Log("child moved:" +args.Snapshot);
            Debug.Log("value:" +  args.Snapshot.GetRawJsonValue());
        }
    }
    #endregion

    #region FirestoreData

    public void CreateDocumentInFireStore()
    {
        _firebaseFirestore.Collection("users").Document(_firebaseUser.UserId).SetAsync(_firestoreData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Data created successfully in Firestore.");
                ScreenManager.instance.ChangeScreenForwards("SettingUpAccount");
            }
            else
            {
                Debug.LogError("Failed to create data in Firestore: " + task.Exception);
            }
        });
    }

    #endregion
    #endregion
    
    #region Sending and Recieving Traces
    public void UploadTrace(string fileLocation, float radius, Vector2 location, MediaType mediaType, List<string> usersToSendToList)
    {
        Debug.Log(" UploadTrace()");
        Debug.Log(" UploadTrace(): File Location:" + fileLocation);
        
        //PUSH DATA TO REAL TIME DB
        string key = _databaseReference.Child("Traces").Push().Key;
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        
        //draw temp circle until it uploads and the map is cleared on update
        SendTraceManager.instance.isSendingTrace = true;
        _drawTraceOnMap.sendingTraceTraceLoadingObject = new TraceObject(location.x, location.y, radius, usersToSendToList.Count, "null",thisUserModel.name,  DateTime.UtcNow.ToString(), 24, mediaType.ToString(), "temp");
        _drawTraceOnMap.DrawCirlce(location.x, location.y, radius, DrawTraceOnMap.TraceType.SENDING, "null");
            
        //update global traces
        childUpdates["Traces/" + key + "/senderID"] = _firebaseUser.UserId;
        childUpdates["Traces/" + key + "/senderName"] = thisUserModel.name;
        childUpdates["Traces/" + key + "/sendTime"] = DateTime.UtcNow.ToString();
        childUpdates["Traces/" + key + "/durationHrs"] = 24;
        childUpdates["Traces/" + key + "/mediaType"] = mediaType.ToString();
        childUpdates["Traces/" + key + "/lat"] = location.x;
        childUpdates["Traces/" + key + "/long"] = location.y;
        childUpdates["Traces/" + key + "/radius"] = radius;
        if (PlayerPrefs.GetInt("LeaveTraceIsVisable") == 1)
        {
            childUpdates["Traces/" + key + "/isVisable"] = true;
        }
        else
        {
            childUpdates["Traces/" + key + "/isVisable"] = false;
        }

        int count = 0;
        foreach (var user in usersToSendToList) //each of the users in usersToSendToList is a UID
        {
            count++;
            //update data for within trace
            childUpdates["Traces/" + key + "/Reciver/" + user + "/HasViewed"] = false;
            childUpdates["Traces/" + key + "/Reciver/" + user + "/ProfilePhoto"] = "null";
            //update data for each user
            childUpdates["TracesRecived/" + user +"/"+ key + "/Sender"] = thisUserModel.userID;
            Debug.Log("Count" + count);
        }
        Debug.Log("Userse to Send to Count:" + usersToSendToList.Count);
        childUpdates["Traces/" + key + "/numPeopleSent"] = count;
        childUpdates["TracesSent/" + _firebaseUser.UserId.ToString() +"/" + key] = DateTime.UtcNow.ToString();
        //UPLOAD IMAGE
        StorageReference traceReference = _firebaseStorageReference.Child("/Traces/" + key);
        traceReference.PutFileAsync(fileLocation)
            .ContinueWith((Task<StorageMetadata> task) => {
                if (task.IsFaulted || task.IsCanceled) {
                    // Uh-oh, an error occurred!
                    Debug.Log("FB Error: Failed to Upload File");
                    Debug.Log("FB Error:" + task.Exception.ToString());
                    SendTraceManager.instance.isSendingTrace = false;
                    //tell user there was an error
                }
                else {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    // string md5Hash = metadata.Md5Hash;
                    Debug.Log("FB: Finished uploading...");
                    //upload metadata to real time DB
                    _databaseReference.UpdateChildrenAsync(childUpdates);
                    SendTraceManager.instance.isSendingTrace = false;
                    
                    try //test
                    {
                        SendTraceManager.instance.SendNotificationToUsersWhoRecivedTheTrace();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            });
    }
    public void MarkTraceAsOpened(string traceID)
    {
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        childUpdates["TracesRecived/" + _firebaseUser.UserId + "/" + traceID] = null;
        childUpdates["Traces/" + traceID + "/Reciver/"+ _firebaseUser.UserId +"/HasViewed"] = true;
        _databaseReference.UpdateChildrenAsync(childUpdates);
        //Update Map
        TraceManager.instance.recivedTraceObjects[TraceManager.instance.GetRecivedTraceIndexByID(traceID)].hasBeenOpened = true;
    }
    public IEnumerator GetRecievedTrace(string traceID)
    {
        var DBTask = _databaseReference.Child("Traces").Child(traceID).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            double lat = 0;
            double lng = 0;
            float radius = 0;
            int numPeopleSent = 0;
            string mediaType = "";
            string senderID = "";
            string senderName = "";
            string sendTime = "";
            float durationHours = 0;

            foreach (var thing in DBTask.Result.Children)
            {
                switch (thing.Key.ToString())
                {
                    case "lat":
                    {
                        try
                        {
                            lat = (double)thing.Value;
                        }
                        catch (Exception e)
                        {
                            lat = 0;
                        }
                        break;
                    }
                    case "long":
                    {
                        try
                        {
                            lng = (double)thing.Value;
                        }
                        catch (Exception e)
                        {
                            lng = 0;
                        }
                        
                        break;
                    }
                    case "radius":
                    {
                        try
                        {
                            radius = float.Parse(thing.Value.ToString());
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("failed to parse string to float");
                        }
                        break;
                    }
                    case "mediaType":
                    { 
                        mediaType = thing.Value.ToString();
                        break;
                    }
                    case "senderID":
                    { 
                        senderID = thing.Value.ToString();
                        break;
                    }
                    case "senderName":
                    {
                        senderName = thing.Value.ToString();
                        break;
                    }
                    case "numPeopleSent":
                        numPeopleSent = Int32.Parse(thing.Value.ToString());
                        break;
                    case "sendTime":
                    {
                        sendTime = thing.Value.ToString();
                        break;
                    }
                    case "durationHours":
                    {
                        durationHours = (float)thing.Value;
                        break;
                    }
                }
            }
            if (lat != 0 && lng != 0 && radius != 0) //check for malformed data entry
            {
                var trace = new TraceObject(lng, lat, radius, numPeopleSent, senderID, senderName, sendTime, 20, mediaType,traceID);
                TraceManager.instance.recivedTraceObjects.Add(trace);
                TraceManager.instance.UpdateMap(new Vector2());
            }
        }
    }
    public IEnumerator GetSentTrace(string traceID)
    {
        var DBTask = _databaseReference.Child("Traces").Child(traceID).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            double lat = 0;
            double lng = 0;
            float radius = 0;
            bool hasBeenOpened;
            string senderID = "";
            int numPeopleSent = 0;
            string senderName = "";
            string sendTime = "";
            string mediaType = "";
            float durationHours = 0;

            foreach (var thing in DBTask.Result.Children)
            {
                switch (thing.Key.ToString())
                {
                    case "lat":
                    {
                        // Debug.Log(traceID + "lat: " + thing.Value);
                        // Debug.Log(thing.Value);
                        try
                        {
                            lat = (double)thing.Value;
                        }
                        catch (Exception e)
                        {
                            lat = 0;
                        }
                        break;
                    }
                    case "long":
                    {
                        try
                        {
                            lng = (double)thing.Value;
                        }
                        catch (Exception e)
                        {
                            lng = 0;
                        }
                        
                        break;
                    }
                    case "radius":
                    {
                        try
                        {
                            radius = float.Parse(thing.Value.ToString());
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("failed to parse string to float");
                        }
                        break;
                    }
                    case "senderID":
                    {
                        senderID = thing.Value.ToString();
                        break;
                    }
                    case "senderName":
                    {
                        senderName = thing.Value.ToString();
                        break;
                    }
                    case "sendTime":
                    {
                        sendTime = thing.Value.ToString();
                        break;
                    }
                    case "numPeopleSent":
                        numPeopleSent = Int32.Parse(thing.Value.ToString());
                        break;
                    case "mediaType":
                    {
                        mediaType = thing.Value.ToString();
                        break;
                    }
                    case "durationHours":
                    {
                        durationHours = (float)thing.Value;
                        break;
                    }
                }
            }
            
            if (lat != 0 && lng != 0 && radius != 0)
            {
                var trace = new TraceObject(lng, lat, radius, numPeopleSent, senderID,senderName, sendTime, 20, mediaType,traceID);
                TraceManager.instance.sentTraceObjects.Add(trace);
                TraceManager.instance.UpdateMap(new Vector2());
            }
        }
    }
    public IEnumerator GetTracePhotoByUrl(string _url, System.Action<Texture> callback)
    {
        var request = new UnityWebRequest();
        var url = "";
        
        Debug.Log("test:");
        StorageReference pathReference = _firebaseStorage.GetReference("Traces/"+_url);
        Debug.Log("path refrence:" + pathReference);

        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        if (!task.IsFaulted && !task.IsCanceled) {
            Debug.Log("Download URL: " + task.Result);
            url = task.Result + "";
            Debug.Log("Actual  URL: " + url);
        }
        else
        {
            Debug.Log("task failed:" + task.Result);
        }
        
        request = UnityWebRequestTexture.GetTexture((url)+"");
        
        yield return request.SendWebRequest(); //Wait for the request to complete
        
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("error:" + request.error);
        }
        else
        {
            Debug.Log("Correctly Got Image From Database");
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    public IEnumerator GetTraceVideoByUrl(string _url, System.Action<string> callback)
    {
        var request = new UnityWebRequest();
        var url = "";

        Debug.Log("test:");
        StorageReference pathReference = _firebaseStorage.GetReference("Traces/" + _url);
        Debug.Log("path refrence:" + pathReference);

        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (!task.IsFaulted && !task.IsCanceled)
        {
            Debug.Log("Download URL: " + task.Result);
            Debug.Log("Actual  URL: " + url);
            url = task.Result + "";
        }
        else
        {
            Debug.Log("task failed:" + task.Result);
        }

        //video stuff needs to go here
        request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest(); //Wait for the request to complete

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("error:" + request.error);
        }
        else
        {
            Debug.Log("Correctly Got Video From Database");
            var path = Application.persistentDataPath + "/" + "ReceivedTraceVideo" + ".mp4";
            File.WriteAllBytes(path, request.downloadHandler.data);
            Debug.Log("Downloaded Video!");
            Debug.Log("Video Location:" + path);
            callback(path);
        }
    }
    #endregion
    
    //possibly useful
    private void RestTutorial()
    {
        PlayerPrefs.SetInt("TutorialOnHomeScreen", 1);
        PlayerPrefs.SetInt("TutorialOnCamera", 1);
        PlayerPrefs.SetInt("TutorialOnSelectScreen", 1);
        PlayerPrefs.SetInt("TutorialOnSelectRadius", 1);
    }
    
    private void DeleteFile(String _location) 
    { 
        _firebaseStorageReference = _firebaseStorageReference.Child(_location);
        _firebaseStorageReference.DeleteAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                Debug.Log("File deleted successfully.");
            }
            else {
                // Uh-oh, an error occurred!
            }
        });
    }
    public void CollectAnalytics()
    {
        //friend count
        //FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectItem, new Parameter(FirebaseAnalytics.ParameterValue, _allFriends.Count));
        
    }

    public enum LoginStatus
    {
        LoggedIn, LoggedOut
    }
}
