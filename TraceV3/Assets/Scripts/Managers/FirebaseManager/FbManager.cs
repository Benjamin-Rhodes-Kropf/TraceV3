using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Storage;
using Unity.VisualScripting;
using UnityEngine.Networking;
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

    [Header("Login Settings")] 
    [SerializeField] private bool autoLogin;
    [SerializeField] private bool useAdminForLogin;
    [SerializeField] private string adminUser;
    [SerializeField] private string adminPass;
    [SerializeField] private bool resetPlayerPrefs;

    [Header("Maps References")]
    [SerializeField] private DrawTraceOnMap _drawTraceOnMap;
    
    [Header("User Data")] 
    public Texture userImageTexture;
    public UserModel thisUserModel;
    [SerializeField] private List<UserModel> users;
    
    
    public List<UserModel> AllUsers
    {
        get { return users; }
    }
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
        
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                if (autoLogin)
                {
                    if (useAdminForLogin)
                    {
                        PlayerPrefs.SetString("Username", adminUser);
                        PlayerPrefs.SetString("Password", adminPass);
                    }
                }
                else
                {
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
                if (myReturnValue.IsSuccessful)
                {
                    Debug.Log("FbManager: Logged in!");
                    
                    ScreenManager.instance.ChangeScreenFade("HomeScreen");
                }
                else
                {
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
            callbackObject.IsSuccessful = false;
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    callbackObject.message = message;
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    callbackObject.message = message;
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    callbackObject.message = message;
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    callbackObject.message = message;
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    callbackObject.message = message;
                    break;
            }
            Debug.Log("FBManager: failed to log in");
            callbackObject.IsSuccessful = false;
            callbackObject.message = message;
            callback(callbackObject);
            yield break;
        }

        _firebaseUser = LoginTask.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})", _firebaseUser.DisplayName, _firebaseUser.Email);
        Debug.Log("logged In: user profile photo is: " + _firebaseUser.PhotoUrl);
        callbackObject.IsSuccessful = true;
        
        //stay logged in
        PlayerPrefs.SetString("Username", _email);
        PlayerPrefs.SetString("Password", _password);
        PlayerPrefs.Save();

        //once user logged in
        GetAllUsers(); //Todo: we really should not be doing this
        GetCurrentUserData(_password);
        ContinuesListners();
        InitializeFCMService();
        
        //set login status
        if (callbackObject.IsSuccessful == true)
        {
            StartCoroutine(SetUserLoginStatus(true, isSusscess =>
            {
                if (isSusscess)
                {
                    Debug.Log("FbManager: SetUserLoginStatus: Done!");
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
        _databaseReference.Child("users").ChildChanged += HandleUserChanged;
        _databaseReference.Child("users").Child(_firebaseUser.UserId).ChildRemoved -= HandleRemoveUser;
        
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
            if (task.IsCompleted)
            {
                // Iterate through the children of the "users" node and add each username to the list
                DataSnapshot snapshot = task.Result;
                    string email = snapshot.Child("email").Value.ToString();
                    //string frindCount = snapshot.Child("friendCount").Value.ToString();
                    string displayName = snapshot.Child("name").Value.ToString();
                    string username = snapshot.Child("username").Value.ToString();
                    string phoneNumber = snapshot.Child("phoneNumber").Value.ToString();
                    string photoURL = snapshot.Child("userPhotoUrl").Value.ToString();
                    thisUserModel = new UserModel(_firebaseUser.UserId,email,0,displayName,username,phoneNumber,photoURL, password);
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
    private string GenerateUserProfileJson(string username, string name, string userPhotoLink, string email, string phone) {
        TraceUserInfoStructure traceUserInfoStructure = new TraceUserInfoStructure(username, name, userPhotoLink, email, phone);
        string json = JsonUtility.ToJson(traceUserInfoStructure);
        return json;
    }
    public IEnumerator RegisterNewUser(string _email, string _password, string _username, string _phoneNumber,  System.Action<String,AuthError> callback)
    {
        if (_username == "")
        {
            callback("Missing Username", AuthError.None); //having a blank nickname is not really a DB error so I return a error here
            yield break;
        }
        Task<FirebaseUser> RegisterTask  =null;
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
        
        var json = GenerateUserProfileJson( _username, "null", "null",_email, _phoneNumber);
        _databaseReference.Child("users").Child(_firebaseUser.UserId.ToString()).SetRawJsonValueAsync(json);
       
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
            callback(true);
        }
    }
    public IEnumerator SetUserProfilePhotoUrl(string _photoUrl, System.Action<bool> callback)
    {
        Debug.Log("Db update photoUrl to :" + _photoUrl);
        //Set the currently logged in user nickName in the database
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("userPhotoUrl").SetValueAsync(_photoUrl);
        
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
            callback(true);
        }
    }
    public IEnumerator SetUserPhoneNumber(string _phoneNumber, System.Action<bool> callback)
    {
        Debug.LogError("Is Database Reference is Null  ? "+ _databaseReference == null);
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("phoneNumber").SetValueAsync(_phoneNumber);

        Debug.LogError("Is Database Completion is Null  ? "+ DBTask == null);

        while (DBTask.IsCompleted is false)
            yield return new WaitForEndOfFrame();
        
        // yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

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
            Debug.Log("Download URL: " + task.Result);
            var url = task.Result.Path + "";
            Debug.Log("Actual  URL: " + url);
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
    public void GetProfilePhotoFromFirebaseStorage(string userId, Action<Texture> onSuccess, Action<string> onFailed) {
        StartCoroutine(GetProfilePhotoFromFirebaseStorageRoutine(userId, (myReturnValue) => {
            if (myReturnValue != null)
            {
                onSuccess?.Invoke(myReturnValue);
            }

            {
                onFailed?.Invoke("Image not Found");
            }
        }));
    }
    public IEnumerator GetProfilePhotoFromFirebaseStorageRoutine(string userId, System.Action<Texture> callback)
    {
        // var request = new UnityWebRequest();
        var url = "";
        StorageReference pathReference = _firebaseStorage.GetReference("ProfilePhoto/"+userId+".png");
        var task = pathReference.GetDownloadUrlAsync();

        while (task.IsCompleted is false) yield return new WaitForEndOfFrame();

        if (!task.IsFaulted && !task.IsCanceled) {
            url = task.Result + "";
        }
        else
        {
            //Debug.Log("task failed:" + task.Result);
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
                     Debug.Log("SNAP :" + snap.Key);
                     try
                     {
                         UserModel userData = new UserModel();
                         userData.userId =  snap.Key.ToString(); 
                         userData.Email = snap.Child("email").Value.ToString();
                         userData.DisplayName =snap.Child("name").Value.ToString();
                         userData.Username = snap.Child("username").Value.ToString();
                         userData.PhoneNumber =  snap.Child("phoneNumber").Value.ToString();
                         userData.PhotoURL = snap.Child("userPhotoUrl").Value.ToString();
                         users.Add(userData); 
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e);
                         throw;
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
            userData.userId =  args.Snapshot.Key.ToString(); 
            Debug.Log("HandleUserAdded UserID:" + userData.userId);
            userData.Email = args.Snapshot.Child("email").Value.ToString();
            Debug.Log("HandleUserAdded email:" + userData.Email);
            userData.DisplayName =args.Snapshot.Child("name").Value.ToString();
            Debug.Log("HandleUserAdded name:" + userData.DisplayName);
            userData.Username = args.Snapshot.Child("username").Value.ToString();
            if (userData.Username == "null" || string.IsNullOrEmpty(userData.Username))
            {
                Debug.Log("User Is Not Setup Correctly");
                return;
            }
            Debug.Log("HandleUserAdded username:" + userData.Username);
            userData.PhoneNumber =  args.Snapshot.Child("phoneNumber").Value.ToString();
            if (userData.PhoneNumber == "") //todo add more
            {
                return;
            }
            Debug.Log("HandleUserAdded phone:" + userData.PhoneNumber);
            userData.PhotoURL = args.Snapshot.Child("userPhotoUrl").Value.ToString();
            Debug.Log("HandleUserAdded photo:" + userData.PhotoURL);
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
            var userToChange = GetUserByID(userID);
            if (userToChange == null)
            {
                //create a new user
                try
                {
                    userToChange = new UserModel();
                    userToChange.userId = userID;
                    userToChange.Email = args.Snapshot.Child("email").Value.ToString();
                    Debug.Log("HandleUserAdded email:" + userToChange.Email);
                    userToChange.DisplayName = args.Snapshot.Child("name").Value.ToString();
                    Debug.Log("HandleUserAdded name:" + userToChange.DisplayName);
                    userToChange.Username = args.Snapshot.Child("username").Value.ToString();
                    if (userToChange.Username == "null" || string.IsNullOrEmpty(userToChange.Username))
                    {
                        Debug.Log("User Is Not Setup Correctly");
                        return;
                    }
                    Debug.Log("HandleUserAdded username:" + userToChange.Username);
                    userToChange.PhoneNumber = args.Snapshot.Child("phoneNumber").Value.ToString();
                    Debug.Log("HandleUserAdded phone:" + userToChange.PhoneNumber);
                    userToChange.PhotoURL = args.Snapshot.Child("userPhotoUrl").Value.ToString();
                    Debug.Log("HandleUserAdded photo:" + userToChange.PhotoURL);
                    users.Add(userToChange);
                }
                catch
                {
                    Debug.Log("Friend Malformed");
                }
                return;
            }
            
            //add to existing user
            userToChange.Email = args.Snapshot.Child("email").Value.ToString();
            Debug.Log("HandleUserAdded email:" + userToChange.Email);
            userToChange.DisplayName =args.Snapshot.Child("name").Value.ToString();
            Debug.Log("HandleUserAdded name:" + userToChange.DisplayName);
            userToChange.Username = args.Snapshot.Child("username").Value.ToString();
            if (userToChange.Username == "null" || string.IsNullOrEmpty(userToChange.Username))
            {
                Debug.Log("User Is Not Setup Correctly");
                return;
            }
            Debug.Log("HandleUserAdded username:" + userToChange.Username);
            userToChange.PhoneNumber =  args.Snapshot.Child("phoneNumber").Value.ToString();
            if (userToChange.PhoneNumber == "") //todo add more
            {
                return;
            }
            Debug.Log("HandleUserAdded phone:" + userToChange.PhoneNumber);
            userToChange.PhotoURL = args.Snapshot.Child("userPhotoUrl").Value.ToString();
            Debug.Log("HandleUserAdded photo:" + userToChange.PhotoURL);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private UserModel GetUserByID(string userToGetID)
    {
        foreach (var userObject in users)
        {
            if (userObject.userId == userToGetID)
            {
                return userObject;
            }
        }
        return null;
    }
    
    private void HandleRemoveUser(object sender, ChildChangedEventArgs args)
    {
        //todo: handle remove user
    }
    #endregion
    #region -User Actions
    
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
            Debug.Log("SentTraceAdded:" + args.Snapshot.Key);
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
        _drawTraceOnMap.DrawCirlce(location.x, location.y, radius, DrawTraceOnMap.TraceType.SENDING, "temp");
        
        //update global traces
        childUpdates["Traces/" + key + "/senderID"] = _firebaseUser.UserId;
        childUpdates["Traces/" + key + "/senderName"] = thisUserModel.DisplayName;
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
        foreach (var user in usersToSendToList)
        {
            count++;
            //update data for within trace
            childUpdates["Traces/" + key + "/Reciver/" + user + "/HasViewed"] = false;
            childUpdates["Traces/" + key + "/Reciver/" + user + "/ProfilePhoto"] = "null";
            //update data for each user
            childUpdates["TracesRecived/" + user +"/"+ key + "/Sender"] = thisUserModel.userId;
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
                }
                else {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    // string md5Hash = metadata.Md5Hash;
                    Debug.Log("FB: Finished uploading...");
                    //upload metadata to real time DB
                    _databaseReference.UpdateChildrenAsync(childUpdates);
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
                        Debug.Log(traceID + "lat: " + thing.Value);
                        Debug.Log(thing.Value);
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
                        Debug.Log(traceID + "radius: " + thing.Value);
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
}
