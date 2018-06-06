using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Linq.Expressions;

public static class Extensions
{
    public static T GetRequired<T>() where T : UnityEngine.Object
    {
        var obj = MonoBehaviour.FindObjectOfType<T>();

        //if (obj == null)
            //throw new Exception(String.Format("A {0} must be present in the scene", typeof(T).Name));

        return obj;
    }

    public static T GetRequired<T>(string name) where T : UnityEngine.Object
    {
        T[] objs = Resources.FindObjectsOfTypeAll<T>();
        foreach (T obj in objs)
        {
            if (obj != null && obj.name == name)
                return obj;
        }

        throw new Exception(String.Format("A {0} must be present in the scene", name));
    }

    public static T GetRequired<T>(GameObject parent, string name) where T : UnityEngine.Object
    {
        T[] objs = parent.GetComponentsInChildren<T>(true);
        foreach (T obj in objs)
        {
            if (obj != null && obj.name == name)    // JK COMMENT: I don't know why but obj can be null.
                return obj;
        }

        throw new Exception(String.Format("A {0} must be present in the scene", name));
    }

    public static T? EnumParseOrNull<T>(string value) where T : struct
    {
        try
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        catch
        {
            return null;
        }
    }

    public static string GetVariableName<T>(Expression<Func<T>> memberExpression)
    {
        MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
        return expressionBody.Member.Name;
    }

    public static void SwitchActiveState(this GameObject obj, bool active)
    {
        UnityEngine.Component[] children = obj.GetComponentsInChildren<UnityEngine.Component>(true);
        foreach (UnityEngine.Component child in children)
        {
            if (child.name == "ActiveState")
                child.gameObject.SetActive(active);
            else if (child.name == "InactiveState")
                child.gameObject.SetActive(!active);
        }
    }
}

