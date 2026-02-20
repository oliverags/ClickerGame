using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class TimedObjective {
    public string description;
    public float targetValue;
    public float timeLimit;
    public Objective.ObjectiveType type;
    public bool isCompleted;
    public float reward;
    [HideInInspector]
    public float startTime;
}

[System.Serializable]
public class Objective {
    public string description;
    public float targetValue;
    public enum ObjectiveType { Count, Income, Upgrades }
    public ObjectiveType type;
    public bool isCompleted;
    public float reward;
}

public class GameManager : MonoBehaviour {
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text prestigeText;
    [SerializeField] private Objective[] objectives;
    [SerializeField] private TimedObjective[] timedObjectives;
    [SerializeField] private StoreUpgrade[] storeUpgrades;
    [SerializeField] private int updatesPerSecond = 5;
    [SerializeField] private float prestigeCost = 1000f;
    [SerializeField] private float prestigeBonus = 0.1f;

    private float count = 0;
    private float lastIncomeValue = 0;
    private int prestigeLevel = 0;
    private float prestigeMultiplier = 1f;
    private float nextTimeCheck = 1;
    private SaveSystem saveSystem;

    public float GetCount() => count;
    public int GetPrestigeLevel() => prestigeLevel;
    public float GetPrestigeMultiplier() => prestigeMultiplier;

    private void Start() {
        if (countText == null || incomeText == null || objectiveText == null) {
            Debug.LogError("[ERROR] Missing UI references in GameManager!");
            return;
        }

        if (storeUpgrades == null || storeUpgrades.Length == 0) {
            Debug.LogWarning("[WARNING] No upgrades registered!");
        }

        saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem == null) {
            Debug.LogWarning("[WARNING] SaveSystem not found. A new game will be started.");
        }

