using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class GameScript : MonoBehaviour
{
    public static GameScript Instance;

    [Header("Ui Panels")]
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject InGameUi;
    [SerializeField] private GameObject LevelCompleteMenu;
    [SerializeField] private GameObject LevelFailMenu;
    [SerializeField] private GameObject Tutorial;

    [Header("Ui Texts")]
    [SerializeField] private TextMeshProUGUI igLvlText;
    [SerializeField] private TextMeshProUGUI igScoreText;
    [SerializeField] private TextMeshProUGUI lvlCompLvlText;
    [SerializeField] private TextMeshProUGUI lvlCompScoreText;
    [SerializeField] private TextMeshProUGUI lvlFailLvlText;
    [SerializeField] private TextMeshProUGUI lvlFailScoreText;
    [SerializeField] private TextMeshProUGUI lvlCompStarTopText;
    [SerializeField] private TextMeshProUGUI lvlCompStarText;

    [Header("GameAssets")]
    [SerializeField] private GameObject easySlope;
    [SerializeField] private Transform easyMaps;
    [SerializeField] private GameObject[] EasyEnvironments;
    [SerializeField] private Transform lvlCompTopStarIcon;
    [SerializeField] private Transform lvlCompUiStarParent;

    [Header("Colors, Textures & Materials")]
    [SerializeField] private Material slopeMat;
    [SerializeField] private Texture slopeTexture;
    [SerializeField] private Color CanyonSlopeColor;
    [SerializeField] private Color SnowySlopeColor;

    private bool firstTap = false;
    public bool FirstTap { get { return firstTap; } }
    //private bool scoring = false;
    private bool mouseDown = false;
    private readonly string highScoreKey = "HighScore";

    private void Awake()
    {
        GameAnalytics.Initialize();
        Application.targetFrameRate = 60;
        if (Instance == null)
        {
            Instance = this;
        }

        if (!PlayerPrefs.HasKey("LevelNo"))
        {
            PlayerPrefs.SetInt("LevelNo", GameManager.Instance.levelNo);
        }
        else
        {
            GameManager.Instance.levelNo = PlayerPrefs.GetInt("LevelNo");
        }

        if (!PlayerPrefs.HasKey("StarCount"))
        {
            PlayerPrefs.SetInt("StarCount", 0);
        }

        if (!PlayerPrefs.HasKey("EnvironmentIndex"))
        {
            PlayerPrefs.SetInt("EnvironmentIndex", GameManager.Instance.environmentIndex);
        }
        else
        {
            GameManager.Instance.environmentIndex = PlayerPrefs.GetInt("EnvironmentIndex");
        }

        if (!PlayerPrefs.HasKey("TutorialShown"))
        {
            PlayerPrefs.SetInt("TutorialShown", 0);
        }

        if (!PlayerPrefs.HasKey(highScoreKey))
        {
            PlayerPrefs.SetInt(highScoreKey, 0);
        }

        GameManager.Instance.Score = 0;
        GameManager.Instance.starCount = 0;
        GameManager.Instance.gameActive = GameManager.Instance.GameFailed = GameManager.Instance.GameCompleted = GameManager.Instance.touchDown = false;
    } 

    private void Start()
    {
        FindObjectOfType<TyreScript>().rb.isKinematic = true;
        SelectEnvironment();
        SelectDifficulty();
    }

    private void SelectEnvironment()
    {
        if (GameManager.Instance.environmentIndex >= 10)
        {
            GameManager.Instance.environmentIndex = 0;
        }

        if (GameManager.Instance.environmentIndex < 6)
        {
            slopeMat.color = CanyonSlopeColor;
            slopeMat.mainTexture = slopeTexture;
            EasyEnvironments[0].SetActive(true);
            EasyEnvironments[1].SetActive(false);
        }
        else
        {
            slopeMat.color = SnowySlopeColor;
            slopeMat.mainTexture = null;
            EasyEnvironments[1].SetActive(true);
            EasyEnvironments[0].SetActive(false);
        }
    }

    private void SelectDifficulty()
    {
        int levelIndex = 0;
        if (GameManager.Instance.levelNo <= easyMaps.childCount)
        {
            levelIndex = GameManager.Instance.levelNo - 1;
        }
        else
        {
            levelIndex = Random.Range(0, easyMaps.childCount);
        }

        easySlope.SetActive(true);
        easyMaps.GetChild(levelIndex).gameObject.SetActive(true);
    }

    public IEnumerator IncreaseSpeed(float startingSpeed)
    {
        if (GameManager.Instance.isTutorial)
        {
            GameManager.Instance.isTutorial = false;
        }

        GameManager.Instance.TyreSpeedX = startingSpeed;
        while (GameManager.Instance.TyreSpeedX < 1f)
        {
            GameManager.Instance.TyreSpeedX += 0.01f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void InputDown()
    {
        StartCoroutine(InputDownAction());
    }

    IEnumerator InputDownAction()
    {
        mouseDown = true;
        if (!firstTap)
        {
            firstTap = true;
            MainMenu.SetActive(false);
            ShowInGameUi();
            GameManager.Instance.TyreSpeedX = 0f;
            StartCoroutine(StartingCameraAnim());
        }

        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.isTutorial)
        {
            if (PlayerPrefs.GetInt("TutorialShown") == 0)
            {
                PlayerPrefs.SetInt("TutorialShown", 1);
                ShowInGameUi();
                StartCoroutine(ChangeCameraOffset());
                StartCoroutine(IncreaseSpeed(0f));
            }
        }

        if (mouseDown)
        {
            GameManager.Instance.touchDown = true;
        }
    }

    public void InputUp()
    {
        mouseDown = false;
        GameManager.Instance.touchDown = false;
    }

    IEnumerator ChangeCameraOffset()
    {
        for (int i = 0; i < 100; i++)
        {
            Camera.main.GetComponent<CameraScript>().offset = Vector3.Lerp
                (Camera.main.GetComponent<CameraScript>().offset,
                Camera.main.GetComponent<CameraScript>().originalOffset,
                0.03f);
            yield return new WaitForEndOfFrame();
        }

        Camera.main.GetComponent<CameraScript>().offset = new Vector3(0.26f, 4.15f, -6.3f);
    }

    internal void ShowInGameUi()
    {
        MainMenu.SetActive(false);
        LevelCompleteMenu.SetActive(false);
        LevelFailMenu.SetActive(false);
        InGameUi.SetActive(true);
        Tutorial.SetActive(false);

        igLvlText.text = "Level " + GameManager.Instance.levelNo;

        if (GameManager.Instance.Score < 10)
        {
            igScoreText.text = "0" + GameManager.Instance.Score;
        }
        else
        {
            igScoreText.text = GameManager.Instance.Score.ToString();
        }
    }

    IEnumerator StartingCameraAnim()
    {
        //iTween.MoveTo(Camera.main.gameObject, iTween.Hash("x", 0f, "y", 37.3f, "z", 11.81592f, "islocal", true, "time", 2f, "easetype", "linear"));
        //yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(0.1f);

        GameManager.Instance.gameActive = true;
        FindObjectOfType<TyreScript>().rb.isKinematic = false;
        Camera.main.gameObject.GetComponent<CameraScript>().enabled = true;
        StartCoroutine(IncreaseSpeed(0f));
    }

    internal void ShowTutorialMenu()
    {
        Tutorial.SetActive(true);
        LevelCompleteMenu.SetActive(false);
        LevelFailMenu.SetActive(false);
        MainMenu.SetActive(false);
        InGameUi.SetActive(false);
    }

    internal void LevelCompleted()
    {
        if (GameManager.Instance.Score > PlayerPrefs.GetInt(highScoreKey))
        {
            PlayerPrefs.SetInt(highScoreKey, GameManager.Instance.Score);
        }

        Camera.main.transform.GetComponentInChildren<ParticleSystem>().Play();
        MainMenu.SetActive(false);
        LevelCompleteMenu.SetActive(true);
        LevelFailMenu.SetActive(false);
        InGameUi.SetActive(false);
        Tutorial.SetActive(false);

        lvlCompLvlText.text = "Level " + GameManager.Instance.levelNo;
        if (GameManager.Instance.Score < 10)
        {
            lvlCompScoreText.text = "0" + PlayerPrefs.GetInt(highScoreKey).ToString();
        }
        else
        {
            lvlCompScoreText.text = PlayerPrefs.GetInt(highScoreKey).ToString();
        }

        int tempStarCount = PlayerPrefs.GetInt("StarCount");
        
        if (tempStarCount < 10)
        {
            lvlCompStarTopText.text = "0" + tempStarCount.ToString();
        }
        else
        {
            lvlCompStarTopText.text = tempStarCount.ToString();
        }
        tempStarCount += GameManager.Instance.starCount;
        PlayerPrefs.SetInt("StarCount", tempStarCount);

        if (GameManager.Instance.starCount < 10)
        {
            lvlCompStarText.text = "0" + GameManager.Instance.starCount.ToString();
        }
        else
        {
            lvlCompStarText.text = GameManager.Instance.starCount.ToString();
        }

        DisablePhysics();
        StartCoroutine(TyreCelebrationAnim(FindObjectOfType<RotateTyre>().gameObject));
        StartCoroutine(StarCounterAnim());
        StartCoroutine(UnparentUiStars());
    }

    IEnumerator StarCounterAnim()
    {
        int beforeCounter = int.Parse(lvlCompStarText.text);
        int afterCounter = 0;
        int tempStarCount = int.Parse(lvlCompStarTopText.text);

        yield return new WaitForSeconds(0.5f);

        while (beforeCounter > 0)
        {
            beforeCounter--;
            afterCounter++;

            if (beforeCounter < 10)
            {
                lvlCompStarText.text = "0" + beforeCounter.ToString();
            }
            else
            {
                lvlCompStarText.text = beforeCounter.ToString();
            }

            if ((tempStarCount + afterCounter) < 10)
            {
                lvlCompStarTopText.text = "0" + (tempStarCount + afterCounter).ToString();
            }
            else
            {
                lvlCompStarTopText.text = (tempStarCount + afterCounter).ToString();
            }

            yield return new WaitForSeconds(0.05f);
        }

    }

    IEnumerator UnparentUiStars()
    {
        foreach (Transform child in lvlCompUiStarParent)
        {
            child.parent = lvlCompTopStarIcon;
            StartCoroutine(UiStarTravelling(child.gameObject));
            yield return new WaitForSeconds(0.075f);
        }
    }

    IEnumerator UiStarTravelling(GameObject star)
    {
        iTween.MoveTo(star, iTween.Hash("x", 0f, "y", 0f, "islocal", true, "time", 0.5f, "easetype", "linear"));
        yield return new WaitForSeconds(0.25f);

        iTween.ScaleTo(star, iTween.Hash("x", 1f, "y", 1f, "islocal", true, "time", 0.25f, "easetype", "linear"));
        yield return new WaitForSeconds(0.25f);

        Destroy(star);
    }

    internal void LevelFailed()
    {
        StartCoroutine(LevelFailAction());
    }

    IEnumerator LevelFailAction()
    {
        yield return new WaitForSeconds(1f);

        MainMenu.SetActive(false);
        LevelCompleteMenu.SetActive(false);
        LevelFailMenu.SetActive(true);
        InGameUi.SetActive(false);
        Tutorial.SetActive(false);

        lvlFailLvlText.text = "Level " + GameManager.Instance.levelNo;
        if (GameManager.Instance.Score < 10)
        {
            lvlFailScoreText.text = "0" + PlayerPrefs.GetInt(highScoreKey).ToString();
        }
        else
        {
            lvlFailScoreText.text = PlayerPrefs.GetInt(highScoreKey).ToString();
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        GameManager.Instance.levelNo++;
        GameManager.Instance.environmentIndex++;
        PlayerPrefs.SetInt("LevelNo", GameManager.Instance.levelNo);
        PlayerPrefs.SetInt("EnvironmentIndex", GameManager.Instance.environmentIndex);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DisablePhysics()
    {
        if (FindObjectOfType<TyreScript>() != null)
        {
            FindObjectOfType<TyreScript>().rb.isKinematic = true;
        }
    }

    IEnumerator TyreCelebrationAnim(GameObject tyre)
    {
        float yPos = tyre.transform.localPosition.y;
        float yAdd = 0.5f;

        yield return new WaitForSeconds(2f);

        if (tyre != null)
        {
            tyre.transform.localRotation = Quaternion.Euler(90f, 0f, -180f);
            yield return new WaitForSeconds(0.1f);

            iTween.MoveTo(tyre, iTween.Hash("x", yPos + yAdd, "islocal", true, "time", 0.2f, "easetype", "linear", "looptype", "pingPong"));
            yield return new WaitForSeconds(1f);

            iTween.RotateTo(tyre, iTween.Hash("x", 450f, "islocal", true, "time", 0.5f, "easetype", "easeInOutExpo", "looptype", "loop"));
        }
    }
}
