using UnityEngine;
using UnityEngine.EventSystems;

public class SwitchInput : MonoBehaviour
{
    void Start()
    {
        EventBus._.On(Evt.STOP_SHOOTING, (object obj) =>
        {
            this.gameObject.SetActive(!!!(obj as SwitchBlobEvent).IsSwitchInProgress);
        });
    }

    public void PointerDown(BaseEventData baseEventData)
    {
        EventBus._.Emit(Evt.SWITCH_BLOBS);
    }
}
