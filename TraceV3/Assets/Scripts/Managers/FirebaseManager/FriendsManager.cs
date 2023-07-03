using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using UnityEngine;
using Firebase.Database;


public partial class FbManager
{
    //[HideInInspector] public List<string> _previousRequestFrom;
    public List<FriendRequests> _allReceivedRequests;
    public List<FriendRequests> _allSentRequests;
    public List<FriendModel> _allFriends;

    #region Continues Listners
    private void HandleReceivedFriendRequest(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleFriendRequest");
        try
        {
            if (args.Snapshot != null && args.Snapshot.Value != null)
            {
                string senderId = args.Snapshot.Child("senderId").Value.ToString();
                string receiverId = args.Snapshot.Child("receiverId").Value.ToString();

                //I dont need these because of new Db structure?
                //  if (receiverId != _firebaseUser.UserId)
                //  {
                //      _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildAdded -= HandleReceivedFriendRequest;
                //      return;
                //  }
                //
                // if (FriendRequestManager.Instance.IsRequestAllReadyInList(senderId) || senderId == _firebaseUser.UserId)
                // {
                //     Debug.Log("RequestAllReadyInList: " + senderId);
                //     _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildAdded -= HandleReceivedFriendRequest; //Todo Why do this?
                //     return;
                // }

                print("Receiver ID : "+ receiverId);
                print("Sender ID : "+ senderId);
                
                // Get the friend request data
                string requestId = args.Snapshot.Key;
                string status = args.Snapshot.Child("status").Value.ToString();

                // Display the friend request to the user and provide options to accept or decline it
                var request = new FriendRequests
                {
                    RequestID = requestId,
                    ReceiverId = receiverId,
                    SenderID = senderId
                };
                
                Debug.Log("HandleReceivedFriendRequest ADD");
                _allReceivedRequests.Add(request); //Todo: this ain't workin it causes double requests
                SoundManager.instance.PlaySound(SoundManager.SoundType.Notification);
                HelperMethods.PlayHeptics();
                ContactsCanvas.UpdateRedMarks?.Invoke();

                if (ContactsCanvas.UpdateRequestView != null)
                    ContactsCanvas.UpdateRequestView?.Invoke();
                
                // Display friend request UI here...
            }
        }
        catch (Exception e)
        {
            //_databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildAdded -= HandleReceivedFriendRequest; //Todo Why do this?
            Debug.Log("Exception From HandleFriendRequest");
        }
    }
    private void HandleReceivedRemovedRequests(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleRemovedRequests");
        try
        {
            if (args.Snapshot is not { Value: { } }) return;
            
            string requestId = args.Snapshot.Key;
            
            FriendRequestManager.Instance.RemoveRecievedRequest(requestId);
          
            //why unsubscribe?
            //_databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).ChildRemoved -= HandleReceivedRemovedRequests; //Todo Why do this?
            //_databaseReference.Child("allFriendRequests").ChildRemoved -= HandleReceivedRemovedRequests;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private void HandleSentFriendRequest(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleFriendRequest");
        try
        {
            if (args.Snapshot != null && args.Snapshot.Value != null)
            {
                string senderId = args.Snapshot.Child("senderId").Value.ToString();
                string receiverId = args.Snapshot.Child("receiverId").Value.ToString();

                //I dont need these because of new Db structure?
                // if (receiverId != _firebaseUser.UserId)
                // {
                //     // _databaseReference.Child("allFriendRequests").ChildAdded -= HandleReceivedFriendRequest;
                //     _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildAdded -= HandleSentFriendRequest;
                //     return;
                // }
                //
                // if (FriendRequestManager.Instance.IsRequestAllReadyInList(senderId) || senderId == _firebaseUser.UserId)
                // {
                //     Debug.Log("RequestAllReadyInList: " + senderId);
                //     //_databaseReference.Child("allFriendRequests").ChildAdded -= HandleReceivedFriendRequest; //Todo Why do this?
                //     _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).ChildAdded -= HandleSentFriendRequest;
                //     return;
                // }

                print("Receiver ID : "+ receiverId);
                print("Sender ID : "+ senderId);
                
                // Get the friend request data
                string requestId = args.Snapshot.Key;
                string status = args.Snapshot.Child("status").Value.ToString();

                // Display the friend request to the user and provide options to accept or decline it
                var request = new FriendRequests
                {
                    RequestID = requestId,
                    ReceiverId = receiverId,
                    SenderID = senderId
                };
                
                Debug.Log("_allSentRequests.Add");
                _allSentRequests.Add(request); //Todo: this ain't workin it causes double requests
                SoundManager.instance.PlaySound(SoundManager.SoundType.Notification);
                HelperMethods.PlayHeptics();
                ContactsCanvas.UpdateRedMarks?.Invoke();

                if (ContactsCanvas.UpdateRequestView != null)
                    ContactsCanvas.UpdateRequestView?.Invoke();
                
                // Display friend request UI here...
            }
        }
        catch (Exception e)
        {
            _databaseReference.Child("allFriendRequests").ChildAdded -= HandleReceivedFriendRequest;
            Debug.Log("Exception From HandleFriendRequest");
        }
    }
    private void HandleSentRemovedRequests(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleSentRemovedRequests");
        try
        {
            if (args.Snapshot is not { Value: { } }) return;
            
            string requestId = args.Snapshot.Key;
            
            FriendRequestManager.Instance.RemoveSentRequest(requestId);
          
            //why unsubscribe?
            //_databaseReference.Child("allFriendRequests").ChildRemoved -= HandleReceivedRemovedRequests;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    private void HandleFriends(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleFriends");
        try
        {
            if (args.Snapshot == null || args.Snapshot.Value == null) return;
            var friendId = args.Snapshot.Key.ToString();

            if (string.IsNullOrEmpty(friendId)) return;
            
            var friend = new FriendModel
            {
                friend = friendId
            };
            
            print("New Friend Added :: "+ friendId);
            _allFriends.Add(friend);
            BackgroundNotificationManager.Instance.SendNotificationUsingFirebaseUserId(friendId, FbManager.instance.thisUserModel.DisplayName , "accepted your friend request!");
            if (ContactsCanvas.UpdateFriendsView != null)
                ContactsCanvas.UpdateFriendsView?.Invoke();
            ContactsCanvas.UpdateRedMarks();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private void HandleRemovedFriends(object sender, ChildChangedEventArgs args)
    {
        try
        {
            if (args.Snapshot == null || args.Snapshot.Value == null) return;
            var friendId = args.Snapshot.Key.ToString();
            FriendsModelManager.Instance.RemoveFriendFromList(friendId);
            
           
            
            _databaseReference.Child("Friends").Child(_firebaseUser.UserId).ChildRemoved -= HandleRemovedFriends;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    #endregion
    
    public IEnumerator SendFriendRequest(string friendId, Action<bool> callback)
    {
        //old
        // string requestId = _databaseReference.Child("allFriendRequests").Push().Key;
        // Dictionary<string, object> requestData = new Dictionary<string, object>();
        // requestData["senderId"] = _firebaseUser.UserId;
        // requestData["receiverId"] = friendId;
        // requestData["status"] = "pending";
        //
        // var request = new FriendRequests()
        // {
        //     ReceiverId = friendId,
        //     RequestID = requestId,
        //     SenderID = _firebaseUser.UserId
        // };
        //
        // // Create a new friend request node
        // var task = _databaseReference.Child("allFriendRequests").Child(requestId).SetValueAsync(requestData);
        //
        // while (task.IsCompleted is false)
        //     yield return new WaitForEndOfFrame();
        //
        // if (task.IsCanceled || task.IsFaulted)
        // {
        //     print(task.Exception.Message);
        //     callback(false);
        // }
        // else
        // {
        //     FriendRequestManager.Instance._allSentRequests.Add(requestId,request);
        //     callback(true);
        // }
        
        //new
        string requestId = _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).Child(friendId).ToString();

        Dictionary<string, object> requestData = new Dictionary<string, object>();
        requestData["senderId"] = _firebaseUser.UserId;
        requestData["receiverId"] = friendId;
        requestData["status"] = "pending";

        var request = new FriendRequests()
        {
            RequestID = requestId,
            ReceiverId = friendId,
            SenderID = _firebaseUser.UserId
        };
        
        // Create a new friend request node
        var task = _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).Child(friendId).SetValueAsync(requestData);
        _databaseReference.Child("FriendsReceive").Child(friendId).Child(_firebaseUser.UserId).SetValueAsync(requestData);

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (task.IsCanceled || task.IsFaulted)
        {
            print(task.Exception.Message);
            callback(false);
        }
        else
        {
            Debug.Log("SendFriendRequest FriendRequestManager ADD:" + requestId);
            FriendRequestManager.Instance._allSentRequests.Add(requestId,request);
            callback(true);
        }
    }
    public IEnumerator AcceptFriendRequest(string requestId, string senderId, Action<bool> callback)
    {
        //old
        // _databaseReference.Child("allFriendRequests").Child(requestId).RemoveValueAsync();
        //
        // // var friend = new FriendModel
        // // {
        // //     friend = senderId
        // // };
        //
        // print("Friend Request Accept Called With  SenderID ::  "+ senderId);
        //
        // var task = _databaseReference.Child("Friends").Child(_firebaseUser.UserId).Child(senderId).SetValueAsync(false);
        // _databaseReference.Child("Friends").Child(senderId).Child(_firebaseUser.UserId).SetValueAsync(false);
        // while (task.IsCompleted is false)
        //     yield return new WaitForEndOfFrame();
        //
        // if (task.IsCanceled || task.IsFaulted)
        // {
        //     print(task.Exception.Message);
        //     callback(false);
        // }
        // else
        // {
        //     // _allFriends.Add(friend);
        //     callback(true);
        // }
        
        //new
        _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId).Child(senderId).RemoveValueAsync();
        _databaseReference.Child("FriendsSent").Child(senderId).Child(_firebaseUser.UserId).RemoveValueAsync();
        
        // var friend = new FriendModel
        // {
        //     friend = senderId
        // };
        
        print("Friend Request Accept Called With  SenderID ::  "+ senderId);
        
        //create friends
        var task = _databaseReference.Child("Friends").Child(_firebaseUser.UserId).Child(senderId).SetValueAsync(false);
        _databaseReference.Child("Friends").Child(senderId).Child(_firebaseUser.UserId).SetValueAsync(false);
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
    public IEnumerator SetBestFriend( string friendID, bool isBestFriend, Action<bool> callback)
    {
        var task = _databaseReference.Child("Friends").Child(_firebaseUser.UserId).Child(friendID).SetValueAsync(isBestFriend);

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (task.IsCanceled || task.IsFaulted)
        {
            print("False Called");
            callback(false);
        }
        else
        {
            print("True Called");
            callback(true);
        }
    }
    public void CancelFriendRequest(string senderId)
    {
        // Delete the friend request node
        _databaseReference.Child("FriendsReceive").Child(senderId).Child(_firebaseUser.UserId).RemoveValueAsync();
        _databaseReference.Child("FriendsSent").Child(_firebaseUser.UserId).Child(senderId).RemoveValueAsync();

    }

    #region Query Functions
    private IEnumerator RetrieveReceivedFriendRequests()
    {
        Debug.Log("RetrieveReceivedFriendRequests");
        // Get the current user's ID
        var userId = _firebaseUser.UserId;
        var friendRequestsRef = _databaseReference.Child("FriendsReceive").Child(_firebaseUser.UserId);
        var task = friendRequestsRef.GetValueAsync();
        
        while (task.IsCompleted is false) yield return new WaitForEndOfFrame();

        if (task.IsCanceled || task.IsFaulted)
        {
            print(task.Exception.Message);
        }
        else
        {
            DataSnapshot snapshot = task.Result;
            foreach (var request in snapshot.Children)
            {
                var senderId = request.Child("senderId").Value.ToString();
                if (FriendRequestManager.Instance.IsRequestAllReadyInList(senderId, false) is false)
                {
                    FriendRequests req = new FriendRequests();
                    req.SenderID = senderId;
                    req.ReceiverId = request.Child("receiverId").Value.ToString();
                    req.RequestID = request.Key;
                    print("Request ID :: "+ req.RequestID);
                    print("Sender ID :: "+ req.SenderID);
                    print("Receiver ID :: "+ req.ReceiverId);
                    Debug.Log("ADDED friend request:" + req.SenderID);
                    _allReceivedRequests.Add(req);
                }
            }
        }
    }
    private IEnumerator RetrieveSentFriendRequests()
    {
        // Get the current user's ID
        string userId = _firebaseUser.UserId;
        DatabaseReference friendRequestsRef = _databaseReference.Child("FriendsSent");
        var task = friendRequestsRef.Child(userId).GetValueAsync();
        
        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (task.IsCanceled || task.IsFaulted)
        {
            print(task.Exception.Message);
        }
        else
        {
            DataSnapshot snapshot = task.Result;
            foreach (var request in snapshot.Children)
            {
                string receiverId = request.Child("receiverId").Value.ToString();
                if (FriendRequestManager.Instance.IsRequestAllReadyInList(receiverId) is false)
                {
                    FriendRequests req = new FriendRequests();
                    req.SenderID = request.Child("senderId").Value.ToString();;
                    req.ReceiverId = receiverId;
                    req.RequestID = request.Key;
                    
                    Debug.Log("RetrieveSentFriendRequests FriendRequestManager ADD:" + req.RequestID);
                    FriendRequestManager.Instance._allSentRequests.Add(req.RequestID,req);
                }
            }
        }
    }
    public void GetSpecificUserData(string userId, Action<UserModel> callBack)
    {
        StartCoroutine(GetSpecificUserDataCoroutine(userId, callBack));
    }
    private IEnumerator GetSpecificUserDataCoroutine(string userId, Action<UserModel> callBack)
    {
        // Get a reference to the "users" node in the database
        DatabaseReference usersRef = _databaseReference.Child("users");

        // Attach a listener to the "users" node
        var task = usersRef.Child(userId).GetValueAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;
            string email = snapshot.Child("email").Value.ToString();
            string frindCount = snapshot.Child("friendCount").Value.ToString();
            string displayName = snapshot.Child("name").Value.ToString();
            string username = snapshot.Child("username").Value.ToString();
            string phoneNumber = snapshot.Child("phone").Value.ToString();
            string photoURL = snapshot.Child("userPhotoUrl").Value.ToString();
            UserModel user = new UserModel(_firebaseUser.UserId, email, int.Parse(frindCount), displayName, username,
                phoneNumber, photoURL);
            callBack(user);
        }

        if (task.IsFaulted)
        {
            Debug.LogError(task.Exception);
        }
    }
    #endregion

    #region Friendship
    public IEnumerator RetrieveFriends()
    {
        // Get the current user's ID
        string userId = _firebaseUser.UserId;
        DatabaseReference friendRequestsRef = _databaseReference.Child("Friends");

        var task = friendRequestsRef.Child(userId).GetValueAsync();

        while (task.IsCompleted is false)
            yield return new WaitForEndOfFrame();

        if (task.IsCanceled || task.IsFaulted)
        {
            print(task.Exception.Message);
        }
        else
        {
            DataSnapshot snapshot = task.Result; 
            foreach (var friend in snapshot.Children)
            {
                var friendID = friend.Key.ToString();
                // print("IsBest Friends :: " + friend.Value.ToString());
                var isBestFriend = false;
    
                if (friend.Value.ToString().Equals("*") is false)
                    isBestFriend= System.Convert.ToBoolean(friend.Value.ToString());
                
                if (FriendsModelManager.Instance.IsAlreadyFriend(friendID) is false)
                {
                    var fri = new FriendModel
                    {
                        friend = friendID,
                        isBestFriend = isBestFriend
                    };
                    _allFriends.Add(fri);
                }
            }
        }

        
    }
    public void RemoveFriends(string friendshipId)
    {
        // Delete the friend request node
        _databaseReference.Child("Friends").Child(_firebaseUser.UserId).Child(friendshipId).RemoveValueAsync();
        _databaseReference.Child("Friends").Child(friendshipId).Child(_firebaseUser.UserId).RemoveValueAsync();
    }
    #endregion
    
    private void HandleFriendsManagerClearData()
    {
        _allReceivedRequests.Clear();
        _allSentRequests.Clear();
        _allFriends.Clear();
    }
}