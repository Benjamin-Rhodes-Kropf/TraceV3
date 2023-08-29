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

    public void DisplayComments(Dictionary<string,TraceCommentObject> traceCommentObjects)
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

        foreach (var traceComment in traceCommentObjects)
        {
            GameObject instantiatedComment = Instantiate(commentViewPrefab, verticalLayoutGroup);

            if (verticalLayoutGroup.childCount > 2)
            {
                // Set it to be the third child
                instantiatedComment.transform.SetSiblingIndex(2);
            }

            var audioView =  instantiatedComment.GetComponent<AudioView>();
            audioView.UpdateDisplayedData(traceComment.Value.id, traceComment.Value.id,traceComment.Value.senderName, traceComment.Value.time, new List<float>());
            audioView.CommentAudioManager = _commentAudioManager; //pass ref to play sound
            comments.Add(traceComment.Key,instantiatedComment);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
