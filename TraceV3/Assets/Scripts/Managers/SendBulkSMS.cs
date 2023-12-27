using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
public class SendBulkSMS : MonoBehaviour
{
    // Singleton instance
    private static SendBulkSMS _instance;
    public static SendBulkSMS Instance => _instance;

    [Header("external")] 
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;
    
    [Header("Twilio")]
    public  string accountSid = "YOUR_TWILIO_ACCOUNT_SID";
    public string authToken = "YOUR_TWILIO_AUTH_TOKEN";
    public string fromNumber = "YOUR_TWILIO_PHONE_NUMBER"; // e.g. +1234567890
    public string mediaUrl = "MEDIA_URL_HERE";
    
    [Header("Trace")]
    public string baseUrl = "https://api.mapbox.com/styles/v1/mapbox/satellite-v9/static/";
    public string imageUrl = "url-https%3A%2F%2Fstatic.wixstatic.com%2Fmedia%2F8708ae_9d90308609c04aa8a7d18b08b3baf231~mv2.png%2Fv1%2Ffill%2Fw_421%2Ch_421%2Cal_c%2Clg_1%2Cq_85%2Cenc_auto%2FTrace%2520Circle%2520(1).png";
    public string accessToken = ""; //removed for security
    public float zoomLevel = 17.71f;
    public Vector2Int imageSize = new Vector2Int(1280, 1280);
    
    
    // Awake is called before Start
    void Awake()
    {
        // Ensure only one instance of MapboxGeocoding exists in the scene
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SendTraceSMS(List<string> toNumbers, Vector2 coordinates)
    {
        Debug.Log("SENDING SMS");
        //get name of location and send sms to phone numbers selected
        Debug.Log("SEND GOT LOCATION SMS");
        string message = "";
        if(MapboxGeocoding.Instance.locationName != null) 
            message = FbManager.instance.thisUserModel.name + " left you a Trace in " + MapboxGeocoding.Instance.userLocationName + "! To see it download 'Leave a Trace' on the Appstore.";
        else
            message = "Ben left you a Trace!";
        
        Debug.Log("sending message:" + message);
        StringBuilder sb = new StringBuilder();
        sb.Append(baseUrl);
        sb.Append(imageUrl);
        sb.AppendFormat("({0},{1})/{2},{3},{4},0/{5}x{6}?access_token={7}", coordinates.x, coordinates.y, coordinates.x, coordinates.y, zoomLevel, imageSize.x, imageSize.y, accessToken);
        Debug.Log("starting send corutine");
        StartCoroutine(SendSMS(toNumbers, message, sb.ToString()));
    }
    
    public IEnumerator SendSMS(List<string> toNumbers, string message, string mediaUrl)
    {
        Debug.Log("SEND STARTED CORUTINE GOT LOCATION SMS");
        string url = "https://api.twilio.com/2010-04-01/Accounts/" + accountSid + "/Messages.json";
        foreach (string toNumber in toNumbers)
        {
            WWWForm form = new WWWForm();
            form.AddField("From", fromNumber);
            form.AddField("To", toNumber);
            form.AddField("Body", message);
            if (!string.IsNullOrEmpty(mediaUrl))
            {
                form.AddField("MediaUrl", mediaUrl);
            }
            
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(accountSid + ":" + authToken)));

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + www.error);
            }
            else
            {
                Debug.Log("Success! Message sent to: " + toNumber);
            }
        }
    }


}

