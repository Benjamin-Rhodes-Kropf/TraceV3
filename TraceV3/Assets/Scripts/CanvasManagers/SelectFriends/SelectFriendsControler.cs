using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using VoxelBusters.EssentialKit;

public class SelectFriendsControler : MonoBehaviour
{
    private SelectFriendsCanvas _view;
    private List<SendToFriendView> _allFriendsView;
    public static Dictionary<string, bool> whoToSendTo;
    public Transform bestFriendText;
    public Transform allFriendsText;
    public Transform contactsText;

    public static SelectFriendsControler s_Instance;
    
    //Cell View Data 
    private List<SendTraceCellViewData> _bestFriendsData;
    private List<SendTraceCellViewData> _friendsData;
    private List<SendTraceCellViewData> _contactsData;
    
    
    public void Init(SelectFriendsCanvas view)
    {
        this._view = view;
        _allFriendsView = new List<SendToFriendView>();
        this._view._searchBar.onValueChanged.AddListener(OnSearchBarValueChange);
        whoToSendTo = new Dictionary<string, bool>();


        _bestFriendsData = new List<SendTraceCellViewData>();
        _friendsData = new List<SendTraceCellViewData>();
        _contactsData = new List<SendTraceCellViewData>();
        s_Instance = this;
        LoadAllFriends();
    }
    public void UnInitialize()
    {
        _allFriendsView.Clear();
        this._view._searchBar.onValueChanged.RemoveAllListeners();
        Debug.Log("SelectFriendsControler: UnInitialize()");
    }

    public void SelectAllFriends(bool isSelected)
    {
        foreach (var bestFriend in _bestFriendsData)
        {
            bestFriend._isSelected = isSelected;
            whoToSendTo[bestFriend._userId] = isSelected;
        }

        foreach (var friend in _friendsData)
        {
            friend._isSelected = isSelected;
            whoToSendTo[friend._userId] = isSelected;
        }
        
        _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
    }
    
    
    public void SelectBestFriends(bool isSelected)
    {
        foreach (var bestFriend in _bestFriendsData)
        {
            bestFriend._isSelected = isSelected;
            whoToSendTo[bestFriend._userId] = isSelected;
        }

        foreach (var friend in _friendsData)
        {
            friend._isSelected = false;
            whoToSendTo[friend._userId] = false;
        }
        
        _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
    }


    public void UpdateListDataForThisIndex(int index, bool isSelected, bool isBestFriend, bool isContact)
    {
        if (isBestFriend)
            _bestFriendsData[index]._isSelected = isSelected;
        else if (isContact)
            _contactsData[index]._isSelected = isSelected;
        else
            _friendsData[index]._isSelected = isSelected;
    }
    
    private void LoadAllFriends()
    {
        Debug.Log("LoadAllFriends");
        int numOfBestFriends = 0;
        var users = UserDataManager.Instance.GetAllFriends();
        foreach (var user in users)
        {
            bool isBestFriend = FriendsModelManager.Instance.IsBestFriend(user.userID);
            if (isBestFriend) numOfBestFriends++;
            try
            {
                whoToSendTo.Add(user.userID, false);
            }
            catch (Exception e)
            {
                Debug.LogWarning("user already added in dictionary");
            }
            #region Lists  Updates Data for Friends and Best Friends
            var data = new SendTraceCellViewData
            {
                _textData = user.name,
                _userId = user.userID,
                _isSelected = false,
                _isBestFriend = isBestFriend,
                _isContact = false
            };
            var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("friends", user.userID);
            if (persistentData.updateImage)
            {
                user.PPTexture((sprite =>
                {
                    try
                    {
                        data._profilePicture = (Texture2D)sprite;
                        StartCoroutine(SaveToPersistentStorage((Texture2D)sprite,user.userID,"friends"));
                    }
                    catch (Exception e)
                    {
                        print(e.Message);
                    }
                }));
            }
            else
            {
                data._profilePicture = persistentData.texture;
            }
            #endregion
            UpdateDataLists(data,isBestFriend,false);
            // UpdateFriendViewInfo(user, isBestFriend);
        }
        if (users.Count < 1)
        {
            Debug.Log("No Friends Yet");
            _view.DisplayNoFriendsText();
        }

        #region Contacts Data Load
#if !UNITY_EDITOR
        NativeContactsManager.s_Instance.LoadUserContacts(OnReadContactsFinish,OnReadContactsFailed);
#elif UNITY_EDITOR
        foreach (var contact in NativeContactsManager.s_Instance._SampleContacts)
        {
            var sample = new SendTraceCellViewData
            {
                _textData =  contact._name,
                _isSelected = false,
                _userId = contact._phone,
                _profilePicture = contact._profilePicture,
                _isBestFriend = false,
                _isContact = true
            };
            _contactsData.Add(sample);
        }
        
        _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
#endif 
        #endregion
        
        
        // _view.CheckAndChangeVisualsForNoFriends(numOfBestFriends); 
    }

    private void OnReadContactsFailed(string message)
    {
        _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
        Debug.Log(message);
    }
        
