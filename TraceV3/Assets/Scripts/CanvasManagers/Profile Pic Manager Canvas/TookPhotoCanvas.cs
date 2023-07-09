using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TookPhotoCanvas : MonoBehaviour
{
    public Button _moveBack;
    public Button _doneButton;
    public Image _profileImage;

    private TookPhotoCanvasController _controller;

    private void OnEnable()
    {
       if (_controller is null)
            _controller = new TookPhotoCanvasController(this);
        _controller.Init();
        
        ChangeProfileImage();
    }
    
    public void ChangeProfileImage()
       {
          Debug.Log("ChangeProfileImage");
          NativeMethodsManager.OpenGalleryToPickMedia(TakePictureFromGallery);
       }
       private void TakePictureFromGallery(string path)
       {
          bool isHeic = false;
          Debug.Log("ImageConvert: Path :: "+ path);
          if (string.IsNullOrEmpty(path))
             return;
          
          // Check if the image is in HEIC format
          if (Path.GetExtension(path).Equals(".heic", System.StringComparison.OrdinalIgnoreCase))
          {
             Debug.Log("ImageConvert: Convert Heic To Png");
             if (path == "null")
             {
                Debug.Log("Path is Null");
                return;
             }
             isHeic = true;
          }
          else
          {
             Debug.Log("ImageConvert: Already PNG");
          }
          byte[] imageData = File.ReadAllBytes(path);
          File.Delete(Application.persistentDataPath + "/output.png");
          DisplayPhotoToProfileImage(imageData, isHeic);
       }
       private void DisplayPhotoToProfileImage(byte[] imageBytes, bool isHeic)
       {
          // Create a new Texture2D object
          Texture2D texture = new Texture2D(2, 2);
    
          // Load the image data into the texture
          bool success = texture.LoadImage(imageBytes);
    
          if (success)
          {
             //_profileImage.sprite = texture.to;
             Debug.Log("Texture loaded successfully");
             // _profileImage.sprite = HelperMethods.PrepareProfilePhoto(texture);
             // if (isHeic)
             // {
             //    _profileImage.sprite= Sprite.Create(HelperMethods.RotateTextureClockwise(_profileImage.sprite.texture), new Rect(new Vector2(0,0),new Vector2(_profileImage.sprite.texture.width,_profileImage.sprite.texture.height)), new Vector2(0.5f,0.5f));
             // }
             //FbManager.instance.UploadProfilePicture(_profileImage);
          }
          else
          {
             // Failed to load texture
             Debug.Log("Failed to load texture");
          }
       }

    private void OnDisable()
    {
        _controller.Uninit();
    }
}
