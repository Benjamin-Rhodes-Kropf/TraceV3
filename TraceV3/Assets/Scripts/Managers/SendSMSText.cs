using System;
using UnityEngine;
using System.Collections;

public class SendSMSText : MonoBehaviour
{
    private static SendSMSText _instance;
    public static SendSMSText Instance => _instance;
    
    //https://maps.apple.com/?ll=42.097553,-71.181867&q=Trace&t=h
    string url = "api.twilio.com/2010-04-01/Accounts/";
    string service="/Messages.json";
    public string from;
    public string to;
    public string account_sid;
    public string auth;
    public string body;

    private void Start()
    {
        // SendSMS();
    }

    public void SendSMS (string phoneNumber = "", string body = "")
    {
        WWWForm form = new WWWForm ();
        form.AddField ("To", to);
        form.AddField ("From", from);
        //string bodyWithoutSpace = body.Replace (" ", "%20");//Twilio doesn't need this conversion
        form.AddField ("Body", body);
        string completeurl = "https://"+account_sid+":" + auth +"@" +url+account_sid+service;
        Debug.Log (completeurl);
        WWW www = new WWW (completeurl, form);
        StartCoroutine (WaitForRequest (www));
    }

    public void SendTraceSMS(string phoneNumber, string body, float lat, float lng, float radius)
    {
        WWWForm form = new WWWForm ();
        form.AddField ("To", to);
        form.AddField ("From", from);
        //string bodyWithoutSpace = body.Replace (" ", "%20");//Twilio doesn't need this conversion
        form.AddField ("Body", body);
        string completeurl = "https://"+account_sid+":" + auth +"@" +url+account_sid+service;
        Debug.Log (completeurl);
        WWW www = new WWW (completeurl, form);
        StartCoroutine (WaitForRequest (www));
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