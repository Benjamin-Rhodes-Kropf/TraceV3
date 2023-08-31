using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioView : MonoBehaviour
{
    [Header("Contents")]
    [SerializeField] private TMP_Text _displayNameAndTime;
    [SerializeField] private string traceID;
    [SerializeField] private string audioID;
    [SerializeField] private int[] wave;
    [SerializeField] private List<Sprite> waveOptions;
    [SerializeField] private List<GameObject> audioWaves;
    [SerializeField] private Slider _slider;
    public CommentAudioManager CommentAudioManager;
    public string location;
    
    

    [Header("Refrences")] [SerializeField] 
    private GameObject singleWave;

    public void UpdateDisplayedData(string traceID, string audioID, string displayName, string time, float[] wave)
    {
        this.traceID = traceID;
        this.audioID = audioID;
        this._displayNameAndTime.text = displayName + "|" + HelperMethods.ReformatDate(time);
        if (wave.Length > 1)
        {
            this.wave = MapFloatArrayToInt(wave, 1,10);
            int counter = 0;
            foreach (var gameObject in audioWaves)
            {
                if(counter >= this.wave.Length)
                    return;
                
                gameObject.GetComponent<Image>().sprite = waveOptions[this.wave[counter]];
                counter++;
            }
        }
    }
    
    public static int[] MapFloatArrayToInt(float[] inputArray, int targetMinValue, int targetMaxValue)
    {
        int[] mappedArray = new int[inputArray.Length];

        float inputRange = inputArray.Max() - inputArray.Min();
        int targetRange = targetMaxValue - targetMinValue;

        for (int i = 0; i < inputArray.Length; i++)
        {
            float normalizedValue = (inputArray[i] - inputArray.Min()) / inputRange;
            int mappedValue = Mathf.RoundToInt(normalizedValue * targetRange) + targetMinValue;
            mappedArray[i] = Mathf.Clamp(mappedValue, targetMinValue, targetMaxValue);
        }

        return mappedArray;
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
        CommentAudioManager.PlayAudio(location, _slider);
    }
}
