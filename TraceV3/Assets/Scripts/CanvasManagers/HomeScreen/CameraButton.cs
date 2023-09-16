using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraButton : MonoBehaviour
{
    [SerializeField] private ScreenManager _screenManager;
    [SerializeField] private DragAndZoomInertia _dragAndZoomInertia;
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;
    [SerializeField] private HomeScreenTutorial _homeScreenTutorial;
    
    public void CameraButtonPressed()
    {
        if (FbManager.instance.IsFirebaseInitialised)
        {
            _screenManager.ChangeScreenNoAnim("Camera Screen");
            _screenManager.LoadArScene();
            _dragAndZoomInertia.SetMapVelocityToZero();
            _onlineMapsLocationService.UpdatePosition();
            FbManager.instance.AnalyticsOnCameraPressed();
            _homeScreenTutorial.TutorialCameraButtonPressed();
        }
        else
        {
            Debug.Log("Problem 1:" + FbManager.instance.IsFirebaseInitialised.ToString());
            Debug.Log("Problem 2:" + FbManager.instance.IsFirebaseUserLoggedIn.ToString());
            Debug.Log("Problem 3:" + FbManager.instance.IsFirebaseUserInitialised.ToString());
            NotificationManager.Instance.SendLocalNotification("Hold Up!, ","Give Us A Moment To Connect to the Internet", 1);
        }
    }
}
