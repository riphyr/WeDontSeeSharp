using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerJournalUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject journalPanel;

    [Header("Menus à désactiver")]
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private PauseMenu.PauseMenuManager pauseMenuManager;
    [SerializeField] private PlayerUsing playerUsing;
    [SerializeField] private CameraLookingAt cameraLookingAt;

    [Header("Réinitialisation menu")]
    [SerializeField] private ListButtonGenerator buttonGenerator;
    [SerializeField] private PreviewManager previewManager;
    [SerializeField] private EntranceAnimator slidePreviewAnimator;
    [SerializeField] private ContentEntrance contentEntrance;

    private bool isJournalOpen = false;
    private KeyCode journalKey;

    private void Start()
    {
        HideJournal();
        LoadJournalKey();
    }

    private void Update()
    {
        if (Input.GetKeyDown(journalKey))
        {
            if (isJournalOpen)
                HideJournal();
            else
                ShowJournal();
        }
        else if (isJournalOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HideJournal();
        }

        LoadJournalKey();
    }

    private void LoadJournalKey()
    {
        string key = PlayerPrefs.GetString("Journal", "J");
        journalKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }

    public void ShowJournal()
    {
        isJournalOpen = true;
        if (slidePreviewAnimator) slidePreviewAnimator.ResetSlide();
        if (contentEntrance) contentEntrance.ResetContent();
        if (journalPanel) journalPanel.SetActive(true);

        if (playerInventoryUI) playerInventoryUI.enabled = false;
        if (playerScript) playerScript.enabled = false;
        if (pauseMenuManager) pauseMenuManager.enabled = false;
        if (playerUsing) playerUsing.enabled = false;
        if (cameraLookingAt) cameraLookingAt.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideJournal()
    {
        isJournalOpen = false;
        ResetJournalUI();
        if (journalPanel) journalPanel.SetActive(false);

        if (playerInventoryUI) playerInventoryUI.enabled = true;
        if (playerScript) playerScript.enabled = true;
        if (pauseMenuManager) pauseMenuManager.enabled = true;
        if (playerUsing) playerUsing.enabled = true;
        if (cameraLookingAt) cameraLookingAt.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ResetJournalUI()
    {
        previewManager?.ClearPreview();
        buttonGenerator?.ResetSelectedButton();

        slidePreviewAnimator.ResetSlide();
        contentEntrance.ResetContent();
    }
}
