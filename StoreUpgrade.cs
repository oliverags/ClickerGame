using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StoreUpgrade : MonoBehaviour
{
    [Header("Componets")]
    public TMP_Text priceText;
    public TMP_Text IncomeInfoText;
    public Button button;
    public Image characterImage;
    public TMP_Text upgradeNameText;

    [Header("GeneratorValues")]
    public String upgradeName;
    public int startPrice = 15;
    public float UpgradePriceMultiplier;
    public float cookiesPerUpgrade = 0.1f   ;

    [Header("GameManager")]
    public GameManager gameManager;


    int level = 0;


    private void Start()
    {
        UpdateUI();
    }

  public  void UpdateUI()
    {
        priceText.text = CalculatePrice().ToString();
        IncomeInfoText.text = level.ToString() + " x " + cookiesPerUpgrade + "/s";
        bool canAfford = gameManager.count >= CalculatePrice();
        button.interactable = canAfford;

        bool isPurchase = level > 0;
        characterImage.color = isPurchase ? Color.white : Color.black; 
        upgradeNameText.text = isPurchase ? upgradeName : "???";
        }


    int CalculatePrice()
    {
        int price = Mathf.RoundToInt(startPrice * Mathf.Pow(UpgradePriceMultiplier, level));
        return price;

    }

    public float CalculateIncomePerSecond()
    {
        return cookiesPerUpgrade * level;
    }
    

    public void ClickAction()
    {
        int price = CalculatePrice();
        bool purchaseSuccess = gameManager.PurchaseAction(price);
        if(purchaseSuccess)
        {
            level++;
            UpdateUI();
        }

    }

    public int GetLevel()
    {
        return level;
    }

    public void ResetLevel()
    {
        level = 0;
        UpdateUI();
    }
}
