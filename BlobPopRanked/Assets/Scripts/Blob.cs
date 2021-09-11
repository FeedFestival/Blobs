using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using System.Linq;
using Assets.Scripts.utils;

public class Blob : MonoBehaviour
{
    public int Id;
    public Vector3 Pos;
    [SerializeField]
    public BlobReveries BlobReveries;
    private BlobDebugInfo _blobDebugInfo;
    [Header("Game Props")]
    public List<int> Neighbors;
    public List<int> StickedTo;
    private float? _radius;
    public bool HasAnyNeighbors;
    public bool CanDestroyNeighbors;
    List<int> _linkedNeighbors;

    internal float GetRadius()
    {
        if (_radius.HasValue == false)
        {
            var fullSize = transform.localScale.z;
            var currentSize = transform.localScale.x;
            // Debug.Log("currentSize: " + currentSize);
            var perc = percent.What(currentSize, fullSize);
            var radius = GetComponent<CircleCollider2D>().radius;
            _radius = percent.Find(perc, radius);
        }
        return _radius.Value;
    }

    internal void SetPosition(Vector3 pos, bool createdInRow = true)
    {
        StickedTo = new List<int>();
        if (createdInRow)
        {
            SetId();
            StickedTo.Add(HiddenSettings._.CeilId);
        }
        else
        {
            gameObject.tag = TAG.Blob;
        }
        gameObject.layer = LayerMask.NameToLayer(LAYER.Blob);

        SetWorldPosition(pos);
        gameObject.transform.position = Pos;
    }

