using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedScrollerDemos.MultipleCellTypesDemo;
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
    private bool sendToThisFriend = false;
    private string _uid = "";
    public bool _isBestFriend = false;
    public bool _isContact = false;
    public int _Index;


    private CellViewRow _cellView;
    private Action<bool> SelectionCallBack;

    private Coroutine _downloadRoutine;
    public string friendUID => _uid;

    public void UpdateUIElements(SendTraceCellViewData data, int index, Action<bool> changeSelectionCallBack)
    {
        
        friendViewToggle.onValueChanged.RemoveAllListeners();
        
        _nickName.text = data._textData;
        _uid = data._userId;
        friendViewToggle.isOn = data._isSelected;
        _profilePic.texture = data._profilePicture;
        _isBestFriend = data._isBestFriend;
        _isContact = data._isContact;
        _Index = index;
        SelectionCallBack = null;
        SelectionCallBack = changeSelectionCallBack;
        
        friendViewToggle.onValueChanged.AddListener(TogglePressed);
        
    }
    
    
    public void UpdateFrindData(bool isBestFriend, UserModel user = null, bool isFriendAdd = false)
    {
        _isBestFriend = isBestFriend;
        Debug.Log("UpdateFriendData");
        if (user != null)
        {
            _nickName.text = user.name;
            _uid = user.userID;
            _downloadRoutine = user._downloadPCoroutine;
            
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
    }

    IEnumerator SaveToPersistentStorage(Texture2D texture2D)
    {
        texture2D.Apply();
        yield return new WaitForEndOfFrame();
        PersistentStorageHandler.s_Instance.SaveTextureToPersistentStorage(texture2D,"friends",_uid);
    }

    public void TogglePressed(bool isOn)
    {
        sendToThisFriend = isOn;
        SelectFriendsControler.whoToSendTo[_uid] = sendToThisFriend;
        SelectFriendsControler.s_Instance.UpdateListDataForThisIndex(_Index,friendViewToggle.isOn,_isBestFriend,_isContact);
        SelectionCallBack?.Invoke(friendViewToggle.isOn);
    }

    public void SetToggleState(bool setToggleState)
    {
        Debug.Log("Set Toggle State to:" + setToggleState);
        friendViewToggle.SetIsOnWithoutNotify(setToggleState);
        sendToThisFriend = setToggleState;
        SelectFriendsControler.whoToSendTo[_uid] = sendToThisFriend;
    }
    public void DestroySelf()
    {
        if (_downloadRoutine != null)
            StopCoroutine(_downloadRoutine);
    
        Destroy(this.gameObject);
    }
}
