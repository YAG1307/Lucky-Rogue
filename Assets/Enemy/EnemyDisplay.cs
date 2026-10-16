using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyDisplay : MonoBehaviour
{
    public TMP_Text enemyNameText;
    public Image enemyIconImage;
    public TMP_Text healthText;
    public TMP_Text rewardText;
    public TMP_Text cheatInfoText;
    public Sprite fallSkeletonSprite;
    public Sprite hollowTreeSprite;
    public Sprite pumpkinMonsterSprite;

    private int maxHealth;

    public void SetupDisplay(EnemyType type, int currentHealth, int ante = 1)
    {
        string eName = "";
        int maxHP = 3;
        int baseReward = 2;
        string cheatDesc = "";
        Sprite icon = null;

        int tierOffset = (ante - 1) / 3;

        switch (type)
        {
            case EnemyType.FallSkeleton:
                eName = "Fall Skeleton";
                maxHP = 3;
                baseReward = 2 + tierOffset; 
                cheatDesc = "Swaps your top cards between two random lanes.";
                icon = fallSkeletonSprite;
                break;

            case EnemyType.HollowTree:
                eName = "Hollow Tree";
                maxHP = 4;
                baseReward = 3 + tierOffset;
                cheatDesc = "Destroys top card in your lead lane.";
                icon = hollowTreeSprite;
                break;

            case EnemyType.PumpkinMonster:
                eName = "Pumpkin Monster";
                maxHP = 3;
                baseReward = 2 + tierOffset;
                cheatDesc = "Disables your ability to discard cards.";
                icon = pumpkinMonsterSprite;
                break;
        }

        maxHealth = maxHP;
        if (enemyNameText != null) enemyNameText.text = eName;
        if (enemyIconImage != null && icon != null) enemyIconImage.sprite = icon;
        if (cheatInfoText != null) cheatInfoText.text = cheatDesc;
        if (rewardText != null)
        {
            rewardText.text = $"Reward: {new string('$', baseReward)}";
        }

        UpdateHealth(currentHealth);
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"LP: {currentHealth}";
        }
    }
}