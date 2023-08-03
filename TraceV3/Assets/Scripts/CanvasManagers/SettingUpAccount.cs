using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUpAccount : MonoBehaviour
{
    private void OnEnable()
    {
        if(PlayerPrefs.GetInt("IsInvited") == 0)
            StartCoroutine(WaitThenGoToQueScreen());
        if(PlayerPrefs.GetInt("IsInvited") == 1)
            StartCoroutine(WaitThenGoToHomeScreen());
    }
    IEnumerator WaitThenGoToHomeScreen()
    {
        yield return new WaitForSeconds(2f);
        PlayerPrefs.SetInt("ShowTutorial", 1); //reset tutorial for new user
        PlayerPrefs.SetInt("FirstTimeOnContacts", 1); //reset tutorial for new user
        ScreenManager.instance.ChangeScreenFade("HomeScreen");
    }
    IEnumerator WaitThenGoToQueScreen()
    {
        yield return new WaitForSeconds(2f);
        ScreenManager.instance.ChangeScreenFade("UserInQue");
    }
}
