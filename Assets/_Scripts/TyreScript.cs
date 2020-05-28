using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TyreScript : MonoBehaviour
{
    [Header("Touch Controls")]
    [Tooltip("Speed with which tyre is moving left and right")]
    [Range(1, 10)]
    [SerializeField] private float movementSpeed;

    [Header("Movement Settings")]

    [Range(1, 10)]
    [Tooltip("Speed with which tyre moves forward")]
    [SerializeField] private float forwardSpeed;

    [Tooltip("Check for moving with force or velocity")]
    [SerializeField] private bool moveWithForce;

    [Range(0f, 1f)]
    [Tooltip("Rotation Speed for tyre")]
    [SerializeField] private float rotationSpeed;

    [Tooltip("Speed After which fire particles play")]
    [SerializeField] private float fireCausingSpeed;

    [Header("Effects")]    
    [SerializeField] private ParticleSystem fireParticle;

    [Header("Deflector Jumper Settings")]
    [Range(0, 10)]
    [Tooltip("Force exerted on tyre in given direction after triggering with a deflector")]
    [SerializeField] private float deflectForceMag;

    [Range(0, 10)]
    [Tooltip("Force exerted on tyre in given direction after triggering with a jumper")]
    [SerializeField] private float jumperForceMag;

    [Range(0, 1)]
    [Tooltip("Slows tyre's velocity based on this value")]
    [SerializeField] private float bumpFactor;

    [Header("Sounds")]
    [SerializeField] private AudioSource starPickingSound;

    private float ForwardSpeed 
    { 
        get { return forwardSpeed * 100; }
    }
    private float MovementSpeed
    {
        get 
        { 
            if (moveWithForce)
            {
                return movementSpeed * 100;
            }
            else
            {
                return movementSpeed * 10;
            }
        }
    }

    [HideInInspector]
    public Rigidbody rb;
    private float horizontalMovement;
    private float prevMovement;
    private bool deflected = false;
    private bool jumped = false;
    private bool airborne = false;
    private Transform slope;
    private bool isFire = false;
    private bool resetFire = false;
    private float timeSinceLastFire = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        slope = GameObject.FindGameObjectWithTag("slope").transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "finishLine")
        {
            isFire = false;
            resetFire = true;
            StopFireParticles();
            if (!GameManager.Instance.GameFailed)
            {
                GameManager.Instance.GameCompleted = true;
            }
        }
        else if (collision.transform.tag == "obstacle" || collision.transform.tag == "tyreObs" || collision.transform.tag == "hammerObs" || collision.transform.tag == "spannerObs")
        {
            isFire = false;
            resetFire = true;
            StopFireParticles();
            if (!GameManager.Instance.GameCompleted)
            {
                if (collision.transform.tag == "tyreObs")
                {
                    if (!GameManager.Instance.GameFailed)
                    {
                        collision.transform.GetComponent<Rigidbody>().AddForce(Vector3.forward * 0.2f, ForceMode.Impulse);
                    }
                    else
                    {
                        collision.transform.GetComponent<Rigidbody>().AddForce(Vector3.forward * 0.05f, ForceMode.Impulse);
                    }
                }
                GameManager.Instance.GameFailed = true;
            }
        }
        else if (collision.transform.tag == "deadZone")
        {
            isFire = false;
            resetFire = true;
            StopFireParticles();
            if (!GameManager.Instance.GameCompleted)
            {
                GameManager.Instance.GameFailed = true;
                //Destroy(gameObject, 0.9f);
            }
        }
        else if (collision.transform.tag == "slope")
        {
            if (jumped)
            {
                jumped = false;
                airborne = false;
                GameManager.Instance.gameActive = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "deflector")
        {
            airborne = true;
            isFire = false;
            resetFire = true;
            rb.AddForce(Vector3.up * deflectForceMag, ForceMode.Impulse);
            rb.velocity *= bumpFactor;
            GameManager.Instance.gameActive = false;
            deflected = true;
            if (GameManager.Instance.TyreSpeedX >= 0.9f)
            {
                StartCoroutine(GameScript.Instance.IncreaseSpeed(0.5f));
            }
            StartCoroutine(ActiveControls());
            StartCoroutine(WobbleTyre());
        }
        else if (other.tag == "jumper")
        {
            airborne = true;
            GameManager.Instance.gameActive = false;
            rb.AddForce(Vector3.up * jumperForceMag * 5f, ForceMode.Impulse);
            jumped = true;
            other.transform.localPosition = new 
                Vector3(other.transform.localPosition.x, other.transform.localPosition.y + other.transform.localScale.y, other.transform.localPosition.z);
        }
        else if (other.tag == "tutorialTrigger")
        {
            if (PlayerPrefs.GetInt("TutorialShown") == 0)
            {
                if (!GameManager.Instance.isTutorial)
                {
                    Camera.main.GetComponent<CameraScript>().originalOffset = new Vector3(0.26f, 4.15f, -6.3f);
                    Camera.main.GetComponent<CameraScript>().offset = Camera.main.GetComponent<CameraScript>().startingOffset;
                    GameManager.Instance.TyreSpeedX = 0f;
                    GameScript.Instance.ShowTutorialMenu();
                    GameManager.Instance.isTutorial = true;
                }
            }
        }
        else if (other.tag == "hammerObs")
        {
            if (fireParticle.isPlaying)
            {
                for (int i = 0; i < other.transform.parent.childCount; i++)
                {
                    other.transform.parent.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                    var colls = other.transform.parent.GetChild(i).GetComponentsInChildren<Collider>();
                    foreach (var c in colls)
                    {
                        c.enabled = false;
                    }

                    Destroy(other.transform.parent.GetChild(i).gameObject, 2.2f);
                    if (i == 0)
                    {
                        iTween.Stop(other.transform.parent.GetChild(0).gameObject);
                        Destroy(other.transform.parent.GetChild(0).gameObject.GetComponent<MerryGoRound>());
                        other.transform.parent.GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                }
            }

            StopFireParticles();
        }
        else if (other.tag == "obstacle")
        {
            if (fireParticle.isPlaying)
            {
                other.transform.GetComponentInChildren<ParticleSystem>().Play();
                other.transform.GetComponent<MeshRenderer>().enabled = false;
                other.transform.GetComponent<Collider>().enabled = false;
                Destroy(other.gameObject, 2.2f);
            }
            
            StopFireParticles();
        }
        else if (other.tag == "tyreObs")
        {
            StartCoroutine(TyreTriggerEffect(other.transform));
        }
        else if (other.tag == "spannerObs")
        {
            if (!GameManager.Instance.GameCompleted)
            {
                //GameManager.Instance.GameFailed = true;
                other.GetComponent<MeshRenderer>().enabled = false;
                other.GetComponent<Collider>().enabled = false;
                Transform piecesParent = other.transform.GetChild(0);

                foreach (Transform piece in piecesParent)
                {
                    piece.GetComponent<MeshCollider>().convex = true;
                }

                piecesParent.gameObject.SetActive(true);
                Destroy(piecesParent.gameObject, 2.2f);

                foreach (Transform piece in piecesParent)
                {
                    piece.GetComponent<Rigidbody>().AddForce(Vector3.forward * 0.0005f, ForceMode.Impulse);
                }
            }
            StopFireParticles();
        }
        else if (other.tag == "deadZone")
        {
            StopFireParticles();
            if (!GameManager.Instance.GameCompleted)
            {
                GameManager.Instance.GameFailed = true;
            }
        }
        else if (other.tag == "stars")
        {
            if (!GameManager.Instance.GameFailed && !GameManager.Instance.GameCompleted)
            {
                if (starPickingSound.pitch > 2f)
                {
                    starPickingSound.pitch = 1f;
                }
                other.GetComponent<MeshRenderer>().enabled = false;
                other.enabled = false;
                other.GetComponentInChildren<ParticleSystem>().Play();
                starPickingSound.Play();
                starPickingSound.pitch += 0.1f;
                GameManager.Instance.Score += GameManager.Instance.levelNo;
                GameManager.Instance.starCount++;
                other.tag = "Untagged";
                Destroy(other.gameObject, 3f);
            }
        }
    }

    IEnumerator TyreTriggerEffect(Transform t)
    {
        //foreach (Transform t in tyreParent)
        //{
        //    t.tag = "Untagged";
        //    Destroy(t.gameObject, 2f);
        //}

        t.tag = "Untagged";
        Destroy(t.gameObject, 2f);

        yield return new WaitForSeconds(0.1f);

        t.GetComponent<Collider>().isTrigger = false;
        t.GetComponent<Rigidbody>().useGravity = true;
        t.GetComponent<Rigidbody>().AddForce(new Vector3(0f, Random.Range(0.01f, 0.03f), 1f) * Random.Range(0.4f, 0.5f), ForceMode.Impulse);
        t.GetComponent<Collider>().isTrigger = true;

        //foreach (Transform t in tyreParent)
        //{
        //    t.GetComponent<Collider>().isTrigger = false;
        //    t.GetComponent<Rigidbody>().useGravity = true;
        //    t.GetComponent<Rigidbody>().AddForce(new Vector3(0f, Random.Range(0.01f, 0.03f), 1f) * Random.Range(0.4f, 0.5f), ForceMode.Impulse);
        //    t.GetComponent<Collider>().isTrigger = true;
        //}

        StopFireParticles();
    }

    IEnumerator WobbleTyre()
    {
        //for (int i = 0; i < 4; i++)
        //{
        //    rotationSpeed *= -2f;
        //    yield return new WaitForSeconds(0.1f);
        //}
        //for (int i = 0; i < 4; i++)
        //{
        //    rotationSpeed /= 2;
        //}

        GameObject t = GetComponentInChildren<RotateTyre>().gameObject;
        float at = 0.05f;
        float wt = 0.075f;
        iTween.RotateTo(t, iTween.Hash("x", 80f, "islocal", true, "time", at, "easetype", "linear"));
        yield return new WaitForSeconds(wt);

        iTween.RotateTo(t, iTween.Hash("x", 90f, "islocal", true, "time", at, "easetype", "linear"));
        yield return new WaitForSeconds(wt);

        iTween.RotateTo(t, iTween.Hash("x", 100f, "islocal", true, "time", at, "easetype", "linear"));
        yield return new WaitForSeconds(wt);

        iTween.RotateTo(t, iTween.Hash("x", 90f, "islocal", true, "time", at, "easetype", "linear"));
        yield return new WaitForSeconds(wt);
    }

    IEnumerator ActiveControls()
    {
        yield return new WaitForSeconds(0.4f);
        
        GameManager.Instance.gameActive = true;
        deflected = false;
    }

    private void PlayFireParticles()
    {
        fireParticle.Play();
        GameObject[] hammers = GameObject.FindGameObjectsWithTag("hammerObs");
        foreach (GameObject hammer in hammers)
        {
            var hammerCols = hammer.GetComponentsInChildren<Collider>();
            foreach (Collider c in hammerCols)
            {
                c.isTrigger = true;
            }
        }
        
        GameObject[] Obstacles = GameObject.FindGameObjectsWithTag("obstacle");
        foreach (GameObject obs in Obstacles)
        {
            obs.GetComponent<Collider>().isTrigger = true;
        }

        GameObject[] Tyres = GameObject.FindGameObjectsWithTag("tyreObs");
        foreach (GameObject t in Tyres)
        {
            t.GetComponent<Collider>().isTrigger = true;
            t.GetComponent<Rigidbody>().useGravity = false;
        }

        GameObject[] Spanners = GameObject.FindGameObjectsWithTag("spannerObs");
        foreach (GameObject s in Spanners)
        {
            s.GetComponent<Collider>().isTrigger = true;
        }
    }

    private void StopFireParticles()
    {
        fireParticle.Stop();
        GameObject[] hammers = GameObject.FindGameObjectsWithTag("hammerObs");
        foreach (GameObject hammer in hammers)
        {
            var hammerCols = hammer.GetComponentsInChildren<Collider>();
            foreach (Collider c in hammerCols)
            {
                c.isTrigger = false;
            }
        }

        GameObject[] Obstacles = GameObject.FindGameObjectsWithTag("obstacle");
        foreach (GameObject obs in Obstacles)
        {
            obs.GetComponent<Collider>().isTrigger = false;
        }

        GameObject[] Tyres = GameObject.FindGameObjectsWithTag("tyreObs");
        foreach (GameObject t in Tyres)
        {
            t.GetComponent<Collider>().isTrigger = false;
            t.GetComponent<Rigidbody>().useGravity = true;
        }

        GameObject[] Spanners = GameObject.FindGameObjectsWithTag("spannerObs");
        foreach (GameObject s in Spanners)
        {
            s.GetComponent<Collider>().isTrigger = false;
        }
    }
   
    private void Update()
    {
        if (!GameManager.Instance.isTutorial)
        {
            if (GameManager.Instance.gameActive && !GameManager.Instance.GameFailed && !GameManager.Instance.GameCompleted)
            {
                if (resetFire)
                {
                    timeSinceLastFire = 0f;
                    resetFire = false;
                }

                if (!isFire)
                {
                    timeSinceLastFire += Time.deltaTime;
                    if (timeSinceLastFire > 5f)
                    {
                        isFire = true;
                        timeSinceLastFire = 0f;
                        if (!fireParticle.isPlaying)
                        {
                            PlayFireParticles();
                        }
                    }
                }

                transform.Translate(slope.forward * 0.7f * GameManager.Instance.TyreSpeedX, Space.Self);

                if (GameManager.Instance.touchDown)
                {
                    horizontalMovement = Input.GetAxis("Horizontal");
                    horizontalMovement = horizontalMovement * Time.deltaTime * MovementSpeed;

                    if (Mathf.Abs(horizontalMovement) > 0.4f)
                    {
                        prevMovement = horizontalMovement;
                    }
                    transform.Rotate(0f, prevMovement * rotationSpeed * 0.5f, 0f);
                }

                transform.GetChild(0).Rotate(0f, 0f, -prevMovement * rotationSpeed * 1.5f);

                if (transform.position.z >= GameObject.FindGameObjectWithTag("finishLine").transform.position.z)
                {
                    if (!GameManager.Instance.GameCompleted)
                    {
                        GameManager.Instance.GameCompleted = true;
                        isFire = false;
                        resetFire = true;
                        StopFireParticles();
                    }
                }
            }
            else
            {
                if (airborne && !GameManager.Instance.GameFailed && !GameManager.Instance.GameCompleted)
                {
                    transform.Translate(slope.forward * 0.7f * GameManager.Instance.TyreSpeedX, Space.Self);
                }

                if (GameManager.Instance.touchDown)
                {
                    horizontalMovement = Input.GetAxis("Horizontal");
                    horizontalMovement = horizontalMovement * Time.deltaTime * MovementSpeed * 0.7f;

                    if (Mathf.Abs(horizontalMovement) > 0.4f)
                    {
                        prevMovement = horizontalMovement;
                    }
                    transform.Rotate(0f, prevMovement * rotationSpeed * 0.5f, 0f);
                }

                if (deflected || jumped)
                {
                    transform.GetChild(0).Rotate(0f, 0f, -prevMovement * rotationSpeed * 1.5f);
                }
            }

            if (!airborne)
            {
                if (!jumped && !deflected && GameManager.Instance.gameActive)
                {
                    if (transform.GetChild(0).eulerAngles.z <= 220f || transform.GetChild(0).eulerAngles.z >= 320f)
                    {
                        GameManager.Instance.GameFailed = true;
                        isFire = false;
                        resetFire = true;
                        StopFireParticles();
                        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                        transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!airborne && !GameManager.Instance.GameFailed && !GameManager.Instance.GameCompleted)
        {
            rb.velocity = Vector3.zero;
        }
    }

    //private void FixedUpdate()
    //{
    //if (rb.velocity.magnitude <= fireCausingSpeed)
    //{
    //    if (fireParticle.gameObject.activeInHierarchy)
    //    {
    //        fireParticle.Stop();
    //        fireParticle.gameObject.SetActive(false);
    //    }
    //}
    //else
    //{
    //    if (!fireParticle.gameObject.activeInHierarchy)
    //    {
    //        fireParticle.gameObject.SetActive(true);
    //    }
    //    //playFireParticles
    //    if (!fireParticle.isPlaying)
    //    {
    //        fireParticle.Play();
    //    }
    //}
    //}
}
