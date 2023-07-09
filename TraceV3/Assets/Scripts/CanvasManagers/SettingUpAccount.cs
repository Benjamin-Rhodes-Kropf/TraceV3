using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUpAccount : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(WaitThenChangeScreen());
    }

    IEnumerator WaitThenChangeScreen()
    {
        yield return new WaitForSeconds(2f);
        ScreenManager.instance.ChangeScreenFade("HomeScreen");

    }
}
