using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//still having problems with map markers culling when still in view
public class ScaleMapElements : MonoBehaviour
{
    [SerializeField]private OnlineMapsMarkerManager markerManager;
    [SerializeField]private OnlineMaps map;
    [SerializeField] private float radiusSize;
    [SerializeField] private float scaleLimitForSwitchImage;
    [SerializeField] private float scaleOfPin;
    [SerializeField] private AnimationCurve scaler;
    [SerializeField] private AnimationCurve scalerFineTune;

    public void Start()
    {
        map.OnChangeZoom += UpdateTraceScale;
        StartCoroutine(WaitUntilUserMapPinHasBeenCreated());
    }

    IEnumerator WaitUntilUserMapPinHasBeenCreated() //todo: not great implimentation
    {
        while (markerManager.items.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }
        UpdateTraceScale();
    }
    
    public void UpdateTraceScale()
    {
        try
        {
            markerManager.items[0].scale = 0.1f;
        }
        catch (Exception e)
        {
            Debug.LogWarning("User Pin Has Not Been Created");
        }
        
        var traceScale = new double();
        var zoomfloat = map.floatZoom;
        traceScale = scaler.Evaluate(zoomfloat)+scalerFineTune.Evaluate(zoomfloat);

        //Sets the way a trace looks and changes with zoom depending on scale
        for (int i = 1; i < markerManager.items.Count; i++)
        {
            var item = markerManager.items[i];
           // item.SwitchDisplayedImage(true);
           // item.displayingTexture = OnlineMapsMarker.DisplayingTexture.Circle;
           // item.scale = (float)(traceScale * radiusSize * markerManager.items[i].radius);
           //Debug.Log("radius:" + item.radius);
           
            if ( traceScale * radiusSize * item.radius < scaleLimitForSwitchImage)
            {
                item.SwitchDisplayedImage(false);
                item.scale = scaleOfPin;
            }
            else
            {
                item.SwitchDisplayedImage(true);
                item.scale = (float)(traceScale * radiusSize * markerManager.items[i].radius);
            }
        }
    }
}
