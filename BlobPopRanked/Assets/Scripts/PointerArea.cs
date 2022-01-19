using Assets.HeadStart.Core;
using UnityEngine;

public class PointerArea : MonoBehaviour
{
    private EvtPackage _evtPackage;

    void Start()
    {
        _evtPackage = __.Event.On(Evt.ACTIVATE_POINTER_AREA, (bool active) =>
        {
            gameObject.SetActive(active);
        });
    }

    void OnDestroy()
    {
        _evtPackage.disposable.Dispose();
        // __.Event.Unregister(Evt.ACTIVATE_POINTER_AREA);
    }
}
