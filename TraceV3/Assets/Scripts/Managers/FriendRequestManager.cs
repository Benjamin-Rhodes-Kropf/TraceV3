using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CanvasManagers;

public class FriendRequestManager
{
    private static FriendRequestManager instance = null;
    public Dictionary<string, FriendRequests> _allSentRequests;


    public static FriendRequestManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FriendRequestManager();
            }

            return instance;
        }
    }
    private FriendRequestManager()
    { 
        _allSentRequests = new Dictionary<string, FriendRequests>();
    }


    private FriendRequests GetRequestBySenderID(string senderId, bool isReceivedRequest = true)
    {
        RemoveDuplicates();
        Debug.Log("GetRequestBySenderID: isReceivedRequest:" + isReceivedRequest.ToString());
        if (isReceivedRequest)
        {
            FriendRequests friendRequest =
                    (from request in FbManager.instance._allReceivedRequests
                        where request.SenderID.Equals(senderId)
                        select request).FirstOrDefault();

                return friendRequest;
        }
        else
        {
                Debug.Log("_allSentRequests.Keys");
                foreach (var id in _allSentRequests.Keys)
                {
                    Debug.Log("GetRequestBySenderID:" + _allSentRequests[id].ReceiverId + "compare to:" + id);
                }
                foreach (var id in _allSentRequests.Keys)
                {
                    Debug.Log("GetRequestBySenderID:" + _allSentRequests[id].ReceiverId + "compare to:" + id);
                    var isExist = _allSentRequests[id].ReceiverId == senderId;
                    if (isExist)
                    {
                        Debug.Log("return" + isExist);
                        return _allSentRequests[id];
                    }
                }
                return null;
        }
    }

    private FriendRequests GetReceivedRequestByRequestID(string requestId)
    {
        FriendRequests friendRequest =
            (from request in FbManager.instance._allReceivedRequests
                where request.RequestID.Equals(requestId)
                select request).FirstOrDefault();
        return friendRequest;
        
    }
    private FriendRequests GetSentRequestByRequestID(string requestId)
    {
        FriendRequests friendRequest =
            (from request in FbManager.instance._allSentRequests
                where request.RequestID.Equals(requestId)
                select request).FirstOrDefault();
        return friendRequest;
    }

    public bool IsRequestAllReadyInList(string senderId, bool isReceivedRequest = true)
    {
        try
        {
            var request = GetRequestBySenderID(senderId, isReceivedRequest);
            if (request == null)
                return false;

            return true;
        }
        catch(Exception e)
        {
            Debug.Log("Exception Caught in FriendsRequestManager.IsRequestAllReadyInList "+ e.Message);
            return false;
        }        
    }
    
    public void RemoveRequestFromList(string senderId, bool isReceivedRequest  = true)
    {
        var request = GetRequestBySenderID(senderId, isReceivedRequest);

        if (isReceivedRequest)
            FbManager.instance._allReceivedRequests.Remove(request);
        else
        {
            Debug.Log("_allSentRequests.Remove(senderId):" + senderId);
            _allSentRequests.Remove(senderId);
            foreach (var thing in _allSentRequests)
            {
                Debug.Log("Key" + thing.Key);   
            }
        }
    }
    
    public string GetRequestID(string senderId, bool isReceivedRequest = true)
    {
        string requestID = "";
        var request = GetRequestBySenderID(senderId,  isReceivedRequest);
        
        if (request != null)
            requestID = request.RequestID;
        
        return requestID;
    }

    public void RemoveRecievedRequest(string requestId)
    {
        var receivedRequest = GetReceivedRequestByRequestID(requestId);
        if (receivedRequest != null)
            FbManager.instance._allReceivedRequests.Remove(receivedRequest);

        //update view
        if (ContactsCanvas.UpdateRequestView != null)
            ContactsCanvas.UpdateRequestView?.Invoke();
    }
    
    public void RemoveSentRequest(string requestId)
    {
        var sentRequest = GetSentRequestByRequestID(requestId);
        if (sentRequest != null)
            FbManager.instance._allSentRequests.Remove(sentRequest);
        
        //update view
        if (ContactsCanvas.UpdateRequestView != null)
            ContactsCanvas.UpdateRequestView?.Invoke();
    }

    public void RemoveRequestByUserID(string userId)
    {
        var receivedRequest = GetRequestBySenderID(userId);
        if (receivedRequest != null)
            FbManager.instance._allReceivedRequests.Remove(receivedRequest);
        else
        {
            _allSentRequests.Remove(receivedRequest.RequestID);
        }
        if (ContactsCanvas.UpdateRequestView != null)
            ContactsCanvas.UpdateRequestView?.Invoke();
    }
    

    private void RemoveDuplicates()
    {
        List<FriendRequests> distinctList1 = FbManager.instance._allReceivedRequests.Distinct().ToList();
        if (distinctList1.Count() != FbManager.instance._allReceivedRequests.Count())
        {
            FriendRequests duplicateItem = FbManager.instance._allReceivedRequests.Except(distinctList1).FirstOrDefault();
            FbManager.instance._allReceivedRequests.RemoveAll(item => item.Equals(duplicateItem));
        }
    }
    
}
