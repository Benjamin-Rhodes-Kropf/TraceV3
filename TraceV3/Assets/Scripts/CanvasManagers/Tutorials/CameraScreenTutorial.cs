using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScreenTutorial : MonoBehaviour
{
    [Header("Tutorial Screens")] 
    [SerializeField] private GameObject _tutorialCamera;

    public void OnEnable()
    {
        HideTutorial();
        StartCoroutine(UpdateTutorialDisplay());
    }

    #region Tutorial //todo: move to a new Script
    IEnumerator UpdateTutorialDisplay()
    {
        yield return new WaitForSeconds(3f);
        UpdateTutorial();
    }
    private void UpdateTutorial()
    {
        HideTutorial();
        switch (PlayerPrefs.GetInt("ShowTutorial"))
        {
            case 3:
                _tutorialCamera.SetActive(true);
                break;
        }
    }
    private void HideTutorial()
    {
        _tutorialCamera.SetActive(false);
    }
    
    public void TutorialCameraButtonPressed() //called when cam button pressed
    {
        if (PlayerPrefs.GetInt("ShowTutorial") == 2)
        {
            PlayerPrefs.SetInt("ShowTutorial", 3);
            UpdateTutorial();
        }
    }
    #endregion
}
