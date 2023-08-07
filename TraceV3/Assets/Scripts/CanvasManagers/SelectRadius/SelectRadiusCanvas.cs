using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SelectRadiusCanvas : MonoBehaviour
{
    [Header("External")] [SerializeField] private DrawTraceOnMap 
        _drawTraceOnMap;
    [SerializeField] private DragAndZoomInertia dragAndZoomInertia;
    [SerializeField] private OnlineMapsMarkerManager markerManager;
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;
    [SerializeField] private OnlineMaps map;
    [SerializeField] private Image radius;

    [SerializeField]private UnityEngine.UI.Slider _radiusSlider;
    [SerializeField] public AnimationCurve radiusSize;
    [SerializeField] private AnimationCurve zoomScaleValue;
    [SerializeField] public AnimationCurve scaler;
    [SerializeField]private Button _isVisableToggleButton;
    [SerializeField]private bool _isTraceVisable;
    [SerializeField]private GameObject traceIsVisable;
    [SerializeField]private GameObject traceIsHidden;
    //[SerializeField]private bool firstTimeEnabled;
   
    private void OnEnable()
    {
        MapboxGeocoding.Instance.GetUserLocationName(); //not a great time to call this
        // if (!firstTimeEnabled)
        // {
        //     firstTimeEnabled = true;
        //     return;
        // }
        
        _onlineMapsLocationService.updatePosition = true;
        
        if (PlayerPrefs.GetFloat("LeaveTraceSliderRadiusValue") != 0)
        {
            Debug.Log("Playerpref is NOT 0");
            _radiusSlider.value = PlayerPrefs.GetFloat("LeaveTraceSliderRadiusValue");
        }
        else
        {
            Debug.Log("Playerpref is 0 setting to half");
            PlayerPrefs.SetFloat("LeaveTraceSliderRadiusValue", 0.5f);
            _radiusSlider.value = 0.5f;
            SetRadius();
        }
        if (PlayerPrefs.GetInt("LeaveTraceIsVisable") != 0)
        {
            if (PlayerPrefs.GetInt("LeaveTraceIsVisable") == 1)
            {
                _isTraceVisable = true;
                traceIsVisable.SetActive(true);
                traceIsHidden.SetActive(false);
            }
            else
            {
                _isTraceVisable = false;
                traceIsVisable.SetActive(false);
                traceIsHidden.SetActive(true);
            }
        }
        else
        {
            PlayerPrefs.GetInt("LeaveTraceIsVisable", 1);
            _isTraceVisable = true;
            traceIsVisable.SetActive(true);
            traceIsHidden.SetActive(false);
        }
    }

    public void SendTraceButton()
    {
        dragAndZoomInertia.setZoomMode(false);
        PlayerPrefs.SetFloat("LeaveTraceSliderRadiusValue", _radiusSlider.value);
        if(_isTraceVisable)
            PlayerPrefs.SetInt("LeaveTraceIsVisable", 1);
        else
        {
            PlayerPrefs.SetInt("LeaveTraceIsVisable", -1);
        }
    }
    public void SetRadius()
    {
        SendTraceManager.instance.SetRadius(_radiusSlider.value);
        dragAndZoomInertia.setZoomMode(true);
        dragAndZoomInertia.setTargetZoom(zoomScaleValue.Evaluate(_radiusSlider.value));
    }

    private void FixedUpdate()
    {
        var scale = scaler.Evaluate(map.floatZoom)*radiusSize.Evaluate(_radiusSlider.value);
        radius.rectTransform.localScale = new Vector3(scale,scale,scale);
    }


    public void ToggleTraceVisability()
    {
        _isTraceVisable = !_isTraceVisable;
        
        if (_isTraceVisable)
        {
            traceIsVisable.SetActive(true);
            traceIsHidden.SetActive(false);
        }
        else
        {
            traceIsVisable.SetActive(false);
            traceIsHidden.SetActive(true);
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
