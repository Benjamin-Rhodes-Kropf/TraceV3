using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class NativeMethodsManager
{
    public static string finalPath;
    public static void OpenGalleryToPickMedia(NativeGallery.MediaPickCallback callback)
    {

        // NativeGallery.GetImageFromGallery((path) =>
        // {
        //     Debug.Log("Image path: " + path);
        //     if (IsHEICFile(path))
        //     {
        //         Debug.Log("Load Image From Path: Image is HEIC");
        //     }
        //     if (path != null)
        //     {
        //         // Create Texture from selected image
        //         Texture2D
        //             texture = NativeGallery.LoadImageAtPath(path,
        //                 1024); // image will be downscaled if its width or height is larger than 1024px
        //         if (texture == null)
        //         {
        //             Debug.Log("Couldn't load texture from " + path);
        //             return;
        //         }
        //
        //         NativeGallery.GetImageFromGallery(callback);
        //         // Use 'texture' here
        //         // ...
        //     }
        // });

        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read,NativeGallery.MediaType.Image);
        if (permission== NativeGallery.Permission.Denied)
            NativeGallery.OpenSettings();
        if (permission == NativeGallery.Permission.ShouldAsk)
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read,NativeGallery.MediaType.Image);
        if (permission == NativeGallery.Permission.Granted)
            NativeGallery.GetImageFromGallery(callback);
    }
    
    private static bool IsHEICFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        if (Regex.IsMatch(extension, @"\.(heic|heif)$", RegexOptions.IgnoreCase))
        {
            return true;
        }
        return false;
    }
}
