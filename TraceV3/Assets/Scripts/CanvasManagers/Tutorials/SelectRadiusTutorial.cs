using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectRadiusTutorial : MonoBehaviour
{
    [Header("Tutorial Screens")] 
    [SerializeField] private GameObject _selectRadius;
    [SerializeField] private GameObject _HitSend;

    public void OnEnable()
    {
        HideTutorial();
        StartCoroutine(UpdateTutorialDisplay());
    }

    #region Tutorial //todo: move to a new Script
    IEnumerator UpdateTutorialDisplay()
    {
        yield return new WaitForSeconds(1f);
        UpdateTutorial();
    }
    private void UpdateTutorial()
    {
        HideTutorial();
        Debug.Log("Tutorial Number:" + PlayerPrefs.GetInt("ShowTutorial"));
        switch (PlayerPrefs.GetInt("ShowTutorial"))
        {
            case 4:
                _selectRadius.SetActive(true);
                Debug.Log("Show Radius");
                break;
            case 5:
                _HitSend.SetActive(true);
                Debug.Log("Show Send");
                break;
        }
    }
    private void HideTutorial()
    {
        _selectRadius.SetActive(false);
        _HitSend.SetActive(false);
    }
    
    public void TutorialSelectRadiusMoved() //called when cam button pressed
    {
        if (PlayerPrefs.GetInt("ShowTutorial") == 4)
        {
            Debug.Log("Tutorial Number: Update");
            PlayerPrefs.SetInt("ShowTutorial", 5);
            UpdateTutorial();
        }
    }
    #endregion
}
