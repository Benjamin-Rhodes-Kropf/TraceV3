using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueueScreenCanvas : MonoBehaviour
{
    public TMP_Text _queueSpot;
    public TMP_Text _usernameText;
    public TMP_Text _profileNameText;
    public RawImage _profileImage;


    private QueueScreenCanvasController _controller;
    
    #region UnityEvents
    private void OnEnable()
    {
        if (_controller == null)
            _controller = new QueueScreenCanvasController();
            
        if (FbManager.instance.IsFirebaseUserInitialised)
            _controller.Init(this);
        
        //get spot in line
        StartCoroutine(FbManager.instance.GetOrSetSpotInQueue(FbManager.instance.thisUserModel.userID, (myReturnValue) => {
            if (myReturnValue != -1000000)
            {
                if (myReturnValue <= 0)
                {
                    _queueSpot.text = "0";
                    //add user to invited list
                    Debug.Log("user invited");
                    StartCoroutine(FbManager.instance.AddUserToInvitedListAndGoToHomeScreen(FbManager.instance.thisUserModel.phone));
                }
                else
                {
                    _queueSpot.text = myReturnValue.ToString();
                }
            }
            else
            {
                _queueSpot.text = "Error";
            }
        }));
    }
    private void OnDisable()
    {
        _controller.UnInitialize();
    }
    public void MoreInfo()
    {
        Application.OpenURL("http://Www.leaveatrace.app/queue");
    }
    #endregion
}
