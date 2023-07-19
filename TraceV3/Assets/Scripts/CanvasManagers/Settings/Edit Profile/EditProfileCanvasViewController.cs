using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditProfileCanvasViewController
{
    public EditProfileCanvasView _view;
    
    
    public void Init(EditProfileCanvasView editProfileCanvasView)
    {
        _view = editProfileCanvasView;
        UpdateData();
    }

    public void UnInitialize()
    {
        
    }


    private void UpdateData()
    {
        _view._email.text = FbManager.instance.thisUserModel.email;
        _view._username.text = FbManager.instance.thisUserModel.username;
        _view._usernameLarge.text = FbManager.instance.thisUserModel.username;
        _view._profileName.text = FbManager.instance.thisUserModel.name;
        _view._profileNameLarge.text = FbManager.instance.thisUserModel.name;
        _view._password.text = "***********";
        FbManager.instance.thisUserModel.ProfilePicture(sprite =>
        {
            _view._profilePicture.sprite = sprite;
        });
    }
    
}
