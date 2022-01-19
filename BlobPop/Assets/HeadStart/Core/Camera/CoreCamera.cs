using System;
using UnityEngine;
using UnityEngine.UI;

public class CoreCamera : MonoBehaviour
{
    public RectTransform Canvas;
    public RectTransform Views;
    public Image LoadingOverlay;
    public CoreCameraSettings CoreCameraSettings;
    private Camera _camera;
    private CameraHelper _cameraHelper;
    public delegate void OnCameraSetupDone();
    private OnCameraSetupDone _onCameraSetupDone;
    private float _currentCameraSize;
    private float _toCameraSize;
    private int? _alignCameraToHelperTwid;
    public const float CAMERA_SETUP_TIME = 1.0f;    // 4.4f
    private bool _foundTheSweetSpot;
    private bool _debugActivated;

    void Start()
    {
        _camera = gameObject.GetComponent<Camera>();
        _currentCameraSize = PlayerPrefs.GetFloat("orthographicSize");
        if (_currentCameraSize == 0)
        {
            Debug.LogWarning("Please run MainMenu for [Initial Setup] Camera Adjustment");
            _currentCameraSize = 1;
        }
        _camera.orthographicSize = _currentCameraSize;
    }

    internal float GetCameraCurrentSize()
    {
        return _currentCameraSize;
    }

    internal void InitSetup(CameraHelper cameraHelper, OnCameraSetupDone onCameraSetupDone)
    {
        _camera.orthographicSize = 1;
        _cameraHelper = cameraHelper;
        _onCameraSetupDone = onCameraSetupDone;

        zoomOut();
    }

    private void zoomOut()
    {
        _currentCameraSize = _camera.orthographicSize;
        _toCameraSize = _currentCameraSize * 2;

        _alignCameraToHelperTwid = LeanTween.value(
            Main._.CoreCamera.gameObject,
            _currentCameraSize,
            _toCameraSize,
            CAMERA_SETUP_TIME
        ).id;
        LeanTween.descr(_alignCameraToHelperTwid.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_alignCameraToHelperTwid.Value).setOnUpdate((float val) =>
        {
            _camera.orthographicSize = val;
            if (_cameraHelper.CanSeeBounds())
            {
                _foundTheSweetSpot = true;
                foundSpot();
            }
        });
        LeanTween.descr(_alignCameraToHelperTwid.Value).setOnComplete(() =>
        {
            if (_foundTheSweetSpot == false)
            {
                zoomOut();
                return;
            }
        });
    }

    private void foundSpot()
    {
        LeanTween.cancel(_alignCameraToHelperTwid.Value);
        _alignCameraToHelperTwid = null;

        _currentCameraSize = _camera.orthographicSize;
        PlayerPrefs.SetFloat("orthographicSize", _currentCameraSize);

        _onCameraSetupDone();
    }

    void Update()
    {
        if (_debugActivated)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Tilde)
            || Input.GetKeyUp(KeyCode.AltGr)
            || Input.GetKeyUp(KeyCode.BackQuote)
        )
        {
            ActivateDebug();
        }
    }

    private void ActivateDebug()
    {
        string debugConsoleName = "IngameDebugConsole";
        GameObject go = GameObject.Find(debugConsoleName);
        if (go == null)
        {
            go = Instantiate(CoreCameraSettings.IngameDebugConsole, Vector3.zero, Quaternion.identity);
        }
        go.name = debugConsoleName;
        _debugActivated = true;
        go.SetActive(true);
    }
}