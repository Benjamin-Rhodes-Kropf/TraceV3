using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class UserModel
{
    public string objectID;
    public string userId;
    public string Email;
    public string name;
    public string Username;
    public string Phone;
    public string PhotoURL;
    public string Password;

    public string ID => string.IsNullOrEmpty(userId) ? objectID : userId;

    private Sprite profilePicture = null;
    public void ProfilePicture(Action<Sprite> callback)
    {
        if (profilePicture == null)
        {
            DownloadProfilePicture((sprite =>
            {
                profilePicture = sprite;
                callback(profilePicture);
            }));
        }
        else
        {
            callback(profilePicture);
        }
    }

    public void DownloadProfilePicture(Action<Sprite> callback)
    {
        FbManager.instance.GetProfilePhotoFromFirebaseStorage(userId, (tex) =>
        {
           profilePicture = Sprite.Create(ChangeTextureType(tex), new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1.0f);;
           callback(profilePicture);
        }, (message) =>
        {
            Debug.Log(message);
        });
    }
    
    private Texture2D ChangeTextureType(Texture texture)
    {
        return Texture2D.CreateExternalTexture(
            texture.width,
            texture.height,
            TextureFormat.RGB24,
            false, false,
            texture.GetNativeTexturePtr());
    }
    

    public UserModel()
    {
        
    }
    
    public UserModel(string userId, string email, string name, string username, string phone, string photoURL, string password = "")
    {
        this.userId = userId;
        this.Email = email;
        this.name = name;
        this.Username = username;
        this.Phone = phone;
        this.PhotoURL = photoURL;
        this.Password = password;
    }
}
