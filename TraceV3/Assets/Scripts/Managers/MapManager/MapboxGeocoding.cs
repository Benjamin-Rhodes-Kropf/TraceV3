using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapboxGeocoding : MonoBehaviour
{
    // Singleton instance
    private static MapboxGeocoding _instance;
    public static MapboxGeocoding Instance => _instance;
    
    [Header("Refrences")] 
    [SerializeField] private OnlineMaps _onlineMaps;
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;

    
    [Header("Mapbox API Settings")]
    public string accessToken = "pk.eyJ1IjoiYmVucmsxMDAiLCJhIjoiY2xlNXRqMmZwMGc4cTNwbnh4OWcxYjhhbSJ9.tKfaEUT7hvBsZml5ucE5CA";
    public string geocodingBaseUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/";
    
    [Header("Location Info")]
    public float latitude; // Input the latitude value here
    public float longitude; // Input the longitude value here
    
    [Header("Location Info")]
    public string locationName; // The location name to search for (e.g., "New York City")
    public string userLocationName;
    public MapboxResponseData _MapboxResponseData;
    public List<Feature> filteredFeatures;
    // Awake is called before Start
    void Awake()
    {
        // Ensure only one instance of MapboxGeocoding exists in the scene
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GetUserLocationName()
    {
        _onlineMapsLocationService.GetLocation(out longitude, out latitude);
        StartCoroutine(MapboxGeocoding.Instance.GetGeocodingData(longitude ,latitude, (result) => {
            if (result != "null")
            {
                userLocationName = result;
            }
        }));
    }
    
    public IEnumerator GetGeocodingData(float longitude, float latitude, System.Action<string> locationCallback)
    {
        Debug.Log("SEND GetGeocodingData");
        //Format the URL for the GET request
        // Format the URL for the GET request
        string location = longitude + "," + latitude;
        string url = geocodingBaseUrl + WWW.EscapeURL(location) + ".json?access_token=" + accessToken;

        //Make the GET request
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // Parse the response data
                string responseData = www.downloadHandler.text;
                Debug.Log("Geocoding API Response: " + responseData);
                
                
                // Deserialize the JSON response into a custom data structure
                MapboxResponseData mapboxData = JsonUtility.FromJson<MapboxResponseData>(responseData);
                _MapboxResponseData = mapboxData;
                
                //get zoom level
                if (_onlineMaps != null)
                {
                    // Get the current zoom level from the map
                    float zoomLevel = _onlineMaps.floatZoom;

                    // Now you can use the zoom level to set your location string or perform other actions
                    locationCallback(GetLocationName(zoomLevel, mapboxData));
                }
                
                // Extract and print relevant information from the response
                foreach (var feature in mapboxData.features)
                {
                    Debug.Log("Place Name: " + feature.place_name);
                    Debug.Log("Latitude: " + feature.center[1]);
                    Debug.Log("Longitude: " + feature.center[0]);
                    Debug.Log("--------------");
                }
            }
        }
    }
    
    private string GetLocationName(float zoomLevel, MapboxResponseData mapboxData)
    {
        
        
        // Use a list to store the features without postcodes
        // List<Feature> filteredFeatures = new List<Feature>();
        filteredFeatures = new List<Feature>();


        // Iterate through mapboxData.features and filter out features with "postcode" in their ids
        foreach (var feature in mapboxData.features)
        {
            if (!feature.id.Contains("postcode"))
            {
                // Add the feature to the filteredFeatures list if it does not contain "postcode"
                filteredFeatures.Add(feature);
            }
        }

        // Get the relevant context text based on the zoom level
        // Adjust the zoom level thresholds and context text selection based on your requirements
        if (zoomLevel > 18)
        {
            foreach (var feature in filteredFeatures)
            {
                if (feature.context != null && feature.context.Length > 1)
                {
                    // Return the first context text (you can choose based on your preference)
                    return filteredFeatures[1].text;
                }
            }
        }
        else if (zoomLevel > 10)
        {
            // Use the second context text (if available)
            foreach (var feature in filteredFeatures)
            {
                if (feature.context != null && feature.context.Length > 2)
                {
                    return filteredFeatures[2].text;;
                }
            }
        }
        else if (zoomLevel > 6)
        {
            // Use the third context text (if available)
            foreach (var feature in filteredFeatures)
            {
                if (feature.context != null && feature.context.Length > 3)
                {
                    return filteredFeatures[3].text;;
                }
            }
        }
        else if (zoomLevel > 0)
        {
            // Use the third context text (if available)
            foreach (var feature in filteredFeatures)
            {
                if (feature.context != null && feature.context.Length > 4)
                {
                    return filteredFeatures[4].text;;
                }
            }
        }

        //failsafe
        for (int i = filteredFeatures.Count - 1; i >= 0; i--)
        {
            var name = filteredFeatures[i].text;
            // Do something with 'name' (e.g., print, store, etc.)

            // If you want to return the last name in the loop, you can use the return statement here.
            // Otherwise, the loop will continue iterating and overwrite 'name' in each iteration.
            locationName = name;
            return name;
        }

        // If no context text is available, return the default location name
        return "null";
    }
}

[System.Serializable]
public class MapboxResponseData
{
    public string type;
    public double[] query;
    public Feature[] features;
    public string attribution;
}

[System.Serializable]
public class Feature
{
    public string id;
    public string[] place_type;
    public double relevance;
    public Properties properties;
    public string text;
    public string place_name;
    public double[] center;
    public Geometry geometry;
    public Context[] context;
}

[System.Serializable]
public class Properties
{
    public string accuracy;
    public string mapbox_id;
}

[System.Serializable]
public class Geometry
{
    public string type;
    public double[] coordinates;
}

[System.Serializable]
public class Context
{
    public string id;
    public string mapbox_id;
    public string text;
}