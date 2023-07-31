using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScreenTutorial : MonoBehaviour
{
    [Header("Tutorial Screens")] 
    [SerializeField] private GameObject _tutorialWelomeScreen;
    [SerializeField] private GameObject _tutorialLetsLeaveScreen;
    [SerializeField] private GameObject _tutorialCongratsFinishScreen;
    public void OnEnable()
    {
        PlayerPrefs.SetInt("ShowTutorial", 1); //todo: remove this
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
            case 9:
                _tutorialCongratsFinishScreen.SetActive(true);
                break;
        }
    }
    private void HideTutorial()
    {
        _tutorialWelomeScreen.SetActive(false);
        _tutorialLetsLeaveScreen.SetActive(false);
        _tutorialCongratsFinishScreen.SetActive(false);
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
    #endregion
}
