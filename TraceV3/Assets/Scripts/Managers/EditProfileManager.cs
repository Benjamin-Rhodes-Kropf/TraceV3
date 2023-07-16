using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditProfileManager : MonoBehaviour
{
    [SerializeField]private TMP_Text[] displayName, userName;
    [SerializeField]private TMP_Text emailId;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;


    public RawImage profileImage;
    

    private void OnEnable()
    {
        if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone7_8)_verticalLayoutGroup.spacing = -45;
        if (ScreenSizeManager.instance.currentModel == iPhoneModel.iPhone7Plus_8Plus)_verticalLayoutGroup.spacing = -45;
    }

    void Start()
    {
        FbManager.instance.thisUserModel.ProfilePicture((sprite =>
        {
            profileImage.texture = sprite.texture;
        }));
        foreach (var item in displayName)
        {
            item.text = FbManager.instance.thisUserModel.name;
        }
        foreach (var item in userName)
        {
            item.text = FbManager.instance.thisUserModel.Username;
        }
        emailId.text = FbManager.instance.thisUserModel.Email;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
