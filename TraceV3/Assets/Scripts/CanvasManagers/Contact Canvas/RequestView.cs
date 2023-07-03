using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequestView : MonoBehaviour
{
    public TMP_Text _displayName;
    public TMP_Text _userName;
    public Image _profilePicture;
    public Button _acceptButton;
    public Button _removeButton;
    public TMP_Text _buttonText;
    public Image _buttonImage;

    private string requestId = "";
    private string senderId = "";

    public void UpdateRequestView(UserModel user, bool isReceivedRequest  = true)
    {
        requestId = FriendRequestManager.Instance.GetRequestID(user.userId, isReceivedRequest);
        senderId = user.userId;
        user.ProfilePicture((sprite =>
        {
            try
            {
                _profilePicture.sprite = sprite;
            }
            catch (Exception e)
            {
                
            }
        }));
        _userName.text = user.Username;
        _displayName.text = user.DisplayName;

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
