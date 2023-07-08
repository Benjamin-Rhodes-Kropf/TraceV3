using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCanvas : MonoBehaviour
{
   public TMP_Text _usernameText;
   public TMP_Text _profileNameText;
   public Image _profileImage;

   private SettingCanvasController _controller;

   [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
   
   #region UnityEvents
   private void OnEnable()
   {
      if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone7_8)_verticalLayoutGroup.spacing = -73;
      if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone7Plus_8Plus)_verticalLayoutGroup.spacing = -73;

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
      Debug.Log("Path :: "+ path);
      if (string.IsNullOrEmpty(path))
         return;
      
      
      // Check if the image is in HEIC format
      // Check if the image is in HEIC format
      if (Path.GetExtension(path).Equals(".heic", System.StringComparison.OrdinalIgnoreCase))
      {
         ConvertHeicToTexture(path);
      }
      else
      {
         LoadImageToTexture(path);
      }
   }
   private void ConvertHeicToTexture(string path)
   {
      // Convert the HEIC image to a Texture2D
      byte[] heicBytes = File.ReadAllBytes(path);
      //byte[] pngBytes = ImageConversion.ConvertHeicToPng(heicBytes);
      //LoadImageBytesToTexture(pngBytes);
   }
   private void LoadImageToTexture(string imagePath)
   {
      // Load the image data from file
      byte[] imageData = File.ReadAllBytes(imagePath);

      // Convert the image data to a Texture2D
      LoadImageBytesToTexture(imageData);
   }
   private void LoadImageBytesToTexture(byte[] imageBytes)
   {
      // Create a new Texture2D object
      Texture2D texture = new Texture2D(2, 2);

      // Load the image data into the texture
      bool success = texture.LoadImage(imageBytes);

      if (success)
      {
         // Texture loaded successfully
         // You can now use the 'texture' variable as the image texture
         // For example, assign it to a material or display it on a UI element
         Debug.Log("Texture loaded successfully");
         // Do something with the texture...
      }
      else
      {
         // Failed to load texture
         Debug.Log("Failed to load texture");
      }
   }
   
   //other
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
