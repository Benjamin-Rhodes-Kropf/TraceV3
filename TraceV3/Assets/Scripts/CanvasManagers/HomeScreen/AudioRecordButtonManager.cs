using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class AudioRecordButtonManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header(@"Settings"), Range(5f, 60f), Tooltip(@"Maximum duration that button can be pressed.")]
    public float maxDuration = 10f;

    [SerializeField] private CommentAudioManager _commentAudioManager;
    [Header(@"UI")] public Image countdown;

    [Header(@"Events")] 
    public UnityEvent onTouchDown;
    
    
    public UnityEvent onRecordTouchUp;
    private Image button;
    private bool touch;
    private bool isDragging;

    private void Awake()
    {
        button = GetComponent<Image>();
    }

    private void Start() => Reset();

    private void Reset()
    {
        button.fillAmount = 1.0f;
        countdown.fillAmount = 0.0f;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => ForceRec();
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => touch = false;
    
    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
    
    public void ForceRec()
    {
        if(FbManager.instance.IsFirebaseUserInitialised && FbManager.instance.IsFirebaseUserLoggedIn)
            StartCoroutine(ForceRecord());
        else
            NotificationManager.Instance.SendLocalNotification("Hold Up!", "Wait For Us to Get Connected!", 1);
    }

    public void Haptic()
    {
        HapticManager.instance.PlaySelectionHaptic();
    }

    public IEnumerator ForceRecord()
    {
        touch = true;
        // Start recording
        onTouchDown?.Invoke();
        // Animate the countdown
        var startTime = Time.time;
        while (touch)
        {
            var ratio = (Time.time - startTime) / maxDuration;
            touch = ratio <= 1f;
            countdown.fillAmount = ratio;
            button.fillAmount = 1f - ratio;
            yield return null;
        }
        Reset();
        SendTraceManager.instance.videoLength = Time.time - startTime; //keep track of vid length for analytics
        onRecordTouchUp?.Invoke();
    }

    void RecordAnimation()
    {
        
    }
    
    void StopRecordingAnimation()
    {
        
    }
}
