using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CanvasManagers;
using UnityEngine;

public class FriendsModelManager 
{
    private static FriendsModelManager instance = null;

    public static FriendsModelManager Instance
    {
        get
        {
            if (instance == null)
                instance = new FriendsModelManager();

            return instance;
        }
    }
    private FriendsModelManager()
    {
        
    }

    public static FriendModel GetFriendModelByOtherFriendID(string otherFriend)
    {
        var friend = (from fri in FbManager.instance._allFriends
            where fri.friendID == otherFriend
            select fri).FirstOrDefault();

        if (friend == null)
        {
            return new FriendModel();
        }
        return friend;
    }

    public bool IsAlreadyFriend(string otherFriend)
    {
        try
        {
            Debug.Log("IsAlreadyFriend");
            GetFriendModelByOtherFriendID(otherFriend);
            return true;
        }
        catch
        {
            return false;
        }        
    }

    public Relationship GetRelationship(string id)
    {
        return GetFriendModelByOtherFriendID(id).relationship;
    }

    public void SetBestFriend(string id, bool isBestFriend)
    {
        //Todo : Update Data in Local List
        for (var i = 0; i < FbManager.instance._allFriends.Count; i++)
        {
            if (FbManager.instance._allFriends[i].friendID.Equals(id))
            {
                if (isBestFriend)
                {
                    FbManager.instance._allFriends[i].relationship = Relationship.BestFriend;
                    break;
                }
                else
                {
                    FbManager.instance._allFriends[i].relationship = Relationship.Friend;
                    break;
                }
                
            }
        }
    }

    public void RemoveFriendFromList(string otherFriend)
    {
        Debug.Log("RemoveFriendFromList");
        var friend = GetFriendModelByOtherFriendID(otherFriend);
        FbManager.instance._allFriends.Remove(friend);
        if (ContactsCanvas.UpdateFriendsView != null)
            ContactsCanvas.UpdateFriendsView?.Invoke();
    }
    
    private static void RemoveDuplicates()
    {
        List<FriendModel> distinctList1 = FbManager.instance._allFriends.Distinct().ToList();
        if (distinctList1.Count() != FbManager.instance._allFriends.Count())
        {
            FriendModel duplicateItem = FbManager.instance._allFriends.Except(distinctList1).FirstOrDefault();
            FbManager.instance._allFriends.RemoveAll(item => item.Equals(duplicateItem));
        }
    }
}
