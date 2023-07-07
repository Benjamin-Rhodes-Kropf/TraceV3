using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUpAccount : MonoBehaviour
{
    private void OnEnable()
    {
        if (FbManager.instance.IsFirebaseUserInitialised)
            UploadProfilePicture();
    }


    private void UploadProfilePicture()
    {
        var bytes = TookPhotoCanvasController._profilePicture.texture.EncodeToPNG();
        StartCoroutine(FbManager.instance.UploadProfilePhoto(bytes, (isUploaded, url) =>
        {
            if (isUploaded)
            {
                StartCoroutine(FbManager.instance.SetUserProfilePhotoUrl(url,
                    (isUrlSet) =>
                    {
                        if (isUrlSet)
                        {
                            Debug.Log("SetUserProfilePhotoUrl");
                            ScreenManager.instance.ChangeScreenFade("HomeScreen");
                        }
                        else
                        {
                            Debug.Log("Failed To SetUserProfilePhotoUrl");
                            ScreenManager.instance.ChangeScreenFade("HomeScreen");
                        }
                    }));
            }
        }));
    }
    
}
