using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Analytics;

//todo: add loading time of DB and storage assets to Analytics 
//todo: tracking batches of people
public partial class FbManager
{
    private float startTime = 0;
    private float ElapsedTime = 0;
    private string screenName = "null";
    public void AnalyticsStartTimer(string screenName)
    {
        Debug.Log("Analytics: AnalyticsStartTimer");
        Debug.Log("Analytics: screenName:" + screenName);

        this.screenName = screenName;
        startTime = Time.time;
    }

    public void AnalyticsStopTimer()
    {
        Debug.Log("Analytics: AnalyticsStopTimer");
        ElapsedTime = Time.time - startTime;
        Debug.Log("Analytics: timeSpentOnScreen " + screenName +":" + ElapsedTime);
        AnalyticsTimeSpentOnScreen();
    }

    public void AnalyticsTimeSpentOnScreen()
    {
        Debug.Log("Analytics: TimeSpentOnScreen");
        Debug.Log("screen_name:" + screenName);
        Debug.Log("time_spent_on_screen:" + ElapsedTime);
        FirebaseAnalytics.LogEvent("time_spent_on_screen", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("screen_name", screenName),
            new Parameter("time_spent_on_screen", ElapsedTime)
        });
    }
    
    //todo: add location tracking to determine how user is using app in different places/how far do they move between opening the app (delta)
    public void AnalyticsOnLocationChanged(double lat, double lng)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: AnalyticsOnLocationChanged");
        FirebaseAnalytics.LogEvent("location_changed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("date_of_location", DateTime.UtcNow.ToString()),
            new Parameter("location_lat", lat),
            new Parameter("location_lng", lng)
        });
    }
    public void AnalyticsSetBatchNumber(string batchNumber)
    {
        Debug.Log("Analytics: AnalyticsSetBatchNumber");
        Debug.Log("batchNumber:" + batchNumber);
        FirebaseAnalytics.SetUserProperty("batch_number",batchNumber.ToString());
    }
    public void AnalyticsSetTracesReceived(string tracesReceived)
    {
        Debug.Log("Analytics: AnalyticsSetTracesReceived");
        Debug.Log("tracesReceived:" + tracesReceived);
        FirebaseAnalytics.SetUserProperty("traces_received",tracesReceived.ToString());
    }
    public void AnalyticsSetTracesSent(string tracesSent)
    {
        Debug.Log("Analytics: AnalyticsSetTracesReceived");
        Debug.Log("tracesReceived:" + tracesSent);
        FirebaseAnalytics.SetUserProperty("traces_sent",tracesSent.ToString());
    }
    
    #region Home Screen
    public void AnalyticsOnCameraPressed()
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnCameraPressed");
        FirebaseAnalytics.LogEvent("home_camera_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnFriendsPressed()
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnFriendsPressed");
        FirebaseAnalytics.LogEvent("home_friends_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnUserSettingsPressed()
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnUserSettingsPressed");
        FirebaseAnalytics.LogEvent("home_settings_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnSwitchViewSelectionPressed()
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnSwitchViewSelectionPressed");
        FirebaseAnalytics.LogEvent("home_switch_view_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    #endregion

    public void AnalyticsOnSendTrace(int numberOfPeopleSelected, float lengthOfVideo, int camFlippedCount)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnSendTrace");
        Debug.Log("length_of_video:" + lengthOfVideo);
        Debug.Log("camera_flipped_count:" + camFlippedCount);
        Debug.Log("number_of_people_sent_to:" + numberOfPeopleSelected);
        FirebaseAnalytics.LogEvent("trace_sent", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("date_of_video", DateTime.UtcNow.ToString()),
            new Parameter("length_of_video", lengthOfVideo),
            new Parameter("camera_flipped_count", camFlippedCount),
            new Parameter("number_of_people_sent_to", numberOfPeopleSelected),
        });
    }
    public void AnalyticsOnTracePressed(string usernameOfTraceSender, string timeSinceTraceLeft, string traceType) //todo: add functionality here
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnTracePressed");
        Debug.Log("username_of_trace_sender:" + usernameOfTraceSender);
        Debug.Log("time_since_trace_left:" + timeSinceTraceLeft);
        FirebaseAnalytics.LogEvent("trace_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("trace_type", traceType),
            new Parameter("username_of_trace_sender", usernameOfTraceSender),
            new Parameter("time_since_trace_left", timeSinceTraceLeft),
        });
    }
    
    #region Friends Screen
    public void AnalyticsOnSearchBarPressed(string typeOfSearch)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnSearchBarPressed");
        Debug.Log("type_of_search:" + typeOfSearch);
        FirebaseAnalytics.LogEvent("friends_search_bar_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("type_of_search", typeOfSearch),
        });
    }
    
    public void AnalyticsOnSelectBarPressed(string selected)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnSelectBarPressed");
        Debug.Log("selected_button" + selected);
        FirebaseAnalytics.LogEvent("friends_select_bar_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("selected_button", selected),
        });
    }
    
    public void AnalyticsOnRemoveFriend(string friendUsername)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: AnalyticsOnRemoveFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_removed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    public void AnalyticsOnAddContact(string contact)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnAddFriend");
        Debug.Log("friend_username:" + contact);
        FirebaseAnalytics.LogEvent("friends_contact_added", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", contact),
        });
    }
    public void AnalyticsOnHeartFriend(string friendUsername)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnAddFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_hearted", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    public void AnalyticsOnAcceptFriend(string friendUsername)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnAcceptFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_accepted", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    
    public void AnalyticsSetUserFriendCount(int friendCount) //todo: add spot where this is called
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: AnalyticsSetUserFriendCount");
        Debug.Log("friendCount:" + friendCount);
        FirebaseAnalytics.SetUserProperty("friends_friend_count",friendCount.ToString());
    }
    
    public void AnalyticsOnSendFriendRequest(string friendUsername)
    {
        if(!IsFirebaseUserInitialised) return; //make sure fb user exists
        Debug.Log("Analytics: OnSendFriendRequest");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_request_sent", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    #endregion
}
