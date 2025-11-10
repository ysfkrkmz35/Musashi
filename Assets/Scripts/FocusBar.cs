using UnityEngine;
using UnityEngine.UI;

public class FocusBar : MonoBehaviour
{
    public Slider playerFocus;
    public Slider enemyFocus;

    public void UpdatePlayerFocus(float current, float max)
    {
        if (playerFocus) playerFocus.value = current / max;
    }

    public void UpdateEnemyFocus(float current, float max)
    {
        if (enemyFocus) enemyFocus.value = current / max;
    }
}
