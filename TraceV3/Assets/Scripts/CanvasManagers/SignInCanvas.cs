using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SignInCanvas : MonoBehaviour
{
    [Header("Canvas Components")]
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private bool signInFromQueue;
    private void OnEnable()
    {
        Debug.Log("Sign In Canvas Enabled");
    }

    public void LoginButtonHit()
    {
        if (signInFromQueue)
        {
            FbManager.instance.Logout(FbManager.LoginStatus.LoggedOut);
        }
        
        Debug.Log("Login Button Hit!");
        StartCoroutine(FbManager.instance.Login(username.text, password.text, (myReturnValue) => {
            if (myReturnValue.callbackEnum == CallbackEnum.SUCCESS)
            {
                FbManager.instance.SetUserLoginSatus(true);
                if (PlayerPrefs.GetInt("IsInvited") == 0)
                {
                    StartCoroutine(FbManager.instance.ManagerUserPermissions(isUserAllowedInApp =>
                    {
                        if (isUserAllowedInApp == true)
                        {
                            Debug.Log("user is allowed");
                            ScreenManager.instance.ChangeScreenNoAnim("HomeScreen");

                        }
                        else
                        {
                            Debug.Log("user is not allowed yet");
                            ScreenManager.instance.ChangeScreenNoAnim("UserInQue");
                        }
                    }));
                }
                else
                {
                    ScreenManager.instance.ChangeScreenNoAnim("HomeScreen");
                }
            }
            else
            {
                // if (myReturnValue.callbackEnum == CallbackEnum.CONNECTIONERROR) //todo: make this work
                // {
                //     ScreenManager.instance.ChangeScreenForwards("ConnectionError");
                //     return;
                // }
                ShowMessage("Your Email or Password is incorrect!");
            }
            
        }));
    }


    private void ShowMessage(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
            
        StartCoroutine(HelperMethods.TimedActionFunction(3f, ()=>
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }));
    }
}
