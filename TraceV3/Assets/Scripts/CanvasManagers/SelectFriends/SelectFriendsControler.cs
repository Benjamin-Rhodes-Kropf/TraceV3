using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SelectFriendsControler : MonoBehaviour
{
    private SelectFriendsCanvas _view;
    private List<SendToFriendView> _allFriendsView;
    public Transform bestFriendText;
    public Transform allFriendsText;
    public void Init(SelectFriendsCanvas view)
    {
        this._view = view;
        _allFriendsView = new List<SendToFriendView>();
        this._view._searchBar.onValueChanged.AddListener(OnSearchBarValueChange);
        LoadAllFriends();
    }
    public void UnInitialize()
    {
        _allFriendsView.Clear();
        this._view._searchBar.onValueChanged.RemoveAllListeners();
        Debug.Log("SelectFriendsControler: UnInitialize()");
    }
    
    private void LoadAllFriends()
    {
        Debug.Log("LoadAllFriends");
        int numOfBestFriends = 0;
        var users = UserDataManager.Instance.GetAllFriends();
        foreach (var user in users)
        {
            bool isBestFriend = FriendsModelManager.Instance.IsBestFriend(user.userId);
            if (isBestFriend) numOfBestFriends++;
            UpdateFriendViewInfo(user, isBestFriend);
        }
        if (users.Count < 1)
        {
            Debug.Log("No Friends Yet");
            _view.DisplayNoFriendsText();
        }
        _view.CheckAndChangeVisualsForNoFriends(numOfBestFriends); 
    }
    
    private void UpdateFriendViewInfo(UserModel user, bool isBestFriend)
    {
        Debug.Log("UpdateFriendViewInfo");
        int bestFriendsIndex = bestFriendText.GetSiblingIndex();
        int allFriendsIndex = allFriendsText.GetSiblingIndex();
        SendToFriendView view = GameObject.Instantiate(_view.friendViewPrefab, _view._displayFrindsParent);
        if (isBestFriend)
        {
            view.GetComponent<Transform>().SetSiblingIndex(bestFriendsIndex+1);
        }
        else
        {
            view.GetComponent<Transform>().SetSiblingIndex(allFriendsIndex+1);
        }
        view.UpdateFrindData(isBestFriend, user,false);
        _allFriendsView.Add(view);
        _view._friendsList.Add(view);
    }

    private void ClearFriendsView()
    {
        if(_allFriendsView.Count <= 0)
            return;
        foreach (var view in _allFriendsView)
            GameObject.Destroy(view.gameObject);
        _allFriendsView.Clear();
    }
    
    public void UpdateFriendsLayout()
    {
        if (_view._friendsScroll.activeInHierarchy)
            LoadAllFriends();
    }
    
    public void UpdateFriendsSendTo()
    {
        SendTraceManager.instance.usersToSendTo.Clear();
        Debug.Log("UpdateFriendsSendTo()");
        foreach (var view in _allFriendsView)
        {
            if (view.sendToThisFriend)
            {
                Debug.Log("UpdateFriendsSendTo:" + view.friendUID);
                SendTraceManager.instance.usersToSendTo.Add(view.friendUID);
            }
        }
    }



    private void OnSearchBarValueChange(string inputText)
    {
        ClearFriendsView();
        if (inputText.Length <= 1)
        {
            LoadAllFriends();
            return;
        }
        
        Debug.Log("Search Friends");
        int numOfBestFriends = 0;
        var users = UserDataManager.Instance.GetFriendsByName(inputText);
        foreach (var user in users)
        {
            bool isBestFriend = FriendsModelManager.Instance.IsBestFriend(user.userId);
            UpdateFriendViewInfo(user, isBestFriend);
        }
    }
}
