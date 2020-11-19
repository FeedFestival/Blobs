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
    public bool HasAnyNeighbors;
    public bool CanDestroyNeighbors;
    private int? _forcePushTweenId;
    private Vector2 _initialPos;
    private int? _reflectTweenId;
    private Vector2 _reflectToPos;
    List<int> _linkedNeighbors;
    public List<StickingGlue> StickingGlues;

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
        StickedTo = new List<int>();
        if (createdInRow)
        {
            SetId();
            StickedTo.Add(HiddenSettings._.CeilId);
        }
        else
        {
            gameObject.tag = "Blob";
        }
        gameObject.layer = LayerMask.NameToLayer("Blob");

        if (Game._.Level<LevelRandomRanked>().debugLvl._debugBlobs)
        {
            if (IdText != null) IdText.text = Id.ToString();
        }

        SetWorldPosition(pos);
        gameObject.transform.position = Pos;
    }

    public void SetId()
    {
        Id = Game._.GetUniqueId();
        gameObject.name = "_Blob " + Id;
    }

    internal void Descend()
    {
        float spacing = Game._.Level<LevelRandomRanked>().Spacing;
        SetWorldPosition(new Vector3(Pos.x, Pos.y - spacing, Pos.z));
        if (Pos.y < HiddenSettings._.WallStickLimit)
        {
            RemoveSticked(HiddenSettings._.CeilId);
        }
    }

    private void SetWorldPosition(Vector3 pos)
    {
        Pos = pos;
        // Debug.Log("blob" + Id + " Pos: " + Pos);
    }

    internal void SetColor(BlobColor blobColor, bool instant = true)
    {
        BlobColor = blobColor;
        if (instant)
        {
            Sprite.color = Game._.Level<LevelRandomRanked>().GetColorByBlobColor(BlobColor);
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
            if (Game._.Level<LevelRandomRanked>().debugLvl._neighborsProcess)
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

        if (Game._.Level<LevelRandomRanked>().debugLvl._debugBlobs
            && Game._.Level<LevelRandomRanked>().debugLvl._neighborsProcess)
        {
            if (NeighborText != null) NeighborText.text = utils.DebugList<int>(Neighbors);
        }
    }

    internal void Kill()
    {
        List<Blob> blobsRef = Game._.Level<LevelRandomRanked>().Blobs;

        foreach (var stickedTo in StickedTo)
        {
            int index = blobsRef.FindIndex(b => b.Id == stickedTo);
            if (index >= 0 && index < blobsRef.Count)
            {
                blobsRef[index].RemoveSticked(Id);
                utils.AddIfNone(blobsRef[index].Id, ref Game._.Level<LevelRandomRanked>().Affected);
            }
        }
        if (Game._.Level<LevelRandomRanked>().debugLvl._stickingProcess)
        {
            Debug.Log(utils.DebugList<int>(StickedTo, "StickedTo"));
        }

        GetComponent<CircleCollider2D>().enabled = false;
        if (Game._.Level<LevelRandomRanked>().debugLvl._blobKilling)
        {
            float transparency = 0.15f;
            Color color = Game._.Level<LevelRandomRanked>().GetColorByBlobColor(BlobColor);
            color.a = transparency;
            Sprite.color = color;
            color = HiddenSettings._.White;
            color.a = transparency;
            IdText.color = color;
            color = HiddenSettings._.White;
            color.a = transparency;
            NeighborText.color = color;
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

        if (Game._.Level<LevelRandomRanked>().debugLvl._neighborsProcess)
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

    public void StickTo(Blob otherBlob, bool viceVersa = true)
    {
        if (viceVersa)
        {
            if (Game._.Level<LevelRandomRanked>().debugLvl._stickingProcess)
            {
                Debug.Log("blob" + Id + " sticked to otherBlob" + otherBlob.Id);
            }

            int index = StickingGlues.FindIndex(sG => sG.gameObject.activeSelf == false);
            if (otherBlob.StickedTo.Contains(Id) == false)
            {
                StickingGlues[index].SetStickedTo(stickedTo: otherBlob, BlobColor);
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
            if (Game._.Level<LevelRandomRanked>().debugLvl._stickingProcess)
            {
                Debug.Log("blob" + Id + " removed stickedBlob[" + id + "]");
            }
            StickedTo.RemoveAt(index);

            index = StickingGlues.FindIndex(sG => sG.StickedTo == id);
            if (index >= 0)
            {
                StickingGlues[index].Unstick();
            }
        }
    }

    public void CheckSurroundings(Blob otherBlob = null)
    {
        if (otherBlob == null)
        {
            StickedTo.Add(HiddenSettings._.CeilId);
        }

        List<Blob> proximityBlobs = FindBlobsInProximity();
        AnimateShockwave(proximityBlobs);
        SetupNeighbors(proximityBlobs, otherBlob);
        CanDestroyNeighbors = MeetsRequirementsToDestroy();
    }

    private List<Blob> FindBlobsInProximity()
    {
        return Game._.Level<LevelRandomRanked>().Blobs
            .Where(b =>
            {
                float distance = 0;
                bool inProximity = BlobService.AreClose(transform, b.transform, ref distance, proximity: true);
                if (Game._.Level<LevelRandomRanked>().debugLvl._proximity)
                {
                    Debug.Log("blob" + Id + " and proximityBlob" + b.Id + " distance: " + distance +
                        "(min: " + HiddenSettings._.NeighborProximity + ") inProximity: " + inProximity);
                }
                return inProximity;
            }).ToList();
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
            if (Game._.Level<LevelRandomRanked>().debugLvl._proximity && areNeighbors)
            {
                Debug.Log("From Proximity found blob" + proximityBlob.Id + " as neighbor");
            }
            if (areNeighbors && HasAnyNeighbors == false)
            {
                HasAnyNeighbors = true;
            }
        }
    }

    public void AnimateElasticSettle(BlobHitStickyInfo blobHitStickyInfo)
    {

        if (_reflectTweenId.HasValue)
        {
            LeanTween.cancel(_reflectTweenId.Value);
            _reflectTweenId = null;
        }

        // BUG Here when hittings sticky ceil
        Vector2 dirFromOtherBlobToThisOne = (transform.localPosition - blobHitStickyInfo.otherBlob.transform.localPosition).normalized;
        // Debug.Log("dirFromOtherBlobToThisOne: " + dirFromOtherBlobToThisOne);
        Ray ray = new Ray(blobHitStickyInfo.otherBlob.transform.localPosition, dirFromOtherBlobToThisOne);
        Vector2 pos = ray.GetPoint(0.5f);
        _initialPos = pos;

        _reflectToPos = (Vector2)transform.localPosition + ((Vector2)blobHitStickyInfo.ReflectDir.normalized * HiddenSettings._.BallStickyReflectDistanceModifier);
        _reflectTweenId = LeanTween.moveLocal(gameObject,
            _reflectToPos,
            HiddenSettings._.BlobForcePushAnimL
            ).id;
        LeanTween.descr(_reflectTweenId.Value).setEase(LeanTweenType.easeOutExpo);
        LeanTween.descr(_reflectTweenId.Value).setOnComplete(() =>
        {
            ElasticBack();
        });
    }

    private void AnimateShockwave(List<Blob> proximityBlobs)
    {
        foreach (Blob proxiBlob in proximityBlobs)
        {
            float distance = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.y),
                        new Vector2(proxiBlob.transform.position.x, proxiBlob.transform.position.y)
                    );
            Vector2 dir = (proxiBlob.transform.position - transform.position).normalized;
            var proxiBlobDir = (dir * ((1 - distance) + 0.1f)) * 0.2f;
            proxiBlob.ForcePush(proxiBlobDir);
        }
    }

    private void ForcePush(Vector2 proxiBlobDir)
    {
        if (_forcePushTweenId.HasValue)
        {
            LeanTween.cancel(_forcePushTweenId.Value);
            _forcePushTweenId = null;
        }
        _initialPos = transform.localPosition;
        _forcePushTweenId = LeanTween.moveLocal(gameObject,
            _initialPos + proxiBlobDir,
            HiddenSettings._.BlobForcePushAnimL
            ).id;
        LeanTween.descr(_forcePushTweenId.Value).setEase(LeanTweenType.easeOutExpo);
        LeanTween.descr(_forcePushTweenId.Value).setOnComplete(() =>
        {
            ElasticBack();
        });
    }

    private void ElasticBack()
    {
        if (_forcePushTweenId.HasValue)
        {
            LeanTween.cancel(_forcePushTweenId.Value);
            _forcePushTweenId = null;
        }
        _forcePushTweenId = LeanTween.moveLocal(gameObject,
            _initialPos,
            HiddenSettings._.BlobElasticBackAnimL
            ).id;

        List<int> eases = new List<int>() {
            (int)LeanTweenType.easeInOutBack,
            (int)LeanTweenType.easeOutElastic,
            (int)LeanTweenType.easeOutBounce
        };
        LeanTweenType ease = (LeanTweenType)percent.GetRandomFromList<int>(eases);

        LeanTween.descr(_forcePushTweenId.Value).setEase(ease);
    }

    private bool MeetsRequirementsToDestroy()
    {
        if (HasAnyNeighbors)
        {
            _linkedNeighbors = new List<int>();
            FindLinkedNeighbors(this);

            if (Game._.Level<LevelRandomRanked>().debugLvl._destroyCheck)
            {
                Debug.Log(utils.DebugList<int>(_linkedNeighbors, "_linkedNeighbors"));
            }

            if (_linkedNeighbors.Count > HiddenSettings._.MinNeighborCountToDestroy)
            {
                _linkedNeighbors = null;
                return true;
            }
        }

        if (Game._.Level<LevelRandomRanked>().debugLvl._destroyCheck)
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
            Blob foundNeighbor = Game._.Level<LevelRandomRanked>().Blobs
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
