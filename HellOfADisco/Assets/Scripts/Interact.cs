using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] UIControl uiControl;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject closedChest;
    [SerializeField] GameObject shotgun;
    [SerializeField] GameObject openedChest;
    [SerializeField] GameObject gunDescription;
    [SerializeField] GameObject doorLight;
    [SerializeField] GameObject exitPanel;
    string objetName;
    public GameObject note;
    bool noteRead;
    bool chestOpened;
    bool npcDone;

    private void Start()
    {
        objetName = gameObject.name;
    }

    private void Update()
    {
        if (objetName == "Door") 
        {
            if (GameManager.Instance.remainingEnemies <= 0) { doorLight.SetActive(true); }
        }
        else if (objetName == "Note" && noteRead && uiControl.objectToClose == null)
        {
            note.SetActive(false); noteRead = false;  uiControl.DialogueOn(3); playerController.interacted = false; GameManager.Instance.onAnimation2 = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (objetName == "NPCInteractor" && !npcDone)  { uiControl.DialogueOn(5); shotgun.SetActive(true); npcDone = true; }
        }
        if (other.CompareTag("Player"))
        {
            if (objetName == "Shotgun") { playerController.shotgun = true; playerController.holdShotgun = true; doorLight.SetActive(true); gameObject.SetActive(false); AudioManager.Instance.PlaySFX(5); }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController.interacted)
            {
                if (objetName == "Note" && !noteRead) { note.SetActive(true); noteRead = true; uiControl.noteDone = true; uiControl.objectToClose = note; GameManager.Instance.onAnimation2 = true; }
                if (objetName == "Chest" && uiControl.noteDone && !uiControl.chestDone) { chestOpened = true; uiControl.chestDone = true; ui.SetActive(true);  closedChest.SetActive(false); openedChest.SetActive(true); gunDescription.SetActive(true); playerController.gun = true; playerController.holdGun = true; doorLight.SetActive(true); AudioManager.Instance.PlaySFX(5); }
                if (objetName == "Door1" && uiControl.chestDone) { StartCoroutine(NextScene()); }
                if (objetName == "Door2" && playerController.shotgun) { StartCoroutine(NextScene()); }
                if (objetName == "Door" && GameManager.Instance.remainingEnemies <=0) { StartCoroutine(NextScene()); }
                playerController.interacted = false;
            }
        }
    }
    private void OnTriggerExit2D (Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (objetName == "Chest" && chestOpened) { chestOpened = false; closedChest.SetActive(true); openedChest.SetActive(false); gunDescription.SetActive(false); }
        }
    }

    IEnumerator NextScene()
    {
        exitPanel.SetActive(true);
        yield return new WaitForSeconds(2);

        GameManager.Instance.NextScene();
        yield return null;
    }
}
