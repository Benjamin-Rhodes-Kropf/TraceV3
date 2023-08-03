using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScreenTutorial : MonoBehaviour
{
    [Header("Tutorial Screens")] 
    [SerializeField] private GameObject _tutorialWelomeScreen;
    [SerializeField] private GameObject _tutorialLetsLeaveScreen;
    [SerializeField] private GameObject _tutorialInviteOrAddFriendsScreen;
    [SerializeField] private GameObject _tutorialEditProfileInfo;
    [SerializeField] private GameObject _tutorialCongratsFinishScreen;
    [SerializeField] private GameObject _tutorialClickForMoreInfo;

    public void OnEnable()
    {
        //PlayerPrefs.SetInt("ShowTutorial", 6);
        HideTutorial();
        if (PlayerPrefs.GetInt("ShowTutorial") == 1)
        {
            StartCoroutine(UpdateTutorialDisplay(2));
        }
        else
        {
            StartCoroutine(UpdateTutorialDisplay(0.25f));
        }
    }

    #region Tutorial //todo: move to a new Script
    IEnumerator UpdateTutorialDisplay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
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
                _tutorialInviteOrAddFriendsScreen.SetActive(true);
                break;
            case 7:
                _tutorialEditProfileInfo.SetActive(true);
                break;
            case 8:
                _tutorialCongratsFinishScreen.SetActive(true);
                break;
            case 9:
                _tutorialClickForMoreInfo.SetActive(true);
                break;
            case 10:
                HideTutorial();
                break;
        }
    }
    private void HideTutorial()
    {
        _tutorialWelomeScreen.SetActive(false);
        _tutorialLetsLeaveScreen.SetActive(false);
        _tutorialInviteOrAddFriendsScreen.SetActive(false);
        _tutorialEditProfileInfo.SetActive(false);
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
            HideTutorial();
        }
        UpdateTutorialDisplay(0.5f);
    }

    public void InviteOrAddFriendsPressed()
    {
        Debug.Log("InviteOrAddFriendsPressed Screen Pressed");
        if (PlayerPrefs.GetInt("ShowTutorial") == 6)
        {
            PlayerPrefs.SetInt("ShowTutorial", 7); 
            HideTutorial();
        }
        UpdateTutorialDisplay(0.4f);
    }
    public void EditProfilePressed()
    {
        Debug.Log("EditProfilePressed Screen Pressed");
        if (PlayerPrefs.GetInt("ShowTutorial") == 7)
        {
            PlayerPrefs.SetInt("ShowTutorial", 8); 
            UpdateTutorial();
        }
    }
    
    public void CongratsScreenPressed()
    {
        Debug.Log("Congrats Screen Pressed");
        PlayerPrefs.SetInt("ShowTutorial", 9);
        UpdateTutorial();
    }

    public void ClickForMoreInfoPressed()
    {
        Debug.Log("ClickForMoreInfoPressed Screen Pressed");
        if(PlayerPrefs.GetInt("ShowTutorial") == 9)
            PlayerPrefs.SetInt("ShowTutorial", 10);
        UpdateTutorial();
    }
    
    #endregion
}
