using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace CanvasManagers
{
    public enum UserTabs
    {
        Contacts, Friends, Requests
    }
    public partial class ContactsCanvasController
    {
        private ContactsCanvas _view;
        private Image _previousSelectedButton;
        private Color32 _selectedButtonColor = new Color32(128, 128, 128, 255);
        private Color32 _unSelectedButtonColor = new Color32(128, 128, 128, 0);
        private List<IAddressBookContact> _allContacts;
        private UserTabs _CurrentSelectedUserTab;
        
        private static readonly Regex _compiledUnicodeRegex = new Regex(@"[^\u0000-\u007F]", RegexOptions.Compiled);
        public void Init(ContactsCanvas view)
        {
            this._view = view;
            _view._usernameInput.onValueChanged.AddListener(OnChangeInput);
            _view._contactsButton.onClick.AddListener(OnContactsSelection);
            _view._friendsButton.onClick.AddListener(OnFriendsSelection);
            _view._requestsButton.onClick.AddListener(OnRequestsSelection);

            if(!CheckIfFirstTimeOnScreen())
                OnFriendsSelection();
            UpdateSelectionPanelView();
        }
        
        public void UnInitialize()
        {
            _view._usernameInput.onValueChanged.RemoveAllListeners();
            _view._contactsButton.onClick.RemoveAllListeners();
            _view._friendsButton.onClick.RemoveAllListeners();
            _view._requestsButton.onClick.RemoveAllListeners();
        }

        private bool CheckIfFirstTimeOnScreen()
        {
            if (PlayerPrefs.GetInt("FirstTimeOnContacts") == 1)
            {
                PlayerPrefs.SetInt("FirstTimeOnContacts", 0);
                SelectionPanelClick("Contacts");
                return true;
            }
            return false;
        }


        public void UpdateSelectionPanelView()
        {
            _view._redRequestMark.SetActive(FbManager.instance._allReceivedRequests.Count > 0);
        }
        private void FrindsListInit()
        {
            foreach (var friend in _view._friendsList)
            {
                friend.gameObject.SetActive(false);
            }
        }

        private List<GameObject> searchList;
        private string lastInputText = "";

        private void OnChangeInput(string inputText)
        {
            OnInputValueChange(inputText);
        }
        
        private async Task OnInputValueChange(string inputText)
        {
            if (inputText.Length <= 1 && searchList.Count>0)
            {
                ClearSearchList();
                SelectPreviouslySelectedScreen();
            }
            else
            {
                await CheckForChanges();
            }

            // var canUpdate = inputText.Length > 1;
            //
            // if (!canUpdate) return;
            //
            // inputText = inputText.ToLower();
            // ClearSearchList();
            // SelectionPanelClick("SearchBar");
            // searchList = new List<GameObject>();
            // SearchUsersInDB(inputText);

        }

        private async Task CheckForChanges()
        {
            string currentValue = _view._usernameInput.text;

            // Wait for the check interval to see if any more changes occur
            await Task.Delay((int)(0.25f * 1000));

            // After the wait, check if the value is still the same as before the wait
            if (currentValue == _view._usernameInput.text && currentValue != lastInputText)
            {
                currentValue = currentValue.ToLower();
                ClearSearchList();
                SelectionPanelClick("SearchBar");
                searchList = new List<GameObject>();
                SearchUsersInDB(currentValue);
                lastInputText = currentValue;
            }
        }
        
        
        
        private void ClearSearchList()
        {
            if (searchList is not { Count: > 0 })
                return;
            foreach (var ob in searchList)
            {
              GameObject.Destroy(ob);
            }

            Resources.UnloadUnusedAssets();
        }
        private void ClearFriendList()
        {
            foreach (var friend in _view._friendsList)
            {
                friend.gameObject.SetActive(false);
            }
        }
        
        // TODO: Delete These Functions
        // private void PopulateFriendsList(List<UserModel> users, bool IsFriendsList = false)
        // {
        //     int allFrindsTileCount = _view._friendsList.Count;
        //     int allUsersCount = users.Count;
        //     bool isNeedToAddMoreTiles = allUsersCount > allFrindsTileCount;
        //     int totalUsers = isNeedToAddMoreTiles ? allUsersCount : allFrindsTileCount;
        //
        //     for (int userIndex = 0; userIndex < totalUsers; userIndex++)
        //     {
        //         if (isNeedToAddMoreTiles)
        //         {
        //             if (userIndex < _view._friendsList.Count)
        //             {
        //                 var friend = _view._friendsList[userIndex];
        //                 friend.UpdateFriendData(users[userIndex], IsFriendsList);
        //                 friend.gameObject.SetActive(true);
        //                 
        //             }
        //             else
        //             {
        //                 FriendView friend = GameObject.Instantiate(_view.friendViewPrefab, _view._displayFrindsParent);
        //                 _view._friendsList.Add(friend);
        //                 friend.UpdateFriendData(users[userIndex], IsFriendsList);
        //                
        //             }
        //         }
        //         else
        //         {
        //             if (userIndex < users.Count)
        //             {
        //                 var friend = _view._friendsList[userIndex];
        //                 friend.UpdateFriendData(users[userIndex], IsFriendsList);
        //                 friend.gameObject.SetActive(true);
        //                 
        //             }
        //             else
        //             {
        //                 var friend = _view._friendsList[userIndex];
        //                 friend.gameObject.SetActive(false);
        //             }
        //         }
        //     }
        // }
        // private void PopulateFriendUIObject(FriendView friendView, UserModel data)
        // {
        //     friendView.UpdateFriendData(data);
        //     friendView.gameObject.SetActive(true);
        //     friendView._addRemoveButton.onClick.RemoveAllListeners();
        // }


        private List<RequestView> _allRequests;

        public void UpdateRequestLayout()
        {
            if ( _view._requestsScroll.activeInHierarchy)
                LoadAllRequestsNew();
                //LoadAllRequestsOld();
        }
        
        private void OnRequestsSelection()
        {
            //LoadAllRequestsOld();
            LoadAllRequestsNew();
            SelectionPanelClick("Requests");
        }

        private void LoadAllRequestsOld()
        {
            var users = UserDataManager.Instance.GetReceivedFriendRequestedOld();
            ClearRequestView();
            _allRequests = new List<RequestView>();
            if (users.Count > 0)
            {
                foreach (var user in users)
                    UpdateRequestInfo(user);
            }
            
            var sentRequests = UserDataManager.Instance.GetSentFriendRequestsOld();
            
            if (sentRequests.Count > 0)
            {                
                foreach (var user in sentRequests)
                    UpdateRequestInfo(user, false);
            }
            
            _view._requestText.text = $"Requests ({users.Count + sentRequests.Count})";
        }
        private void LoadAllRequestsNew()
        {
            var users = UserDataManager.Instance.GetReceivedFriendRequestedOld();
            ClearRequestView();
            _allRequests = new List<RequestView>();
            if (users.Count > 0)
            {
                foreach (var user in users)
                    UpdateRequestInfo(user);
            }
            
            var sentRequests = UserDataManager.Instance.GetSentFriendRequestsOld();
            
            if (sentRequests.Count > 0)
            {                
                foreach (var user in sentRequests)
                    UpdateRequestInfo(user, false);
            }
            
            _view._requestText.text = $"Requests ({users.Count + sentRequests.Count})";
        }

        private void ClearRequestView()
        {
            if (_allRequests == null)
                return;
            if (_allRequests.Count <= 0)
                return;
            
            foreach (var request in _allRequests)
                GameObject.Destroy(request.gameObject);
        }
        private void UpdateRequestInfo(UserModel _user, bool isReceivedRequest = true)
        {
            RequestView view = GameObject.Instantiate(_view._requestPrefab,_view._requestParent);
            view.UpdateRequestView(_user, isReceivedRequest);
            _allRequests.Add(view);
        }

        private List<FriendView> _allFriendsView;

        public void UpdateFriendsLayout()
        {
            if (_view._friendsScroll.activeInHierarchy)
                LoadAllFriends();
        }
        private void OnFriendsSelection()
        {
            LoadAllFriends();
            SelectionPanelClick("Friends");
        }

        private void LoadAllFriends()
        {
            var users = UserDataManager.Instance.GetAllFriends();
            Debug.Log("Update Layout Called");
            ClearFriendsView();
            
            _view._numberOfFriendsCountTitle.text = $"{users.Count} Friends";
            _view._numberOfFriendsCountScroll.text = $"My Friends ({users.Count})";
            _allFriendsView = new List<FriendView>();
            
            foreach (var user in users)
                UpdateFriendViewInfo(user);
        }


        private void UpdateFriendViewInfo(UserModel user)
        {
            FriendView view = GameObject.Instantiate(_view.friendViewPrefab, _view._displayFrindsParent);
            view.UpdateFriendData(user,true, FriendsModelManager.Instance.IsBestFriend(user.userID));
            _allFriendsView.Add(view);
        }
        private void ClearFriendsView()
        {
            if (_allFriendsView == null)
            {
                _allRequests = new List<RequestView>();
                return;
            }
            
            if(_allFriendsView.Count <= 0)
                return;
            foreach (var view in _allFriendsView)
                GameObject.Destroy(view.gameObject);
        }

        private void SelectionPanelClick(string _selectedButton)
        {
            if (_previousSelectedButton != null)
                _previousSelectedButton.color = _unSelectedButtonColor;
            switch (_selectedButton)
            {
                case "Contacts":
                    _previousSelectedButton = _view._contactsButton.GetComponent<Image>();
                    _view._contactsScroll.SetActive(true);
                    _view._friendsScroll.SetActive(false);
                    _view._requestsScroll.SetActive(false);                    
                    _view._searchScroll.SetActive(false);
                    _CurrentSelectedUserTab = UserTabs.Contacts;
                    break;
                case "Friends":
                    _previousSelectedButton = _view._friendsButton.GetComponent<Image>();
                    _view._contactsScroll.SetActive(false);
                    _view._friendsScroll.SetActive(true);
                    _view._requestsScroll.SetActive(false);                    
                    _view._searchScroll.SetActive(false);                    
                    _CurrentSelectedUserTab = UserTabs.Friends;

                    break;
                case "Requests":
                    _previousSelectedButton = _view._requestsButton.GetComponent<Image>();
                    _view._contactsScroll.SetActive(false);
                    _view._friendsScroll.SetActive(false);
                    _view._requestsScroll.SetActive(true);                    
                    _view._searchScroll.SetActive(false);
                    _CurrentSelectedUserTab = UserTabs.Requests;
                    break;
                default:
                    _view._contactsScroll.SetActive(false);
                    _view._friendsScroll.SetActive(false);
                    _view._requestsScroll.SetActive(false);
                    _view._searchScroll.SetActive(true);
                    break;
            }

            _previousSelectedButton.color = _selectedButtonColor;
        }


        private bool isLoaded = false;
        private void OnContactsSelection()
        {
#if !UNITY_EDITOR
            NativeContactsManager.s_Instance.LoadUserContacts(OnReadContactsFinish,OnReadContactsFailed);
#elif UNITY_EDITOR
            _view._enhanceScroller.LoadLargeData(null,NativeContactsManager.s_Instance._SampleContacts);
#endif          
            SelectionPanelClick("Contacts");
        }

        private void OnReadContactsFailed(string message)
        {
            Debug.Log(message);
        }
        
        private void OnReadContactsFinish(IAddressBookContact[] contacts)
        {
            if (isLoaded)
                return;
            
            _view._enhanceScroller.LoadLargeData(contacts);
            _allContacts = new List<IAddressBookContact>();
            _allContacts.AddRange(contacts);
            isLoaded = true;
        }
        
        private String StripUnicodeCharactersFromString(string inputValue)
        {
            return _compiledUnicodeRegex.Replace(inputValue, String.Empty);
        }

        private void LogContactInfo(IAddressBookContact contact)
        {
            try
            {
                 // ContactView view = GameObject.Instantiate(_view._contactPrfab,_view._contactParent);
                 // view.UpdateContactInfo(contact);
                _allContacts.Add(contact);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void TryGetContactsByName(string name, out List<IAddressBookContact> selectedContacts)
        {
            Debug.Log("Searching Contacts");

            selectedContacts = new List<IAddressBookContact>();
            
            if (_allContacts is not { Count: > 0 })
                return;

            Debug.Log("Searching Contacts with name "+ name   + _allContacts.Count);
            // Todo : Check Why Contain Function for Contact Given Name is not working ?
            
            if (string.IsNullOrEmpty(name) is false )
            {
                var list = _allContacts.Where(contact => (contact.FirstName + " "+ contact.LastName).ToLower().Contains(name, StringComparison.InvariantCultureIgnoreCase)  && contact.PhoneNumbers.Length>0).ToList();
                selectedContacts.AddRange(list);
            }
        }
    }

    [Serializable]
    public struct Contact
    {
        public string givenName;
        public string phoneNumber;
        public Texture2D profile;
    }
}

