using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCallback : MonoBehaviour
{
    public delegate void OnTriggered(int id);
    private OnTriggered _onTriggered;

    public void TriggerEntered(OnTriggered onTriggered)
    {
        if (_onTriggered == null)
        {
            _onTriggered = onTriggered;
        }
        gameObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Blob")
        {
            Debug.Log(collider.name);
            _onTriggered(collider.GetComponent<Blob>().Id);
        }
    }
}
