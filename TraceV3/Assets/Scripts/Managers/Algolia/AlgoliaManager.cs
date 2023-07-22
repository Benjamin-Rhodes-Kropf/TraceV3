using System.Collections;
using System.Collections.Generic;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using UnityEngine;

public class AlgoliaManager : MonoBehaviour
{
    private SearchClient client;
    private SearchIndex index;
    public static AlgoliaManager instance;
    
    
    // Start is called before the first frame update
    void Start()
    {
        InitAlgolia();
    }

    private void InitAlgolia()
    {
        instance = this;
        client = new SearchClient("Q324B39BS7", "f5c161e91e27b4bd94cdf05a5bd53ef4");
        index = client.InitIndex("users");
    }
    

    public List<UserModel> SearchUser(string parameter)
    {
        var result  =  index.Search<UserModel>(new Query(parameter));
        
        // //filter current user out of search request
        List<UserModel> filteredResult = new List<UserModel>();
        
        //check if user exists in other lists
        int i = 0;
        foreach (var userID in result.Hits)
        {
            var objID = userID.objectID;

            //check if user exists locally in other capacity
             if (isUserThisUser(objID))
             {
                 i++;
                 continue;
             }

             if (isUserFriend(objID))
             {
                 i++;
                 continue;
             }

             if (isUserRequestSent(objID))
             {
                 i++;
                 continue;
             }

             if (isUserRequestRecived(objID))
             {
                 i++;
                 continue;
             }

             filteredResult.Add( result.Hits[i]);
             i++;
        }
        
        return filteredResult;
    }
    
    private bool isUserThisUser(string objID)
    {
        if (objID == FbManager.instance.thisUserModel.userID)
        {
            return true;
        }
        return false;
    }
    private bool isUserFriend(string objID)
    {
        foreach (var friend in FbManager.instance._allFriends)
        {
            if (objID == friend.friendID)
            {
                return true;
            }
        }
        return false;
    }
    private bool isUserRequestSent(string objID)
    {
        foreach (var request in FbManager.instance._allSentRequests)
        {
            if (objID == request.ReceiverId)
            {
                return true;
            }
        }
        return false;
    }
    private bool isUserRequestRecived(string objID)
    {
        foreach (var request in FbManager.instance._allReceivedRequests)
        {
            if (objID == request.SenderID)
            {
                return true;
            }
        }
        return false;
    }
    
    
}
//
// public class TraceData
// {
//     public string objectID { get; set; }
//     public string name{ get; set; }
//     public int score{ get; set; }
//     public int friendCount{ get; set; }
//     public string phone{ get; set; }
//     public string phoneNumber { get; set; }
//     public bool isLogedIn{ get; set; }
//     public bool isOnline{ get; set; }
//     public string email{ get; set; }
//     public string username{ get; set; }
//     public string path{ get; set; }
//     public string photoUrl { get; set; }
// }
