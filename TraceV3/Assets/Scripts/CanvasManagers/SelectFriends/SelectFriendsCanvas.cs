using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectFriendsCanvas : MonoBehaviour{
    
    [Header("Friend Info")]
    public SendToFriendView friendViewPrefab;
    public Transform _displayFrindsParent;
    public GameObject _youDontHaveAnyFriendsYetText;
    public List<SendToFriendView> _friendsList;
    public GameObject _friendsScroll;
    
    private SelectFriendsControler _controller;
    
    public static Action UpdateFriendsView;
    public static Action UpdateFriendsSendTo;


    #region UnityEvents

    private void OnEnable()
    {
        //objects
        _youDontHaveAnyFriendsYetText.SetActive(false);
        
        //controler
        if (_friendsList == null)
            _friendsList = new List<SendToFriendView>();
        if (_controller == null)
            _controller = gameObject.AddComponent<SelectFriendsControler>();
        _controller.Init(this);

        //listners
        UpdateFriendsView += _controller.UpdateFriendsLayout;
    }

    public void DisplayNoFriendsText()
    {
        Debug.Log("Displaying No Friends Text");
        _youDontHaveAnyFriendsYetText.SetActive(true);
    }
    
    public void BackToMainScene() {
        //change the bool so that the main canavs can be enabled after the main scene is loaded
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
        UpdateFriendsView -= _controller.UpdateFriendsLayout;
        //UpdateFriendsSendTo -= _controller.UpdateFriendsSendTo;
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
