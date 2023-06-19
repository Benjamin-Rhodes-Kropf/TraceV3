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
    [SerializeField] private AnimationCurve scaler;
    
    private void Update()
    {
        try
        {
            markerManager.items[0].scale = 0.1f;
        }
        catch (Exception e)
        {
            print("Scale Marker Error :: "+e.Message);
        }
        
        var traceScale = new double();
        var zoomfloat = map.floatZoom;
        traceScale = scaler.Evaluate(zoomfloat);

        for (int i = 1; i < markerManager.items.Count; i++)
        {
            if (traceScale * radiusSize >0)
            {
                markerManager.items[i].scale = (float)(traceScale * radiusSize);   
            }
        }
    }
}
