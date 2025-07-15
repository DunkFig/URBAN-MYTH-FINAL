using UnityEngine;
using TMPro;

public class CollectibleUI : MonoBehaviour
{
    [Header("Assign your TMP Text fields here")]
    public TMP_Text coinText;
    public TMP_Text gemText;
    public TMP_Text keyText;

    [Header("Script to trigger when 50 coins are found")]
    public MonoBehaviour secondaryScript; // generic reference — can be any script

    private bool rewardTriggered = false; // Prevents multiple triggers

    void OnEnable()
    {
        // Subscribe to updates
        CollectibleManager.OnTypeCountChanged += UpdateCount;

        // Initialize display
        coinText.text = CollectibleManager.Instance.GetCount(CollectibleType.Coin).ToString();
        gemText.text  = CollectibleManager.Instance.GetCount(CollectibleType.Gem).ToString();
        keyText.text  = CollectibleManager.Instance.GetCount(CollectibleType.Key).ToString();
    }

    void OnDisable()
    {
        CollectibleManager.OnTypeCountChanged -= UpdateCount;
    }

    void UpdateCount(CollectibleType type, int newCount)
    {
        switch (type)
        {
            case CollectibleType.Coin:
                coinText.text = newCount.ToString();

                if (newCount >= 2 && !rewardTriggered && secondaryScript != null)
                {
                    rewardTriggered = true;

                    // Call the method on the secondary script
                    secondaryScript.SendMessage("TriggerReward");
                }
                break;

            case CollectibleType.Gem:
                gemText.text = newCount.ToString();
                break;

            case CollectibleType.Key:
                keyText.text = newCount.ToString();
                break;
        }
    }
}
