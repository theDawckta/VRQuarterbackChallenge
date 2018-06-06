using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GVRSwipe : MonoBehaviour
{
    //placeholder code for GVR swiping - works, but needs adjusted to prevent swipes from overtaking clicks
    private Vector2 _PrevPos;
    private Vector2 _LastPos;
    private bool _IsReleased;

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
    void Update()
    {
        if (GvrController.IsTouching)
        {
            _PrevPos = _LastPos;
            _LastPos = GvrController.TouchPos;
            _IsReleased = false;
        }
        else 
        {
            if (!_IsReleased)
            {
                _IsReleased = true;
                if (_LastPos != null && _PrevPos != null)
                {

                    float xDiff = Mathf.Abs (_PrevPos.x - _LastPos.x);
                    float yDiff = Mathf.Abs (_PrevPos.y - _LastPos.y);

                    if (xDiff > 0)
                    {
                        if (xDiff >= yDiff)
                        {   
                            float swipeDist = _PrevPos.x - _LastPos.x;
                            if (swipeDist < -0.12f)
                            {
                                EventManager.Instance.ControllerSwipeEvent("left");
                            } 
                            else if (swipeDist > 0.12f)
                            {
                                EventManager.Instance.ControllerSwipeEvent("right");
                            }
                        }
                    }
                }
            }
        }
    }
#endif
}