using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersistentStorageHandler : MonoBehaviour
{
    public static PersistentStorageHandler s_Instance;

    private void Awake()
    {
        Debug.Log("persistentDataPath:" + Application.persistentDataPath);
        s_Instance = this;
    }


    public void SaveTextureToPersistentStorage(Texture2D texture,string folderName,string userId)
    {
        var friendsFolderPath = Path.Combine(Application.persistentDataPath, folderName);

        if (!Directory.Exists(friendsFolderPath))
            Directory.CreateDirectory(friendsFolderPath);

        var textureData = texture.EncodeToPNG();
        var filePath = Path.Combine(friendsFolderPath, userId+".png");
        File.WriteAllBytes(filePath, textureData);
    }



    public (bool updateImage,Texture2D texture) GetTextureFromPersistentStorage(string folderName, string userId)
    {
        var friendsFolderPath = Path.Combine(Application.persistentDataPath, folderName);
        string filePath = Path.Combine(friendsFolderPath, userId+".png");

        if (File.Exists(filePath))
        {
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);
            TimeSpan timeSinceLastWrite = DateTime.Now - lastWriteTime;
            if (timeSinceLastWrite.TotalDays > 7)
            {
                return (true, null);
            }
            
            byte[] textureData = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(128, 128,TextureFormat.RGB24,false); // Provide initial dimensions, it will be overridden by LoadImage.
            loadedTexture.LoadImage(textureData);
            return (false,loadedTexture);
        }
        return (true, null);
    }



    public Texture2D CropImage(Texture2D newTexture)
    {
        int originalWidth = newTexture.width;
        int originalHeight = newTexture.height;

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
                Color pixel = newTexture.GetPixel(startX + x, startY + y);
                croppedTexture.SetPixel(x, y, pixel);
            }
        }
        // Apply changes to the cropped texture
        croppedTexture.Apply();
        Destroy(newTexture);
        
        Texture2D reducedTexture = new Texture2D(128, 128, croppedTexture.format, false);

        // Copy the content of the original texture to the new one with resizing
        RenderTexture rt = new RenderTexture(128, 128, 24);
        Graphics.Blit(croppedTexture, rt);
        RenderTexture.active = rt;
        reducedTexture.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        reducedTexture.Apply();
        RenderTexture.active = null;
        rt.Release();
        Destroy(croppedTexture);
        Destroy(rt);
        return reducedTexture;
    }
}
