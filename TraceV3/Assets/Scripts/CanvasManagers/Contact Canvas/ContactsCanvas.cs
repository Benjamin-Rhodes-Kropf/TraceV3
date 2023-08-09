using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedScrollerDemos.SuperSimpleDemo;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CanvasManagers
{
    public class ContactsCanvas : MonoBehaviour
    {
        public UserView userViewPrefab;
        public TMP_InputField _usernameInput;
        public TMP_Text _numberOfFriendsCountTitle;
        public TMP_Text _numberOfFriendsCountScroll;
        public Transform _displayFrindsParent;
        public List<UserView> _friendsList;

        [Header("Contacts Info")] 
        public ContactView _contactPrfab;
        public Transform _contactParent;
        public ContactScrollManager _enhanceScroller;
        
        [Header("Requests Info")] 
        public RequestView _requestPrefab;
        public Transform _requestParent;
        public TMP_Text _requestText;
        public GameObject _redRequestMark;

        [Header("Toggle Panels")] 
        public GameObject _contactsScroll;
        public GameObject _friendsScroll;
        public GameObject _requestsScroll;
        public GameObject _searchScroll;
        
        [Header("Panel Toggle Buttons")] 
        public Button _contactsButton;
        public Button _friendsButton;
        public Button _requestsButton;

        [Header("Search Bar Content")] 
        public Transform _searchscrollParent;
        public TMP_Text _searchTabTextPrefab;

        [Header("Testing")] 
        public List<Contact> _testContactList;


        private ContactsCanvasController _controller;

        public static Action UpdateRequestView;
        public static Action UpdateFriendsView;
        public static Action UpdateRedMarks;

        #region UnityEvents

        private void OnEnable()
        {
            if (_controller == null)
                _controller = new ContactsCanvasController();
            _controller.Init(this);
            UpdateRequestView += _controller.UpdateRequestLayout;
            UpdateFriendsView += _controller.UpdateFriendsLayout;
            UpdateRedMarks += _controller.UpdateSelectionPanelView;

            Debug.Log("# OF FRIENDS:" + FbManager.instance._allFriends.Count);
            Debug.Log("# OF SENT REQUESTS:" + FbManager.instance._allSentRequests.Count);
            Debug.Log("# OF RECEIVED REQUESTS:" + FbManager.instance._allReceivedRequests.Count);
        }

        private void OnDisable()
        {
            _controller.UnInitialize();
            UpdateRequestView -= _controller.UpdateRequestLayout;
            UpdateFriendsView -= _controller.UpdateFriendsLayout;
            UpdateRedMarks -= _controller.UpdateSelectionPanelView;
        }

        public void ClearFriends()
        {
            _friendsList.Clear();   
        }

        #endregion
    }
}