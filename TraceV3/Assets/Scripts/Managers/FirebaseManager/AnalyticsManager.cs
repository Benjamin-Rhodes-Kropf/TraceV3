using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Analytics;

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
    
    #region Home Screen
    public void AnalyticsOnCameraPressed()
    {
        Debug.Log("Analytics: OnCameraPressed");
        FirebaseAnalytics.LogEvent("home_camera_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnFriendsPressed()
    {
        Debug.Log("Analytics: OnFriendsPressed");
        FirebaseAnalytics.LogEvent("home_friends_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnUserSettingsPressed()
    {
        Debug.Log("Analytics: OnUserSettingsPressed");
        FirebaseAnalytics.LogEvent("home_settings_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    public void AnalyticsOnSwitchViewSelectionPressed()
    {
        Debug.Log("Analytics: OnSwitchViewSelectionPressed");
        FirebaseAnalytics.LogEvent("home_switch_view_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
        });
    }
    #endregion

    public void AnalyticsOnSendTrace(int numberOfPeopleSelected, int lengthOfVideo, int camFlippedCount)
    {
        Debug.Log("Analytics: OnSendTrace");
        Debug.Log("length_of_video:" + lengthOfVideo);
        Debug.Log("camera_flipped_count:" + camFlippedCount);
        Debug.Log("number_of_people_sent_to:" + numberOfPeopleSelected);
        FirebaseAnalytics.LogEvent("trace_sent", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("length_of_video", lengthOfVideo),
            new Parameter("camera_flipped_count", camFlippedCount),
            new Parameter("number_of_people_sent_to", numberOfPeopleSelected),
        });
    }
    public void AnalyticsOnTracePressed(string usernameOfTraceSender, string timeSinceTraceLeft)
    {
        Debug.Log("Analytics: OnTracePressed");
        Debug.Log("username_of_trace_sender:" + usernameOfTraceSender);
        Debug.Log("time_since_trace_left:" + timeSinceTraceLeft);
        FirebaseAnalytics.LogEvent("trace_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("username_of_trace_sender", usernameOfTraceSender),
            new Parameter("time_since_trace_left", timeSinceTraceLeft),
        });
    }
    
    #region Friends Screen
    public void AnalyticsOnSearchBarPressed(string typeOfSearch)
    {
        Debug.Log("Analytics: OnSearchBarPressed");
        Debug.Log("type_of_search:" + typeOfSearch);
        FirebaseAnalytics.LogEvent("friends_search_bar_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("type_of_search", typeOfSearch),
        });
    }
    public void AnalyticsOnSelectBarPressed(string selected)
    {
        Debug.Log("Analytics: OnSelectBarPressed");
        Debug.Log("selected_button" + selected);
        FirebaseAnalytics.LogEvent("friends_select_bar_pressed", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("selected_button", selected),
        });
    }
    public void AnalyticsOnAddFriend(string friendUsername)
    {
        Debug.Log("Analytics: OnAddFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_added", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    public void AnalyticsOnHeartFriend(string friendUsername)
    {
        Debug.Log("Analytics: OnAddFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_hearted", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    public void AnalyticsOnAcceptFriend(string friendUsername)
    {
        Debug.Log("Analytics: OnAcceptFriend");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_accepted", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    public void AnalyticsOnSendFriendRequest(string friendUsername)
    {
        Debug.Log("Analytics: OnSendFriendRequest");
        Debug.Log("friend_username:" + friendUsername);
        FirebaseAnalytics.LogEvent("friends_friend_request_sent", new Parameter[] {
            new Parameter("this_user_username", thisUserModel.username),
            new Parameter("friend_username", friendUsername),
        });
    }
    #endregion
}
