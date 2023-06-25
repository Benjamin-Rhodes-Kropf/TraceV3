using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Permissions
{
    public static void CheckAndroidPermissions()
    {
        // handling internet permissions on android platform
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.INTERNET"))
        {
            Debug.Log("No Internet permission");
            Permission.RequestUserPermission("android.permission.INTERNET");
        }

        Debug.Log("Has internet permission");
#endif
    }
}
