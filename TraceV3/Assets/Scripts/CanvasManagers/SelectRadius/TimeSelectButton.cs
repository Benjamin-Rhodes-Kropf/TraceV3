using System;
using System.Collections;
using TMPro;
using UnityEngine;




public class TimeSelectButton : MonoBehaviour
{
    [SerializeField] private SelectRadiusCanvas _selectRadiusCanvas;
    [SerializeField] private Animator _timeSelectAnimator;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private int selectedOption;
    
    public void StartChangeTimeAnim()
    {
        _timeSelectAnimator.Play("changeTime");
    }

    public void PlayHaptic()
    {
        HapticManager.instance.PlaySelectionHaptic();
    }

    public void ChangeTimeSelect() //is called during changeTime Anim
    {
        if (selectedOption < SendTraceManager.instance.TraceExpirationOptions.Count-1)
            selectedOption += 1;
        else
            selectedOption = 0;
        Debug.Log("Change Selected End Time:" + selectedOption);
        _selectRadiusCanvas.SetExpiration(DateTime.UtcNow.AddHours(SendTraceManager.instance.TraceExpirationOptions[selectedOption].hoursFromNow));
        displayText.text = SendTraceManager.instance.TraceExpirationOptions[selectedOption].text;
    }
}
