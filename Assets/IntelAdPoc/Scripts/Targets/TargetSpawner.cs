using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum RouteType
{
    Left, 
    Right,
};  

public class TargetSpawner : MonoBehaviour 
{
    public delegate void OnTargetHitEvent(TargetController target, int baseScore);
    public event OnTargetHitEvent OnTargetHit;
    public delegate void OnTargetMissedEvent();
    public event OnTargetMissedEvent OnTargetMissed;

    public TargetController Target;
	public List<Texture> TargetTextures;

    private List<List<Vector3>> _recieverRoutesLeft = new List<List<Vector3>>();
    private List<List<Vector3>> _recieverRoutesRight = new List<List<Vector3>>();
    private int _routeIndexLeft = 0;
    private int _routeIndexRight = 0;
    private bool _targetsStarted = false;
    private List<TargetController> _targetList = new List<TargetController>();
	private int _targetTextureIndex = 0;
	private System.Random rnd = new System.Random();

    public void StartTargets()
    {
		TargetController newTarget;
        _targetsStarted = true;
        GenerateRoutes();

		_routeIndexLeft = GetRandomIndex(_routeIndexLeft, _recieverRoutesLeft);
		newTarget = Instantiate(Target) as TargetController;
        newTarget.OnRouteFinished += RouteFinished;
        newTarget.OnTargetHit += TargetHit;
        newTarget.OnTargetMissed += TargetMissed;
		_targetList.Add(newTarget);
        StartTarget(_recieverRoutesLeft, _routeIndexLeft, RouteType.Left);
        
		_routeIndexRight = GetRandomIndex(_routeIndexRight + 1, _recieverRoutesRight);
		newTarget = Instantiate(Target) as TargetController;
        newTarget.OnRouteFinished += RouteFinished;
        newTarget.OnTargetHit += TargetHit;
        newTarget.OnTargetMissed += TargetMissed;
        _targetList.Add(newTarget);
        StartTarget(_recieverRoutesRight, _routeIndexRight, RouteType.Right);
    }

    public void StopTargets()
    {
        _targetsStarted = false;
    }

    public void DestroyTargetsAndTargetSpawner()
    {
        for (int i = 0; i < _targetList.Count; i++)
            Destroy(_targetList[i].gameObject);
        Destroy(this);
    }

    private void StartTarget(List<List<Vector3>> routePath, int index, RouteType routeType)
    {
		TargetController newTarget = null;

		while (newTarget == null)
		{
			for (int i = 0; i < _targetList.Count; i++)
			{
				if (_targetList[i].InUse == false)
					newTarget = _targetList[i];
			}
		}

		newTarget.InitTargetController(routePath[index], routeType, TargetTextures[_targetTextureIndex]);
		IncrementTargetTextureIndex();
    }

//	public static void ClearConsole()
//	{
//		var assembly = Assembly.GetAssembly(typeof(SceneView));
//		var type = assembly.GetType("UnityEditor.LogEntries");
//		var method = type.GetMethod("Clear");
//		method.Invoke(new object(), null);
//	}

    void RouteFinished(RouteType routeType, TargetController target)
    {            
        if (_targetsStarted)
        {
            if (routeType == RouteType.Left)
            {
				_routeIndexLeft = GetRandomIndex(_routeIndexLeft, _recieverRoutesLeft);
                StartTarget(_recieverRoutesLeft, _routeIndexLeft, RouteType.Left);
            }
            if (routeType == RouteType.Right)
            {
				_routeIndexRight = GetRandomIndex(_routeIndexRight, _recieverRoutesRight);
                StartTarget(_recieverRoutesRight, _routeIndexRight, RouteType.Right);
            }
        }
    }

    void TargetHit(TargetController target, int baseScore)
    {
        OnTargetHit(target, baseScore);
    }

    void TargetMissed()
    {
        OnTargetMissed();
    }

    void GenerateRoutes()
    {
		// this is quick implementation, TargetController should use a model and be generated from a json file      
		List<Vector3> tempRoute;

		// left routes	      
		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-9.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-9.5f, 0.0f, 20.0f));

        _recieverRoutesLeft.Add(tempRoute);  

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-9.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-9.5f, 0.0f, 6.0f));
		tempRoute.Add(new Vector3(-3.5f, 0.0f, 16.0f));

        _recieverRoutesLeft.Add(tempRoute);  

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-10.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-10.5f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(-1.0f, 0.0f, 14.0f));

        _recieverRoutesLeft.Add(tempRoute);
        
        tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-7.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-7.0f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(-1.0f, 0.0f, 8.0f));

        _recieverRoutesLeft.Add(tempRoute);

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-9.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-9.0f, 0.0f, 6.0f));
        tempRoute.Add(new Vector3(-2.0f, 0.0f, 12.5f));
        
        _recieverRoutesLeft.Add(tempRoute);      

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(-8.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(-8.0f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(-1.0f, 0.0f, 12.0f));

        _recieverRoutesLeft.Add(tempRoute);      

		// right routes      
		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(9.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(9.5f, 0.0f, 20.0f));
        
        _recieverRoutesRight.Add(tempRoute); 

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(10.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(10.5f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(1.0f, 0.0f, 14.0f));

        _recieverRoutesRight.Add(tempRoute);

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(8.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(8.0f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(1.0f, 0.0f, 12.0f));

        _recieverRoutesRight.Add(tempRoute);      

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(7.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(7.0f, 0.0f, 8.0f));
        tempRoute.Add(new Vector3(1.0f, 0.0f, 8.0f));

        _recieverRoutesRight.Add(tempRoute);      

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(9.0f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(9.0f, 0.0f, 6.0f));
        tempRoute.Add(new Vector3(2.0f, 0.0f, 12.5f));
        
        _recieverRoutesRight.Add(tempRoute);  

		tempRoute = new List<Vector3>();
        tempRoute.Add(new Vector3(9.5f, 0.0f, 2.0f));
        tempRoute.Add(new Vector3(9.5f, 0.0f, 6.0f));
        tempRoute.Add(new Vector3(3.5f, 0.0f, 16.0f));

        _recieverRoutesRight.Add(tempRoute);  
    }

    /// <summary>
    /// Gets the random index from a list excluding currentIndex.
    /// </summary>
    /// <param name="currentIndex">Current index.</param>
    /// <param name="list">List.</param>
	int GetRandomIndex(int currentIndex, List<List<Vector3>> list)
	{
		int newIndex = rnd.Next(0, list.Count - 2);

		if (newIndex >= currentIndex)
			newIndex = newIndex + 1;

		return newIndex;
	}

	void IncrementTargetTextureIndex()
    {
		_targetTextureIndex = _targetTextureIndex + 1;
		if (_targetTextureIndex == TargetTextures.Count)
			_targetTextureIndex = 0;
    }
}
