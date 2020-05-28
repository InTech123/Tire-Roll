using UnityEngine;

public class ScaleBtnEffect : MonoBehaviour
{
    [SerializeField] private float animTime;
    [SerializeField] private float xIncrement;
    [SerializeField] private float yIncrement;

    void Start()
    {
        iTween.ScaleTo(gameObject, iTween.Hash("x", gameObject.transform.localScale.x + xIncrement, "y", gameObject.transform.localScale.y + yIncrement, "time", animTime, "easetype", iTween.EaseType.easeOutElastic, "loopType", iTween.LoopType.pingPong , "ignoretimescale", true));
    }
}
