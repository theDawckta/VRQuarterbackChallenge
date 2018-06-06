using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SingleTileHide : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor =>
        {
            if (cursor.SingleTileMode)
                gameObject.SetActive(false);
        });
    }
}