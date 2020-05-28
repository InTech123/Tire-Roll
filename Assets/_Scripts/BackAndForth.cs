using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForth : MonoBehaviour
{
    [SerializeField] private float posA;
    [SerializeField] private float posB;

    [SerializeField] private bool x;
    [SerializeField] private bool y;
    [SerializeField] private bool z;

    [SerializeField] private float timeA;
    [SerializeField] private float timeB;

    private void Start()
    {
        if (x)
        {
            StartCoroutine(BackAndForthMovement("x"));
        }
        else if (y)
        {
            StartCoroutine(BackAndForthMovement("y"));
        }
        else if (z)
        {
            StartCoroutine(BackAndForthMovement("z"));
        }
    }

    IEnumerator BackAndForthMovement(string axis)
    {
        iTween.MoveTo(gameObject, iTween.Hash(axis, posA, "islocal", true, "time", timeA, "easetype", "linear"));
        yield return new WaitForSeconds(timeA + 0.05f);

        iTween.MoveTo(gameObject, iTween.Hash(axis, posB, "islocal", true, "time", timeB, "easetype", "linear", "looptype", "pingPong"));
    }
}