        LoadGameState();
        UpdateUI();
        UpdateObjectiveUI();
        Debug.Log("[SUCCESS] GameManager initialized!");
    }

    private void LoadGameState() {
        if (saveSystem == null) return;
        GameData savedData = saveSystem.LoadGame();
        if (savedData != null) {
            count = savedData.count;
            prestigeLevel = savedData.prestigeLevel;
            prestigeMultiplier = savedData.prestigeMultiplier;

            for (int i = 0; i < savedData.upgradeLevels.Length && i < storeUpgrades.Length; i++) {
                storeUpgrades[i].SetLevel(savedData.upgradeLevels[i]);
            }

            for (int i = 0; i < savedData.completedObjectives.Length && i < objectives.Length; i++) {
                objectives[i].isCompleted = savedData.completedObjectives[i];
            }

            for (int i = 0; i < savedData.completedTimedObjectives.Length && i < timedObjectives.Length; i++) {
                timedObjectives[i].isCompleted = savedData.completedTimedObjectives[i];
            }

            Debug.Log("[SUCCESS] Game restored! Count: " + count + ", Prestige Level: " + prestigeLevel);
        }
    }

    private void UpdateObjectiveUI() {
        string objectiveDisplay = "== OBJECTIVES ==\n";
        int completedCount = 0;

        foreach (var objective in objectives) {
            if (!objective.isCompleted) {
                objectiveDisplay += "- " + objective.description + "\n  " + GetObjectiveProgress(objective) + "\n";
            } else {
                completedCount++;
            }
        }

        objectiveDisplay += "\n== TIMED OBJECTIVES ==\n";

        foreach (var timedObjective in timedObjectives) {
            if (!timedObjective.isCompleted) {
                float timeRemaining = timedObjective.timeLimit - (Time.timeSinceLevelLoad - timedObjective.startTime);
                string timeStr = Mathf.Max(0, timeRemaining).ToString("F1");
                objectiveDisplay += "- " + timedObjective.description + "\n  " + GetObjectiveProgress(timedObjective) + " | Time: " + timeStr + "s\n";
            }
        }

        objectiveDisplay += "\nCompleted: " + completedCount + "/" + objectives.Length;
        objectiveText.text = objectiveDisplay;
    }

    private string GetObjectiveProgress(Objective objective) {
        switch (objective.type) {
            case Objective.ObjectiveType.Count:
                return Mathf.RoundToInt(count) + "/" + objective.targetValue;
            case Objective.ObjectiveType.Income:
                return lastIncomeValue.ToString("F1") + "/" + objective.targetValue;
            case Objective.ObjectiveType.Upgrades:
                int totalUpgrades = 0;
                foreach (var upgrade in storeUpgrades) totalUpgrades += upgrade.GetLevel();
                return totalUpgrades + "/" + objective.targetValue;
            default:
                return "";
        }
    }

    private string GetObjectiveProgress(TimedObjective objective) {
        switch (objective.type) {
            case Objective.ObjectiveType.Count:
                return Mathf.RoundToInt(count) + "/" + objective.targetValue;
            default:
                return "";
        }
    }

    private void CheckObjectives() {
        foreach (var objective in objectives) {
            if (!objective.isCompleted) {
                bool completed = false;
                switch (objective.type) {
                    case Objective.ObjectiveType.Count:
                        if (count >= objective.targetValue) completed = true;
                        break;
                    case Objective.ObjectiveType.Income:
                        if (lastIncomeValue >= objective.targetValue) completed = true;
                        break;
                    case Objective.ObjectiveType.Upgrades:
                        int totalUpgrades = 0;
                        foreach (var upgrade in storeUpgrades) totalUpgrades += upgrade.GetLevel();
                        if (totalUpgrades >= objective.targetValue) completed = true;
                        break;
                }

                if (completed) {
                    objective.isCompleted = true;
                    count += objective.reward;
                    Debug.Log("[OBJECTIVE] Completed: " + objective.description + "! +" + objective.reward + " cookies");
                    UpdateObjectiveUI();
                    UpdateUI();
                }
            }
        }
    }

    public void ClickAction() {
        float clickValue = 1f * prestigeMultiplier;
        count += clickValue;

        foreach (var upgrade in storeUpgrades) {
            upgrade.UpdateUI();
        }

        StartCoroutine(ClickFeedback());
        UpdateUI();
    }

    private IEnumerator ClickFeedback() {
        yield return new WaitForSeconds(0.1f);
    }

    private void UpdateUI() {
        countText.text = FormatNumber(Mathf.RoundToInt(count));
        incomeText.text = lastIncomeValue.ToString("F1") + "/s";
        if (prestigeText != null) prestigeText.text = "Prestige: " + prestigeLevel + " (" + prestigeMultiplier.ToString("F1") + "x)";
    }

    private string FormatNumber(long number) {
        if (number >= 1_000_000_000) return (number / 1_000_000_000f).ToString("F1") + "B";
        if (number >= 1_000_000) return (number / 1_000_000f).ToString("F1") + "M";
        if (number >= 1_000) return (number / 1_000f).ToString("F1") + "K";
        return number.ToString();
    }

    public bool PurchaseAction(int cost) {
        if (count >= cost) {
            count -= cost;

            foreach (var upgrade in storeUpgrades) {
                upgrade.UpdateUI();
            }

            UpdateUI();
            return true;
        }

        Debug.Log("[WARNING] You don't have " + cost + " cookies! You have " + count);
        return false;
    }

    private void Update() {
        if (nextTimeCheck < Time.timeSinceLevelLoad) {
            IdleCalculate();
            CheckObjectives();
            CheckTimedObjectives();
            nextTimeCheck = Time.timeSinceLevelLoad + (1f / updatesPerSecond);
        }
    }

    private void IdleCalculate() {
        float totalIncome = 0;
        foreach (var upgrade in storeUpgrades) {
            totalIncome += upgrade.CalculateIncomePerSecond();
        }

        lastIncomeValue = totalIncome;
        count += totalIncome * prestigeMultiplier / updatesPerSecond;

        foreach (var upgrade in storeUpgrades) {
            upgrade.UpdateUI();
        }

        UpdateUI();
    }

    private void CheckTimedObjectives() {
        foreach (var objective in timedObjectives) {
            if (!objective.isCompleted && objective.startTime == 0) {
                objective.startTime = Time.timeSinceLevelLoad;
                Debug.Log("[TIMED] Objective started: " + objective.description);
            }

            if (!objective.isCompleted) {
                float elapsedTime = Time.timeSinceLevelLoad - objective.startTime;
                if (elapsedTime <= objective.timeLimit) {
                    bool completed = false;
                    if (objective.type == Objective.ObjectiveType.Count && count >= objective.targetValue) completed = true;

                    if (completed) {
                        objective.isCompleted = true;
                        count += objective.reward;
                        Debug.Log("[TIMED] Objective completed: " + objective.description + "! +" + objective.reward + " cookies");
                        UpdateObjectiveUI();
                        UpdateUI();
                    }
                } else {
                    objective.isCompleted = true;
                    Debug.Log("[TIMED] Objective expired: " + objective.description);
                    UpdateObjectiveUI();
                }
            }
        }
    }

    public void Prestige() {
        if (count < prestigeCost) {
            Debug.LogWarning("[WARNING] You need " + prestigeCost + " cookies to prestige! You have " + count);
            return;
        }

        count = 0;
        foreach (var upgrade in storeUpgrades) {
            upgrade.ResetLevel();
        }

        prestigeLevel++;
        prestigeMultiplier = 1f + prestigeLevel * prestigeBonus;
        Debug.Log("[PRESTIGE] PRESTIGED! Level " + prestigeLevel + ", Multiplier: " + prestigeMultiplier.ToString("F1") + "x");
        UpdateUI();
        UpdateObjectiveUI();
    }

    public void SaveGame() {
        if (saveSystem != null) {
            saveSystem.SaveGame(this, storeUpgrades, objectives, timedObjectives);
        }
    }

    private void OnApplicationQuit() {
        SaveGame();
    }
}