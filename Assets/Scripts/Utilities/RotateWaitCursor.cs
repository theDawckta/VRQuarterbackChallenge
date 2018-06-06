using UnityEngine;
using System.Collections;

public class RotateWaitCursor : MonoBehaviour
{
    public Vector3 rotateSpeeds = new Vector3(0.0f, 0.0f, -60.0f);

    /// <summary>
    /// Auto rotates the attached cursor.
    /// </summary>
    void Update()
    {
    	if(gameObject.activeSelf == true)
        	transform.Rotate(rotateSpeeds * Time.smoothDeltaTime);
    }
}
