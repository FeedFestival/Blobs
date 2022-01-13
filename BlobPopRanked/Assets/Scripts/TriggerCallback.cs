using Assets.BlobPopClassic;
using Assets.BlobPopClassic.Blobs;
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
        if (collider.tag == TAG.Blob)
        {
            Debug.Log(collider.name);
            // TODO: make an extensions for Blobs and move this to HeadStart
            _onTriggered(collider.GetComponent<Blob>().Bid);
        }
    }
}
