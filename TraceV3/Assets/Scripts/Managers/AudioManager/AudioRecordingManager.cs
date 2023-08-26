using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AudioRecordingManager : MonoBehaviour
{
    private bool isRecording = false;
    private AudioSource audioSource;
    private List<float> currentRecording = new List<float>();
    private int startRecordingTime;

    [Header("External")] 
    [SerializeField] private OpenTraceManager openTraceManager;
    
    [Header("UI")]
    public GameObject doneOptions;
    public GameObject recordAudio;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    void OnEnable()
    {
        recordAudio.SetActive(true);
        doneOptions.SetActive(false);
    }

    public void StartRecording()
    {
        recordAudio.SetActive(true);
        doneOptions.SetActive(false);
        openTraceManager.MuteVideoAudio();
        isRecording = true;
        currentRecording.Clear();
        
        Microphone.End(null); // Ensure the microphone is turned off
        audioSource.clip = Microphone.Start(null, false, 300, 44100); // Max recording time set to 5 minutes (300 seconds) for safety
        startRecordingTime = Microphone.GetPosition(null); // Store the start position for accurate clip length later
    }

    public void StopRecording()
    {
        if (!isRecording)
            return;

        isRecording = false;
    
        int endRecordingTime = Microphone.GetPosition(null);
        int length = endRecordingTime - startRecordingTime;
        float[] clipData = new float[length];
        
        audioSource.clip.GetData(clipData, startRecordingTime);
        currentRecording.AddRange(clipData);

        Microphone.End(null);
        
        float[] fullClip = currentRecording.ToArray();
        audioSource.clip = AudioClip.Create("RecordedClip", fullClip.Length, 1, 44100, false);
        audioSource.clip.SetData(fullClip, 0);
        
        // After stopping the recording, export the clip data to a WAV file
        ExportClipData(audioSource.clip);
    }

    public void PlayRecording()
    {
        if (!isRecording)
        {
            recordAudio.SetActive(false);
            doneOptions.SetActive(true);
            StartCoroutine(LoadAndPlayWav()); //this plays it from persistant data
        }
    }

    public void StopPlayingRecording()
    {
        if (!isRecording)
        {
            recordAudio.SetActive(true);
            doneOptions.SetActive(false);
            audioSource.Stop();
            Destroy(audioSource.clip);
        }
    }
    
    void ExportClipData(AudioClip clip)
    {
        var data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);
        
        var path = Path.Combine(Application.persistentDataPath, "Recording.wav");
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        {
            // The following values are based on http://soundfile.sapp.org/doc/WaveFormat/
            var bitsPerSample = (ushort)16;
            var chunkID = "RIFF";
            var format = "WAVE";
            var subChunk1ID = "fmt ";
            var subChunk1Size = (uint)16;
            var audioFormat = (ushort)1;
            var numChannels = (ushort)clip.channels;
            var sampleRate = (uint)clip.frequency;
            var byteRate = (uint)(sampleRate * clip.channels * bitsPerSample / 8);  // SampleRate * NumChannels * BitsPerSample/8
            var blockAlign = (ushort)(numChannels * bitsPerSample / 8); // NumChannels * BitsPerSample/8
            var subChunk2ID = "data";
            var subChunk2Size = (uint)(data.Length * clip.channels * bitsPerSample / 8); // NumSamples * NumChannels * BitsPerSample/8
            var chunkSize = (uint)(36 + subChunk2Size); // 36 + SubChunk2Size
            // Start writing the file.
            WriteString(stream, chunkID);
            WriteInteger(stream, chunkSize);
            WriteString(stream, format);
            WriteString(stream, subChunk1ID);
            WriteInteger(stream, subChunk1Size);
            WriteShort(stream, audioFormat);
            WriteShort(stream, numChannels);
            WriteInteger(stream, sampleRate);
            WriteInteger(stream, byteRate);
            WriteShort(stream, blockAlign);
            WriteShort(stream, bitsPerSample);
            WriteString(stream, subChunk2ID);
            WriteInteger(stream, subChunk2Size);
            foreach (var sample in data)
            {
                // De-normalize the samples to 16 bits.
                var deNormalizedSample = (short)0;
                if (sample > 0)
                {
                    var temp = sample * short.MaxValue;
                    if (temp > short.MaxValue)
                        temp = short.MaxValue;
                    deNormalizedSample = (short)temp;
                }
                if (sample < 0)
                {
                    var temp = sample * (-short.MinValue);
                    if (temp < short.MinValue)
                        temp = short.MinValue;
                    deNormalizedSample = (short)temp;
                }
                WriteShort(stream, (ushort)deNormalizedSample);
            }
        }
    }

    public void UploadAudioClipToDatabase()
    {
        
    } 

    public void CancelSendingAudioClip()
    {
        audioSource.Stop();
        Destroy(audioSource.clip);
        recordAudio.SetActive(true);
        doneOptions.SetActive(false);
    } 

    void WriteString(Stream stream, string value)
    {
        foreach (var character in value)
            stream.WriteByte((byte)character);
    }

    void WriteInteger(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 24) & 0xFF));
    }
    void WriteShort(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }
    
    IEnumerator LoadAndPlayWav(string fileName = "Recording.wav")
    {
        string path = Path.Combine(Application.persistentDataPath, fileName).Replace("\\", "/");
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        path = "file:///" + path;
        #else
        path = "file://" + path;
        #endif
        
        Debug.Log("Generated path: " + path);


        using (var request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading WAV: " + request.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}