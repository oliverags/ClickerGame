using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoreUpgrade : MonoBehaviour 
{ 
    [Header("UI Components")] 
    [SerializeField] private TMP_Text priceText; 
    [SerializeField] private TMP_Text incomeInfoText; 
    [SerializeField] private Button button; 
    [SerializeField] private Image characterImage; 
    [SerializeField] private TMP_Text upgradeNameText; 

    [Header("Upgrade Values")] 
    [SerializeField] private string upgradeName; 
    [SerializeField] private int startPrice = 15; 
    [SerializeField] private float upgradePriceMultiplier = 1.15f; 
    [SerializeField] private float cookiesPerUpgrade = 0.1f; 

    [Header("Game Reference")] 
    [SerializeField] private GameManager gameManager; 

    private int level = 0; 
    private Color unlockedColor = Color.white; 
    private Color lockedColor = Color.black; 

    private void Start() 
    { 
        ValidateReferences(); 
        UpdateUI(); 
    } 

    private void ValidateReferences() 
    { 
        if (gameManager == null) 
        { 
            gameManager = FindObjectOfType<GameManager>(); 
            if (gameManager == null) 
            { 
                Debug.LogError("[ERROR] GameManager not found for upgrade: " + upgradeName); 
            } 
        } 
        if (button == null) Debug.LogError("[ERROR] Button not assigned for upgrade: " + upgradeName); 
        if (priceText == null) Debug.LogError("[ERROR] PriceText not assigned for upgrade: " + upgradeName); 
        if (incomeInfoText == null) Debug.LogError("[ERROR] IncomeInfoText not assigned for upgrade: " + upgradeName); 
    } 

    public void UpdateUI() 
    { 
        if (priceText != null) priceText.text = CalculatePrice().ToString(); 
        if (incomeInfoText != null) incomeInfoText.text = level + " x " + cookiesPerUpgrade.ToString("F1") + "/s = " + CalculateIncomePerSecond().ToString("F2") + "/s"; 
        if (gameManager != null) 
        { 
            bool canAfford = gameManager.GetCount() >= CalculatePrice(); 
            if (button != null) button.interactable = canAfford; 
            bool isPurchased = level > 0; 
            if (characterImage != null) characterImage.color = isPurchased ? unlockedColor : lockedColor; 
            if (upgradeNameText != null) upgradeNameText.text = isPurchased ? upgradeName : "???"; 
        } 
        else 
        { 
            Debug.LogWarning("[WARNING] GameManager is null in UpdateUI for upgrade: " + upgradeName); 
        } 
    } 

    public int CalculatePrice() 
    { 
        if (upgradePriceMultiplier <= 0) 
        { 
            Debug.LogError("[ERROR] Invalid PriceMultiplier for " + upgradeName); 
            return startPrice; 
        } 
        int price = Mathf.RoundToInt(startPrice * Mathf.Pow(upgradePriceMultiplier, level)); 
        return Mathf.Max(1, price); 
    } 

    public float CalculateIncomePerSecond() 
    { 
        return cookiesPerUpgrade * level; 
    } 

    public void ClickAction() 
    { 
        if (gameManager == null) 
        { 
            Debug.LogError("[ERROR] GameManager is null in upgrade: " + upgradeName); 
            return; 
        } 
        int price = CalculatePrice(); 
        bool purchaseSuccess = gameManager.PurchaseAction(price); 
        if (purchaseSuccess) 
        { 
            level++; 
            Debug.Log("[SUCCESS] Upgrade purchased: " + upgradeName + " (Level " + level + ")"); 
            UpdateUI(); 
        } 
    } 

    public int GetLevel() 
    { 
        return level; 
    } 

    public void SetLevel(int newLevel) 
    { 
        if (newLevel < 0) 
        { 
            Debug.LogWarning("[WARNING] Attempt to set negative level for " + upgradeName); 
            return; 
        } 
        level = newLevel; 
        UpdateUI(); 
    } 

    public void ResetLevel() 
    { 
        level = 0; 
        UpdateUI(); 
    } 
}