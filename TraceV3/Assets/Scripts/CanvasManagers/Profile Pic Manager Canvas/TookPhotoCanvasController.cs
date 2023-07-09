using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using NativeGalleryNamespace;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TookPhotoCanvasController
{
    private TookPhotoCanvas _view;

    public TookPhotoCanvasController(TookPhotoCanvas view)
    {
        this._view = view;
    }

    public void Init()
    {
        _view._doneButton.onClick.AddListener(OnDoneButtonClick);
        _view._moveBack.onClick.AddListener(OnMoveBackClick);
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
