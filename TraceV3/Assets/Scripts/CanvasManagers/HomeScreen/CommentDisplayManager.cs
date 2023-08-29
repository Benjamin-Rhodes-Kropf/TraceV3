using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class CommentDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject commentViewPrefab;
    [SerializeField] private Transform verticalLayoutGroup;
    [SerializeField] private List<GameObject> comments = new List<GameObject>();

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
            Destroy(comment);
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

            instantiatedComment.GetComponent<AudioView>().UpdateDisplayedData(traceComment.Value.id, traceComment.Value.id, traceComment.Value.senderName, traceComment.Value.time, new List<float>());

            comments.Add(instantiatedComment);
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
