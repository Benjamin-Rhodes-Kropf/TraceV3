using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueScreenCanvasController
{
    private QueueScreenCanvas _view;
    
    public void Init(QueueScreenCanvas queueScreenCanvas)
    {
        _view = queueScreenCanvas;
        UpdateProfile();
    }
    
    private void UpdateProfile()
    {
        _view._usernameText.text = FbManager.instance.thisUserModel.username;
        _view._profileNameText.text = FbManager.instance.thisUserModel.name;
        FbManager.instance.thisUserModel.ProfilePicture(sprite =>
        {
            _view._profileImage.texture = sprite.texture;
        });
    }

    private void GetSpotInLine()
    {
        
    }

    public void UnInitialize()
    {
        //garbage profile image
    }
}
