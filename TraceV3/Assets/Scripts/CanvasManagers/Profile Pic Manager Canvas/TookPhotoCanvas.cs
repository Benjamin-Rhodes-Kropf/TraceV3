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
    public RawImage _profileImage;

    private TookPhotoCanvasController _controller;

    private void OnEnable()
    {
       if (_controller is null)
            _controller = new TookPhotoCanvasController(this);
       _controller.Init();
       _profileImage.texture = HelperMethods.PrepareProfilePhoto(TakePhotoCanvasController.imagePath);
       FbManager.instance.UploadProfilePicture(_profileImage.texture);
       FbManager.instance.CreateUserDocumentInFireStore();
    }
    
    private void TakePictureFromGallery(string path)
    {
        NativeMethodsManager.OpenGalleryToPickMedia(TakePictureFromGallery);

        Debug.Log("ImageConvert: Path :: " + path);
        if (string.IsNullOrEmpty(path))
            return;
        
        _profileImage.texture = HelperMethods.PrepareProfilePhoto(path);
        FbManager.instance.UploadProfilePicture(_profileImage.texture);
    }
       
    

    private void OnDisable()
    {
        _controller.Uninit();
    }
}
