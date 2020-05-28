using System.Collections;
using UnityEngine;

public class StarsAnim : MonoBehaviour
{
    float yPos;

    private void Start()
    {
        yPos = transform.localPosition.y;
        iTween.RotateTo(gameObject, iTween.Hash("y", 720f, "islocal", true, "time", 2f, "easetype", "linear", "looptype", "loop"));
    }

}
