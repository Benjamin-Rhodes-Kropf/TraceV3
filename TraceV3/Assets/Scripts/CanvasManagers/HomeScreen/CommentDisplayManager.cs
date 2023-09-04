using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class CommentDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject commentViewPrefab;
    [SerializeField] private Transform verticalLayoutGroup;
    [SerializeField] private CommentAudioManager _commentAudioManager;
    public Dictionary<string, GameObject> comments = new Dictionary<string, GameObject>();
    
    public void DisplayComments(Dictionary<string, TraceCommentObject> traceCommentObjects)
    {
        if (commentViewPrefab == null)
        {
            Debug.LogError("commentViewPrefab has not been assigned in the Inspector!");
            return;
        }

        // Destroy old comments and clear the list
        foreach (var comment in comments)
        {
            Destroy(comment.Value);
        }

        comments.Clear();

        // Convert dictionary to list and sort by DateTime in descending order
        var sortedTraceComments = new List<KeyValuePair<string, TraceCommentObject>>(traceCommentObjects);
        sortedTraceComments.Sort((pair1, pair2) =>
        {
            DateTime dateTime1;
            DateTime dateTime2;

            if (DateTime.TryParse(pair1.Value.time, out dateTime1) &&
                DateTime.TryParse(pair2.Value.time, out dateTime2))
            {
                return -dateTime1.CompareTo(dateTime2); // '-' for descending order
            }

            return 0; // If the date format is not correct, we keep the original order
        });

        foreach (var traceComment in sortedTraceComments)
        {
            GameObject instantiatedComment = Instantiate(commentViewPrefab, verticalLayoutGroup);

            var audioView = instantiatedComment.GetComponent<AudioView>();

            if (verticalLayoutGroup.childCount > 2)
            {
                // Set it to be the third child
                instantiatedComment.transform.SetSiblingIndex(2);
            }

            if (audioView != null)
            {
                audioView.UpdateDisplayedData(traceComment.Value.id, traceComment.Value.id,
                    traceComment.Value.senderName, traceComment.Value.time, traceComment.Value.soundWave);
                audioView.CommentAudioManager = _commentAudioManager; //pass ref to play sound
                comments.Add(traceComment.Key, instantiatedComment);
            }
            else
            {
                Debug.LogError("AudioView component not found in the instantiated comment prefab.");
                Destroy(instantiatedComment);
            }
        }
    }

    
    //todo:Display When Sending Temp Trace (or delete this code because its not refed by anything) I would make it work with animations
    public void DisplayTempSentMessage()
    {
        GameObject instantiatedComment = Instantiate(commentViewPrefab, verticalLayoutGroup);

        var audioView = instantiatedComment.GetComponent<AudioView>();

        if (verticalLayoutGroup.childCount > 2)
        {
            // Set it to be the third child
            instantiatedComment.transform.SetSiblingIndex(2);
        }

        if (audioView != null)
        {
            TraceCommentObject traceComment = new TraceCommentObject("temp", DateTime.UtcNow.ToString(), FbManager.instance.thisUserModel.userID,FbManager.instance.thisUserModel.name, new float[40]);
            audioView.UpdateDisplayedData(traceComment.id, "sending",
                traceComment.senderName, traceComment.time, traceComment.soundWave);
            audioView.CommentAudioManager = _commentAudioManager; //pass ref to play sound
            comments.Add(traceComment.id, instantiatedComment);
        }
        else
        {
            Debug.LogError("AudioView component not found in the instantiated comment prefab.");
            Destroy(instantiatedComment);
        }
    }
    
    //would work with code above
    public void RemoveTempSentMessage()
    {
        Destroy(comments["temp"]);
        comments.Remove("temp");
    }

    public void ClearComments()
    {
        foreach (var comment in comments)
        {
            Destroy(comment.Value);
        }

        comments.Clear();
    }
}

