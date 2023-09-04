using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//makes the gps emulator more realistic
public class GPSEmulatorRandomMovmentForSim : MonoBehaviour
{
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;
    void Start()
    {
        #if UNITY_EDITOR
        StartCoroutine(RandomlyUpdatePosition());
        #endif
    }

    IEnumerator RandomlyUpdatePosition()
    {
        yield return new WaitForSeconds(4f);
        //Debug.Log("randomly updating emulator position");
        _onlineMapsLocationService.emulatorPosition = _onlineMapsLocationService.emulatorPosition + new Vector2(Random.Range(-0.00001f,0.00001f), Random.Range(-0.00001f,0.00001f));
        StartCoroutine(RandomlyUpdatePosition());
    }
}
