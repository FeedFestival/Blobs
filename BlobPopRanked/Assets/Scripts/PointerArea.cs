using Assets.HeadStart.Core;
using UnityEngine;

public class PointerArea : MonoBehaviour
{
    void Start()
    {
        __.Event.On(Evt.ACTIVATE_POINTER_AREA, (bool active) =>
        {
            gameObject.SetActive(active);
        });
    }
}
