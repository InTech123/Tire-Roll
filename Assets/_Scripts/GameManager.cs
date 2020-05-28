
public class GameManager
{
    private static GameManager instance;

    private GameManager() { }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }

    //private fields
    private bool gameCompleted = false;
    private bool gameFailed = false;
    private int score = 0;

    //public fields
    public bool touchDown = false;
    public bool gameActive = false;
    public float TyreSpeedX = 0f;
    public int levelNo = 1;
    public int environmentIndex = 1;
    public bool isTutorial = false;
    public int starCount = 0;

    //public properties
    public bool GameCompleted
    {
        get 
        { 
            return gameCompleted; 
        }
        set 
        { 
            gameCompleted = value;
            if (gameCompleted)
            {
                GameScript.Instance.LevelCompleted();
            }
        }
    }
    public bool GameFailed
    {
        get
        {
            return gameFailed;
        }
        set
        {
            gameFailed = value;
            if (gameFailed)
            {
                GameScript.Instance.LevelFailed();
            }
        }
    }
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            if (GameScript.Instance.FirstTap)
            {
                GameScript.Instance.ShowInGameUi();
            }
        }
    }

}
