using System.Collections.Generic;
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
    List<IPoolObject> _allBlobs;

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
        return poolBlob;
    }

    IPoolObject CreateBlob(int? index)
    {
        if (index.HasValue == false)
        {
            index = _allBlobs.Count + 1;
        }
        GameObject go = HiddenSettings._.GetAnInstantiated(PrefabBank._.Blob);
        go.transform.SetParent(this.transform);
        var blob = go.GetComponent<Blob>();
        return blob;
        // var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.PointText);
        // go.transform.SetParent(HolderT);
        // var rect = go.GetComponent<RectTransform>();
        // rect.localScale = Vector3.one;
        // var textComponent = go.GetComponent<Text>();
        // var pointParticle = go.GetComponent<PointTextParticle>() as IPointText;
        // pointParticle.Init(index.Value, textComponent);
        // return pointParticle;
    }
}
