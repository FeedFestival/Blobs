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
            var blob = GetComponent<Blob>();

            if (col.transform.tag == "Blob")
            {
                var otherBlob = col.gameObject.GetComponent<Blob>();
                Game._.Player.BlobHitSticky(transform.position.y, blob, otherBlob);
            }
            else
            {
                Game._.Player.BlobHitSticky(transform.position.y, blob);
            }

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

    private void ReflectOffSurface(Vector3 collisionNormal)
    {
        var direction = Vector3.Reflect(lastVelocity.normalized, collisionNormal).normalized;
        _rb.velocity = direction * Mathf.Max(_desiredSpeed, 0f);
    }
}
