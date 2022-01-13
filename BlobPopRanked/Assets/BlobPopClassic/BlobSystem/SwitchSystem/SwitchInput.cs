using Assets.HeadStart.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwitchInput : MonoBehaviour
{
    void Start()
    {
        __.Event.On(Evt.STOP_SHOOTING, (object obj) =>
        {
            this.gameObject.SetActive(!!!(obj as SwitchBlobEvent).IsSwitchInProgress);
        });
    }

    public void PointerDown(BaseEventData baseEventData)
    {
        __.Event.Emit(Evt.SWITCH_BLOBS);
    }
}
