using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingCanvasController
{
    private SettingsCanvas _view;
    
    
    public void Init(SettingsCanvas settingsCanvas)
    {
        _view = settingsCanvas;
        UpdateDate();
    }

    public void UnInitialize()
    {
        
    }
    public void About() {
        Application.OpenURL("https://www.leaveatrace.app/");
    }
    public void ContactUs()
    {
        string email = "me@example.com";
        string subject = MyEscapeURL("Contact Us");
        string body = MyEscapeURL("My Body\r\nFull of non-escaped chars");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    public void GetHelp()
    {
        string email = "me@example.com";
        string subject = MyEscapeURL("Get Help");
        string body = MyEscapeURL("My Body\r\nFull of non-escaped chars");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    private void UpdateDate()
    {
        _view._usernameText.text = FbManager.instance.thisUserModel.Username;
        _view._profileNameText.text = FbManager.instance.thisUserModel.DisplayName;
        FbManager.instance.thisUserModel.ProfilePicture(sprite =>
        {
            _view._profileImage.sprite = sprite;
        });
    }
}
