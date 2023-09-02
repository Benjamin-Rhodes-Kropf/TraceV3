using System;
using System.Collections;
using System.Collections.Generic;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class FriendView : MonoBehaviour
{
    public enum FriendButtonType
    {
        Add,
        Remove, 
        UnFollow,
        Cancel,
        Follow,
        Accept
    }
    
    [SerializeField] private RawImage _profilePic;
    [SerializeField] private TMP_Text _nickName;
    [SerializeField] private TMP_Text _userName;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] public Button _addRemoveButton;
    [SerializeField] private Image _buttonBackground;
    [SerializeField] private Image _bestFriend;
    [SerializeField] private Button _bestFriendButton;
    [SerializeField] private Color[] _colors;
    [SerializeField] private Sprite[] _heartSprite;

    [SerializeField]private string _uid = "";
    [SerializeField]private bool isBestFriend = false;
    [SerializeField]private bool isFriend = false;
    
    public string friendUID {
        get
        {
            return _uid;
        }
    }

    private Coroutine _userCoroutine;
    
    public void OnDestroy()
    {
        if (_userCoroutine != null)
            StopCoroutine(_userCoroutine);
        // Release object references
        _profilePic.texture = null;
        _nickName = null;
        _userName = null;
        _buttonText = null;
        _addRemoveButton = null;
        _buttonBackground = null;
        _bestFriend = null;
        _bestFriendButton = null;
        _colors = null;
        _heartSprite = null;
    }

    public void UpdateFriendData(UserModel user, Relationship relationship)
    {
        //clear old listeners
        _bestFriendButton.onClick.RemoveAllListeners();
        _addRemoveButton.onClick.RemoveAllListeners();
        
        FriendButtonType buttonType;
        switch(relationship)
        {
            case Relationship.Friend:
                isFriend = true;
                isBestFriend = false;
                buttonType = FriendButtonType.Remove;
                _addRemoveButton.onClick.AddListener(RemoveFriends);
                break;
            case Relationship.BestFriend:
                isFriend = true;
                isBestFriend = true;
                buttonType = FriendButtonType.Remove;
                _addRemoveButton.onClick.AddListener(RemoveFriends);
                break;
            case Relationship.Following:
                isFriend = false;
                isBestFriend = false;
                buttonType = FriendButtonType.UnFollow;
                _addRemoveButton.onClick.AddListener(UnFollow);
                break;
            case Relationship.SuperUser:
                isBestFriend = false;
                isFriend = false;
                buttonType = FriendButtonType.Follow;
                _addRemoveButton.onClick.AddListener(FollowSuperUser);

                break;
            default:
                buttonType = FriendButtonType.Add;
                _addRemoveButton.onClick.AddListener(SendFriendRequest);
                break;
        }

        //button text display
        _userCoroutine = user._downloadPCoroutine;
        _userName.text = user.username;
        _nickName.text = user.name;
        _uid = user.ID;
        
        //button style display
        var buttonData = GetButtonData(buttonType);
        _buttonBackground.color = _colors[buttonData.colorIndex];
        _buttonText.text = buttonData.buttonText;
        _bestFriend.sprite = isBestFriend ? _heartSprite[0] : _heartSprite[1];
        _bestFriendButton.gameObject.SetActive(isFriend);
        
        //when clicked switch state 
        _bestFriendButton.onClick.AddListener(OnBestFriendButtonClick);
 
        
        var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("friends", _uid);
        if (persistentData.updateImage)
        {
            user.PPTexture((sprite =>
            {
                try
                {
                    _profilePic.texture = sprite;
                    StartCoroutine(SaveToPersistentStorage((Texture2D)sprite));
                }
                catch (Exception e)
                {
                    print(e.Message);
                }
            }));
        }
        else
        {
            _profilePic.texture = persistentData.texture;
        }
    }


    // public void UpdateFriendData(UserModel user, bool isFriendAdd = false, bool isBestOne = false)
    // {
    //     isFriend = isFriendAdd;
    //     isBestFriend = isBestOne;
    //     _userCoroutine = user._downloadPCoroutine;
    //     _userName.text = user.username;
    //     _nickName.text = user.name;
    //     _uid = user.ID;
    //     
    //     FriendButtonType buttonType = FriendButtonType.Add;
    //     buttonType = isFriendAdd ? FriendButtonType.Remove : FriendButtonType.Add;
    //     var buttonData = GetButtonData(buttonType);
    //     _buttonBackground.color = _colors[buttonData.colorIndex];
    //     _buttonText.text = buttonData.buttonText;
    //     _bestFriend.sprite = isBestFriend ? _heartSprite[0] : _heartSprite[1];
    //     _bestFriendButton.gameObject.SetActive(isFriendAdd);
    //     _bestFriendButton.onClick.RemoveAllListeners();
    //     _addRemoveButton.onClick.RemoveAllListeners();
    //     
    //     _addRemoveButton.onClick.AddListener(isFriendAdd ? RemoveFriends :  SendFriendRequest);
    //     _bestFriendButton.onClick.AddListener(OnBestFriendButtonClick);
    //
    //     var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("friends", _uid);
    //     if (persistentData.updateImage)
    //     {
    //         user.PPTexture((sprite =>
    //         {
    //             try
    //             {
    //                 _profilePic.texture = sprite;
    //                 StartCoroutine(SaveToPersistentStorage((Texture2D)sprite));
    //             }
    //             catch (Exception e)
    //             {
    //                 print(e.Message);
    //             }
    //         }));
    //     }
    //     else
    //     {
    //         _profilePic.texture = persistentData.texture;
    //     }
    //     
    //     
    // }
    
    

    IEnumerator SaveToPersistentStorage(Texture2D texture2D)
    {
        texture2D.Apply();
        yield return new WaitForEndOfFrame();
        PersistentStorageHandler.s_Instance.SaveTextureToPersistentStorage(texture2D,"friends",_uid);
    }
    

    private (string buttonText, int colorIndex) GetButtonData(FriendButtonType buttonType)
    {
        switch (buttonType)
        {
            case FriendButtonType.Add:
                return ("Add", 0);
            case FriendButtonType.Remove:
                return ("Remove", 1);
            case FriendButtonType.Cancel:
                return ("Cancel", 2);
            case FriendButtonType.Follow:
                return ("Follow", 3);
            case FriendButtonType.UnFollow:
                return ("Remove", 1);
            default:
                return ("Add", 0);
        }
    }

    public void UpdateRequestStatus(bool RequestSent)
    {
        if (RequestSent)
        {
            FriendButtonType buttonType = FriendButtonType.Cancel;

            var buttonData = GetButtonData(buttonType);
            _buttonBackground.color = _colors[buttonData.colorIndex];
            _buttonText.text = buttonData.buttonText;
        }
    }

    private void SendFriendRequest()
    {
        _addRemoveButton.interactable = false;
        string friendUID = this.friendUID;
        
        if (friendUID == "")
            return;
        
        if (FriendRequestManager.Instance.IsRequestAllReadyInList(friendUID,false))
            return;
        
        StartCoroutine(FbManager.instance.SendFriendRequest(friendUID, async (IsSuccessful) => {
            if (!IsSuccessful)
            {
                Debug.LogError("Friend request failed at : "+ friendUID);
                return;
            }
            
            UpdateRequestStatus(true);
            _addRemoveButton.interactable = true;
            Debug.Log("friend requested at:" + friendUID);
            Debug.Log("from:" + FbManager.instance.thisUserModel.name);
            NotificationManager.Instance.SendNotificationUsingFirebaseUserId(friendUID, FbManager.instance.thisUserModel.name , "sent you friend request");
        }));
        
        FbManager.instance.AnalyticsOnSendFriendRequest(friendUID);
    }
    
    private void FollowSuperUser()
    {
        Debug.LogWarning("Follow Super User");
        _addRemoveButton.interactable = false;
        string superUserID = this.friendUID;
        
        if (superUserID == "")
            return;

        StartCoroutine(FbManager.instance.FollowSuperUser(superUserID, async (IsSuccessful) => {
            if (!IsSuccessful)
            {
                Debug.LogError("Friend request failed at : "+ superUserID);
                return;
            }
            
            UpdateRequestStatus(true);
            _addRemoveButton.interactable = true;
            Debug.Log("follow super at:" + superUserID);
            Debug.Log("from:" + FbManager.instance.thisUserModel.name);
        }));
        
        FbManager.instance.AnalyticsOnSendFriendRequest(superUserID);
    }
    
    private void UnFollow()
    {
        Debug.Log("Unfollow");
        FbManager.instance.RemoveSuperUser(_uid);
        gameObject.SetActive(false);
        //FbManager.instance.AnalyticsOnRemoveFriend(friendUID);
    }

    private void RemoveFriends()
    {
        Debug.Log("RemoveFriends");
        FriendRequestManager.Instance.RemoveRequestFromList(_uid, false); //new
        FbManager.instance.RemoveFriends(_uid);
        gameObject.SetActive(false);
        FbManager.instance.AnalyticsOnRemoveFriend(friendUID);
    }


    private void OnBestFriendButtonClick()
    {
        if (isFriend is false)
            return;
        _bestFriendButton.interactable = false;
        StartCoroutine(FbManager.instance.SetBestFriend(friendUID, !isBestFriend, (isSuccess) =>
        {
            if (isSuccess)
            {
                isBestFriend = !isBestFriend;
                _bestFriend.sprite = isBestFriend ? _heartSprite[0] : _heartSprite[1];
                FriendsModelManager.Instance.SetBestFriend(friendUID, isBestFriend);
                TraceManager.instance.UpdateMap(new Vector2(0,0));
                FbManager.instance.AnalyticsOnHeartFriend(friendUID);
            }

            _bestFriendButton.interactable = true;
        }));
    }
   
}