using System;using System.Collections.Generic;
using UnityEngine;
public class DrawTraceOnMap : MonoBehaviour
{
    /// <summary>
    /// Number of segments
    /// </summary>
    
    public int segments = 240;
    public TraceObject sendingTraceTraceLoadingObject;
    [SerializeField] private ScaleMapElements _scaleMapElements;
    
    [SerializeField] private int scaleAmount;
    [SerializeField] private OnlineMapsMarkerManager markerManager;
    [SerializeField] private bool showDebugTextures;
    [SerializeField] private Texture2D primaryReceiverTexture;
    [SerializeField] private Texture2D primaryReceivingHollowTexture;
    [SerializeField] private Texture2D secondaryReceiverTexture;
    
    [SerializeField] private Texture2D primaryReceiverTextureBF;
    [SerializeField] private Texture2D primaryReceivingHollowTextureBF;
    [SerializeField] private Texture2D secondaryReceiverTextureBF;

    [SerializeField] private Texture2D primarySentTexture;
    [SerializeField] private Texture2D primarySendingTextureHollow;
    [SerializeField] private Texture2D secondarySentTexture;
    
    [SerializeField] private Texture2D expiredTexture;

    public OnlineMapsMarker DrawCircle(double lat, double lng, float radius, TraceType traceType, string markerID)
    {
        OnlineMapsMarker _onlineMapsMarker = PlaceTrace(lat, lng, radius, traceType, markerID);
        _onlineMapsMarker.displayedTexture = _onlineMapsMarker.secondaryZoomedOutTexture;
        _scaleMapElements.ScaleTrace(_onlineMapsMarker);
        if (showDebugTextures)
        {
            OnlineMapsMarkerManager.CreateItem(lng, lat, "Marker " + OnlineMapsMarkerManager.CountItems);
            OnlineMaps map = OnlineMaps.instance;
            double nlng, nlat;
            OnlineMapsUtils.GetCoordinateInDistance(lng, lat, radius, 90, out nlng, out nlat);
                
            double tx1, ty1, tx2, ty2;
            
            // Convert the coordinate under cursor to tile position
            map.projection.CoordinatesToTile(lng, lat, 20, out tx1, out ty1);
            
            // Convert remote coordinate to tile position
            map.projection.CoordinatesToTile(nlng, nlat, 20, out tx2, out ty2);
            
            // Calculate radius in tiles
            double r = tx2 - tx1;
            
            // Create a new array for points
            OnlineMapsVector2d[] points = new OnlineMapsVector2d[segments];
            
            // Calculate a step
            double step = 360d / (segments-1);
            
            // Calculate each point of circle
            for (int i = 0; i < segments; i++)
            {
                double px = tx1 + Math.Cos(step * i * OnlineMapsUtils.Deg2Rad) * r;
                double py = ty1 + Math.Sin(step * i * OnlineMapsUtils.Deg2Rad) * r;
                map.projection.TileToCoordinates(px, py, 20, out lng, out lat);
                points[i] = new OnlineMapsVector2d(lng, lat);
            }
            // Create a new polygon to draw a circle
            OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingPoly(points, Color.white, 2, new Color(10, 10, 10, 0.1f)));
        }
        return _onlineMapsMarker;
    }

    public OnlineMapsMarker PlaceTrace(double lat, double lng, float radius, TraceType traceType, string markerID)
    {
        OnlineMapsMarker _onlineMapsMarker;
        switch (traceType)
        {
            case TraceType.SENT:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primarySentTexture, secondarySentTexture, primarySendingTextureHollow, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.SENDING:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primarySendingTextureHollow, secondarySentTexture, primarySendingTextureHollow, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.RECEIVED:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceiverTexture, secondaryReceiverTexture, primaryReceivingHollowTexture, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.RECEIVEDBESTFRIEND:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceiverTextureBF, secondaryReceiverTextureBF, primaryReceivingHollowTextureBF, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.OPENING:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceivingHollowTexture, secondaryReceiverTexture, primaryReceivingHollowTexture, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.OPENINGBESTFRIEND:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceivingHollowTextureBF, secondaryReceiverTextureBF, primaryReceivingHollowTextureBF, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.OPENED:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceivingHollowTexture, secondaryReceiverTexture, primaryReceivingHollowTexture, expiredTexture, markerID);
                return _onlineMapsMarker;
            case TraceType.OPENEDBESTFRIEND:
                _onlineMapsMarker = markerManager.AddTraceToMap(lat, lng, radius, primaryReceivingHollowTextureBF, secondaryReceiverTextureBF, primaryReceivingHollowTextureBF, expiredTexture, markerID);
                return _onlineMapsMarker;
        }
        return null;
    }
    
    public void Clear()
    {
        for (int i = OnlineMapsDrawingElementManager.CountItems; i >=  0; i--)
        {
            OnlineMapsDrawingElementManager.RemoveItemAt(i);
        }
        for (int i = OnlineMapsMarkerManager.CountItems; i >  0; i--)
        {
            OnlineMapsMarkerManager.RemoveItemAt(i);
        }

        //draw loading trace
        if (SendTraceManager.instance.isSendingTrace)
        {
            DrawCircle(SendTraceManager.instance.location.x, SendTraceManager.instance.location.y, SendTraceManager.instance.selectedRadius, TraceType.SENDING, "loading");
        }
    }
    
    public enum TraceType {RECEIVED, RECEIVEDBESTFRIEND, SENT, SENDING, OPENING, OPENINGBESTFRIEND, OPENED, OPENEDBESTFRIEND};
}

