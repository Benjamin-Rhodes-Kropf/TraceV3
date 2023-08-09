using System;
using System.Collections;
using System.Collections.Generic;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UserView : MonoBehaviour
{
    
    public enum FriendButtonType
    {
        Add,
        Remove,
        Cancel,
        Accept,
        Follow
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

    private string _uid = "";
    private bool isBestFriend = false;
    private bool isFriend = false;
    private bool isSuperUser = false;

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
    
    public void UpdateFriendData(UserModel user, FriendModel.RelationShipType relationShipType)
    {
        //todo: make switch case for relationship type
        switch (relationShipType)
        {
            case FriendModel.RelationShipType.Friend:
                this.isFriend = true;
                break;
            case FriendModel.RelationShipType.BestFriend:
                this.isFriend = true;
                this.isBestFriend = true;
                break;
            case FriendModel.RelationShipType.Following:
                this.isSuperUser = true;
                break;
            case FriendModel.RelationShipType.User:
                //nothing here because it is only for friends or people the user follows
                break;
            case FriendModel.RelationShipType.SuperUser:
                this.isSuperUser = true;
                //nothing here because it is only for friends or people the user follows
                break;
        }

        _userCoroutine = user._downloadPCoroutine;
        _userName.text = user.username;
        _nickName.text = user.name;
        _uid = user.ID;
        FriendButtonType buttonType = FriendButtonType.Add;
        buttonType = isFriend ? FriendButtonType.Remove : FriendButtonType.Add;
        var buttonData = GetButtonData(buttonType);
        _buttonBackground.color = _colors[buttonData.colorIndex];
        _buttonText.text = buttonData.buttonText;
        _bestFriend.sprite = isBestFriend ? _heartSprite[0] : _heartSprite[1];
        _bestFriendButton.gameObject.SetActive(isFriend);
        _bestFriendButton.onClick.RemoveAllListeners();
        _addRemoveButton.onClick.RemoveAllListeners();
        _addRemoveButton.onClick.AddListener(isFriend ? RemoveFriends :  SendFriendRequest);
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