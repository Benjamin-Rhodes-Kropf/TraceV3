using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUpAccount : MonoBehaviour
{
    private void OnEnable()
    {
        if(PlayerPrefs.GetInt("IsInQueue") == 1)
            StartCoroutine(WaitThenGoToQueScreen());
        if(PlayerPrefs.GetInt("IsInQueue") == 0)
            StartCoroutine(WaitThenGoToHomeScreen());
    }
    IEnumerator WaitThenGoToHomeScreen()
    {
        yield return new WaitForSeconds(2f);
        ScreenManager.instance.ChangeScreenFade("HomeScreen");

    }
    IEnumerator WaitThenGoToQueScreen()
    {
        yield return new WaitForSeconds(2f);
        ScreenManager.instance.ChangeScreenFade("UserInQue");

    }
}
