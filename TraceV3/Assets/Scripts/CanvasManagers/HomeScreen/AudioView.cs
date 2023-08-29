using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioView : MonoBehaviour
{
    [Header("Contents")]
    [SerializeField] private TMP_Text _displayNameAndTime;
    [SerializeField] private string traceID;
    [SerializeField] private string audioID;
    [SerializeField] private List<float> audioSample;
    public CommentAudioManager CommentAudioManager;
    public string location;
    
    

    [Header("Refrences")] [SerializeField] 
    private GameObject singleWave;

    public void UpdateDisplayedData(string traceID, string audioID, string displayName, string time, List<float> audioSample)
    {
        this.traceID = traceID;
        this.audioID = audioID;
        this._displayNameAndTime.text = displayName + "|" + HelperMethods.ReformatDate(time);
        this.audioSample = audioSample;
    }

    public void GenerateAudioWaveDisplay()
    {
        
    }

    public void GetAudioRecording()
    {
        
    }

    public void PlayAudioRecording()
    {
        Debug.Log("Play Audio Recording:");
        CommentAudioManager.StopVideo();
        CommentAudioManager.PlayAudio(location);
    }
}
