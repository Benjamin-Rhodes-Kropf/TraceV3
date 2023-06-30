using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using NativeGalleryNamespace;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TookPhotoCanvasController
{
    private TookPhotoCanvas _view;

    public static Sprite _profilePicture;

    public TookPhotoCanvasController(TookPhotoCanvas view)
    {
        this._view = view;
    }

    public void Init()
    {
        _view._doneButton.onClick.AddListener(OnDoneButtonClick);
        _view._moveBack.onClick.AddListener(OnMoveBackClick);
        LoadImageFromPath();
    }
    
    // private void LoadImageFromPath()
    // {
    //     if (TakePhotoCanvasController.imagePath == "")
    //         return;
    //     
    //     Texture2D tex = new Texture2D(2, 2);
    //     byte[] imageBytes = System.IO.File.ReadAllBytes(TakePhotoCanvasController.imagePath);
    //     tex.LoadImage(imageBytes);
    //     _profilePicture = CropTexture(tex);
    //     _view._profilePicture.sprite = _profilePicture;
    // }
    
    private void LoadImageFromPath()
    {
        Debug.Log("Load Image From Path:" + TakePhotoCanvasController.imagePath);
        if (TakePhotoCanvasController.imagePath == "")
            return;
        Debug.Log("Load Image From Path: Image Not Null");

        //deal with diffrent image types
        if (IsHEICFile(TakePhotoCanvasController.imagePath))
        {
            Debug.Log("Load Image From Path: Image is HEIC");
            LoadHeicImage(TakePhotoCanvasController.imagePath);
        }
        else
        {
            Debug.Log("Load Image From Path: Image is not HEIC");
            // Handle other image formats (e.g., PNG, JPG)
            byte[] imageBytes = System.IO.File.ReadAllBytes(TakePhotoCanvasController.imagePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            _profilePicture = CropTexture(tex);
            _view._profilePicture.sprite = _profilePicture;
        }
    }
    private bool IsHEICFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        // Check if the extension indicates a HEIC file
        if (Regex.IsMatch(extension, @"\.(heic|heif)$", RegexOptions.IgnoreCase))
        {
            return true;
        }

        return false;
    }
    
    private void LoadHeicImage(string filePath)
    {
        Debug.Log("Load Heic Image");
        string url = "file://" + filePath;

        // Load the HEIC file using UnityWebRequest
        WWW request = new WWW(url);
        while (!request.isDone) { }
        Debug.Log("Load Heic Image REQUEST DONE");

        if (string.IsNullOrEmpty(request.error))
        {
            Debug.Log("Load Heic Image is not null");
            // Get the loaded texture
            Texture2D texture = request.texture;
            byte[] jpgData = texture.EncodeToJPG();
            
            // Create a new texture from the JPG data
            Debug.Log("Texture2D jpgTexture = new Texture2D(2, 2);");
            Texture2D jpgTexture = new Texture2D(2, 2);
            jpgTexture.LoadImage(jpgData);
            Debug.Log("jpgTexture.LoadImage(jpgData);");
            _profilePicture = CropTexture(jpgTexture);
            _view._profilePicture.sprite = _profilePicture;
        }
        else
        {
            Debug.LogError("Failed to load HEIC image: " + request.error);
        }
    }

    private Sprite CropTexture(Texture2D texture)
    {
        Sprite croppedSprite = null;
        Texture2D originalTexture = texture;
        int squareSize = Mathf.Min(originalTexture.width, originalTexture.height);
        Rect croppingRect = new Rect((originalTexture.width - squareSize) / 2, (originalTexture.height - squareSize) / 2, squareSize, squareSize);
        Texture2D croppedTexture = new Texture2D((int)croppingRect.width, (int)croppingRect.height);
        croppedTexture.SetPixels(originalTexture.GetPixels((int)croppingRect.x, (int)croppingRect.y, (int)croppingRect.width, (int)croppingRect.height));
        croppedTexture.Apply();
        croppedSprite = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
        return croppedSprite;
    }
    
    public void Uninit()
    {
        _view._doneButton.onClick.RemoveAllListeners();
        _view._moveBack.onClick.RemoveAllListeners();
    }
    
    private void OnDoneButtonClick()
    {
                    ScreenManager.instance.ChangeScreenForwards("SettingUpAccount");
        
    }


    private void OnMoveBackClick()
    {
        ScreenManager.instance.ChangeScreenForwards("TakePhoto");
    }
  
}
