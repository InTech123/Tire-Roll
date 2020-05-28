using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingEffect : MonoBehaviour
{
    [SerializeField] private float scaleA;
    [SerializeField] private float scaleB;

    [SerializeField] private bool x;
    [SerializeField] private bool y;
    [SerializeField] private bool z;

    [SerializeField] private float timeA;
    [SerializeField] private float timeB;

    [SerializeField] private string loopType;

    private void Start()
    {
        if (x)
        {
            StartCoroutine(ScaleEffectAction("x"));
        }
        
        if (y)
        { 
            StartCoroutine(ScaleEffectAction("y"));
        }
        
        if (z)
        {
            StartCoroutine(ScaleEffectAction("z"));
        }
    }

    IEnumerator ScaleEffectAction(string axis)
    {
        iTween.ScaleTo(gameObject, iTween.Hash(axis, scaleA, "islocal", true, "time", timeA, "easetype", "linear"));
        yield return new WaitForSeconds(timeA + 0.05f);

        iTween.ScaleTo(gameObject, iTween.Hash(axis, scaleB, "islocal", true, "time", timeB, "easetype", "linear", "looptype", loopType));
    }
}