    public void SetId()
    {
        Id = Game._.GetUniqueId();
        gameObject.name = "_Blob " + Id;

        if (ClasicLv._.__debug__._debugBlobs)
        {
            var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.BlobDebugInfoPrefab);
            _blobDebugInfo = go.GetComponent<BlobDebugInfo>();
            _blobDebugInfo.SetId(transform, Id);
        }
    }

    internal void Descend()
    {
        float spacing = ClasicLv._.Spacing;
        SetWorldPosition(new Vector3(Pos.x, Pos.y - spacing, Pos.z));
        if (Pos.y < HiddenSettings._.WallStickLimit)
        {
            RemoveSticked(HiddenSettings._.CeilId);
        }
    }

    private void SetWorldPosition(Vector3 pos)
    {
        Pos = pos;
    }

    internal void AddNeighbor(int? id, Blob otherBlob = null, bool goDeep = true)
    {
        if (id.HasValue == false && otherBlob == null)
        {
            Debug.LogError("No Id or Blob");
            return;
        }
        if (id.HasValue == false)
        {
            id = otherBlob.Id;
        }
        if (otherBlob == null)
        {
            otherBlob = ClasicLv._.GetBlobById(id.Value);
        }

        if (Neighbors.Contains(id.Value))
        {
            if (ClasicLv._.__debug__._neighborsProcess)
            {
                Debug.LogWarning("      - blob(" + Id + ") has " + id + " as neighbor.");
            }
            return;
        }

        if (goDeep)
        {
            if (otherBlob.IsNeighbor(this))
            {
                otherBlob.AddNeighbor(id: null, otherBlob: this, goDeep: false);
            }
        }

        Neighbors.Add(id.Value);

        if (ClasicLv._.__debug__._debugBlobs
            && ClasicLv._.__debug__._neighborsProcess)
        {
            if (_blobDebugInfo != null) _blobDebugInfo.NeighborText.text = __debug.DebugList<int>(Neighbors);
        }
    }

    internal void Kill()
    {
        List<Blob> blobsRef = ClasicLv._.Blobs;

        foreach (var stickedTo in StickedTo)
        {
            int index = blobsRef.FindIndex(b => b.Id == stickedTo);
            if (index >= 0 && index < blobsRef.Count)
            {
                blobsRef[index].RemoveSticked(Id);
                __utils.AddIfNone(blobsRef[index].Id, ref ClasicLv._.Affected);
            }
        }
        if (ClasicLv._.__debug__._stickingProcess)
        {
            Debug.Log(__debug.DebugList<int>(StickedTo, "StickedTo"));
        }

        GetComponent<CircleCollider2D>().enabled = false;
        if (ClasicLv._.__debug__._blobKilling)
        {
            _blobDebugInfo.FakeKill(BlobReveries.BlobColor, ref BlobReveries.Sprite);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    internal void CalculateNeighbors(List<Blob> allBlobs)
    {
        foreach (var otherBlob in allBlobs)
        {
            IfNeighborAdd(otherBlob);
        }
    }

    internal bool IfNeighborAdd(Blob otherBlob, bool goDeep = true)
    {
        bool isNeighbor = IsNeighbor(otherBlob);
        if (isNeighbor)
        {
            AddNeighbor(id: null, otherBlob: otherBlob, goDeep: goDeep);
        }
        return isNeighbor;
    }

    internal bool IsNeighbor(Blob otherBlob)
    {
        float distance = 0;
        var areClose = BlobService.AreClose(transform, otherBlob.transform, ref distance);

        if (areClose)
        {
            StickTo(otherBlob);
        }

        if (ClasicLv._.__debug__._neighborsProcess)
        {
            var pre = "NO ... distance: " + distance;
            if (areClose)
            {
                pre = "YES !!! distance: " + distance;
            }
            Debug.Log(pre + " between " + Id + "(x:" + transform.position.x + ",y:" + transform.position.y + ") and "
                + otherBlob.Id + "(x:" + otherBlob.transform.position.x + ",y:" + otherBlob.transform.position.y + ") max: "
                + HiddenSettings._.NeighborTestDistance);
        }

        var areSameColor = BlobReveries.BlobColor == otherBlob.BlobReveries.BlobColor;
        return areClose && areSameColor;
    }

    public void StickTo(Blob otherBlob, bool viceVersa = true)
    {
        if (viceVersa)
        {
            if (ClasicLv._.__debug__._stickingProcess)
            {
                Debug.Log("blob" + Id + " sticked to otherBlob" + otherBlob.Id);
            }

            int index = BlobReveries.StickingGlues.FindIndex(sG => sG.gameObject.activeSelf == false);
            if (otherBlob.StickedTo.Contains(Id) == false)
            {
                BlobReveries.StickingGlues[index].SetStickedTo(stickedTo: otherBlob, BlobReveries.BlobColor);
            }

            otherBlob.StickTo(this, viceVersa: false);
        }

        if (StickedTo.Contains(otherBlob.Id))
        {
            return;
        }
        StickedTo.Add(otherBlob.Id);
        StickedTo = StickedTo.OrderByDescending(s => s).ToList();
    }

    private void RemoveSticked(int id)
    {
        int index = StickedTo.FindIndex(s => s == id);
        if (index >= 0)
        {
            if (ClasicLv._.__debug__._stickingProcess)
            {
                Debug.Log("blob" + Id + " removed stickedBlob[" + id + "]");
            }
            StickedTo.RemoveAt(index);
            BlobReveries.RemoveStickingGlue(id);
        }
    }

    public void CheckSurroundings(Blob otherBlob = null)
    {
        if (otherBlob == null)
        {
            StickedTo.Add(HiddenSettings._.CeilId);
        }

        List<Blob> proximityBlobs = BlobService.FindBlobsInProximity(ClasicLv._.Blobs, this);

        BlobReveries.AnimateShockwave(proximityBlobs);

        if (otherBlob != null)
        {
            otherBlob.BlobReveries
                .StretchOnCollision((otherBlob.transform.position - transform.position).normalized, null, hitSticky: true);
        }

        SetupNeighbors(proximityBlobs, otherBlob);
        CanDestroyNeighbors = MeetsRequirementsToDestroy();
    }

    private void SetupNeighbors(List<Blob> proximityBlobs, Blob otherBlob)
    {
        HasAnyNeighbors = otherBlob == null ? false
            : IfNeighborAdd(otherBlob: otherBlob);
        foreach (var proximityBlob in proximityBlobs)
        {
            if (otherBlob != null && proximityBlob.Id == otherBlob.Id)
            {
                continue;
            }
            bool areNeighbors = IfNeighborAdd(otherBlob: proximityBlob);
            if (ClasicLv._.__debug__._proximity && areNeighbors)
            {
                Debug.Log("From Proximity found blob" + proximityBlob.Id + " as neighbor");
            }
            if (areNeighbors && HasAnyNeighbors == false)
            {
                HasAnyNeighbors = true;
            }
        }
    }

    private bool MeetsRequirementsToDestroy()
    {
        if (HasAnyNeighbors)
        {
            _linkedNeighbors = new List<int>();
            FindLinkedNeighbors(this);

            if (ClasicLv._.__debug__._destroyCheck)
            {
                Debug.Log(__debug.DebugList<int>(_linkedNeighbors, "_linkedNeighbors"));
            }

            if (_linkedNeighbors.Count > HiddenSettings._.MinNeighborCountToDestroy)
            {
                _linkedNeighbors = null;
                return true;
            }
        }

        if (ClasicLv._.__debug__._destroyCheck)
        {
            Debug.Log("Has no Neighbors to Destroy");
        }
        return false;
    }

    private void FindLinkedNeighbors(Blob blob)
    {
        foreach (int neighborId in blob.Neighbors)
        {
            if (_linkedNeighbors.Contains(neighborId))
            {
                continue;
            }
            Blob foundNeighbor = ClasicLv._.Blobs
                    .FirstOrDefault(b => b.Id == neighborId);
            _linkedNeighbors.Add(neighborId);

            if (foundNeighbor == null)
            {
                continue;
            }

            FindLinkedNeighbors(foundNeighbor);
        }
    }
}
