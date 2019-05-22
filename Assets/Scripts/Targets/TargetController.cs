using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public delegate void OnRouteFinishedEvent(RouteType routeType, TargetController target);
    public event OnRouteFinishedEvent OnRouteFinished;
    public delegate void OnTargetHitEvent(TargetController target, int baseScore);
    public event OnTargetHitEvent OnTargetHit;
    public delegate void OnTargetMissedEvent();
    public event OnTargetMissedEvent OnTargetMissed;

    [HideInInspector]
    public bool IsCurrent;
    [HideInInspector]
    public bool IsFrozen = false;
	[HideInInspector]
    public bool InUse = false;
	public float TargetSpeed = 3.8f;
    public RouteType TargetRouteType { get; private set; }
    public float Height = 2.0f;
    public string RouteLayerName;
    public GameObject Marker;
    public GameObject Target;
    public GameObject PlaneColliderHolder;
    public GameObject MovingTarget;
    public TargetParticleController TargetParticle;
	public float FlashSpeed = 0.5f;
	public MeshRenderer CushionRenderer;
	public MeshRenderer StandRenderer;

    private LineRenderer _routeLine;
    private Vector3[] _routePositions;
    private float _routeTime;
    private int _baseScore;
    private int _score;
	private int _streak;
    private Vector3 _originalMarkerScale;
    private string _targetObjectName;
	private Renderer _targetRenderer;
	private float _routeDistance = 0.0f;
	private float _distanceTraveled = 0.0f;
	private bool _getDistance = false;
	private bool _flashing = false;
	private Vector3 _lastPosition;
	private float _startScale;

    void Awake()
    {
        _routeLine = GetComponent<LineRenderer>();
        _originalMarkerScale = Marker.transform.localScale;
        _targetObjectName = Target.name;
        Marker.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
		_targetRenderer = Target.GetComponent<Renderer>();
		_startScale = MovingTarget.transform.localScale.x;
		MovingTarget.transform.localScale = Vector3.zero;
    }

    void Update()
    {
		Target.transform.LookAt(Camera.main.transform, Vector3.right);
		Target.transform.eulerAngles = new Vector3(-90.0f, 90.0f, 0.0f) + Target.transform.eulerAngles;
        Marker.transform.LookAt(Camera.main.transform);

		if (_getDistance && (_routeDistance - _distanceTraveled > 0.86f))
		{
			_distanceTraveled = _distanceTraveled + Vector3.Distance(MovingTarget.transform.position, _lastPosition);
			_lastPosition = MovingTarget.transform.position;
		}
		else if(!_flashing)
		{
			MovingTarget.transform.DOScale(0.0f, 0.2f).SetEase(Ease.InBack);
			_getDistance = false;
			_flashing = true;
		}
    }

	public void InitTargetController(List<Vector3> positions, RouteType routeType, Texture targetTexture = null)
    {
		if(targetTexture != null)
			_targetRenderer.material.mainTexture = targetTexture;
		
        _routeLine.positionCount = positions.Count;
        _routeLine.SetPositions(positions.ToArray());
        TargetRouteType = routeType;

        // draw planes off of line render to have something to hit test against for marker placement
        for (int i = 0; i < positions.Count - 1; i++)
        {
            Vector3 centerPoint;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);         
            Destroy(cube.GetComponent<MeshRenderer>());

			// used for getting total distance traveled for target
			_routeDistance = _routeDistance + Vector3.Distance(positions[i], positions[i + 1]);

            cube.GetComponent<Collider>().isTrigger = true;
            cube.layer = LayerMask.NameToLayer(RouteLayerName);
            centerPoint = Vector3.Lerp(positions[i], positions[i + 1], 0.5f);
            cube.transform.SetParent(PlaneColliderHolder.transform);
            cube.transform.position = new Vector3(centerPoint.x, centerPoint.y + (Height / 5), centerPoint.z);
            cube.transform.rotation = Quaternion.FromToRotation(Vector3.left, positions[i] - positions[i + 1]);
            cube.transform.localScale = new Vector3(Vector3.Distance(positions[i], positions[i + 1]), Height, 0.01f);
        }

		_routeTime = _routeDistance / TargetSpeed;
		StartTarget();
    }
		
    void StartTarget()
    {
		InUse = true;
        _routePositions = new Vector3[_routeLine.positionCount];
        _routeLine.GetPositions(_routePositions);
		_routeLine.enabled = true;
		MovingTarget.transform.position = _routePositions[0];
		_lastPosition = _routePositions[0];
		MovingTarget.transform.DOScale(_startScale, 0.2f).SetEase(Ease.OutBack);
        MovingTarget.transform.DOPath(_routePositions, _routeTime).SetEase(Ease.Linear).OnComplete(() => RouteFinished(TargetRouteType));
		//_lastPosition = MovingTarget.transform.position;
		_getDistance = true;
    }
    
    public void PositionMarker(Vector3 hitPosition)
    {
        if(!IsFrozen)
            Marker.transform.position = new Vector3(hitPosition.x, hitPosition.y, hitPosition.z) + (Vector3.back) * 0.1f;
    }

    public void ShowMarker()
    {   
        if(Marker.transform.localScale == Vector3.zero)
            Marker.transform.DOScale(_originalMarkerScale, 0.3f);
    }

    public void HideMarker(bool force = false)
    {
        Marker.transform.DOScale(0.0f, 0.3f).OnComplete(() => {
            if (IsCurrent && !force)
                ShowMarker();

            IsFrozen = false;
        });
    }

	void StartFlashing()
	{
		CushionRenderer.enabled = !CushionRenderer.enabled;
		StandRenderer.enabled = !StandRenderer.enabled;
	}

    public void RouteFinished(RouteType routeType)
	{
		ResetTarget();
		OnRouteFinished(routeType, this);  
    }

    public void OnCollisionEnter(Collision collider)
    {
        ProjectileController projectile = collider.gameObject.GetComponent<ProjectileController>();
        if (projectile != null && projectile.HitSomeThing == false)
        {
            projectile.HitSomeThing = true;
            for (int i = 0; i < collider.contacts.Length; i++)
            {
                if (IsFrozen)
                {
                    HideMarker();
                }
                if (collider.contacts[i].thisCollider.name == _targetObjectName)
                {
                    _baseScore = (int)MovingTarget.transform.position.z;
					if (OnTargetHit != null)
                        OnTargetHit(this, _baseScore);
                    
                    TargetParticleController targetParticle = Instantiate(TargetParticle, Target.transform.position, Quaternion.identity) as TargetParticleController;
					targetParticle.SetText(_score.ToString(), _streak);

					ResetTarget();
					if (OnRouteFinished != null)
                        OnRouteFinished(TargetRouteType, this);
                }
            }
        }
	}

    void ResetTarget()
	{
		DOTween.Kill(MovingTarget.transform);
        _routeLine.enabled = false;
        InUse = false;
        _flashing = false;
        _getDistance = false;
        _distanceTraveled = 0.0f;
        _routeDistance = 0.0f;
        foreach (Transform child in PlaneColliderHolder.transform)
            Destroy(child.gameObject);
	}

    public void SetScore(int score, int streak)
    {
        _score = score;
		_streak = streak;
    }

	public void OnTriggerEnter(Collider other)
	{
        ProjectileController projectile = other.gameObject.GetComponent<ProjectileController>();
        if(projectile != null && projectile.HitSomeThing == false)
        {
            projectile.HitSomeThing = true;
            if (IsFrozen)
            {
                HideMarker();
            }
            OnTargetMissed();
        }
	}
}
