using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Inicio declaración Singleton
    private static GameManager instance; //Declaración estática de la base de datos
    public static GameManager Instance //Declaración de la llave que accede a los datos públicos de la base de datos
    { 
        get
        {
            if (instance == null)
            {
                Debug.Log("GameManager is null");
            }
            return instance;
        }
    }
    //Fin de la declaración de Singleton

    [Header("Stats")]
    public float health;
    public float maxHealth;
    public int lives;
    public int maxlives;
    public bool playerDead;
    public bool levelDone;
    public bool onAnimation;
    public bool onAnimation2;
    public bool damageAmplified;
    public bool pierce;
    public bool menuOn;

    [Header("SceneManagement")]
    public int actualScene;
    public int maxEnemies;
    public int remainingEnemies;
    public int enemiesToSpawn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        maxHealth = 100;
        maxlives = 3;
        health = maxHealth;
        lives = maxlives;
        playerDead = false;
        levelDone = false;
        menuOn = false;
    }

    private void Update()
    {
        HealthControl();
        HideCursor();
    }

    public void HealthControl()
    {
        if (health <= 0)
        {
            health = 0;
            playerDead = true;
        }
        else if (health > 100)
        {
            health = 100;
        }
    }

    void HideCursor()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;

        if (actualScene == 0 || actualScene == 6 || menuOn) { Cursor.visible = true; }
        else { Cursor.visible = false;}
    }

    #region SceneManager

    public void RestartLevel()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;
        string thisScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(thisScene);
        playerDead = false;
        health = maxHealth;

        Debug.Log(actualScene);
        if (actualScene == 1) { onAnimation = true; onAnimation2 = false; menuOn = false; AudioManager.Instance.PlayMusic(4); RestoreStats(); }
        else if (actualScene == 2) { maxEnemies = 50; AudioManager.Instance.PlayMusic(3); }
        else if (actualScene == 3) { AudioManager.Instance.PlayMusic(2); }
        else if (actualScene == 4) { maxEnemies = 75; AudioManager.Instance.PlayMusic(3); }
        else if (actualScene == 5) { onAnimation2 = true; AudioManager.Instance.PlayMusic(5); }
        else if (actualScene == 6) { onAnimation2 = false; AudioManager.Instance.PlayMusic(1); }

        remainingEnemies = maxEnemies;
        enemiesToSpawn = maxEnemies;
    }

    public void RestoreStats()
    {
        maxHealth = 100;
        maxlives = 3;
        health = maxHealth;
        lives = maxlives;
        playerDead = false;
    }
    public void NextScene()
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(sceneToLoad);

        actualScene = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(actualScene);
        if (actualScene == 0) { onAnimation = true; onAnimation2 = false; menuOn = false; AudioManager.Instance.PlayMusic(4); RestoreStats(); }
        else if (actualScene == 1) { maxEnemies = 50; AudioManager.Instance.PlayMusic(3); }
        else if (actualScene == 2) { maxEnemies = 0; AudioManager.Instance.PlayMusic(2); }
        else if (actualScene == 3) { maxEnemies = 75; AudioManager.Instance.PlayMusic(3); }
        else if (actualScene == 4) { onAnimation2 = true; AudioManager.Instance.PlayMusic(5); }
        else if (actualScene == 5) { onAnimation2 = false; AudioManager.Instance.PlayMusic(1); }

        remainingEnemies = maxEnemies;
        enemiesToSpawn = maxEnemies;
    }

    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);

        actualScene = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(actualScene);
        if (actualScene == 0) { onAnimation = true; onAnimation2 = false; menuOn = false; AudioManager.Instance.PlayMusic(0); }
        else if (actualScene == 1) { maxEnemies = 50; }
        else if (actualScene == 3) { maxEnemies = 75; }
        else if (actualScene == 4) { onAnimation = false; onAnimation2 = true; }
        else { maxEnemies = 0; remainingEnemies = 0; }

        remainingEnemies = maxEnemies;
        enemiesToSpawn = maxEnemies;
    }

    public void ExitGame()
    {
        Debug.Log("Exit game is a succes");
        Application.Quit();
    }

    #endregion
}
