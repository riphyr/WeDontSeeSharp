using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager Instance;

    [Header("UI")]
    public GameObject consolePanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;
    private List<string> consoleLines = new List<string>();
    private const int maxLines = 15;

    [Header("Composants à désactiver pendant la console")]
    [SerializeField] private MonoBehaviour playerScript;
    [SerializeField] private MonoBehaviour playerUsing;
    [SerializeField] private MonoBehaviour cameraLookingAt;
    [SerializeField] private MonoBehaviour pauseMenuManager;
    [SerializeField] private MonoBehaviour playerJournalUI;
    [SerializeField] private MonoBehaviour playerInventoryUI;

    [HideInInspector] public bool isOpen = false;

    void Awake()
    {
        Instance = this;
        consolePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Quote) && !isOpen) ToggleConsole(true);

        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape)) { ToggleConsole(false); return; }

        if (EventSystem.current.currentSelectedGameObject != inputField.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            PrintToConsole("> " + command);
            ExecuteCommand(command);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    void ToggleConsole(bool open)
    {
        Debug.Log("Toggle console");
        
        isOpen = open;
        consolePanel.SetActive(open);

        if (open)
        {
            inputField.text = "";
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerScript) playerScript.enabled = !open;
        if (playerUsing) playerUsing.enabled = !open;
        if (cameraLookingAt) cameraLookingAt.enabled = !open;
        if (pauseMenuManager) pauseMenuManager.enabled = !open;
        if (playerJournalUI) playerJournalUI.enabled = !open;
        if (playerInventoryUI) playerInventoryUI.enabled = !open;
    }

    void ExecuteCommand(string input)
    {
        string[] args = input.Split(' ');
        CommandHandler.Instance.Execute(args);
    }// assigné dans l’Inspector

    public void PrintToConsole(string line)
    {
        consoleLines.Add(line);

        if (consoleLines.Count > maxLines)
            consoleLines.RemoveAt(0); // retire la plus ancienne

        outputText.text = string.Join("\n", consoleLines);
    }
}
