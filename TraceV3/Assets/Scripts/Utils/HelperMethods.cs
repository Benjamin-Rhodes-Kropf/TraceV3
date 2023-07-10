using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using SA.iOS.Social;
using Object = UnityEngine.Object;

public static class HelperMethods
{
    public static Sprite LoadSprite(string path, string filename, string fallBackFile = "")
    {
        var texture2D = Resources.Load<Texture2D>(path + filename);
        if (texture2D != null)
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        else
        {       
            texture2D = Resources.Load<Texture2D>(path + fallBackFile);
            if (texture2D != null)
                return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            else
                return null;
        }
    }
    public static List<T> AppendList<T>(this List<T> sourceList, List<T> listToAppend)
    {
        foreach (var item in listToAppend)
        {
            sourceList.Add(item);
        }

        return sourceList;
    }
    public static void MeshCulling(GameObject parent, int value)
    {
        Renderer[] renderComponents = parent.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderComponents.Length; i++)
            renderComponents[i].material.SetInt("_Cull", value);
    }
    public static void SetLayer(GameObject parent, int layer, bool includeChildrens = true)
    {
        // Setting the Layer for Proper Lighting
        parent.layer = layer;
        Transform[] layerTransforms = parent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < layerTransforms.Length; i++)
            layerTransforms[i].gameObject.layer = layer;
    }
    public static void SendSMS(string phoneNumber, string contents)
    {
        ISN_TextMessage.Send(contents, phoneNumber);
    }
    public static string ConvertActualString(string input)
    {

        Debug.LogError("ConvertActualString " + input);
        int character = input.Last();
        while (character < '!' || character > '~')
        {
            input = input.Remove(input.Count() - 1);
            character = input.Last();
        }


        return input;
    }
    public static IEnumerator DownloadImage(string url, Action<UnityWebRequest> OnSuccess, Action<string> OnFail)
    {
        Uri uriResult;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!result)
        {
            if (OnFail != null)
                OnFail.Invoke("Invalid Url");
        }
        else
        {
            //Debug.Log("Download Start url : " + url);
            UnityWebRequest m_Request = UnityWebRequestTexture.GetTexture(url);
#if UNITY_WEBGL
            m_Request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            m_Request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            m_Request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            m_Request.SetRequestHeader("Access-Control-Allow-Origin", "*");
#endif
            yield return m_Request.SendWebRequest();

            if (m_Request.isNetworkError || m_Request.isHttpError)
            {
                Debug.Log("Download Error : " + m_Request.error + " url : " + url);
                if (OnFail != null)
                    OnFail.Invoke(m_Request.error);
            }
            else
            {
                //Debug.Log("Download Successfull url : " + url);
                if (OnSuccess != null)
                    OnSuccess.Invoke(m_Request);
            }
        }
    }
    public static IEnumerator DelayedCall(float delay, Action OnSuccess)
    {
        yield return new WaitForSeconds(delay);
        if (OnSuccess != null)
            OnSuccess.Invoke();
    }
    public static bool isBadName(string name)
    {
        string userEnteredName = name.ToLower();
        List<string> badNames = new List<string> ();
        foreach (string badName in badNames)
        {
            if (badName.Contains(userEnteredName))
            {
                return true;
            }
            else if (userEnteredName.Contains(badName))
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsEmailValid(string emailId)
    {
        Regex mailValidator = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$");
        string stringToReturn = null;

        bool returnValue = false;

        if (string.IsNullOrEmpty(emailId))
        {
            stringToReturn = "Field cannot be empty";
            returnValue = false;
        }
        else if (mailValidator.IsMatch(emailId))
        {
            stringToReturn = null;
            returnValue = true;
        }
        else
        {
            stringToReturn = "Invalid Email Format";
            returnValue = false;
        }

        //return stringToReturn;
        return returnValue;
    }
    public static string IsValidPassword(string password)
    {
        Regex passwordValidator = new Regex(@"^(?=.*[a-z])(?=.*\d).{6,}$");
        string stringToReturn = null;

        if (string.IsNullOrEmpty(password))
        {
            stringToReturn = "Password cannot be empty";
        }
        else if (password.Length >= 6)
        {
            stringToReturn = null;
        
            if (!passwordValidator.IsMatch(password))
            {
                stringToReturn = "Invalid Password: Must include at least 1 lowercase letter and 1 digit";
            }
        }
        else
        {
            stringToReturn = "Length minimum 6 characters";
        }

        return stringToReturn;
    }
    public static string GetKeyFromQuery(string url)
    {
        string dataToReturn = null;

        Debug.Log("GetKeyFromQuery : URL : " + url);
        UriBuilder uri = new UriBuilder(url);
        Debug.Log("GetKeyFromQuery : Query : " + uri.Query);

        var data = url.Split('?');

        if (data == null || data.Length <= 0)
            dataToReturn = null;

        else if (data.Length > 1)
        {
            Debug.Log("GetKeyFromQuery : data : " + data[1]);
            
            var queryDataSplit = data[1].Split('=');

            if (queryDataSplit == null || queryDataSplit.Length <= 0)
                dataToReturn = null;
            else
                dataToReturn = queryDataSplit[0];
        }

        return dataToReturn;
    }
    public static List<string> GetEnumList<T>()
    {
        List<string> _list = new List<string>();
        
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            _list.Add(value.ToString());
        }

        return _list;
    }
    public static IEnumerator TimedActionFunction(float timer, Action callback)
    {
        yield return new WaitForSeconds(timer);
        callback?.Invoke();
    }
    public static IEnumerator LerpScroll(ScrollRect _scrollRect,float target, float overTime)
	{
		float startTime = Time.time;
		while (Time.time < startTime + overTime)
		{
			_scrollRect.verticalNormalizedPosition = Mathf.Lerp(_scrollRect.verticalNormalizedPosition, target, (Time.time - startTime) / overTime);
			yield return null;
		}
		_scrollRect.verticalNormalizedPosition = target;
	}
    public static void PlayHeptics()
    {
        MMVibrationManager.Haptic(HapticTypes.LightImpact);
    }
    public static void OpenLink(string _Link)
    {
#if UNITY_WEBGL
        OpenWindow(_Link);
#else
        Application.OpenURL(_Link);
#endif
    }
    
    
    
    public static Texture2D RotateTextureClockwise(Texture2D inputTexture)
    {
        int width = inputTexture.width;
        int height = inputTexture.height;

        // Create a new texture with rotated dimensions
        var rotatedTexture = new Texture2D(height, width);

        // Rotate the pixels
        Color[] inputPixels = inputTexture.GetPixels();
        Color[] rotatedPixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                rotatedPixels[(width - x - 1) * height + y] = inputPixels[y * width + x];
            }
        }

        // Set the rotated pixels to the new texture
        rotatedTexture.SetPixels(rotatedPixels);
        rotatedTexture.Apply();

        // Display the rotated texture (optional)
        return(rotatedTexture);
    }


    [DllImport("__Internal")]
    private static extern int _ConvertHEICToPNG(string heicFilePath, string pngFilePath);
    
    // public static Texture2D PrepareProfilePhoto(string path)
    // {
    //     bool isHeic = false;
    //     string outputFilePath = "";
    //
    //     // Check if the image is in HEIC format
    //     if (Path.GetExtension(path).Equals(".heic", System.StringComparison.OrdinalIgnoreCase))
    //     {
    //         isHeic = true;
    //        
    //         try
    //         {
    //             Debug.Log("Deleted Contents at:" + outputFilePath);
    //             File.Delete(outputFilePath);
    //             Debug.Log("Deleted");
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.Log("FAILED: failed to delelte");
    //         }
    //
    //         Debug.Log("Converting HEIC To PNG");
    //         try
    //         {
    //             Debug.Log("Trying To Convert");
    //             outputFilePath = Application.persistentDataPath + "/output.png";
    //             Debug.Log("ImageConvert: Convert Heic To Png");
    //             Debug.Log("DataPath:" + outputFilePath);
    //             
    //             _ConvertHEICToPNG(path, outputFilePath);
    //             Debug.Log("Succsefully Converted");
    //             Debug.Log("Path Extension: "+ Path.GetExtension(outputFilePath));
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.Log("FAILED: failed to _ConvertHEICToPNG");
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("ImageConvert: Already PNG");
    //         outputFilePath = path; //set output file to input
    //         Debug.Log("FileType:" + Path.GetExtension(outputFilePath));
    //     }
    //
    //     Debug.Log("ReadingALLBytes");
    //
    //     Texture2D texture = new Texture2D(2, 2);
    //     byte[] imageData = System.IO.File.ReadAllBytes(outputFilePath);
    //     // Load the image data into the texture
    //     texture.LoadImage(imageData);
    //
    //     Debug.Log("Crop");
    //     
    //     //Crop
    //     int originalWidth = texture.width;
    //     int originalHeight = texture.height;
    //
    //     int size = Mathf.Min(originalWidth, originalHeight);
    //
    //     // Calculate the starting position for cropping
    //     int startX = (originalWidth - size) / 2;
    //     int startY = (originalHeight - size) / 2;
    //
    //     // Create a new Texture2D to store the cropped image
    //     Texture2D croppedTexture = new Texture2D(size, size);
    //
    //     // Copy the pixels from the original texture to the cropped texture
    //     for (int y = 0; y < size; y++)
    //     {
    //         for (int x = 0; x < size; x++)
    //         {
    //             Color pixel = texture.GetPixel(startX + x, startY + y);
    //             croppedTexture.SetPixel(x, y, pixel);
    //         }
    //     }
    //
    //     // Apply changes to the cropped texture
    //     croppedTexture.Apply();
    //     Debug.Log("Image Cropped");
    //     
    //     if (isHeic)
    //     {
    //         Debug.Log("Returning Cropped and Rotated Texture");
    //         return HelperMethods.RotateTextureClockwise(croppedTexture);
    //     }
    //     else
    //     {
    //         Debug.Log("Returning Cropped Texture");
    //         return croppedTexture;
    //     }
    // }
    
    public static Texture2D PrepareProfilePhoto(string path)
{
    bool isHeic = false;
    string outputFilePath = "";

    // Check if the image is in HEIC format
    if (Path.GetExtension(path).Equals(".heic", System.StringComparison.OrdinalIgnoreCase))
    {
        isHeic = true;

        // Delete previous output file
        outputFilePath = Application.persistentDataPath + "/output.png";
        if (File.Exists(outputFilePath))
        {
            try
            {
                File.Delete(outputFilePath);
                Debug.Log("Deleted previous output file");
            }
            catch (Exception e)
            {
                Debug.Log("Failed to delete previous output file: " + e.Message);
            }
        }

        Debug.Log("Converting HEIC To PNG");
        try
        {
            _ConvertHEICToPNG(path, outputFilePath);
            Debug.Log("Successfully converted HEIC to PNG");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to convert HEIC to PNG: " + e.Message);
        }
    }
    else
    {
        Debug.Log("ImageConvert: Already PNG");
        outputFilePath = path; // Set output file to input
        Debug.Log("FileType: " + Path.GetExtension(outputFilePath));
    }

    Debug.Log("Reading ALL Bytes");

    Texture2D texture = new Texture2D(2, 2);
    if (File.Exists(outputFilePath))
    {
        byte[] imageData = System.IO.File.ReadAllBytes(outputFilePath);
        // Load the image data into the texture
        texture.LoadImage(imageData);
    }
    else
    {
        Debug.LogError("Image file does not exist: " + outputFilePath);
    }

    Debug.Log("Crop");

    // Crop
    int originalWidth = texture.width;
    int originalHeight = texture.height;

    int size = Mathf.Min(originalWidth, originalHeight);

    // Calculate the starting position for cropping
    int startX = (originalWidth - size) / 2;
    int startY = (originalHeight - size) / 2;

    // Create a new Texture2D to store the cropped image
    Texture2D croppedTexture = new Texture2D(size, size);

    // Copy the pixels from the original texture to the cropped texture
    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            Color pixel = texture.GetPixel(startX + x, startY + y);
            croppedTexture.SetPixel(x, y, pixel);
        }
    }

    // Apply changes to the cropped texture
    croppedTexture.Apply();
    Debug.Log("Image Cropped");

    if (isHeic)
    {
        Debug.Log("Returning Cropped and Rotated Texture");
        return HelperMethods.RotateTextureClockwise(croppedTexture);
    }
    else
    {
        Debug.Log("Returning Cropped Texture");
        return croppedTexture;
    }
}
    
    public static string ReformatRecipients(int recipients)
    {
        if (recipients == 0)
        {
            return "";
        }
        if (recipients == 1)
        {
            return " to 1 person";
        }
        else
        {
            return " to " + recipients + " people";
        }
    }


    public static string ReformatDate(string currentDate)
    {
        Debug.Log("Parsing:" + currentDate);
        DateTime currentDateTime = DateTime.Parse(currentDate);
        DateTime otherDateTime = DateTime.Parse(DateTime.UtcNow.ToString());
        TimeSpan timeDifference = otherDateTime - currentDateTime;

        if (currentDateTime.Year >= otherDateTime.Year)
        {
            if (timeDifference.Days == 1)
            {
                return Math.Round(timeDifference.TotalDays) + " day ago";
            }
            if (timeDifference.Days < 14)
            {
                return Math.Round(timeDifference.TotalDays) + " days ago";
            }
            if (Math.Round(timeDifference.TotalHours) == 1)
            {
                return Math.Round(timeDifference.TotalHours) + " hour ago";
            }
            if (timeDifference.TotalHours > 1)
            {
                return Math.Round(timeDifference.TotalHours) + " hours ago";
            }
            if (Math.Round(timeDifference.TotalMinutes) == 1)
            {
                return Math.Round(timeDifference.TotalMinutes) + " minute ago";
            }
            return Math.Round(timeDifference.TotalMinutes) + " minutes ago";
        }
        return currentDateTime.ToShortDateString();
    }


#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void OpenWindow(string url);
#endif
}
