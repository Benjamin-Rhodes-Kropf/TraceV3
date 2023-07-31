using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScreenTutorial : MonoBehaviour
{
    [Header("Tutorial Screens")] 
    [SerializeField] private GameObject _tutorialWelomeScreen;
    [SerializeField] private GameObject _tutorialLetsLeaveScreen;
    [SerializeField] private GameObject _tutorialCongratsFinishScreen;
    [SerializeField] private GameObject _tutorialClickForMoreInfo;

    public void OnEnable()
    {
        HideTutorial();
        StartCoroutine(UpdateTutorialDisplay());
    }

    #region Tutorial //todo: move to a new Script
    IEnumerator UpdateTutorialDisplay()
    {
        yield return new WaitForSeconds(2f);
        UpdateTutorial();
    }
    private void UpdateTutorial()
    {
        HideTutorial();
        switch (PlayerPrefs.GetInt("ShowTutorial"))
        {
            case 1:
                _tutorialWelomeScreen.SetActive(true);
                break;
            case 2:
                _tutorialLetsLeaveScreen.SetActive(true);
                break;
            case 6:
                _tutorialCongratsFinishScreen.SetActive(true);
                break;
            case 7:
                _tutorialClickForMoreInfo.SetActive(true);
                break;
            case 8:
                HideTutorial();
                break;
        }
    }
    private void HideTutorial()
    {
        _tutorialWelomeScreen.SetActive(false);
        _tutorialLetsLeaveScreen.SetActive(false);
        _tutorialCongratsFinishScreen.SetActive(false);
        _tutorialClickForMoreInfo.SetActive(false);
    }
    public void WelcomeScreenTutorialPressed()
    {
        PlayerPrefs.SetInt("ShowTutorial", 2);
        UpdateTutorial();
    }
    public void TutorialCameraButtonPressed() //called when cam button pressed
    {
        if (PlayerPrefs.GetInt("ShowTutorial") == 2)
        {
            PlayerPrefs.SetInt("ShowTutorial", 3);
            UpdateTutorial();
        }
    }

    public void CongratsScreenPressed()
    {
        Debug.Log("Congrats Screen Pressed");
        PlayerPrefs.SetInt("ShowTutorial", 7);
        UpdateTutorial();
    }

    public void ClickForMoreInfoPressed()
    {
        Debug.Log("ClickForMoreInfoPressed Screen Pressed");
        PlayerPrefs.SetInt("ShowTutorial", 8);
        UpdateTutorial();
    }
    
    #endregion
}
