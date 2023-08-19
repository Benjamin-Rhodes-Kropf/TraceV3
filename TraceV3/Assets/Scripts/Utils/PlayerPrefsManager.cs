using System;
using System.Collections.Generic;
using UnityEngine;
using  Newtonsoft.Json;

public class PlayerPrefsManager : MonoBehaviour
{
    public static PlayerPrefsManager Instance;
    private const string m_ReceivedTracesKey = "DBReceivedTraces_Key";
    private const string m_SentTracesKey = "DBSentTraces_Key";
    private const string m_UsersKey = "DBUsers_Key";
    private const string m_FriendsKey = "DBFriends_Key";

    void Awake()
    {
        Instance = this;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus)
        {
            Debug.Log("iOS app went to background");
            Debug.Log("Saving Player Prefs");
            if(TraceManager.instance.receivedTraceObjects.Count > 0)
                SaveReceivedTracesToPlayerPrefs(TraceManager.instance.receivedTraceObjects);
            if(TraceManager.instance.receivedTraceObjects.Count > 0)
                SaveSentTracesToPlayerPrefs(TraceManager.instance.sentTraceObjects);
            if(FbManager.instance._allFriends.Count > 0)
                SaveFriendsToPlayerPrefs(FbManager.instance._allFriends);
            if(FbManager.instance.users.Count > 0)
                SaveUsersToPlayerPrefs(FbManager.instance.users);
            PlayerPrefs.SetInt("DBDataCached", 1);
        }
        else
        {
            Debug.Log("iOS app came to foreground");
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Saving Player Prefs");
        if(TraceManager.instance.receivedTraceObjects.Count > 0)
            SaveReceivedTracesToPlayerPrefs(TraceManager.instance.receivedTraceObjects);
        if(TraceManager.instance.receivedTraceObjects.Count > 0)
            SaveSentTracesToPlayerPrefs(TraceManager.instance.sentTraceObjects);
        if(FbManager.instance._allFriends.Count > 0)
            SaveFriendsToPlayerPrefs(FbManager.instance._allFriends);
        if(FbManager.instance.users.Count > 0)
            SaveUsersToPlayerPrefs(FbManager.instance.users);
        PlayerPrefs.SetInt("DBDataCached", 1);
    }
    
    public void SaveReceivedTracesToPlayerPrefs(List<TraceObject> traces)
    {
        string traceJSON = JsonConvert.SerializeObject(traces);
        Debug.Log(traceJSON);
        PlayerPrefs.SetString(m_ReceivedTracesKey,traceJSON);
        PlayerPrefs.Save();
    }
    
    public void SaveSentTracesToPlayerPrefs(List<TraceObject> traces)
    {
        string traceJSON = JsonConvert.SerializeObject(traces);
        Debug.Log(traceJSON);
        PlayerPrefs.SetString(m_SentTracesKey,traceJSON);
        PlayerPrefs.Save();
    }
    
    public void SaveFriendsToPlayerPrefs(List<FriendModel> friends)
    {
        string friendsJSON = JsonConvert.SerializeObject(friends);
        Debug.Log(friendsJSON);
        PlayerPrefs.SetString(m_FriendsKey,friendsJSON);
        PlayerPrefs.Save();
    }
    
    public void SaveUsersToPlayerPrefs(List<UserModel> users)
    {
        string usersJSON = JsonConvert.SerializeObject(users);
        Debug.Log(usersJSON);
        PlayerPrefs.SetString(m_UsersKey,usersJSON);
        PlayerPrefs.Save();
    }
    
    
    public List<TraceObject> GetReceivedTracesFromPlayerPrefs()
    {
        var traces = new List<TraceObject>();
        var tracesString = PlayerPrefs.GetString(m_ReceivedTracesKey,"");
        traces = JsonConvert.DeserializeObject<List<TraceObject>>(tracesString);
        return traces;
    }
    public List<TraceObject> GetSentTracesFromPlayerPrefs()
    {
        var traces = new List<TraceObject>();
        var tracesString = PlayerPrefs.GetString(m_SentTracesKey,"");
        traces = JsonConvert.DeserializeObject<List<TraceObject>>(tracesString);
        return traces;
    }
    public List<FriendModel> GetFriendsFromPlayerPrefs()
    {
        var friends = new List<FriendModel>();
        var friendsString = PlayerPrefs.GetString(m_FriendsKey,"");
        friends = JsonConvert.DeserializeObject<List<FriendModel>>(friendsString);
        return friends;
    }
    
    public List<UserModel> GetUsersFromPlayerPrefs()
    {
        var users = new List<UserModel>();
        var usersString = PlayerPrefs.GetString(m_UsersKey,"");
        users = JsonConvert.DeserializeObject<List<UserModel>>(usersString);
        return users;
    }
}
