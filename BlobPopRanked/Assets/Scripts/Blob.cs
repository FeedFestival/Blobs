using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Assets.Scripts.utils;

public class Blob : MonoBehaviour
{
    public int Id;
    public Vector3 Pos;
    [SerializeField]
    public BlobColor BlobColor;
    public SpriteRenderer Sprite;
    public TextMesh IdText;
    public TextMesh NeighborText;
    [Header("Game Props")]
    public List<int> Neighbors;
    public List<int> StickedTo;
    public float Radius;
    private float? _radius;

    internal float GetRadius()
    {
        if (_radius.HasValue == false)
        {
            _radius = GetComponent<CircleCollider2D>().radius;
            Radius = _radius.Value;
        }
        return _radius.Value;
    }

    internal void SetPosition(Vector3 pos, bool createdInRow = true)
    {
        Id = Game._.GetUniqueId();
        IdText.text = Id.ToString();
        gameObject.name = "_Blob " + Id;

        StickedTo = new List<int>();
        if (createdInRow)
        {
            StickedTo.Add(HiddenSettings._.CeilId);
        }
        else
        {
            gameObject.tag = "Blob";
        }

        Pos = pos;
        gameObject.transform.position = Pos;
    }

    internal void Descend()
    {
        float spacing = Game._.Level<LevelRandomRanked>().Spacing;
        Pos = new Vector3(Pos.x, Pos.y - spacing, Pos.z);
        if (Pos.y < HiddenSettings._.WallStickLimit)
        {
            RemoveSticked(HiddenSettings._.CeilId);
        }
        gameObject.transform.position = Pos;
    }

    private void RemoveSticked(int id)
    {
        int index = StickedTo.FindIndex(s => s == id);
        if (index >= 0)
        {
            // Debug.Log("blob" + Id + " posY: " + Pos.y + " removed CeilIndex: " + index);
            StickedTo.RemoveAt(index);
        }
    }

    internal void SetColor(BlobColor blobColor, bool instant = true)
    {
        BlobColor = blobColor;
        if (instant)
        {
            Sprite.color = HiddenSettings._.GetColorByBlobColor(BlobColor);
        }
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
            otherBlob = Game._.Level<LevelRandomRanked>().GetBlobById(id.Value);
        }


        if (Neighbors.Contains(id.Value))
        {
            if (Game._.Level<LevelRandomRanked>().DebugController.NeighborDebug == true)
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
        ShowNeighbors_Debug();
    }

    internal void Kill()
    {
        List<Blob> blobsRef = Game._.Level<LevelRandomRanked>().Blobs;
        string debug = "";
        foreach (var stickedTo in StickedTo)
        {
            if (Game._.Level<LevelRandomRanked>().DebugController.DebugRemoveSticky)
            {
                debug += stickedTo + ", ";
            }

            int index = blobsRef.FindIndex(b => b.Id == stickedTo);
            if (index >= 0 && index < blobsRef.Count)
            {
                blobsRef[index].RemoveSticked(Id);
                utils.AddIfNone(blobsRef[index].Id, ref Game._.Level<LevelRandomRanked>().Affected);
            }
        }
        if (Game._.Level<LevelRandomRanked>().DebugController.DebugRemoveSticky)
        {
            Debug.Log("StickedTo: " + debug);
        }

        GetComponent<CircleCollider2D>().enabled = false;
        float transparency = 0.15f;
        Color color = HiddenSettings._.GetColorByBlobColor(BlobColor);
        color.a = transparency;
        Sprite.color = color;
        color = HiddenSettings._.White;
        color.a = transparency;
        IdText.color = color;
        color = HiddenSettings._.White;
        color.a = transparency;
        NeighborText.color = color;
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
            if (Game._.Level<LevelRandomRanked>().DebugController.DebugStickingTo)
            {
                Debug.Log("blob" + Id + " sticked to otherBlob" + otherBlob.Id);
            }
            StickTo(otherBlob);
            otherBlob.StickTo(this);
        }

        if (Game._.Level<LevelRandomRanked>().DebugController.NeighborDebug == true)
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

        var areSameColor = BlobColor == otherBlob.BlobColor;
        return areClose && areSameColor;
    }

    private void ShowNeighbors_Debug()
    {
        NeighborText.text = "";
        foreach (var n in Neighbors)
        {
            NeighborText.text += n.ToString() + ", ";
        }
    }

    public void StickTo(Blob otherBlob)
    {
        if (StickedTo.Contains(otherBlob.Id))
        {
            return;
        }
        StickedTo.Add(otherBlob.Id);
        StickedTo = StickedTo.OrderByDescending(s => s).ToList();
    }

    public void CheckSurroundings(Blob otherBlob = null)
    {
        bool hasAnyNeighbors = otherBlob == null ? false
            : IfNeighborAdd(otherBlob: otherBlob);
        bool canDestroyNeighbors = otherBlob == null ? false
            : otherBlob.Neighbors.Count > 1;
        bool canDestroy;

        List<Blob> blobsRef = Game._.Level<LevelRandomRanked>().Blobs;
        foreach (var proximityBlob in blobsRef)
        {
            if (otherBlob != null && proximityBlob.Id == otherBlob.Id)
            {
                continue;
            }
            float distance = 0;
            var inProximity = BlobService.AreClose(transform, proximityBlob.transform, ref distance, proximity: true);
            if (inProximity)
            {
                if (Game._.Level<LevelRandomRanked>().DebugController.DebugProximity)
                {
                    Debug.Log("blob" + Id + " and proximityBlob" + proximityBlob.Id +
                    " distance: " + distance +
                    "(min: " + HiddenSettings._.NeighborProximity +
                    ") inProximity: " + inProximity);
                }

                bool areNeighbors = IfNeighborAdd(otherBlob: proximityBlob);
                if (Game._.Level<LevelRandomRanked>().DebugController.DebugProximity && areNeighbors)
                {
                    Debug.Log("From Proximity found blob" + proximityBlob.Id + " as neighbor");
                }
                if (areNeighbors && hasAnyNeighbors == false)
                {
                    hasAnyNeighbors = true;
                }
                canDestroy = proximityBlob.Neighbors.Count > 1;
                if (canDestroy && canDestroyNeighbors == false)
                {
                    canDestroyNeighbors = true;
                }
            }
        }

        canDestroy = Neighbors.Count > 1;
        if (canDestroy && canDestroyNeighbors == false)
        {
            canDestroyNeighbors = true;
        }

        Game._.Level<LevelRandomRanked>().TryDestroyNeighbors(hasAnyNeighbors, canDestroyNeighbors, this);
    }
}
