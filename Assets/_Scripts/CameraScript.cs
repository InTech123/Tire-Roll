using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Follow Settings")]
    
    [Tooltip("Target to follow")]
    [SerializeField] private Transform target;
    
    [Tooltip("constant distance from object")]
    public Vector3 offset;

    [Tooltip("starting view of camera")]
    public Vector3 startingOffset;

    [Tooltip("smooth speed to cover distance")]
    [SerializeField] private float smoothSpeed = 0.125f;

    [Tooltip("Enabling will make camera look towards the target")]
    [SerializeField] private bool LookTowardsTarget;

    private bool speeding = false;
    [HideInInspector]
    public Vector3 originalOffset;

    private void Start()
    {
        speeding = false;
        originalOffset = offset;
        offset = startingOffset;
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        if (LookTowardsTarget)
        {
            transform.LookAt(target);
        }
    }

    IEnumerator IncrementSpeed()
    {
        float tempSpeed = smoothSpeed;
        smoothSpeed = 0;

        while (smoothSpeed < tempSpeed)
        {
            smoothSpeed += 0.01f;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 100; i++)
        {
            offset = Vector3.Lerp(offset, originalOffset, 0.03f);
            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.GameFailed && !GameManager.Instance.GameCompleted)
        {
            FollowTarget();
            if (!speeding)
            {
                speeding = true;
                StartCoroutine(IncrementSpeed());
            }
        }
    }

}
