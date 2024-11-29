using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIControl : MonoBehaviour
{
    [SerializeField] Image hpBar;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject onlyGunSelected;
    [SerializeField] GameObject gunSelected;
    [SerializeField] GameObject shotgunSelected;
    [SerializeField] GameObject lvlCompletePanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject interactPanel;
    [SerializeField] GameObject options;
    [SerializeField] GameObject[] dialogues;
    [SerializeField] PlayerController playerController;
    [SerializeField] Image[] lives;
    [SerializeField] GameObject shotgunBulletPrefab;
    [SerializeField] Transform shootPointPos;
    [SerializeField] GameObject finalPanel;
    [SerializeField] TextMeshProUGUI enemyCounterText;

    public GameObject objectToClose;
    bool animationOn;
    bool canClose;
    public bool levelDone;
    public bool gameOverDone;
    public bool dialogueOn;
    public bool noteDone;
    public bool chestDone;
    int actualScene;

    // Start is called before the first frame update
    void Start()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;

        if (actualScene != 0)
        {
            levelDone = false;
            gameOverDone = false;
            animationOn = false;
            pauseMenu.SetActive(false);
            lvlCompletePanel.SetActive(false);
            gameOverPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;

        if (actualScene != 0)
        {
            HealthBarSystem();
            LivesSystem();
            ChangeWeapon();
            Animation();
            Interact();
            EnemyCounter();
        }


        if (actualScene == 2 || actualScene == 4)
        {
            if (GameManager.Instance.remainingEnemies <= 0 && !levelDone) { StartCoroutine(LevelCompletedText()); }
        }
    }

    void Animation()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;

        if (actualScene == 1 && GameManager.Instance.onAnimation && !animationOn) 
        {
            StartCoroutine(Animation1());
        }
        else if (actualScene == 2 && !dialogueOn)
        {
            DialogueOn(4);
        }
        else if (actualScene == 5 && GameManager.Instance.onAnimation2 && !animationOn)
        {
            StartCoroutine(Animation2());
        }
    }

    public void DialogueOn(int dialogNum)
    {
        dialogueOn = true;
        Debug.Log("DialogueOn");
        dialogues[dialogNum].SetActive(true);
        StartCoroutine(CloseDialogue(dialogNum));
    }

    IEnumerator CloseDialogue(int dialogNum)
    {
        yield return new WaitForSeconds(5.5f);
        dialogues[dialogNum].SetActive(false);

        yield return null;
    }

    void Interact()
    {
        if (playerController.canInteract) { interactPanel.SetActive(true); }
        else { interactPanel.SetActive(false); }
    }

    IEnumerator Animation1()
    {
        animationOn = true;
        
        dialogues[0].SetActive(true);
        yield return new WaitForSeconds(5);

        dialogues[0].SetActive(false);
        yield return new WaitForSeconds(1);

        dialogues[1].SetActive(true);
        yield return new WaitForSeconds(5);

        dialogues[1].SetActive(false);
        yield return new WaitForSeconds(1);

        dialogues[2].SetActive(true);
        yield return new WaitForSeconds(5);

        dialogues[2].SetActive(false);

        GameManager.Instance.onAnimation = false;
        animationOn = false;
        yield return null;
    }

    IEnumerator Animation2()
    {
        animationOn = true;
        Debug.Log("Animation");

        dialogues[6].SetActive(true);
        yield return new WaitForSeconds(5);

        dialogues[6].SetActive(false);
        yield return new WaitForSeconds(2.5f);

        dialogues[7].SetActive(true);
        yield return new WaitForSeconds(5);

        dialogues[7].SetActive(false);
        yield return new WaitForSeconds(2);

        Instantiate(shotgunBulletPrefab, shootPointPos.position, shootPointPos.rotation);
        AudioManager.Instance.PlaySFX(0);
        yield return new WaitForSeconds(.05f);

        finalPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        GameManager.Instance.NextScene();

        yield return null;
    }

    public void HealthBarSystem()
    {
        hpBar.fillAmount = GameManager.Instance.health / GameManager.Instance.maxHealth;
        if (GameManager.Instance.health <= 0 && !gameOverDone) { StartCoroutine(GameOverText()); }
    }

    void EnemyCounter()
    {
        if (enemyCounterText != null)
        {
            enemyCounterText.text = "Enemies: " + GameManager.Instance.remainingEnemies.ToString();
        }
    }

    public void LivesSystem()
    {
        for (int i = 0; i < lives.Length; i++)
        {
            if (i >= GameManager.Instance.lives)
            {
                lives[i].enabled = false;
            }
            else
            {
                lives[i].enabled = true;
            }
        }

        if(GameManager.Instance.lives <= 0 && !gameOverDone) { StartCoroutine(GameOverText()); }
    }

    void ChangeWeapon()
    {
        if (playerController.gun)
        {
            if (!playerController.shotgun) { onlyGunSelected.SetActive(true); gunSelected.SetActive(false); shotgunSelected.SetActive(false); }
            else if (playerController.holdGun && !playerController.holdShotgun) { onlyGunSelected.SetActive(false); gunSelected.SetActive(true); shotgunSelected.SetActive(false); }
            else if (!playerController.holdGun && playerController.holdShotgun) { onlyGunSelected.SetActive(false); gunSelected.SetActive(false); shotgunSelected.SetActive(true); }
        }
        else { onlyGunSelected.SetActive(false); gunSelected.SetActive(false); shotgunSelected.SetActive(false); }
    }

    public void PauseMenu(InputAction.CallbackContext context)
    {
        if (context.started && !gameOverDone && objectToClose == null && !GameManager.Instance.menuOn)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            GameManager.Instance.menuOn = true;
            canClose = false;
            StartCoroutine(nameof(CanCloseMenu));
            Debug.Log("abrir");
        }
    }

    IEnumerator CanCloseMenu() 
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log("canClose");
        canClose = true;

        yield return null;
    }

    IEnumerator LevelCompletedText()
    {
        levelDone = true;
        lvlCompletePanel.SetActive(true);
        AudioManager.Instance.PlaySFX(9);
        yield return new WaitForSeconds(3f);
        lvlCompletePanel.SetActive(false);
    }

    IEnumerator GameOverText()
    {
        playerController.death = true;
        gameOverDone = true;
        gameOverPanel.SetActive(true);
        AudioManager.Instance.PlaySFX(3);
        yield return new WaitForSeconds(5f);
        GameManager.Instance.RestartLevel();
    }

    public void Close(InputAction.CallbackContext context)
    {
        if (context.started && objectToClose != null)
        {
            objectToClose.SetActive(false);
            objectToClose = null;
        }
        else if (context.started && GameManager.Instance.menuOn && canClose)
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
            GameManager.Instance.menuOn = false;
            canClose = false;
            Debug.Log("cerrar");
        }
    }

    public void ButtonExit()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ExitGame();
    }

    public void ButtonMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.PlayMusic(0);
        GameManager.Instance.LoadScene(0);
    }

    public void ButtonOptions()
    {
        options.SetActive(true);
        objectToClose = options;
    }

    public void ButtonBack()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        GameManager.Instance.menuOn = false;
        canClose = false;
    }

    public void ButtonRestartLvl()
    {
        Time.timeScale = 1f;
        int actualScene = SceneManager.GetActiveScene().buildIndex;

        GameManager.Instance.LoadScene(actualScene);
    }

    public void ButtonNextLvl()
    {
        Time.timeScale = 1f;
        GameManager.Instance.NextScene();
    }
}
