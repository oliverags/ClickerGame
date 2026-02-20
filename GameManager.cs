using UnityEngine;
using TMPro;
using Unity.VisualScripting;


[System.Serializable]
public class TimedObjective
{
    public string description;
    public float targetValue;
    public float timeLimit; // Em segundos
    public Objective.ObjectiveType type;
    public bool isCompleted;
    public float reward;
    [HideInInspector] public float startTime;
}

[System.Serializable]
public class Objective
{
    public string description; // Ex.: "Coletar 100 cookies"
    public float targetValue; // Ex.: 100 para count, 5 para upgrades, etc.
    public enum ObjectiveType { Count, Income, Upgrades }
    public ObjectiveType type;
    public bool isCompleted;
    public float reward; // Recompensa em cookies por completar o objetivo
}


public class GameManager : MonoBehaviour
{

    [SerializeField] private float prestigeMultiplier = 1f;
    

    [SerializeField] private Objective[] objectives; // Lista de objetivos no Inspector
    [SerializeField] private TMP_Text objectiveText; // Texto na UI para exibir objetivos
    [SerializeField] private TimedObjective[] timedObjectives;

    [SerializeField] TMP_Text countText;    
    [SerializeField] TMP_Text IncomeText;
    
    [SerializeField] StoreUpgrade[] storeUpgrades;
    [SerializeField] int updatesPerSecond = 5 ;
    
    
 



    [HideInInspector] public float count = 0;
    
    
    float nextTimeCheck = 1; 
    float lastIncomeValue = 0;
    private int prestigeLevel = 0;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
        UpdateObjectiveUI();
    }



    private void UpdateObjectiveUI()
    {
        string objectiveDisplay = "Objetivos";
        foreach (var objective in objectives)
        {
            if (!objective.isCompleted)
            {
                objectiveDisplay += $"{objective.description} ({GetObjectiveProgress(objective)})\n";
            }
        }
        objectiveText.text = objectiveDisplay;
    }

    private string GetObjectiveProgress(Objective objective)
    {
        switch (objective.type)
        {
            case Objective.ObjectiveType.Count:
                return $"{Mathf.RoundToInt(count)}/{objective.targetValue}";
            case Objective.ObjectiveType.Income:
                return $"{lastIncomeValue:F1}/{objective.targetValue}";
            case Objective.ObjectiveType.Upgrades:
                int totalUpgrades = 0;
                foreach (var upgrade in storeUpgrades) totalUpgrades += upgrade.GetLevel();
                return $"{totalUpgrades}/{objective.targetValue}";
            default:
                return "";
        }
    }

    private void CheckObjectives()
    {
        foreach (var objective in objectives)
        {
            if (!objective.isCompleted)
            {
                bool completed = false;
                switch (objective.type)
                {
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
                if (completed)
                {
                    objective.isCompleted = true;
                    count += objective.reward; // Recompensa por completar
                    UpdateObjectiveUI();
                    UpdateUI();
                }
            }
        }
    }


    // Update is called once per frame


    public void ClickAction()
    {
        count += prestigeMultiplier; // Aplica o multiplicador ao clicar
        count += lastIncomeValue * 0.02f * prestigeMultiplier;
        UpdateUI();
    }

    void UpdateUI()
    {
        countText.text = Mathf.RoundToInt(count).ToString();  
        IncomeText.text = lastIncomeValue.ToString() + "/s";

    }


    public bool PurchaseAction(int cost)
    {
        if(count > cost)
        {
            count -= cost;
            UpdateUI();
            return true;
        }    
        return false;

    }



    void Update()
    {
        if (nextTimeCheck < Time.timeSinceLevelLoad)
        {
            IdleCalculate();
            CheckObjectives();
            CheckTimedObjectives();
            nextTimeCheck = Time.timeSinceLevelLoad + (1f / updatesPerSecond);
        }
    }

    void IdleCalculate()
    {
        float sum = 0;
        foreach (var storeUpgrade in storeUpgrades)
        {
            sum += storeUpgrade.CalculateIncomePerSecond();
            storeUpgrade.UpdateUI();
        }
        lastIncomeValue = sum;
        count += sum * prestigeMultiplier / updatesPerSecond; // Aplica o multiplicador ao ganho passivo
        UpdateUI();
    }

    private void CheckTimedObjectives()
    {
        foreach (var objective in timedObjectives)
        {
            if (!objective.isCompleted && objective.startTime == 0)
            {
                objective.startTime = Time.timeSinceLevelLoad;
            }
            if (!objective.isCompleted && Time.timeSinceLevelLoad - objective.startTime <= objective.timeLimit)
            {
                bool completed = false;
                switch (objective.type)
                {
                    case Objective.ObjectiveType.Count:
                        if (count >= objective.targetValue) completed = true;
                        break;
                        // Adicione outros tipos conforme necessário
                }
                if (completed)
                {
                    objective.isCompleted = true;
                    count += objective.reward;
                    UpdateObjectiveUI();
                    UpdateUI();
                }
            }
            else if (!objective.isCompleted && Time.timeSinceLevelLoad - objective.startTime > objective.timeLimit)
            {
                objective.isCompleted = true; // Falhou, mas marca como concluído para não verificar novamente
                UpdateObjectiveUI();
            }
        }
    }

    public void Prestige()
    {
        if (count >= 1000) // Requer 1000 cookies para prestigiar
        {
            count = 0;
            foreach (var upgrade in storeUpgrades)
            {
                upgrade.ResetLevel();
            }
            prestigeLevel++;
            prestigeMultiplier = 1f + prestigeLevel * 0.1f; // Aumenta 10% por prestígio
            UpdateUI();
        }
    }

  

   
}


