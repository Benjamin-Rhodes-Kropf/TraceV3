using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCanvas : MonoBehaviour
{
   public TMP_Text _usernameText;
   public TMP_Text _profileNameText;
   public RawImage _profileImage;

   private SettingCanvasController _controller;

   [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
   
   #region UnityEvents
   private void OnEnable()
   {
      if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone6s_7_8)_verticalLayoutGroup.spacing = -73;
      else if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhoneSE2 || ScreenSizeManager.instance.currentModel == iPhoneModel.iPhoneSE3)_verticalLayoutGroup.spacing = -73;
      else if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone6sPlus_6Plus_7Plus_8Plus)_verticalLayoutGroup.spacing = -73;
      else if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhoneSE2)_verticalLayoutGroup.spacing = -73;
      else if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhoneSE3)_verticalLayoutGroup.spacing = -73;



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
   
   //choose new image
   public void ChangeProfileImage()
   {
      Debug.Log("ChangeProfileImage");
      NativeMethodsManager.OpenGalleryToPickMedia(TakePictureFromGallery);
   }

   private void TakePictureFromGallery(string path)
   {
      Debug.Log("ImageConvert: Path :: " + path);
      if (string.IsNullOrEmpty(path))
         return;
      
      

      // Set the texture to the RawImage component
      //_profileImage.texture = texture;
      
      _profileImage.texture = HelperMethods.PrepareProfilePhoto(path);
      FbManager.instance.UploadProfilePicture(_profileImage.texture);
   }
   
   //other
   public void About() {
      Application.OpenURL("https://www.leaveatrace.app/");
   }
   public void ContactUs()
   {
      string email = "bsiegel@leaveatraceapp.com";
      string subject = MyEscapeURL("[Contact Us]");
      string body = MyEscapeURL("Hello! My name is " + FbManager.instance.thisUserModel.name + " and I would like to contact you because");
      Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
   }
   public void GetHelp()
   {
      string email = "bsiegel@leaveatraceapp.com";
      string subject = MyEscapeURL("[Get Help]");
      string body = MyEscapeURL("Hello! My name is " + FbManager.instance.thisUserModel.name + " and I need some help with");
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
