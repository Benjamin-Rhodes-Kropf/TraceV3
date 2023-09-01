using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraButton : MonoBehaviour
{
    [SerializeField] private ScreenManager _screenManager;
    [SerializeField] private DragAndZoomInertia _dragAndZoomInertia;
    [SerializeField] private OnlineMapsLocationService _onlineMapsLocationService;
    [SerializeField] private HomeScreenTutorial _homeScreenTutorial;

    void Start()
    {
        NotificationManager.Instance.SendLocalNotification("Hold Up!, ","Give Us A Moment To Connect to the Internet", 1);
    }
    public void CameraButtonPressed()
    {
        if (FbManager.instance.IsFirebaseInitialised && FbManager.instance.IsFirebaseUserLoggedIn)
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
            NotificationManager.Instance.SendLocalNotification("Hold Up!, ","Give Us A Moment To Connect to the Internet", 1);
        }
    }
}