    private void OnReadContactsFinish(IAddressBookContact[] contacts)
    {
        for (var i = 0; i < contacts.Length; i++)
        {
            var contact = contacts[i];
            if (contact.PhoneNumbers.Length<=0)
                continue;
            try
            {
                whoToSendTo.Add(contact.PhoneNumbers[0], false);
            }
            catch (Exception e)
            {
                Debug.LogWarning("user already added in dictionary");
            }
            
            var data = new SendTraceCellViewData
            {
                _textData = contact.FirstName + " " + contact.LastName,
                _userId = contact.PhoneNumbers[0],
                _isSelected = false,
                _isContact = true,
                _isBestFriend = false
            };
            var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("contacts", contact.PhoneNumbers[0]);
            if (persistentData.updateImage)
            {
                contact.LoadImage((result, error) =>
                {
                    var texture = result.GetTexture();
                    if (texture)
                    {
                        var newTexture = PersistentStorageHandler.s_Instance.CropImage(texture);
                        StartCoroutine(SaveToPersistentStorage((Texture2D)newTexture,contact.PhoneNumbers[0],"contacts"));
                        data._profilePicture = newTexture;
                    }
                });
            }
            else
            {
                data._profilePicture = persistentData.texture;
            }
            // data
            UpdateDataLists(data,false,true);
        }
        _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
    }
    
    IEnumerator SaveToPersistentStorage(Texture2D texture2D,string  uid, string folderName)
    {
        texture2D.Apply();
        yield return new WaitForEndOfFrame();
        PersistentStorageHandler.s_Instance.SaveTextureToPersistentStorage(texture2D,folderName,uid);
    }

    private void UpdateDataLists(SendTraceCellViewData data, bool isBestFriend, bool  isContact)
    {
        if (isBestFriend)
        {
            _bestFriendsData.Add(data);
        }else if (isContact)
        {
            _contactsData.Add(data);
        }
        else
        {
            _friendsData.Add(data);
        }
    }
    
    
    private void UpdateFriendViewInfo(UserModel user, bool isBestFriend, bool isContact = false, SendTraceCellViewData contactData = null)
    {
        Debug.Log("UpdateFriendViewInfo");
        int bestFriendsIndex = bestFriendText.GetSiblingIndex();
        int allFriendsIndex = allFriendsText.GetSiblingIndex();
        int contactsIndex = contactsText.GetSiblingIndex();
        SendToFriendView view = GameObject.Instantiate(_view.friendViewPrefab, _view._displayFrindsParent);
        if (user == null)
        {
            view.transform.SetSiblingIndex(contactsIndex + 1);
            view.UpdateContactData(contactData);
        }
        else
        {
            if (isBestFriend)
            {
                view.GetComponent<Transform>().SetSiblingIndex(bestFriendsIndex + 1);
                view.GetComponent<SendToFriendView>().SetToggleState(whoToSendTo[user.userID]);
            }
            else
            {
                view.GetComponent<Transform>().SetSiblingIndex(allFriendsIndex + 1);
                view.GetComponent<SendToFriendView>().SetToggleState(whoToSendTo[user.userID]);
            }
            view.UpdateFrindData(isBestFriend, user,false);
        }

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
        SendTraceManager.instance.usersToSendTrace.Clear();
        Debug.Log("UpdateFriendsSendTo()");
        foreach (var user in whoToSendTo)
        {
            if (user.Value) //is selected to send to
            {
                SendTraceManager.instance.usersToSendTrace.Add(user.Key);
            }
        }
    }

    public void UpdateCellViewVisuals(string uid,bool isSelected, bool isBestFriend,bool isContact)
    {
        if (isBestFriend)
        {
            for (int i = 0; i < _bestFriendsData.Count; i++)
            {
                var bestFriend = _bestFriendsData[i];
                if (!bestFriend._userId.Equals(uid)) continue;
                
                _bestFriendsData[i]._isSelected = isSelected;
                break;
            }
        }else if (isContact)
        {
            for (int i = 0; i < _contactsData.Count; i++)
            {
                var contact = _contactsData[i];
                if (!contact._userId.Equals(uid)) continue;
                _contactsData[i]._isSelected = isSelected;
                break;
            }
        }
        else
        {
            for (int i = 0; i < _friendsData.Count; i++)
            {
                var friend = _friendsData[i];
                if (!friend._userId.Equals(uid)) continue;
                
                _friendsData[i]._isSelected = isSelected;
                break;
            }
        }
    }
    
    private void OnSearchBarValueChange(string inputText)
    {
        ClearFriendsView();
        if (inputText.Length <= 1)
        {
            _view._enhanceScroll.SetActive(true);
            _view._enhanceScroller.LoadData(_bestFriendsData,_friendsData,_contactsData);
            _view._friendsScroll.SetActive(false);
            return;
        }
        _view._enhanceScroll.SetActive(false);
        _view._friendsScroll.SetActive(true);
        
        Debug.Log("Search Friends");
        int numOfBestFriends = 0;
        var users = UserDataManager.Instance.GetFriendsByNameOld(inputText);
        foreach (var user in users)
        {
            bool isBestFriend = FriendsModelManager.Instance.IsBestFriend(user.userID);
            UpdateFriendViewInfo(user, isBestFriend);
        }

        var contacts = GetContactsByName(inputText);
        foreach (var contact in contacts)
        {
            UpdateFriendViewInfo(null, false, true, contact);
        }
    }



    private List<SendTraceCellViewData> GetContactsByName(string name)
    {
        List<SendTraceCellViewData> selectedUsers = new List<SendTraceCellViewData>();
        if (string.IsNullOrEmpty(name) is false && _contactsData.Count > 0)
        {
            var searchResults = _contactsData.Where(user => user._textData.Contains(name));
            selectedUsers.AddRange(searchResults);
        }
        return selectedUsers;
    }
}

public class SendTraceCellViewData
{
    public string _textData;
    public string _userId;
    public Texture2D _profilePicture;
    public bool _isSelected;
    public bool _isBestFriend;
    public bool _isContact;
}
