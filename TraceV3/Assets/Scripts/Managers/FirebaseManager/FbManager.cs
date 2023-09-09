using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using VoxelBusters.CoreLibrary;
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

    [Header("Developer Settings")]
    [SerializeField] private bool resetPlayerPrefs;
    [SerializeField] private int fakeLoginWaitTime;
    public bool lowConnectivitySmartLogin;
    
    [Header("Maps References")]
    [SerializeField] private DrawTraceOnMap _drawTraceOnMap;
    [SerializeField] private DragAndZoomInertia _dragAndZoomInertia;
    [SerializeField] private OnlineMaps _map;

    [Header("User Data")] 
    public Texture userImageTexture;
    public UserModel thisUserModel;
    public List<UserModel> users;

    private Dictionary<string, object> _firestoreData;
    
    
    public bool IsFirebaseUserLoggedIn;

    public bool IsFirebaseUserInitialised
    {
        get;
        private set;
    }
    
    public bool IsFirebaseInitialised
    {
        get;
        private set;
    }
    
    
   void Awake()
   {
       //do not destroy
       if (instance != null)
       {Destroy(gameObject);}
       instance = this;
       DontDestroyOnLoad(this.gameObject);
       
        //settings
        if (resetPlayerPrefs)
            PlayerPrefs.DeleteAll();
        
        //setup fb for initialization
        IsFirebaseUserInitialised = false;
        IsFirebaseInitialised = false;
        IsFirebaseUserLoggedIn = false;
        dependencyStatus = DependencyStatus.UnavailableUpdating;
        _firebaseStorage = FirebaseStorage.DefaultInstance;
        _firebaseStorageReference = _firebaseStorage.GetReferenceFromUrl(firebaseStorageReferenceUrl);
        _firebaseFirestore = FirebaseFirestore.DefaultInstance;
        
        //Check that all of the necessary dependencies for fb are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
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
        IsFirebaseInitialised = true;
    } 
   
   private void Start()
    {
        StartCoroutine(AutoLogin());
        
        //get player prefs cache while the user is logging in and put user through even if they haven't finished logging in
        if (PlayerPrefs.GetInt("DBDataCached") == 1 && lowConnectivitySmartLogin && PlayerPrefs.GetString("Password") != "null" && PlayerPrefs.GetString("Password") != "") //makes sure user has been logged in before
        {
            LowConnectivityPreLogin();
        }
    }

   public void LowConnectivityPreLogin()
   {
       ScreenManager.instance.ChangeScreenFade("HomeScreen");
       //thisUserModel = PlayerPrefsManager.Instance.GetThisUserFromPlayerPrefs(); //todo: get this one working so it dont crash
       users = PlayerPrefsManager.Instance.GetUsersFromPlayerPrefs();
       _allFriends = PlayerPrefsManager.Instance.GetFriendsFromPlayerPrefs();
       TraceManager.instance.receivedTraceObjects = PlayerPrefsManager.Instance.GetReceivedTracesFromPlayerPrefs();
       TraceManager.instance.sentTraceObjects = PlayerPrefsManager.Instance.GetSentTracesFromPlayerPrefs();
   }

   #region This User
    #region -User Login/Logout
    public IEnumerator AutoLogin()
    {
        //wait for fb to finnish setting up
        while (dependencyStatus != DependencyStatus.Available)
        {
            yield return null;
        }

#if UNITY_EDITOR
        if (lowConnectivitySmartLogin)
        {
            //used for testing local DB caching
            yield return new WaitForSeconds(fakeLoginWaitTime); 
        } 
#endif

        String savedUsername = PlayerPrefs.GetString("Username");
        String savedPassword = PlayerPrefs.GetString("Password");

        if (savedUsername != "null" && savedUsername != "" && savedPassword != "null" && savedPassword != "") //check if empty
        {
            Debug.Log("Auto Logging in with username:" + PlayerPrefs.GetString("Username"));
            Debug.Log("Auto Logging in with password:" + PlayerPrefs.GetString("Password"));

            StartCoroutine(FbManager.instance.Login(savedUsername, savedPassword, (myReturnValue) => {
                switch (myReturnValue.LoginStatus)
                {
                    case global::LoginStatus.Success:
                        Debug.Log("AutoLogin SUCCESS");
                        SetUserLoginSatus(true);

                        if (PlayerPrefs.GetInt("IsInvited") == 1) //if user already invited don't check queue again
                        {
                            if (lowConnectivitySmartLogin && PlayerPrefs.GetInt("DBDataCached") == 1) //user should already be in
                                return;
                            ScreenManager.instance.ChangeScreenFade("HomeScreen");
                            IsFirebaseUserLoggedIn = true;
                        }
                        else
                        {
                            Debug.LogWarning("ManagerUserPermissions");
                            StartCoroutine(FbManager.instance.ManagerUserPermissions(callbackObject =>
                            {
                                if (lowConnectivitySmartLogin && IsFirebaseUserLoggedIn) //user should already be in
                                    return;
                                else if (callbackObject == true)
                                {
                                    ScreenManager.instance.ChangeScreenFade("HomeScreen");
                                    IsFirebaseUserLoggedIn = true;
                                }
                                else
                                {
                                    ScreenManager.instance.ChangeScreenFade("UserInQue");
                                    IsFirebaseUserLoggedIn = true;
                                }
                            }));;
                        }
                        break;
                    case global::LoginStatus.ConnectionError:
                        Debug.LogError("CONNECTION ERROR!");
                        //ScreenManager.instance.ChangeScreenForwards("ConnectionError");
                        break;
                    
                    case global::LoginStatus.UnFinishedRegistration:
                        Debug.LogWarning("Proccesing Unfinished Registration");
                        if(thisUserModel.phone == "" || thisUserModel.phone == "null")
                            ScreenManager.instance.ChangeScreenNoAnim("PhoneNumber");
                        else if(thisUserModel.name == "null" || thisUserModel.name == "" || thisUserModel.username == "null" || thisUserModel.username == "")
                            ScreenManager.instance.ChangeScreenNoAnim("Username");
                        break;
                    default:
                        ScreenManager.instance.ChangeScreenForwards("ConnectionError");
                        break;
                }
            }));
        }
        else
        {
            ScreenManager.instance.WelcomeScreen();
        }
    }
    
    public IEnumerator Login(string _email, string _password, System.Action<CallbackObject> callback)
    {
        Debug.Log("Logging in...");
        var LoginTask = _firebaseAuth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        CallbackObject callbackObject = new CallbackObject();

        if (LoginTask.Exception != null)
        {
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            string message = "Login Failed!";
            
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
                case AuthError.NetworkRequestFailed:
                    message = "ConnectionError";
                    Debug.Log("Trace Network Request Failed");
                    callbackObject.LoginStatus = global::LoginStatus.ConnectionError;
                    break;
                // Add more error cases if necessary
                default:
                    message = "Unknown error: " + errorCode.ToString();
                    break;
            }

            Debug.LogWarning($"FBManager: failed to log in because {errorCode.ToString()}");
            callbackObject.LoginStatus = global::LoginStatus.Failed;
            callbackObject.message = message;
            callback(callbackObject);
            yield break;
        }

        _firebaseUser = LoginTask.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})", _firebaseUser.DisplayName, _firebaseUser.Email);

        //Todo: Remove this for security reasons
        PlayerPrefs.SetString("Username", _email);
        PlayerPrefs.SetString("Password", _password);
        PlayerPrefs.Save();

        ContinuesListners();
        InitializeFCMService();
        
        //get user data (this does slow down the login a bit)
        UserStatus fetchedUserStatus = UserStatus.Error;  // Initialize with a default value

        yield return StartCoroutine(GetCurrentUserDataCoroutine((status) =>
        {
            fetchedUserStatus = status;  // Update the local variable with the status from the callback
            switch (status)
            {
                case UserStatus.Initialized:
                    break;
                case UserStatus.MissingData:
                    Debug.LogWarning("User data is incomplete!");
                    break;
                case UserStatus.Error:
                    Debug.LogError("Error fetching user data.");
                    break;
            }
        }));
        
        // Now we use 'fetchedUserStatus' to decide how to proceed.
        if (fetchedUserStatus == UserStatus.Initialized)
        {
            // All is good, user data is initialized.
            IsFirebaseUserLoggedIn = true;
            callbackObject.LoginStatus = global::LoginStatus.Success;
        }
        else if (fetchedUserStatus == UserStatus.MissingData)
        {
            // Handle scenario when data is missing.
            callbackObject.LoginStatus = global::LoginStatus.UnFinishedRegistration;
            callbackObject.message = "User data is incomplete!";
        }
        else
        {
            // Handle general error scenario.
            callbackObject.LoginStatus = global::LoginStatus.Failed;
            callbackObject.message = "Error fetching user data.";
        }

        callback(callbackObject);
    }

    
    public void SetUserLoginSatus(bool status)
    {
        StartCoroutine(FbManager.instance.SetUserLoginStatus(status, isSusscess =>
        {
            if (isSusscess)
            {
                Debug.Log("FbManager: SetUserLoginStatus: Done!");
            }
        }));
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
    
    public enum UserStatus
    {
        Initialized,
        MissingData,
        Error
    }

    public delegate void UserCallback(UserStatus status);
    private IEnumerator GetCurrentUserDataCoroutine(UserCallback callback)
    {
        DatabaseReference userRef = _databaseReference.Child("users").Child(_firebaseUser.UserId);
        var task = userRef.GetValueAsync();

        // Wait for the task to complete
        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);

        if (task.IsFaulted)
        {
            Debug.LogError(task.Exception);
            callback(UserStatus.Error);
            yield break;
        }

        if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                string email = GetChildValue(snapshot, "email");
                string displayName = GetChildValue(snapshot, "name");
                string username = GetChildValue(snapshot, "username");
                string phone = GetChildValue(snapshot, "phone");
                string photoURL = GetChildValue(snapshot, "photo");

                thisUserModel = new UserModel(_firebaseUser.UserId, email, displayName, username, phone, photoURL, "password");
                
                string superValue = GetChildValue(snapshot, "super");
                if (bool.TryParse(superValue, out bool result) && result)
                {
                    thisUserModel.super = true;
                }
                
                // Check for missing data
                if (string.IsNullOrEmpty(phone) || username == "null" || name == "null")
                {
                    Debug.LogWarning("User Profile Missing Data");
                    callback(UserStatus.MissingData);
                    yield break;
                }
                IsFirebaseUserInitialised = true;
                Debug.Log("User Initialized");
                callback(UserStatus.Initialized);
            }
        }
    }

    private string GetChildValue(DataSnapshot snapshot, string key)
    {
        if (snapshot.HasChild(key))
        {
            return snapshot.Child(key).Value.ToString();
        }
        else
        {
            Debug.LogWarning($"The '{key}' child does not exist in the snapshot.");
            return string.Empty;
        }
    }
    
    
    public void Logout(LoginStatus loginStatus, bool isBackgroundLogout = false)
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
            SetUserLoginSatus(false);
        }
        
        TraceManager.instance.receivedTraceObjects.Clear();
        TraceManager.instance.sentTraceObjects.Clear();
        HomeScreenManager.isInSendTraceView = false;
        thisUserModel = new UserModel();
        IsFirebaseUserInitialised = false;
        _firebaseAuth.SignOut();
        
        //todo: clear player-prefs cache when we logout
        //reset player prefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("Username", "null"); //todo: just make this truly null
        PlayerPrefs.SetString("Password", "null");
        PlayerPrefs.SetInt("IsInvited", 0);
        
        if (!isBackgroundLogout)
            ScreenManager.instance.ChangeScreenForwards("Welcome");
    }
    #endregion
    
    #region -User Registration
    private string GenerateUserProfileJson(string batch, string username, string name, string userPhotoLink, string email, string phone, string createdDate) {
        TraceUserInfoStructure traceUserInfoStructure = new TraceUserInfoStructure(batch,username, name, userPhotoLink, email, phone, createdDate);
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
        
        var json = GenerateUserProfileJson("null", _username, "null", "null",_email, _phoneNumber, DateTime.UtcNow.ToString());
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
                SetUserLoginSatus(true);
                IsFirebaseUserLoggedIn = true;
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
            StartCoroutine(GetCurrentUserDataCoroutine((status) =>
            {
                switch (status)
                {
                    case UserStatus.Initialized:
                        // Handle successful initialization
                        break;
                    case UserStatus.MissingData:
                        Debug.LogWarning("User data is incomplete!");
                        // Handle missing data scenario
                        break;
                    case UserStatus.Error:
                        Debug.LogError("Error fetching user data.");
                        // Handle error scenario
                        break;
                }
            }));
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
            if (_firestoreData == null)
                _firestoreData = new Dictionary<string, object>();
            _firestoreData.Add("name",_nickName);
            callback(true);
        }
    }
    public IEnumerator SetUserPhoneNumber(string _phoneNumber, System.Action<bool> callback)
    {
        Debug.Log("Setting user phone number:" + _firebaseUser.UserId);
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("phone").SetValueAsync(_phoneNumber);
        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            callback(false);
        }
        else
        {
            if (_firestoreData == null)
                _firestoreData = new Dictionary<string, object>();
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
        {
            callback(false);
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
            Debug.Log("Image Uploaded Successfully");
            var url = task.Result.Path + "";
            callback(true,url);
        }
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
                Debug.LogWarning("Image Not Found in Firebase Storage for user:" + userId);
                //onFailed?.Invoke("Image not Found");
            }
        }));
    }
    public IEnumerator GetProfilePhotoFromFirebaseStorageRoutine(string userId, System.Action<Texture> callback)
    {
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogWarning("GetProfilePhotoFromFirebaseStorageRoutine was not called because DependencyStatus.Available is not available");
            yield break;
        }
        
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
        //Debug.Log("Adding:" + userToGetID);

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
        
        //Debug.Log("creating document refrence");

        // Reference to the specific document you want to read from
        DocumentReference docRef = _firebaseFirestore.Collection("users").Document(userToGetID);
        
        //Debug.Log("GetSnapshotAsync");
        // Read the document
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to read data from Firestore: {task.Exception}");
                return;
            }

            //Debug.Log("adding snapshot" + user.userID);
            
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
                    //Debug.Log("adding email" + fieldValue);
                    user.email = fieldValue.ToString();
                }
                if (data.ContainsKey("name"))
                {
                    object fieldValue = data["name"];
                    //Debug.Log("adding name" + fieldValue);
                    user.name = fieldValue.ToString();
                }
                if (data.ContainsKey("phone"))
                {
                    object fieldValue = data["phone"];
                    //Debug.Log("adding phone" + fieldValue);
                    user.phone = fieldValue.ToString();
                }
                if (data.ContainsKey("photo"))
                {
                    object fieldValue = data["photo"];
                    //Debug.Log("adding photo" + fieldValue);
                    user.photo = fieldValue.ToString();
                }
                if (data.ContainsKey("username"))
                {
                    object fieldValue = data["username"];
                    //Debug.Log("adding username" + fieldValue);
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
    #region -User Permissioning
    public IEnumerator SendInvite(string _phoneNumber)
    {
        string cleanedPhoneNumber = _phoneNumber.Substring(_phoneNumber.Length - 10);
        var DBTaskAddInvite = _databaseReference.Child("invited").Child(cleanedPhoneNumber).Child("users").Child(FbManager.instance.thisUserModel.userID).SetValueAsync(DateTime.UtcNow.ToString());
        while (DBTaskAddInvite.IsCompleted is false)
            yield return new WaitForEndOfFrame();
    }
    public IEnumerator ManagerUserPermissions(System.Action<bool> canEnterApp)
    {
        //check if user is un queue
        while (IsFirebaseUserInitialised == false)
        {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(IsUserListedInInvited(thisUserModel.phone, isUserInInvteList =>
        {
            if (!isUserInInvteList) //if invited welcome
            {
                canEnterApp(false);
                // StartCoroutine(GetOrSetSpotInQueue(thisUserModel.userID, spotInLine =>
                // {
                //     if (spotInLine != -1)
                //     {
                //         Debug.Log("Error");
                //     }
                //     canEnterApp(false);
                //     Debug.Log("FbManager: SetUserQueueStatus:" + true);
                // }));
            }
            else //user is invited return true
            {
                canEnterApp(true);
            }
        }));
// #endif
    }
    
    public IEnumerator IsUserListedInInvited(string _phoneNumber, System.Action<bool> callback)
    {
        string cleanedPhoneNumber = _phoneNumber.Substring(_phoneNumber.Length - 10);
        Debug.Log("Checking If User Invited:" + cleanedPhoneNumber);
        var DBTask = _databaseReference.Child("invited").Child(cleanedPhoneNumber).GetValueAsync();
     
        yield return new WaitUntil(() => DBTask.IsCompleted);
        Debug.Log("IsUserListedInInvited Complete");
        
        if (DBTask.Exception != null)
        {
            // Error occurred while retrieving data (user is not invited)
            Debug.LogWarning("user NOT Invited");
            callback(false);
            yield break; 
        }
        else if (DBTask.Result.Exists)
        {
            // User is invited (data exists in the database)
            Debug.LogWarning("user Invited Setting 1");
            PlayerPrefs.SetInt("IsInvited", 1);
            callback(true);
            yield break; 
        }
        else if(!DBTask.Result.Exists)
        {
            Debug.LogWarning("user Not Invited");
            PlayerPrefs.SetInt("IsInvited", 0);
            callback(false);
            yield break; 
        }
        Debug.Log("Smth Strange Happend");
    }

    public void AddInvitesToFriends(string phone)
    {
        StartCoroutine(AddInvitesToFriendsCoroutine(phone));
    }
    public IEnumerator AddInvitesToFriendsCoroutine(string _phoneNumber)
    {
        string cleanedPhoneNumber = _phoneNumber.Substring(_phoneNumber.Length - 10);
        var DBTask = _databaseReference.Child("invited").Child(cleanedPhoneNumber).Child("users").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        
        if (DBTask.Exception != null)
        {
            Debug.Log("user not invited or error");
        }
        else if (DBTask.Result.Exists)
        {
            // Get User Friends from invites
            foreach (var frienduserID in DBTask.Result.Children)
            {
                StartCoroutine(AcceptFriendRequest(thisUserModel.userID, frienduserID.Key.ToString(), (callbackIsSuccess) =>
                {
                    if (callbackIsSuccess)
                    {
                        Debug.Log("Added Friend");
                    }
                    else
                        Debug.LogError("Failed To Add Friend");
                }));
            }
        }
        else if(!DBTask.Result.Exists)
        {
            Debug.Log("User Does Not Have Friends");
        }
    }
    
    public void AddInviteTracesToTracesReceived(string phone)
    {
        StartCoroutine(AddInviteTracesToTracesReceivedCoroutine(phone));
    }

    public IEnumerator AddInviteTracesToTracesReceivedCoroutine(string _phoneNumber)
    {
        string cleanedPhoneNumber = _phoneNumber.Substring(_phoneNumber.Length - 10);
        var DBTask = _databaseReference.Child("invited").Child(cleanedPhoneNumber).Child("traces").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        
        if (DBTask.Exception != null)
        {
            Debug.Log("user has no traces received or error");
        }
        else if (DBTask.Result.Exists)
        {
            // Get Trace Object from invites
            foreach (var traceID in DBTask.Result.Children)
            {
                StartCoroutine(AddTracesReceivedBeforeUserToTracesReceived(traceID.Key.ToString(), traceID.Value.ToString(), (callbackIsSuccess) =>
                {
                    if (callbackIsSuccess)
                    {
                        Debug.Log("Added Friend");
                    }
                    else
                        Debug.LogError("Failed To Add Friend");
                }));
            }
        }
        else if(!DBTask.Result.Exists)
        {
            Debug.Log("User Does Not Have Friends");
        }
    }

    public IEnumerator AddTracesReceivedBeforeUserToTracesReceived(string traceID, string senderID, Action<bool> callback)
    {
        //create friends
        var task = _databaseReference.Child("TracesRecived").Child(_firebaseUser.UserId).Child(traceID).Child("Sender").SetValueAsync(senderID);
        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        if (task.IsCanceled || task.IsFaulted)
        {
            print(task.Exception.Message);
            callback(false);
        }
        else
        {
            // _allFriends.Add(friend);
            callback(true);
        }
    }

    public IEnumerator GetOrSetSpotInQueue(string userID, System.Action<int> callback)
    {
        //check if user is in queue
        Debug.Log("Getting Spot in queue");
        var DBTask = _databaseReference.Child("queue").Child(userID).GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        Debug.Log("Spot in queue");

        if (DBTask.Exception != null) //if problem
        {
            //problem
            Debug.LogWarning("Database Exeption Checking If User In Queue");
            callback(-1000000);
        }
        else if(!DBTask.Result.Exists) //if user not in queue
        {
            Debug.Log("!DBTask.Result.Exists");

            //get the length of the queue now
            Debug.Log("Getting accepted number");
            var GetAllowedNumberTask = _databaseReference.Child("queue").Child("accepted").GetValueAsync();
            yield return new WaitUntil(() => GetAllowedNumberTask.IsCompleted);
            int allowedNumber = 0;
            
            if (!GetAllowedNumberTask.IsFaulted)
                allowedNumber = Convert.ToInt32(GetAllowedNumberTask.Result.Value);

            //get length of current queue
            Debug.Log("Getting Length of Queue");
            var GetQueueLengthTask = _databaseReference.Child("queue").Child("length").GetValueAsync();
            yield return new WaitUntil(() => GetQueueLengthTask.IsCompleted);
            int queuelength = 0;
            
            if (!GetQueueLengthTask.IsFaulted)
                queuelength = Convert.ToInt32(GetQueueLengthTask.Result.Value);
            
            callback(queuelength-allowedNumber); //return spot in line
            
            // put user in queue
            Debug.Log("Adding User to Queue");
            var AddUserToQueue = _databaseReference.Child("queue").Child(userID).SetValueAsync(queuelength);
            yield return new WaitUntil(() => AddUserToQueue.IsCompleted);

            // set user in batch number
            int batchNumber = (queuelength - 1) / 100 + 1;
            var SetUserBatchNumber = _databaseReference.Child("users").Child(userID).Child("batch").SetValueAsync(batchNumber);
            yield return new WaitUntil(() => SetUserBatchNumber.IsCompleted);
            
            // //add one to queue length
            Debug.Log("increasing to queue length");
            var IncreaseQueueLength = _databaseReference.Child("queue").Child("length").SetValueAsync(queuelength + 1);
            yield return new WaitUntil(() => IncreaseQueueLength.IsCompleted);
        }
  
        else if (DBTask.Result.Exists) //if user is in queue
        {
            int allowedNumber = 0;
            int queueNumber = 0;
            
            //user is in queue
            var GetAllowedNumberTask = _databaseReference.Child("queue").Child("accepted").GetValueAsync();
            yield return new WaitUntil(() => GetAllowedNumberTask.IsCompleted);
        
            if (!GetAllowedNumberTask.IsFaulted)
                allowedNumber = Convert.ToInt32(GetAllowedNumberTask.Result.Value);
            
            var GetUserQueueNumber = _databaseReference.Child("queue").Child(userID).GetValueAsync();
            yield return new WaitUntil(() => GetUserQueueNumber.IsCompleted);
            
            if (!GetUserQueueNumber.IsFaulted)
                queueNumber = Convert.ToInt32(GetUserQueueNumber.Result.Value);
            
            //get spot in line
            callback(queueNumber-allowedNumber); //spot in line
        }
    }
    public IEnumerator AddUserToInvitedListAndGoToHomeScreen(string _phoneNumber)
    {
        string cleanedPhoneNumber = _phoneNumber.Substring(_phoneNumber.Length - 10);
        var DBTaskAddInvite = _databaseReference.Child("invited").Child(cleanedPhoneNumber).Child("users").Child(FbManager.instance.thisUserModel.userID).SetValueAsync(DateTime.UtcNow.ToString());
        while (DBTaskAddInvite.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(2f);
        ScreenManager.instance.ChangeScreenForwards("HomeScreen");

    }
    #endregion
    
    
    #region -User Subscriptions
    public void SubscribeOrUnSubscribeToReceivingTraces(bool subscribe)
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("TracesRecived").Child(_firebaseUser.UserId);
        if (subscribe)
        {
            refrence.ChildAdded += HandleChildAdded;
            refrence.ChildChanged += HandleChildChanged;
        }
        else
        {
            refrence.ChildAdded -= HandleChildAdded;
            refrence.ChildChanged -= HandleChildChanged;
        }
        
        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            StartCoroutine(GetReceivedTrace(args.Snapshot.Key)); //todo: why pass key when args.Snapshot probraly has data
        }
        
        void HandleChildChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            Debug.Log("HandleChildChanged");
            StartCoroutine(HandleReceivedTraceChanged(args.Snapshot.Key)); //todo: why pass key when args.Snapshot probraly has data
        }
    }
    public void SubscribeOrUnSubscribeToTraceGroup(bool subscribe, string groupID)
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("TraceGroups").Child(groupID);
        if (subscribe)
        {
            refrence.ChildAdded += HandleChildAdded;
            refrence.ChildChanged += HandleChildChanged;
        }
        else
        {
            refrence.ChildAdded -= HandleChildAdded;
            refrence.ChildChanged -= HandleChildChanged;
        }
        
        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            StartCoroutine(GetReceivedTrace(args.Snapshot.Key));
        }
        
        void HandleChildChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            Debug.Log("HandleChildChanged");
            StartCoroutine(HandleReceivedTraceChanged(args.Snapshot.Key)); //todo: why pass key when args.Snapshot probraly has data
        }
    }
    public void SubscribeOrUnsubscribeToSentTraces(bool subscribe)
    {
        var refrence = FirebaseDatabase.DefaultInstance.GetReference("TracesSent").Child(_firebaseUser.UserId);
        if (subscribe)
        {
            refrence.ChildAdded += HandleChildAdded;
            refrence.ChildChanged += HandleChildChanged;
        }
        else
        {
            refrence.ChildAdded -= HandleChildAdded;
            refrence.ChildChanged -= HandleChildChanged;
        }

        void HandleChildAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            StartCoroutine(GetSentTrace(args.Snapshot.Key));
        }
        
        void HandleChildChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.Log("HandleChildAdded Error");
                return;
            }
            Debug.Log("HandleChildChanged");
            StartCoroutine(HandleSentTraceChanged(args.Snapshot.Key)); //todo: why pass key when args.Snapshot probraly has data
        }
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
    
    #region Sending and Recieving Traces and Comments
    public void UploadTrace(List<string> usersToSendTo, List<string> phonesToSendTo, string fileLocation, float radius, Vector2 location, MediaType mediaType, bool sendToFollowers, DateTime expiration)
    {
        Debug.Log("UploadTrace(): File Location:" + fileLocation);
        
        //Push data to real time database
        string key = _databaseReference.Child("Traces").Push().Key;
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        
        //draw temp circle until it uploads and the map is cleared on update
        SendTraceManager.instance.isSendingTrace = true;

        //todo: can I remove this?
        //covert users to send to into receiver objects
        List<TraceReceiverObject> receiverObjects = new List<TraceReceiverObject>();
        foreach (var user in usersToSendTo)
        {
            var relationship = FriendsModelManager.Instance.GetRelationship(user);
            if(relationship == Relationship.SuperUser || relationship == Relationship.Following)
                continue;
            
            receiverObjects.Add(new TraceReceiverObject(user, false));
        }
        
        _drawTraceOnMap.sendingTraceTraceLoadingObject = new TraceObject(location.x, location.y, radius, receiverObjects, new Dictionary<string, TraceCommentObject>(), "null",thisUserModel.name,  DateTime.UtcNow.ToString(), DateTime.UtcNow.AddHours(24), false, mediaType.ToString(), "temp", true, false);
        _drawTraceOnMap.DrawCircle(location.x, location.y, radius, DrawTraceOnMap.TraceType.SENDING, "null", false);
        
        //update global traces
        childUpdates["Traces/" + key + "/senderID"] = _firebaseUser.UserId;
        childUpdates["Traces/" + key + "/senderName"] = thisUserModel.name;
        childUpdates["Traces/" + key + "/sendTime"] = DateTime.UtcNow.ToString();
        childUpdates["Traces/" + key + "/mediaType"] = mediaType.ToString();
        childUpdates["Traces/" + key + "/lat"] = location.x;
        childUpdates["Traces/" + key + "/long"] = location.y;
        childUpdates["Traces/" + key + "/radius"] = radius;
        childUpdates["Traces/" + key + "/expiration"] = expiration.ToString();
        
        if (PlayerPrefs.GetInt("LeaveTraceIsVisable") == 1)
        {
            childUpdates["Traces/" + key + "/isVisable"] = true;
        }
        else
        {
            childUpdates["Traces/" + key + "/isVisable"] = false;
        }

        int count = 0;
        foreach (var user in usersToSendTo) //each of the users in usersToSendToList is a UID
        {
            count++;
            var relationship = FriendsModelManager.Instance.GetRelationship(user);
            if (relationship == Relationship.SuperUser || relationship == Relationship.Following)
            {
                childUpdates["TraceGroups/" + user + "/" + key] = DateTime.UtcNow.ToString();
            }
            
            childUpdates["Traces/" + key + "/Reciver/" + user + "/HasViewed"] = false;
            childUpdates["Traces/" + key + "/Reciver/" + user + "/ProfilePhoto"] = "null";
            childUpdates["TracesRecived/" + user +"/"+ key + "/Sender"] = thisUserModel.userID;
            childUpdates["TracesRecived/" + user+"/" + key + "/updated"] = DateTime.UtcNow.ToString();
        }
        
        foreach (var phone in phonesToSendTo)
        {
            count++;
            try //incase phone is formated weirdly
            {
                //invite and send trace
                childUpdates["invited/" +  phone.Substring(phone.Length - 10) + "/users/" + thisUserModel.userID] = DateTime.UtcNow.ToString();
                childUpdates["invited/" +  phone.Substring(phone.Length - 10) + "/traces/" + key] = thisUserModel.userID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        if (sendToFollowers)
        {
            childUpdates["TraceGroups/" + thisUserModel.userID + "/" + key] = DateTime.UtcNow.ToString();
        }
        
        childUpdates["Traces/" + key + "/numPeopleSent"] = count;
        childUpdates["TracesSent/" + _firebaseUser.UserId.ToString() +"/" + key] = DateTime.UtcNow.ToString();

        //Upload Content
        StorageReference traceReference = _firebaseStorageReference.Child("/Traces/" + key);
        traceReference.PutFileAsync(fileLocation)
            .ContinueWith((Task<StorageMetadata> task) => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.LogError("FB Error failed to upload with task.exception: " + task.Exception.ToString());
                    SendTraceManager.instance.isSendingTrace = false;
                    return;
                }
                else if(task.IsCompleted)
                {
                    _databaseReference.UpdateChildrenAsync(childUpdates); //update real time DB
                    SendTraceManager.instance.isSendingTrace = false; //done sendingTrace cant callback because its a void
                    Debug.Log("FB: Finished uploading...");
                    Debug.Log("uploaded to:" + "/Traces/" + key);
                    try 
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
    public void MarkTraceAsOpened(TraceObject trace)
    {
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        childUpdates["Traces/" + trace.id + "/Reciver/"+ _firebaseUser.UserId +"/HasViewed"] = true;
        _databaseReference.UpdateChildrenAsync(childUpdates);
        trace.HasBeenOpened = true;
    }

    public void RemoveTraceFromMap(TraceObject trace)
    {
        Debug.Log("Removing Trace From Map");
        var senderRelation = FriendsModelManager.GetFriendModelByOtherFriendID(trace.senderID).relationship;
        if (senderRelation != Relationship.SuperUser && senderRelation != Relationship.Following && trace.senderID != thisUserModel.userID)
        {
            Debug.Log("Removing Received Trace From Map");
            Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
            childUpdates["TracesRecived/" + thisUserModel.userID + "/" + trace.id] = null;
            _databaseReference.UpdateChildrenAsync(childUpdates);
            TraceManager.instance.receivedTraceObjects[trace.id].marker.enabled = false;
            TraceManager.instance.receivedTraceObjects.Remove(trace.id);
            TraceManager.instance.UpdateMap(new Vector2(0,0));
        }else if (trace.senderID == thisUserModel.userID)
        {
            Debug.Log("Removing Sent Trace From Map");
            Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
            childUpdates["TracesSent/" + thisUserModel.userID + "/" + trace.id] = null;
            _databaseReference.UpdateChildrenAsync(childUpdates);
            TraceManager.instance.sentTraceObjects[trace.id].marker.enabled = false;
            TraceManager.instance.sentTraceObjects.Remove(trace.id);
            TraceManager.instance.UpdateMap(new Vector2(0,0));
        }
    }
    public IEnumerator GetReceivedTrace(string traceID)
    {
        if (lowConnectivitySmartLogin)
        {
            var alreadyExistsLocally = TraceManager.instance.receivedTraceObjects.FirstOrDefault(pair => pair.Value.id == traceID).Value;
            if (alreadyExistsLocally != null)
            {
                Debug.Log("Trace already exists locally");
                yield break;
            }
        }
        
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
            DateTime experation = new DateTime();
            bool experationExisits = false;
            bool traceHasBeenOpenedByThisUser = false;
            List<TraceReceiverObject> receivers = new List<TraceReceiverObject>();
            Dictionary<string, TraceCommentObject> comments = new Dictionary<string, TraceCommentObject>();
            
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
                    case "Reciver":
                    {
                        Dictionary<string, object> people = thing.Value as Dictionary<string, object>;
                        foreach (var receiver in people)
                        {
                            var receiverID = receiver.Key;
                            var receiverData = receiver.Value as Dictionary<string, object>;
                            bool hasViewed = (bool)receiverData["HasViewed"];
                            //string profilePhoto = receiverData["ProfilePhoto"].ToString(); //if we ever want profile photo
                            receivers.Add(new TraceReceiverObject(receiverID, hasViewed));
                            
                            //check if this user opened it
                            if (receiverID == FbManager.instance.thisUserModel.userID && hasViewed)
                            {
                                traceHasBeenOpenedByThisUser = true;
                            }
                        }
                        break;
                    }
                    case "comments":
                    {
                        Debug.Log("Getting Comments");
                        Dictionary<string, object> _comments = thing.Value as Dictionary<string, object>;
                        foreach (var comment in _comments)
                        {
                            var commentID = comment.Key;
                            var commentData = comment.Value as Dictionary<string, object>;
                            string time = commentData["time"].ToString();
                            string sender = commentData["senderID"].ToString();
                            string name = commentData["senderName"].ToString();
                            if (commentData.ContainsKey("wave"))
                            {
                                string extractedValuesJson = commentData["wave"].ToString();
                                SerializableFloatArray serializableFloatArray = JsonUtility.FromJson<SerializableFloatArray>(extractedValuesJson);
                                float[] extractedValues = serializableFloatArray.data;
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,extractedValues));
                            }
                            else
                            {
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,new float[]{}));
                            }
                        }
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
                    case "expiration":
                        bool parsedSuccessfully = DateTime.TryParseExact(thing.Value.ToString(), "M/d/yyyy h:mm:ss tt", 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out experation);

                        if (parsedSuccessfully)
                            experationExisits = true;
                        break;
                }
            }
            
            if (lat != 0 && lng != 0 && radius != 0) //check for malformed data entry
            {
                //todo: Take Action Based On If Trace Is Expired
                bool isExpired = experationExisits && HelperMethods.IsTraceExpired(experation);
                var trace = new TraceObject(lng, lat, radius, receivers, comments, senderID, senderName, sendTime, experation, experationExisits, mediaType,traceID, traceHasBeenOpenedByThisUser, isExpired);
                TraceManager.instance.receivedTraceObjects.Add(trace.id,trace);
                BackgroundDownloadManager.s_Instance.DownloadMediaInBackground(trace.id,trace.mediaType);
                TraceManager.instance.UpdateMap(new Vector2());
                FbManager.instance.AnalyticsSetTracesReceived(TraceManager.instance.receivedTraceObjects.Count.ToString());
            }
        }
    }
    public IEnumerator GetSentTrace(string traceID)
    {
        if (lowConnectivitySmartLogin)
        {
            var alreadyExistsLocally = TraceManager.instance.sentTraceObjects.FirstOrDefault(traceObject => traceObject.Value.id == traceID).Value;
            if (alreadyExistsLocally != null)
            {
                Debug.Log("Trace already exists locally");
                yield break;
            }
        }
        
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
            string senderID = "";
            List<TraceReceiverObject> receivers = new List<TraceReceiverObject>();
            Dictionary<string, TraceCommentObject> comments = new Dictionary<string, TraceCommentObject>();
            string senderName = "";
            string sendTime = "";
            string mediaType = "";
            DateTime experation = new DateTime();
            bool experationExisits = false;

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
                    case "Reciver":
                    {
                        Dictionary<string, object> people = thing.Value as Dictionary<string, object>;
                        foreach (var receiver in people)
                        {
                            var receiverID = receiver.Key;
                            var receiverData = receiver.Value as Dictionary<string, object>;
                            bool hasViewed = (bool)receiverData["HasViewed"];
                            //string profilePhoto = receiverData["ProfilePhoto"].ToString(); //if we ever want profile photo
                            receivers.Add(new TraceReceiverObject(receiverID,hasViewed));
                        }
                        break;
                    }
                    case "comments":
                    {
                        Dictionary<string, object> _comments = thing.Value as Dictionary<string, object>;
                        foreach (var comment in _comments)
                        {
                            var commentID = comment.Key;
                            var commentData = comment.Value as Dictionary<string, object>;
                            string time = commentData["time"].ToString();
                            string sender = commentData["senderID"].ToString();
                            string name = commentData["senderName"].ToString();
                            if (commentData.ContainsKey("wave"))
                            {
                                string extractedValuesJson = commentData["wave"].ToString();
                                Debug.Log("extractedValuesJson:" + extractedValuesJson);
                                SerializableFloatArray serializableFloatArray = JsonUtility.FromJson<SerializableFloatArray>(extractedValuesJson);
                                float[] extractedValues = serializableFloatArray.data;
                                Debug.Log("extracted soudWave values:" + extractedValues.Length);
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,extractedValues));
                            }
                            else
                            {
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,new float[]{}));
                            }
                        }
                        break;
                    }
                    case "mediaType":
                    {
                        mediaType = thing.Value.ToString();
                        break;
                    }
                    case "expiration":
                       
                        bool parsedSuccessfully = DateTime.TryParseExact(thing.Value.ToString(), "M/d/yyyy h:mm:ss tt", 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out experation);

                        if (parsedSuccessfully)
                            experationExisits = true;
                        break;
                }
            }
            
            if (lat != 0 && lng != 0 && radius != 0)
            {
                bool isExpired = experationExisits && HelperMethods.IsTraceExpired(experation);
                var trace = new TraceObject(lng, lat, radius, receivers, comments, senderID, senderName, sendTime, experation, experationExisits, mediaType,traceID, false, isExpired);
                TraceManager.instance.sentTraceObjects.Add(trace.id,trace);
                BackgroundDownloadManager.s_Instance.DownloadMediaInBackground(trace.id,trace.mediaType);
                TraceManager.instance.UpdateMap(new Vector2());
                FbManager.instance.AnalyticsSetTracesSent(TraceManager.instance.sentTraceObjects.Count.ToString());
            }
        }
    }
    public IEnumerator HandleReceivedTraceChanged(string traceID)
    {
        if (lowConnectivitySmartLogin)
        {
            var alreadyExistsLocally = TraceManager.instance.receivedTraceObjects.FirstOrDefault(pair => pair.Value.id == traceID).Value;
            if (alreadyExistsLocally != null)
            {
                Debug.Log("Trace already exists locally");
                yield break;
            }
        }
        
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
            bool traceHasBeenOpenedByThisUser = false;
            List<TraceReceiverObject> receivers = new List<TraceReceiverObject>();
            Dictionary<string, TraceCommentObject> comments = new Dictionary<string, TraceCommentObject>();
            DateTime experation = new DateTime();
            bool experationExisits = false;

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
                    case "Reciver":
                    {
                        Dictionary<string, object> people = thing.Value as Dictionary<string, object>;
                        foreach (var receiver in people)
                        {
                            var receiverID = receiver.Key;
                            var receiverData = receiver.Value as Dictionary<string, object>;
                            bool hasViewed = (bool)receiverData["HasViewed"];
                            //string profilePhoto = receiverData["ProfilePhoto"].ToString(); //if we ever want profile photo
                            receivers.Add(new TraceReceiverObject(receiverID, hasViewed));
                            
                            //check if this user opened it
                            if (receiverID == FbManager.instance.thisUserModel.userID && hasViewed)
                            {
                                traceHasBeenOpenedByThisUser = true;
                            }
                        }
                        break;
                    }
                    case "comments":
                    {
                        Debug.Log("Getting Comments");
                        Dictionary<string, object> _comments = thing.Value as Dictionary<string, object>;
                        foreach (var comment in _comments)
                        {
                            var commentID = comment.Key;
                            var commentData = comment.Value as Dictionary<string, object>;
                            string time = commentData["time"].ToString();
                            string sender = commentData["senderID"].ToString();
                            string name = commentData["senderName"].ToString();
                            if (commentData.ContainsKey("wave"))
                            {
                                string extractedValuesJson = commentData["wave"].ToString();
                                SerializableFloatArray serializableFloatArray = JsonUtility.FromJson<SerializableFloatArray>(extractedValuesJson);
                                float[] extractedValues = serializableFloatArray.data;
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,extractedValues));
                            }
                            else
                            {
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,new float[]{}));
                            }
                        }
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
                    case "expiration":
                        bool parsedSuccessfully = DateTime.TryParseExact(thing.Value.ToString(), "M/d/yyyy h:mm:ss tt", 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out experation);

                        if (parsedSuccessfully)
                            experationExisits = true;
                        break;
                }
            }
            if (lat != 0 && lng != 0 && radius != 0) //check for malformed data entry
            {
                bool isExpired = experationExisits && HelperMethods.IsTraceExpired(experation);
                var trace = new TraceObject(lng, lat, radius, receivers, comments, senderID, senderName, sendTime, experation, experationExisits, mediaType,traceID, traceHasBeenOpenedByThisUser, isExpired);
                Debug.Log("Changed:" + trace.id + " to dict");
                TraceManager.instance.sentTraceObjects[trace.id] = trace; //update trace
                TraceManager.instance.RefreshTrace(trace);
                BackgroundDownloadManager.s_Instance.DownloadMediaInBackground(trace.id,trace.mediaType);
                TraceManager.instance.UpdateMap(new Vector2());
                FbManager.instance.AnalyticsSetTracesReceived(TraceManager.instance.receivedTraceObjects.Count.ToString());
            }
        }
    }
    public IEnumerator HandleSentTraceChanged(string traceID)
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
            string senderID = "";
            List<TraceReceiverObject> receivers = new List<TraceReceiverObject>();
            Dictionary<string, TraceCommentObject> comments = new Dictionary<string, TraceCommentObject>();
            string senderName = "";
            string sendTime = "";
            string mediaType = "";
            DateTime experation = new DateTime();
            bool experationExisits = false;
            
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
                    case "Reciver":
                    {
                        Dictionary<string, object> people = thing.Value as Dictionary<string, object>;
                        foreach (var receiver in people)
                        {
                            var receiverID = receiver.Key;
                            var receiverData = receiver.Value as Dictionary<string, object>;
                            bool hasViewed = (bool)receiverData["HasViewed"];
                            //string profilePhoto = receiverData["ProfilePhoto"].ToString(); //if we ever want profile photo
                            receivers.Add(new TraceReceiverObject(receiverID,hasViewed));
                        }
                        break;
                    }
                    case "comments":
                    {
                        Dictionary<string, object> _comments = thing.Value as Dictionary<string, object>;
                        foreach (var comment in _comments)
                        {
                            var commentID = comment.Key;
                            var commentData = comment.Value as Dictionary<string, object>;
                            string time = commentData["time"].ToString();
                            string sender = commentData["senderID"].ToString();
                            string name = commentData["senderName"].ToString();
                            if (commentData.ContainsKey("wave"))
                            {
                                string extractedValuesJson = commentData["wave"].ToString();
                                SerializableFloatArray serializableFloatArray = JsonUtility.FromJson<SerializableFloatArray>(extractedValuesJson);
                                float[] extractedValues = serializableFloatArray.data;
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,extractedValues));
                            }
                            else
                            {
                                comments.Add(commentID,new TraceCommentObject(commentID, time, sender, name,new float[]{}));
                            }
                        }
                        break;
                    }
                    case "mediaType":
                    {
                        mediaType = thing.Value.ToString();
                        break;
                    }
                    case "expiration":
                        bool parsedSuccessfully = DateTime.TryParseExact(thing.Value.ToString(), "M/d/yyyy h:mm:ss tt", 
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out experation);
                        if (parsedSuccessfully)
                            experationExisits = true;
                        break;
                }
            }
            
            if (lat != 0 && lng != 0 && radius != 0)
            {
                bool isExpired = experationExisits && HelperMethods.IsTraceExpired(experation);
                var trace = new TraceObject(lng, lat, radius, receivers, comments, senderID,senderName, sendTime, experation, experationExisits, mediaType,traceID, false, isExpired);
                Debug.Log("Trace Comments Update To:" + comments.Count);
                TraceManager.instance.sentTraceObjects[trace.id] = trace; //update trace
                TraceManager.instance.RefreshTrace(trace); //update if currently being displayed
                BackgroundDownloadManager.s_Instance.DownloadMediaInBackground(trace.id,trace.mediaType);
                TraceManager.instance.UpdateMap(new Vector2());
                FbManager.instance.AnalyticsSetTracesSent(TraceManager.instance.sentTraceObjects.Count.ToString());
                
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
        Debug.Log("Trace Not Stored Locally RetrivingTraceFromDatabase");
        var request = new UnityWebRequest();
        var url = "";

        
        StorageReference pathReference = _firebaseStorage.GetReference("Traces/" + _url);
        Debug.Log("Database Storgage Path:" + pathReference.ToString());

        Debug.Log("Getting path from:" + "Traces/" + _url);
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

        Debug.Log("Downloading Video" + url);
        yield return request.SendWebRequest(); //Wait for the request to complete

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("error:" + request.error);
        }
        else
        {
            Debug.Log("Correctly Got Video From Database");
            var path = Application.persistentDataPath + "/" + "Video" + ".mp4";
            File.WriteAllBytes(path, request.downloadHandler.data);
            Debug.Log("Downloaded Video!");
            Debug.Log("Video Location:" + path);
            callback(path);
        }
    }

    public IEnumerator GetTraceAudioByUrl(string _url, System.Action<string> callback)
    {
        StorageReference pathReference = _firebaseStorage.GetReference("Comments/" + _url);
        Debug.Log("path refrence:" + pathReference);

        var task = pathReference.GetDownloadUrlAsync();

        while (!task.IsCompleted)
            yield return new WaitForEndOfFrame();

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Task failed. Reason: " + task.Exception?.Message);
            yield break;
        }

        string url = task.Result.ToString();
        Debug.Log("Download URL: " + url);

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Request error: " + request.error);
            yield break;
        }

        string pathWithoutExtension = Application.persistentDataPath + "/Comments/" + _url;
        string directoryPath = Path.GetDirectoryName(pathWithoutExtension);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath); // This will create all subdirectories in the path that don't exist
        }
        
        // Ensure the .wav extension
        // Ensure the .wav extension
        string finalPath = Path.ChangeExtension(pathWithoutExtension, ".wav");

        File.WriteAllBytes(finalPath, request.downloadHandler.data);
        Debug.Log("Downloaded Audio!");
        Debug.Log("Audio Location:" + finalPath);
        callback(finalPath);
    }

    
    public void UploadComment(TraceObject trace, string fileLocation, float[] extractedValues)
    {
        Debug.Log(" UploadComment(): File Location:" + fileLocation + "with sound wave length:" + extractedValues.Length);
        Debug.Log("Sound wave:" + extractedValues[0] + ", " + extractedValues[1] + ", " + extractedValues[3] + "...");
        
        //PUSH DATA TO REAL TIME DB
        string key = _databaseReference.Child("Traces").Child(trace.id).Child("comments").Push().Key;
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        
        //update global traces
        childUpdates["Traces/" + trace.id + "/comments/" + key + "/senderName"] = thisUserModel.name;
        childUpdates["Traces/" + trace.id + "/comments/" + key + "/senderID"] = thisUserModel.userID;
        childUpdates["Traces/" + trace.id + "/comments/" + key + "/time"] = DateTime.UtcNow.ToString();
        
        //upload simple sound wave
        SerializableFloatArray serializableFloatArray = new SerializableFloatArray { data = extractedValues };
        string extractedValuesJson = JsonUtility.ToJson(serializableFloatArray);
        childUpdates["Traces/" + trace.id + "/comments/" + key + "/wave"] = extractedValuesJson;
        Debug.Log("SoundWave:" + extractedValuesJson);
        
        if(trace.senderID == thisUserModel.userID)
            childUpdates["TracesSent/" + thisUserModel.userID +"/" + trace.id] = DateTime.UtcNow.ToString(); //change last updated
        
        foreach (var user in trace.people)
        {
            if(user.id == thisUserModel.userID) //make sure we dont make a sent trace become received
                continue;
            
            childUpdates["TracesRecived/" + user.id +"/"+ trace.id + "/updated"] = DateTime.UtcNow.ToString(); //change last updated
            childUpdates["TracesRecived/" + user.id +"/"+ trace.id + "/Sender"] = trace.senderID;
        }

        //Upload Content
        StorageReference traceReference = _firebaseStorageReference.Child("/Comments/" + trace.id + "/" + key);
        traceReference.PutFileAsync(fileLocation)
            .ContinueWith((Task<StorageMetadata> task) => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.LogError("FB Error failed to upload with task.exception: " + task.Exception.ToString());
                    return;
                }
                else if(task.IsCompleted)
                {
                    Debug.Log("FB: Finished uploading...");
                    _databaseReference.UpdateChildrenAsync(childUpdates); //update real time DB
                    try 
                    {
                        //todo: un comment for production
                        SendCommentManager.instance.SendNotificationToUsersWhoRecivedTheComment();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Faied To Send Comment Notification Trying Again");
                        SendCommentManager.instance.SendNotificationToUsersWhoRecivedTheComment();
                    }
                }
            });
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

    public enum LoginStatus
    {
        LoggedIn, LoggedOut
    }


    public IEnumerator GetTraceMediaDownloadURL(string _url, System.Action<string> onSuccess, System.Action onFailed)
    {
        var url = "";
        StorageReference pathReference = _firebaseStorage.GetReference("Traces/" + _url);

        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (!task.IsFaulted && !task.IsCanceled)
        {
            // Debug.Log("Download URL: " + task.Result);
            // Debug.Log("Actual  URL: " + url);
            url = task.Result + "";
            onSuccess(url);
        }
        else
        {
            Debug.Log("task failed:" + task.Result);
            onFailed();
        }
    }
    public IEnumerator GetTraceCommentDownloadURL(string _url, System.Action<string> onSuccess, System.Action onFailed)
    {
        var url = "";
        StorageReference pathReference = _firebaseStorage.GetReference("Comments/" + _url);

        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (!task.IsFaulted && !task.IsCanceled)
        {
            url = task.Result + "";
            onSuccess(url);
        }
        else
        {
            Debug.Log("task failed:" + task.Result);
            onFailed();
        }
    }
}
