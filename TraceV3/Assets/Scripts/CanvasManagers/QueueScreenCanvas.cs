using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueueScreenCanvas : MonoBehaviour
{

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
    }
    private void OnDisable()
    {
        _controller.UnInitialize();
    }
    #endregion
}
