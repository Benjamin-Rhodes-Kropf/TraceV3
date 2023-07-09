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
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read,NativeGallery.MediaType.Image);
        if (permission== NativeGallery.Permission.Denied)
            NativeGallery.OpenSettings();
        if (permission == NativeGallery.Permission.ShouldAsk)
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read,NativeGallery.MediaType.Image);
        if (permission == NativeGallery.Permission.Granted)
            NativeGallery.GetImageFromGallery(callback);
    }
}
