using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class CommandHandler : MonoBehaviour
{
    public static CommandHandler Instance;

    private readonly HashSet<string> validItems = new HashSet<string>
    {
        "Battery", "Bedroom key", "EMFDetector", "Bleach", "Candle", "Capture key",
        "Cassette", "CDDisk", "Flashlight", "Crowbar", "Game room key", "Home office key",
        "Inert solution", "Lemon juice", "Library key", "Lighter", "Magnetophone", "Match",
        "Red wine", "Solvent", "UVFlashlight", "WD40", "Wrench"
    };

    private readonly HashSet<string> validLoreItems = new HashSet<string>
    {
        "Binder", "Clipboard", "Diary", "File", "Letter", "Newspaper", "Photo"
    };

    [Header("References")]
    public PlayerScript playerScript;
    public PlayerInventory playerInventory;
    public LoreDatabase loreDatabase;

    void Awake()
    {
        Instance = this;
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0) return;

        switch (args[0].ToLower())
        {
            case "give": HandleGive(args); break;
            case "gamerule": HandleGamerule(args); break;
            case "get": if (args.Length > 1 && args[1] == "data") HandleGetData(args); break;
            case "scene": HandleScene(args); break;
            default: ConsoleManager.Instance.PrintToConsole("Unknown command : " + args[0]); break;
        }
    }

    private void HandleGive(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleManager.Instance.PrintToConsole("Usage : give item/lore/* [name] [amount]");
            return;
        }

        string type = args[1].ToLower();

        if (type == "item" || type == "lore")
        {
            if (args.Length < 3)
            {
                ConsoleManager.Instance.PrintToConsole("Usage : give " + type + " [name] [amount]");
                return;
            }

            int count = 1;
            bool hasCount = int.TryParse(args[^1], out count);
            int nameStartIndex = 2;
            int nameEndIndex = hasCount ? args.Length - 1 : args.Length;

            string name = string.Join(" ", args.Skip(nameStartIndex).Take(nameEndIndex - nameStartIndex));

            if (type == "item")
            {
                if (!validItems.Contains(name))
                {
                    ConsoleManager.Instance.PrintToConsole("Unknown item : " + name);
                    return;
                }

                playerInventory.AddItem(name, count);
            }
            else
            {
                if (!validLoreItems.Contains(name))
                {
                    ConsoleManager.Instance.PrintToConsole("Unknown lore item : " + name);
                    return;
                }

                playerInventory.AddItem(name, count);
                loreDatabase.MarkAsDiscovered(name);
            }
        }
        else if (type == "*")
        {
            foreach (string name in validItems)
                playerInventory.AddItem(name);

            foreach (string name in validLoreItems)
            {
                playerInventory.AddItem(name);
                loreDatabase.MarkAsDiscovered(name);
            }
        }
        else
        {
            ConsoleManager.Instance.PrintToConsole("Unknown type : " + type);
        }
    }

    private void HandleGamerule(string[] args)
    {
        if (args.Length < 3)
        {
            ConsoleManager.Instance.PrintToConsole("Usage : gamerule ruleName true/false");
            return;
        }

        string rule = args[1].ToLower();
        bool state = bool.Parse(args[2]);

        switch (rule)
        {
            case "infinitestamina":
                playerScript._staminaDrain = state ? 0f : 30f;
                ConsoleManager.Instance.PrintToConsole("Infinite stamina : " + state);
                break;

            case "unvulnerable":
                playerScript.gameObject.tag = state ? "Untagged" : "Player";
                ConsoleManager.Instance.PrintToConsole("Unvulnerable : " + state);
                break;

            default:
                ConsoleManager.Instance.PrintToConsole("Unknown gamerule : " + rule);
                break;
        }
    }

    private void HandleGetData(string[] args)
    {
        if (args.Length < 3)
        {
            ConsoleManager.Instance.PrintToConsole("Usage : get data [type] [subtype]");
            return;
        }

        string type = args[2].ToLower();
        switch (type)
        {
            case "lockpad":
                if (args.Length >= 4)
                {
                    string code;
                    if (args[3] == "undergound") code = "2783";
                    else if (args[3] == "ground_floor") code = "5381";
                    else
                    {
                        ConsoleManager.Instance.PrintToConsole("Unknown floor : " + args[3]);
                        return;
                    }
                    ConsoleManager.Instance.PrintToConsole("Lockpad code (" + args[3] + ") : " + code);
                }
                break;
            case "electrical_box":
                ConsoleManager.Instance.PrintToConsole("Electrical Box code : ON OFF - OFF OFF - ON ON - OFF OFF - ON ON - ON OFF - ON OFF - OFF");
                break;
            case "chemistry_station":
                ConsoleManager.Instance.PrintToConsole("Chemistry combination : Lemon juice + WD40");
                break;
            case "safe":
                ConsoleManager.Instance.PrintToConsole("Safe code : 10-95-03");
                break;
            default:
                ConsoleManager.Instance.PrintToConsole("Unknown data type : " + type);
                break;
        }
    }

    private void HandleScene(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleManager.Instance.PrintToConsole("Usage : scene load/reset [name]");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            ConsoleManager.Instance.PrintToConsole("Only the MasterClient can change the scene.");
            return;
        }

        string mode = args[1].ToLower();
        if (mode == "load" && args.Length >= 3)
        {
            GameManager.Instance.SaveGame(args[2]);
            PhotonNetwork.LoadLevel(args[2]);
        }
        else if (mode == "reset")
        {
            string currentScene = SceneManager.GetActiveScene().name;
            GameManager.Instance.SaveGame(currentScene);
            PhotonNetwork.LoadLevel(currentScene);
        }
        else
        {
            ConsoleManager.Instance.PrintToConsole("Invalid scene command.");
        }
    }
}