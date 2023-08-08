using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.EssentialKit;

namespace CanvasManagers
{
    
    public partial class ContactsCanvasController
    {
        private void SelectPreviouslySelectedScreen()
        {
            switch (_CurrentSelectedUserTab)
            {
                case UserTabs.Contacts:
                    OnContactsSelection();
                    break;
                case UserTabs.Friends:
                    OnFriendsSelection();
                    break;
                case UserTabs.Requests:
                    OnRequestsSelection();
                    break;
            }
            
            //fb Analytics Log Search Bar Pressed And Type
            FbManager.instance.AnalyticsOnSearchBarPressed(_CurrentSelectedUserTab.ToString());
        }



        private void PopulateFriendViews(List<UserModel> friends)
        {
            if (friends.Count <= 0) return;
            
            var text = GameObject.Instantiate(_view._searchTabTextPrefab, _view._searchscrollParent);
            text.text = "Friends";
            searchList.Add(text.gameObject);
            foreach (var friend in friends)
            {
                var view = GameObject.Instantiate(_view.friendViewPrefab, _view._searchscrollParent);
                view.UpdateFriendData(friend,true, FriendsModelManager.Instance.IsBestFriend(friend.userID));
                searchList.Add(view.gameObject);
            }
        }

        private void PopulateReceivedRequests(List<UserModel> requests)
        {
            if (requests.Count > 0)
            {
                var text = GameObject.Instantiate(_view._searchTabTextPrefab, _view._searchscrollParent);
                text.text = "Requests Received";
                searchList.Add(text.gameObject);
                foreach (var request in requests)
                {
                    var view = GameObject.Instantiate(_view._requestPrefab, _view._searchscrollParent);
                    view.UpdateRequestView(request);
                    searchList.Add(view.gameObject);
                }
            }
        }

        private void PopulateSentRequests(List<UserModel> requestsSent)
        {
            if (requestsSent.Count > 0)
            {
                var text = GameObject.Instantiate(_view._searchTabTextPrefab, _view._searchscrollParent);
                text.text = "Requests Sent";
                searchList.Add(text.gameObject);
                foreach (var request in requestsSent)
                {
                    var view = GameObject.Instantiate(_view._requestPrefab, _view._searchscrollParent);
                    view.UpdateRequestView(request,false);
                    searchList.Add(view.gameObject);
                }
            }
        }


        private void PopulateContactViews(List<IAddressBookContact> contacts)
        {
            if (contacts.Count > 0)
            {
                var text = GameObject.Instantiate(_view._searchTabTextPrefab, _view._searchscrollParent);
                text.text = "Contacts";
                searchList.Add(text.gameObject);
                foreach (var contact in contacts)
                {
                    ContactView view = GameObject.Instantiate(_view._contactPrfab,_view._searchscrollParent);
                    view.UpdateContactInfo(contact);
                    searchList.Add(view.gameObject);
                }
            }
        }


        private void PopulateOtherUsers(List<UserModel> others,List<UserModel> friends,List<UserModel> requests,List<UserModel> requestsSent )
        {
            if (others.Count > 0)
            {
                var text = GameObject.Instantiate(_view._searchTabTextPrefab, _view._searchscrollParent);
                text.text = "Others";
                searchList.Add(text.gameObject);
                foreach (var other in others)
                {
                    if (friends.Contains(other)) continue;
                    if (requestsSent.Contains(other)) continue;
                    if (requests.Contains(other)) continue;
                    if (other.userID == FbManager.instance.thisUserModel.userID) continue;
                    var view = GameObject.Instantiate(_view.friendViewPrefab, _view._searchscrollParent);
                    view.UpdateFriendData(other);
                    searchList.Add(view.gameObject);
                }
            }
        }
        
        private void SearchUsersInDB(string inputText)
        {
            UserDataManager.Instance.GetAllUsersBySearch(inputText, out List<UserModel> friends, out List<UserModel> requests,out List<UserModel> requestsSent, out List<UserModel> others, out List<UserModel> superusers);
            TryGetContactsByName(inputText.ToLower(), out List<IAddressBookContact> contacts);
            switch (_CurrentSelectedUserTab)
            {
                case UserTabs.Contacts:
                    PopulateContactViews(contacts);
                    PopulateFriendViews(friends);
                    PopulateReceivedRequests(requests);
                    PopulateSentRequests(requestsSent);
                    PopulateOtherUsers(others,friends,requests,requestsSent);
                    break;
                case UserTabs.Friends:
                    PopulateFriendViews(friends);
                    PopulateReceivedRequests(requests);
                    PopulateSentRequests(requestsSent);
                    PopulateContactViews(contacts);
                    PopulateOtherUsers(others,friends,requests,requestsSent);
                    break;
                case UserTabs.Requests:
                    PopulateReceivedRequests(requests);
                    PopulateSentRequests(requestsSent);
                    PopulateFriendViews(friends);
                    PopulateContactViews(contacts);
                    PopulateOtherUsers(others,friends,requests,requestsSent);
                    break;
            }
            
            //Debug super users Later we will put into list
            foreach (var user in superusers)
            {
                Debug.Log(user.name);
            }
        }
        
        
    }
}
