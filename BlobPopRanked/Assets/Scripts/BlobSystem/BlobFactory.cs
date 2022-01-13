using System.Collections.Generic;
using Assets.Scripts.utils;
using UnityEngine;

public class BlobFactory : MonoBehaviour
{
    private static BlobFactory _blobFactory;
    public static BlobFactory _ { get { return _blobFactory; } }
    void Awake()
    {
        _blobFactory = this;
    }
    public int InitialBlobsCount;
    public List<IPoolObject> _allBlobs;

    public void Init()
    {
        _allBlobs = new List<IPoolObject>();
        for (var i = 0; i < InitialBlobsCount; i++)
        {
            CreateNewPoolBlob(i);
        }
    }

    public Transform BlobsParent()
    {
        return this.transform;
    }

    public IPoolObject GetAvailableBlob()
    {
        var poolBlob = _allBlobs.Find(pT => pT.IsUsed == false);
        // Debug.Log(__debug.DebugList<IPoolObject>(_allBlobs, "_allBlobs", (IPoolObject blob) => {
        //     return ", " + (blob as Blob).Bid + "" + (blob.IsUsed ? "[u] " : " ");
        // }));
        if (poolBlob == null)
        {
            poolBlob = CreateNewPoolBlob();
        }
        return poolBlob;
    }

    IPoolObject CreateNewPoolBlob(int? index = null)
    {
        var poolBlob = CreateBlob(index);
        poolBlob.Hide();
        _allBlobs.Add(poolBlob);
        return _allBlobs[_allBlobs.Count - 1];
    }

    IPoolObject CreateBlob(int? index)
    {
        if (index.HasValue == false)
        {
            index = _allBlobs.Count + 1;
        }
        var prefab = (Main._.Game as BlobPopGame).Blob;
        GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        go.transform.SetParent(this.transform);
        go.name = "Blob";
        var blob = go.GetComponent<Blob>();
        return blob;
    }

    public void AddNewBlobToPool(Blob newBlob)
    {   
        newBlob.transform.SetParent(this.transform);
        _allBlobs.Add(newBlob as IPoolObject);
    }
}
