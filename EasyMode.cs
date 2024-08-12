using UnityEngine;
using UnityEngine.UI;
using HollowKnight;
using System.Collections.Generic;

public class EasyMode : MonoBehaviour
{
    // UI elements for the Easy Mode menu
    private GameObject easyModeMenu;
    private Button yesButton;
    private Button noButton;

    // Easy Mode settings
    private bool easyModeEnabled = false;
    private float playerHealthMultiplier = 2f;
    private float playerDamageMultiplier = 2f;
    private float enemyDamageMultiplier = 0.5f;

    // Quest Book and quest system
    private bool hasQuestBook = false;
    private bool questInProgress = false;
    private QuestBook questBook;

    // Localization system
    private Dictionary<string, string> localizationEN;
    private Dictionary<string, string> localizationFR;
    private Dictionary<string, string> localizationCurrent;

    private void Start()
    {
        // Initialize localization
        InitializeLocalization();

        // Check if a new game is being started
        if (IsNewGame())
        {
            // Create Easy Mode menu
            CreateEasyModeMenu();
        }
    }

    private void InitializeLocalization()
    {
        // English translations
        localizationEN = new Dictionary<string, string>
        {
            {"QuestBookTitle", "Quest Book"},
            {"BuyQuestBook", "You bought the Quest Book!"},
            {"QuestGiven", "Cornifer has given you a quest!"},
            {"QuestCompleted", "You have completed the quest!"},
            {"RewardReceived", "You received a new charm from Cornifer!"},
            {"Quest1Name", "Explore the Forgotten Crossroads"},
            {"Quest1Description", "Find and explore all the hidden areas in the Forgotten Crossroads."},
            // Add more quests here...
        };

        // French translations
        localizationFR = new Dictionary<string, string>
        {
            {"QuestBookTitle", "Livre de Quêtes"},
            {"BuyQuestBook", "Vous avez acheté le Livre de Quêtes !"},
            {"QuestGiven", "Cornifer vous a donné une quête !"},
            {"QuestCompleted", "Vous avez terminé la quête !"},
            {"RewardReceived", "Vous avez reçu un nouveau charme de Cornifer !"},
            {"Quest1Name", "Explorer les Routes Oubliées"},
            {"Quest1Description", "Trouvez et explorez toutes les zones cachées des Routes Oubliées."},
            // Ajoutez plus de quêtes ici...
        };

        // Set the current localization based on the game's language settings
        localizationCurrent = GameManager.instance.language == "fr" ? localizationFR : localizationEN;
    }

    // Method to check if a new game is being started
    private bool IsNewGame()
    {
        // Replace this line with the correct method to check if a new game is being created
        return GameManager.instance.IsNewGame();
    }

    // Create Easy Mode menu
    private void CreateEasyModeMenu()
    {
        easyModeMenu = new GameObject("Easy Mode Menu");
        easyModeMenu.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Create Yes and No buttons
        yesButton = CreateButton("Yeah!");
        noButton = CreateButton("No");

        // Add listeners to the buttons
        yesButton.onClick.AddListener(EnableEasyMode);
        noButton.onClick.AddListener(DisableEasyMode);

        // Show the Easy Mode menu
        easyModeMenu.SetActive(true);
    }

    // Method to create a button
    private Button CreateButton(string text)
    {
        // Create a new button
        Button button = Instantiate(Resources.Load<Button>("UI/Button"));

        // Set the button text
        button.GetComponentInChildren<Text>().text = text;

        // Add the button to the Easy Mode menu
        button.transform.SetParent(easyModeMenu.transform, false);

        return button;
    }

