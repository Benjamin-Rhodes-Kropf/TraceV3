using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

[Serializable]
public class SendToFriendView : MonoBehaviour
{
    public enum FriendButtonType
    {
        Add,
        Remove,
        Cancel,
        Accept
    }
    [SerializeField] private RawImage _profilePic;
    [SerializeField] private TMP_Text _nickName;
    public Toggle friendViewToggle;
    public bool sendToThisFriend = false;
    private string _uid = "";
    public bool _isBestFriend = false;
    public string friendUID {
        get
        {
            return _uid;
        }
    }
    public void UpdateFrindData(bool isBestFriend, UserModel user = null, bool isFriendAdd = false)
    {
        _isBestFriend = isBestFriend;
        Debug.Log("UpdateFriendData");
        if (user != null)
        {
            _nickName.text = user.DisplayName;
            _uid = user.userId;
            
            user.ProfilePicture((sprite =>
            {
                try
                {
                    _profilePic.texture = sprite.texture;
                }
                catch (Exception e)
                {
                    print(e.Message);
                }
            }));
        }
    }

    

    public void TogglePressed()
    {
        sendToThisFriend = friendViewToggle.isOn;
    }

    public void SetToggleState(bool setToggleState)
    {
        Debug.Log("Set Toggle State to:" + setToggleState);
        friendViewToggle.SetIsOnWithoutNotify(setToggleState);
        sendToThisFriend = setToggleState;
    }
    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
