using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionErrorScreen : MonoBehaviour
{
    public void RetryConnectionError()
    {
        StartCoroutine(FbManager.instance.AutoLogin());
    }
}
