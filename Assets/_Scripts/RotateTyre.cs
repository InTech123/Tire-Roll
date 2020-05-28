using UnityEngine;

public class RotateTyre : MonoBehaviour
{
    [SerializeField] private float speed;
    //private Rigidbody tyre;

    private void Start()
    {
        //tyre = FindObjectOfType<TyreScript>().rb;
    }

    private void Update()
    {
        if (!GameManager.Instance.GameCompleted && !GameManager.Instance.GameFailed && !GameManager.Instance.isTutorial)
        {
            transform.Rotate(0f, 0f, -Time.deltaTime * speed * GameManager.Instance.TyreSpeedX);
        }
    }
}
