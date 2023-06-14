using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenManager : MonoBehaviour
{
    public bool isInSentMode;
    public RawImage displayTrace;


    public void ToggleisInSentMode()
    {
        isInSentMode = !isInSentMode;
    }

    public void OpenTrace(string traceID)
    {
        //FbManager.instance.GetTraceToOpen();
        if(traceID == null)
            return;
        
        StartCoroutine(FbManager.instance.GetTracePhotoByUrl(traceID, (texture) =>
        {
            if (texture != null)
            {
                displayTrace.texture = texture;
                FbManager.instance.MarkTraceAsOpened(traceID);
                ScreenManager.instance.OpenPopup("Trace");
            }
            else
            {
                Debug.LogError("LoadTraceImage Failed");
            }
        }));
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
