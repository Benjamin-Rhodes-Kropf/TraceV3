using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserDataManager
{
    private static UserDataManager instance = null;

    public static UserDataManager Instance
    {
        get
        {
            if (instance == null)
                instance = new UserDataManager();

            return instance;
        }
    }
    private UserDataManager()
    {
        
    }
    public List<UserModel> GetUsersByLetters(string name)
    {
        List<UserModel> selectedUsers = new List<UserModel>();

        //new methode for searching users
        selectedUsers = AlgoliaManager.instance.SearchUser(name);
        // Query Syntax
        // IEnumerable<UserModel> _userSearchQuery =
        //     from user in FbManager.instance.AllUsers
        //     where user.Username.Contains(name)
        //     orderby user.Username
        //     select user;
        //
        // selectedUsers.AddRange(_userSearchQuery);

        return selectedUsers;
    }
    public bool IsUsernameAvailable(string userName)
    {
        var users = GetUsersByLetters(userName);
        return users.Count < 1;
    }
    
    public List<UserModel> GetReceivedFriendRequested()
    {
        Debug.Log("GetReceivedFriendRequested");
        List<UserModel> users = new List<UserModel>();
        foreach (var request in FbManager.instance._allReceivedRequests)
        {
            Debug.Log("GetFriendRequested from:" + request.SenderID);
                foreach (var user in FbManager.instance.AllUsers)
                {
                    if (string.Equals(user.userID, request.SenderID))
                    {
                        Debug.Log("ADDED USER:" + user.userID);
                        users.Add(user);
                    };
                }
        }
        return users;
    }
    public List<UserModel> GetSentFriendRequests()
    {
        Debug.Log("GetSentFriendRequests");
        
        List<UserModel> users = new List<UserModel>();
        foreach (var request in FbManager.instance._allSentRequests)
        {
            Debug.Log("GetSentFriendRequested from Key:" + request.ReceiverId);
            var _userSearchQuery =
                from user in FbManager.instance.AllUsers
                where string.Equals(user.userID, request.ReceiverId, StringComparison.Ordinal)
                select user;
            users.AddRange(_userSearchQuery.ToArray());
        }
        return users;
    }
    
    public List<UserModel> GetRequestsByName(string name, bool isReceived =  true)
    {
        var users = isReceived ? GetReceivedFriendRequested() : GetSentFriendRequests();
        List<UserModel> selectedUsers = new List<UserModel>();
        if (string.IsNullOrEmpty(name) is false && users.Count > 0)
        {
            // Query Syntax
            IEnumerable<UserModel> _userSearchQuery =
                from user in users
                where user.username.Contains(name)
                orderby user.username
                select user;
        
            selectedUsers.AddRange(_userSearchQuery);
        }
        return selectedUsers;
    }
    public List<UserModel> GetAllFriends()
    {
        List<UserModel> users = new List<UserModel>();
        foreach (var friendModel in FbManager.instance._allFriends)
        {
            var _userSearchQuery =
                from user in FbManager.instance.AllUsers
                where string.Equals(user.userID, friendModel.friendID, StringComparison.Ordinal)
                select user;
                
            users.AddRange(_userSearchQuery.ToArray());
        }
        return users;
    }
    public List<UserModel> GetFriendsByName(string name)
    {
        var users = GetAllFriends();
            List<UserModel> selectedUsers = new List<UserModel>();
        if (string.IsNullOrEmpty(name) is false && users.Count > 0)
        {
            // Query Syntax
            IEnumerable<UserModel> _userSearchQuery =
                from user in users
                where user.username.ToLower().Contains(name)
                orderby user.username
                select user;
        
            selectedUsers.AddRange(_userSearchQuery);
        }
        return selectedUsers;
    }
    public void GetAllUsersBySearch(string name, out List<UserModel> friends, out List<UserModel> requests, out List<UserModel> requestsSent, out List<UserModel> others)
    {
        friends = new List<UserModel>();
        requests = new List<UserModel>();
        requestsSent = new List<UserModel>();
        others = new List<UserModel>();

        friends = GetFriendsByName(name);
        requests = GetRequestsByName(name);
        requestsSent = GetRequestsByName(name, false);
        others = GetUsersByLetters(name);
        
        Debug.Log("Search Contains:");
        Debug.Log("friends:" + friends.Count);
        Debug.Log("requests:" + requests.Count);
        Debug.Log("requests sent:" + requestsSent.Count);
        Debug.Log("others:" + others.Count);
        foreach (var user in others)
        {
            Debug.Log("other:" + user.name);
        }
    }
}