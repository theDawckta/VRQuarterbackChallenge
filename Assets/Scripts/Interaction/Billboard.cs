using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public GameObject ControlsHolder;
    public Camera UICamera;
    public bool DoChildren;
	public bool AllowRotation = true;

    private Transform _cameraTransform;
    private List<Interactible> _interactibles = new List<Interactible>();

    void Awake()
    {
        _interactibles = gameObject.GetComponentsInChildren<Interactible>(true).ToList();

        _cameraTransform = UICamera.transform;

		if(AllowRotation)
	        FaceCamera(gameObject.transform);

        if (DoChildren)
        {
            foreach (Interactible interactible in _interactibles)
            {
                FaceCamera(interactible.transform);
            }
        }

        ControlsHolder.transform.parent = null;
		transform.position = _cameraTransform.position+(1.5f* Vector3.up)+(Vector3.forward);
        ControlsHolder.transform.SetParent(transform, true);
    }

    void FaceCamera(Transform transform)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position, Vector3.up);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0.0f);
    }
}