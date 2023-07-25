using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class RequestView : MonoBehaviour
{
    public TMP_Text _displayName;
    public TMP_Text _userName;
    public RawImage _profilePicture;
    public Button _acceptButton;
    public Button _removeButton;
    public TMP_Text _buttonText;
    public Image _buttonImage;

    private string requestId = "";
    private string senderId = "";
    
    private Coroutine _userCoroutine;

    public void OnDestroy()
    {
        if (_userCoroutine != null)
            StopCoroutine(_userCoroutine);
    
        // Release object references
        _displayName = null;
        _userName = null;
        _profilePicture.texture = null;
        Destroy( _profilePicture.texture); //destroy
        Destroy( _profilePicture); //destroy
        _acceptButton = null;
        _removeButton = null;
        _buttonText = null;
        _buttonImage = null;
    }
    
    public void UpdateRequestView(UserModel user, bool isReceivedRequest  = true)
    {
        requestId = FriendRequestManager.Instance.GetRequestID(user.userID, isReceivedRequest);
        senderId = user.userID;

        var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("requests",senderId);
        if (persistentData.updateImage)
        {
            user.PPTexture((texture =>
            {
                try
                {
                    _profilePicture.texture = texture;
                    StartCoroutine(SaveToPersistentStorage((Texture2D)texture));
                }
                catch (Exception e)
                {
                
                }
            }));
        }
        else
        {
            _profilePicture.texture = persistentData.texture;
        }
       
        
        _userName.text = user.username;
        _displayName.text = user.name;
        _userCoroutine = user._downloadPCoroutine;
        _acceptButton.onClick.RemoveAllListeners();
        if (isReceivedRequest is false)
        {
            _acceptButton.onClick.AddListener(OnCancelClick);
            _buttonText.text = "Sent";
            _buttonImage.color = Color.red;
        }
        else
        {
            _buttonText.text = "Accept";
            _acceptButton.onClick.AddListener(OnClickAccept);
        }
        
        _removeButton.onClick.RemoveAllListeners();
        _removeButton.onClick.AddListener(OnClickRemove);
    }
    
    
    IEnumerator SaveToPersistentStorage(Texture2D texture2D)
    {
        texture2D.Apply();
        yield return new WaitForEndOfFrame();
        PersistentStorageHandler.s_Instance.SaveTextureToPersistentStorage(texture2D,"requests",senderId);
    }
    
    public void OnClickAccept()
    {
        print("Accept Function Called");
        StartCoroutine(
        FbManager.instance.AcceptFriendRequest(requestId,senderId,(isUpdated =>
        {
            if (isUpdated)
            {
                Debug.Log("requestID:" + requestId);
                Debug.Log("snederID:" + senderId);
                FriendRequestManager.Instance.RemoveRequestFromList(senderId);
                ContactsCanvas.UpdateRedMarks?.Invoke();
                this.gameObject.SetActive(false);
            }
        })));
        NotificationManager.Instance.SendNotificationUsingFirebaseUserId(senderId, FbManager.instance.thisUserModel.name , "accepted your friend request!");
    }

    //  TODO: i.  Remove Request From Local List
    //  TODO: ii. Remove Request From Firebase
    public void OnClickRemove()
    {
        FbManager.instance.CancelFriendRequest(senderId);        
        FriendRequestManager.Instance.RemoveRequestFromList(requestId, _buttonText.text != "Sent");
        ContactsCanvas.UpdateRedMarks?.Invoke();
        gameObject.SetActive(false);
    }


    public void OnCancelClick()
    {
        
    }
    
}