    // Enable Easy Mode
    private void EnableEasyMode()
    {
        if (!easyModeEnabled) // Ensure it doesn't activate multiple times
        {
            easyModeEnabled = true;

            // Increase player health and damage
            Player player = Player.instance;
            if (player != null)
            {
                player.health *= playerHealthMultiplier;
                player.damage *= playerDamageMultiplier;
            }

            // Reduce enemy damage
            foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                if (enemy != null)
                {
                    enemy.damage *= enemyDamageMultiplier;
                }
            }

            // Remove all spikes
            foreach (Spike spike in GameObject.FindObjectsOfType<Spike>())
            {
                Destroy(spike.gameObject);
            }

            // Hide Easy Mode menu
            easyModeMenu.SetActive(false);

            // Activate quest system
            ActivateQuestSystem();
        }
    }

    // Disable Easy Mode
    private void DisableEasyMode()
    {
        if (easyModeEnabled) // Ensure it doesn't deactivate multiple times
        {
            easyModeEnabled = false;

            // Reset player health and damage
            Player player = Player.instance;
            if (player != null)
            {
                player.health /= playerHealthMultiplier;
                player.damage /= playerDamageMultiplier;
            }

            // Reset enemy damage
            foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                if (enemy != null)
                {
                    enemy.damage /= enemyDamageMultiplier;
                }
            }

            // Hide Easy Mode menu
            easyModeMenu.SetActive(false);
        }
    }

    // Activate quest system
    private void ActivateQuestSystem()
    {
        // Create a Quest Book and add it to the player's inventory
        questBook = new QuestBook(localizationCurrent["QuestBookTitle"]);

        // Add the option to buy the Quest Book from Elderbug
        Elderbug elderbug = FindObjectOfType<Elderbug>();
        if (elderbug != null)
        {
            elderbug.OnInteract += OfferQuestBook;
        }

        // Enable quests from Cornifer
        Cornifer cornifer = FindObjectOfType<Cornifer>();
        if (cornifer != null)
        {
            cornifer.OnInteract += OfferQuest;
        }
    }

    // Method to offer the Quest Book
    private void OfferQuestBook(Player player)
    {
        if (!hasQuestBook && player.geo >= 50)
        {
            player.geo -= 50;
            hasQuestBook = true;
            player.AddToInventory(questBook);
            Debug.Log(localizationCurrent["BuyQuestBook"]);
        }
    }

    // Method to offer quests from Cornifer
    private void OfferQuest(Player player)
    {
        if (hasQuestBook && !questInProgress)
        {
            // Start a new quest
            questInProgress = true;
            Quest newQuest = new Quest(localizationCurrent["Quest1Name"], localizationCurrent["Quest1Description"]);
            questBook.AddQuest(newQuest);
            Debug.Log(localizationCurrent["QuestGiven"]);
        }
        else if (hasQuestBook && questInProgress)
        {
            // Complete the quest
            questInProgress = false;
            questBook.CompleteQuest(localizationCurrent["Quest1Name"]);
            GiveQuestReward(player);
        }
    }

    // Reward the player after completing the quest
    private void GiveQuestReward(Player player)
    {
        // Give an optional charm to the player
        Charm newCharm = new Charm("Optional Charm", "A charm that is not essential to complete the game.");
        player.AddCharm(newCharm);
        Debug.Log(localizationCurrent["RewardReceived"]);
    }
}

// Additional class for the Quest Book
public class QuestBook
{
    private string title;
    private List<Quest> quests;
    private List<Quest> completedQuests;

    public QuestBook(string title)
    {
        this.title = title;
        this.quests = new List<Quest>();
        this.completedQuests = new List<Quest>();
    }

    public void AddQuest(Quest quest)
    {
        quests.Add(quest);
        Debug.Log($"New quest added: {quest.name}");
    }

    public void CompleteQuest(string questName)
    {
        Quest quest = quests.Find(q => q.name == questName);
        if (quest != null)
        {
            quests.Remove(quest);
            completedQuests.Add(quest);
            Debug.Log($"Quest completed: {quest.name}");
        }
    }

    // Add other methods as necessary to handle the Quest Book UI and interactions
}

// Additional class for Quests
public class Quest
{
    public string name;
    public string description;

    public Quest(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}
