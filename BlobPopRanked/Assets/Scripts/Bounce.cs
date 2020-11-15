using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    public float MaxDesiredSpeed;
    public float _desiredSpeed;
    private float _minDesiredSpeed;
    private Rigidbody2D _rb;
    Vector3 lastVelocity;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _desiredSpeed = MaxDesiredSpeed;
        _minDesiredSpeed = MaxDesiredSpeed / 1.5f;
    }

    // void Start()
    // {
    //     Shoot();
    // }

    void FixedUpdate()
    {
        //Debug.Log(_rb.velocity.magnitude);
        if (_rb.velocity.magnitude > _desiredSpeed || _rb.velocity.magnitude < _desiredSpeed)
        {
            _rb.velocity = _desiredSpeed * _rb.velocity.normalized;
        }
        lastVelocity = _rb.velocity;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform.tag == "Blob" || col.transform.tag == "StickySurface")
        {
            _rb.velocity = Vector3.zero;
            BlobHitStickyInfo blobHitStickyInfo = new BlobHitStickyInfo(transform.position.y, GetComponent<Blob>());
            blobHitStickyInfo.ReflectDir = GetNormalizedDirection(col.contacts[0].normal);

            if (col.transform.tag == "Blob")
            {
                blobHitStickyInfo.otherBlob = col.gameObject.GetComponent<Blob>();
            }

            Game._.Player.BlobHitSticky(blobHitStickyInfo);

            Destroy(_rb);
            Destroy(this);
            return;
        }
        else if (col.transform.tag == "ReflectSurface")
        {
            ReflectOffSurface(col.contacts[0].normal);
        }
    }

    public void Shoot()
    {
        // var dir = new Vector3(0.5f, 0.5f, 0);
        var dir = Game._.Player.Target.position - transform.position;
        _rb.AddForce(new Vector3(dir.x, dir.y, 0) * _desiredSpeed);
    }

    private Vector3 GetNormalizedDirection(Vector3 collisionNormal)
    {
        return Vector3.Reflect(lastVelocity.normalized, collisionNormal).normalized;
    }

    private void ReflectOffSurface(Vector3 collisionNormal)
    {
        var direction = GetNormalizedDirection(collisionNormal);
        _rb.velocity = direction * Mathf.Max(_desiredSpeed, 0f);
    }
}
