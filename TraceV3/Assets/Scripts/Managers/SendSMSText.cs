using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SendSMSText : MonoBehaviour
{
    private static SendSMSText _instance;
    public static SendSMSText Instance => _instance;
    
    [Header("New Stuff")]
    public string accountSid = "YOUR_ACCOUNT_SID";
    public string authToken = "YOUR_AUTH_TOKEN";
    public string twilioPhoneNumber = "YOUR_TWILIO_PHONE_NUMBER";
    public string recipientPhoneNumber = "RECIPIENT_PHONE_NUMBER";
    public string messageText = "Hello, this is a test message from Unity using Twilio!";
    private string twilioApiUrl = "https://api.twilio.com/2010-04-01/Accounts/";

    private void Start()
    {
        SendTextMessage();
    }

    
    public void SendTextMessage()
    {
        StartCoroutine(SendMessageCoroutine());
    }

    private IEnumerator SendMessageCoroutine()
    {
        string base64Credentials = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
        string twilioUrl = $"{twilioApiUrl}{accountSid}/Messages.json";

        WWWForm form = new WWWForm();
        form.AddField("From", twilioPhoneNumber);
        form.AddField("To", recipientPhoneNumber);
        form.AddField("Body", messageText);

        Dictionary<string, string> headers = form.headers;
        headers["Authorization"] = "Basic " + base64Credentials;

        WWW www = new WWW(twilioUrl, form.data, headers);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Text message sent successfully!");
        }
        else
        {
            Debug.LogError($"Error sending text message: {www.error}");
        }
    }
    
    IEnumerator WaitForRequest (WWW www)
    {
        yield return www;
		
        // check for errors
        if (www.error == null) {
            Debug.Log ("WWW Ok! SMS sent through Web API: " + www.text);
        } else {
            Debug.Log ("WWW Error: " + www.error);
        }    
    }
}