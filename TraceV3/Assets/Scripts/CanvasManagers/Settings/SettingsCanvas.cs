using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCanvas : MonoBehaviour
{
   public TMP_Text _usernameText;
   public TMP_Text _profileNameText;
   public Image _profileImage;

   private SettingCanvasController _controller;
   
   #region UnityEvents

   private void OnEnable()
   {
      if (_controller == null)
         _controller = new SettingCanvasController();
            
      if (FbManager.instance.IsFirebaseUserInitialised)
         _controller.Init(this);
   }

   private void OnDisable()
   {
      _controller.UnInitialize();
   }

   #endregion
   
   public void About() {
      Application.OpenURL("https://www.leaveatrace.app/");
   }
   public void ContactUs()
   {
      string email = "bsiegel@leaveatraceapp.com";
      string subject = MyEscapeURL("[Contact Us]");
      string body = MyEscapeURL("Hello! My name is " + FbManager.instance.thisUserModel.Username + " and I would like to contact you because");
      Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
   }
   public void GetHelp()
   {
      string email = "bsiegel@leaveatraceapp.com";
      string subject = MyEscapeURL("[Get Help]");
      string body = MyEscapeURL("Hello! My name is " + FbManager.instance.thisUserModel.Username + " and I need some help with");
      Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
   }

   public void Logout()
   {
      FbManager.instance.Logout(FbManager.LoginStatus.LoggedIn);
   }
   string MyEscapeURL(string url)
   {
      return WWW.EscapeURL(url).Replace("+", "%20");
   }
}
