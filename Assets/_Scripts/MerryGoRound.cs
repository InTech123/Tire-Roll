using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerryGoRound : MonoBehaviour
{
    [SerializeField] private float rotA;
    [SerializeField] private float rotB;

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
            StartCoroutine(RotateEffect("x"));
        }
        else if (y)
        {
            StartCoroutine(RotateEffect("y"));
        }
        else if (z)
        {
            StartCoroutine(RotateEffect("z"));
        }
    }

    IEnumerator RotateEffect(string axis)
    {
        iTween.RotateTo(gameObject, iTween.Hash(axis, rotA, "islocal", true, "time", timeA, "easetype", "linear"));
        yield return new WaitForSeconds(timeA + 0.05f);

        iTween.RotateTo(gameObject, iTween.Hash(axis, rotB, "islocal", true, "time", timeB, "easetype", "linear", "looptype", loopType));
    }
}
