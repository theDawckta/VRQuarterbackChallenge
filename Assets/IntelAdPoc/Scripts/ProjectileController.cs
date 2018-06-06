using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class ProjectileController : MonoBehaviour
{
    public delegate void OnProjectileHitEvent(string colliderLayerName, ProjectileController projectile);
    public event OnProjectileHitEvent OnProjectileHit;

    [HideInInspector]
    public bool HitSomeThing = false;
    public float Speed = 10.0f;
	public GameObject ParticleHeatingUp;
	public GameObject ParticleOnFire;
	public GameObject ParticleBubbles;
	public GameObject FootballProjectile;
	public GameObject FizzyCanProjectile;

    private Vector3 _targetPosition;
    private Rigidbody _rigid;
    private bool _rotateProjectile = true;
	private TrailRenderer _trailRenderer;

    // Use this for initialization
    void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
		_trailRenderer = this.gameObject.GetComponent<TrailRenderer> ();

		if(_trailRenderer == null)
			throw new Exception("A TrailRender must exist on the projectile.");

		Hide (ParticleHeatingUp);
		Hide (ParticleOnFire);
		Hide (ParticleBubbles);
		Hide (FizzyCanProjectile);
		_trailRenderer.enabled = false;
	
    }

    void Update()
    {
        if (_rotateProjectile)
            transform.rotation = Quaternion.LookRotation(_rigid.velocity) * Quaternion.Euler(90.0f, 0.0f, 0.0f);
    }

	void Hide(GameObject objectToHide)
	{
		if (objectToHide != null)
			objectToHide.SetActive (false);
	}

	void Show(GameObject objectToShow)
	{
		if (objectToShow != null)
			objectToShow.SetActive (true);
	}

    public void SetTarget(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }

	void SetProjectileAndTrails(int streak)
	{

		switch (streak)
		{
		case 0:
		case 1:
		case 2:
			_trailRenderer.enabled = true;
			break;
		case 3:
			Show (ParticleHeatingUp);
			break;
		case 4:
			Show (ParticleOnFire);
			break;
		case 5:
		default: 
			Hide (FootballProjectile);
			Show (FizzyCanProjectile);
			Show (ParticleBubbles);
			break;
		}
	}

	public void Launch(int streak = 0)
    {
		
		SetProjectileAndTrails (streak);
        // calculate vectors
        Vector3 toTarget = _targetPosition - transform.position;
        Vector3 toTargetXZ = toTarget;
        toTargetXZ.y = 0;

        // calculate xz and y
        float y = toTarget.y;
        float xz = toTargetXZ.magnitude;

        // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
        // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
        // so xz = v0xz * t => v0xz = xz / t
        // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
        float t = toTarget.magnitude / Speed;
        float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
        float v0xz = xz / t;

        // create result vector for calculated starting speeds
        Vector3 result = toTargetXZ.normalized;        // get direction of xz but with magnitude 1
        result *= v0xz;                                // set magnitude of xz to v0xz (starting speed in xz plane)
        result.y = v0y;                                // set y to v0y (starting speed of y plane)

        _rigid.velocity = result;
    }

    void OnCollisionEnter(Collision collision)
    {
        OnProjectileHit(LayerMask.LayerToName(collision.gameObject.layer), this);
        // _rotateProjectile just hanging around in case we want to have the projectile persist on field
        _rotateProjectile = false;
        Destroy(gameObject);
    }
}
