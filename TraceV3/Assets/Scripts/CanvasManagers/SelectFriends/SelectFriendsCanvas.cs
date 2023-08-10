using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedScrollerDemos.MultipleCellTypesDemo;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectFriendsCanvas : MonoBehaviour
{

    [Header("Search Bar")] public TMP_InputField _searchBar;

    [Header("Pagination Scroller")] public MultipleCellTypesDemo _enhanceScroller;
    
    
    [Header("Friend Info")]
    public SendToFriendView friendViewPrefab;
    public Transform _displayFrindsParent;
    public GameObject _youDontHaveAnyFriendsYetText;
    public List<SendToFriendView> _friendsList;
    public GameObject _friendsScroll;
    [SerializeField] private SelectFriendsControler _controller;
    [SerializeField] private Transform bestFriendText;
    [SerializeField] private GameObject bestFriendLine;
    [SerializeField] private GameObject bestFriendSpace;
    [SerializeField] private Transform allFriendText;
    [SerializeField] private Transform contactsText;


    public GameObject _enhanceScroll;
    
    //visuals
    public bool selectAllFriends = true;
    public bool toggleBestFriends;
    
    #region UnityEvents

    private void OnEnable()
    {
        
        //objects
        _youDontHaveAnyFriendsYetText.SetActive(false);
        //controller
        _friendsList = new List<SendToFriendView>();
        if (_controller == null)
        {
            _controller = gameObject.AddComponent<SelectFriendsControler>();
            _controller.bestFriendText = bestFriendText;
            _controller.allFriendsText = allFriendText;
            _controller.contactsText = contactsText;
        }
        selectAllFriends = true;
        _controller.Init(this);
        ToggleAllFriends();
    }

    public void CheckAndChangeVisualsForNoFriends(int numOfBestFriends)
    {
        if (numOfBestFriends != 0)
        {
            bestFriendText.gameObject.SetActive(true);
            bestFriendLine.SetActive(true);
            bestFriendLine.SetActive(true);
        }
        else
        {
            bestFriendText.gameObject.SetActive(false);
            bestFriendLine.SetActive(false);
            bestFriendSpace.SetActive(false);
        }
    }

    public void DisplayNoFriendsText()
    {
        _youDontHaveAnyFriendsYetText.SetActive(true);
    }
    
    public void BackToMainScene() {
        Debug.Log("BackToMain");
        ClearFriendsList();
       _controller.UnInitialize();
        ScreenManager.instance.isComingFromCameraScene = true;
        SceneManager.UnloadSceneAsync(1);
        ScreenManager.instance.camManager.cameraPanel.SetActive(false);//disabling the camera panel
        ScreenManager.instance.ChangeScreenNoAnim("HomeScreen");
    }
    
    public void SendButtonPressed()
    {
        StartCoroutine(LoadMap());
        Debug.Log("SendButtonPressed()");
        _controller.UpdateFriendsSendTo();
    }

    public void ToggleAllFriends()
    {
        selectAllFriends = !selectAllFriends;
        _controller.SelectAllFriends(selectAllFriends);
        // if (selectAllFriends)
        // {
        //     foreach (var friendObject in _friendsList)
        //     {
        //         friendObject.SetToggleState(true);
        //     }
        //     toggleBestFriends = true;
        // }else {
        //     foreach (var friendObject in _friendsList)
        //     {
        //         friendObject.SetToggleState(false);
        //     }
        //     toggleBestFriends = false;
        // }
    }
    
    public void ToggleBestFriends()
    {
        toggleBestFriends = !toggleBestFriends;
        _controller.SelectBestFriends(toggleBestFriends);
        // if (toggleBestFriends)
        // {
        //     foreach (var friendObject in _friendsList)
        //     {
        //         if (friendObject._isBestFriend)
        //         {
        //             friendObject.SetToggleState(true);
        //         }
        //     }
        // }else {
        //     foreach (var friendObject in _friendsList)
        //     {
        //         if (friendObject._isBestFriend)
        //         {
        //             friendObject.SetToggleState(false);
        //         }
        //     }
        // }
    }
    
    //load map before screen switch
    IEnumerator LoadMap()
    {
        ScreenManager.instance.isComingFromCameraScene = true;
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(1);
        HomeScreenManager.isInSendTraceView = true;
        while (!unloadOperation.isDone)
        {
            yield return null;
        }
        ScreenManager.instance.ChangeScreenForwards("SelectRadius");
    }
    private void OnDisable()
    {
        ClearFriendsList();
        _controller.UnInitialize();
    }

    public void ClearFriendsList()
    {
        foreach (var obj in _friendsList)
        {
            obj.DestroySelf();
        }
        _friendsList.Clear();
    }
    
    #endregion
    public void TurnOffCamera() {
        ScreenManager.instance.camManager.cameraPanel.SetActive(false);//disabling the camera panel
        ScreenManager.instance.uiController.previewVideoPlayer.gameObject.SetActive(false);//disabling the camera panel
    }
}

