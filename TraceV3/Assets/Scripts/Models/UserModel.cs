using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class UserModel
{
    public string objectID; //todo: get rid of difference between object id and userID
    public string userID;
    public string email;
    public string name;
    public string username;
    public string phone;
    public string photo;
    public string password;
    public bool issuperuser;

    public string ID => string.IsNullOrEmpty(userID) ? objectID : userID; //todo: we dont use this anymore but the workaround is bad see below
    private Sprite profilePicture = null;
    public Coroutine _downloadPCoroutine;

    public void ProfilePicture(Action<Sprite> callback)
    {
        if (profilePicture == null)
        {
            DownloadProfilePicture((sprite =>
            {
                // profilePicture = sprite;
                callback(sprite);
            }));
        }
        else
        {
            callback(profilePicture);
        }
    }

    public void PPTexture(Action<Texture> callback)
    {
        DownloadTexture((callback));
    }
    public void DownloadTexture(Action<Texture> callback)
    {
        // //Todo: This is super dumb because we should not have seperate object id and user id but this is how algolia wants to query
        if (string.IsNullOrEmpty(userID))
        {
            FbManager.instance.GetProfilePhotoFromFirebaseStorage(objectID, callback, Debug.Log,ref _downloadPCoroutine);
        }
        else
        {
            FbManager.instance.GetProfilePhotoFromFirebaseStorage(userID, callback, Debug.Log, ref _downloadPCoroutine);
        }
    }

    public void DownloadProfilePicture(Action<Sprite> callback)
    {
        // //Todo: This is super dumb because we should not have seperate object id and user id but this is how algolia wants to query
        if (string.IsNullOrEmpty(userID))
        {
            FbManager.instance.GetProfilePhotoFromFirebaseStorage(objectID, (tex) =>
            {
                profilePicture = Sprite.Create(ChangeTextureType(tex), new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1.0f);;
                callback(profilePicture);
            }, (message) =>
            {
                Debug.Log(message);
            },ref _downloadPCoroutine);
        }
        else
        {
            FbManager.instance.GetProfilePhotoFromFirebaseStorage(userID, (tex) =>
            {
                profilePicture = Sprite.Create(ChangeTextureType(tex), new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1.0f);;
                callback(profilePicture);
            }, (message) =>
            {
                Debug.Log(message);
            }, ref _downloadPCoroutine);
        }
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
    
    public UserModel(string userId, string email, string name, string username, string phone, string photo, bool issuperuser, string password = "")
    {
        Debug.Log("Creating New User Model:" + userId + ", " + email + ", " + name + ", " + username + ", " + phone + ", " + photo);
        this.userID = userId;
        this.email = email;
        this.name = name;
        this.username = username;
        this.phone = phone;
        this.photo = photo;
        this.issuperuser = issuperuser;
        this.password = password;
    }
}
