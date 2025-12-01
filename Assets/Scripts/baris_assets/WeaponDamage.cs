using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damageAmount = 20;
    public string targetTag; // Player kılıcı için "Enemy", Düşman için "Player" yaz

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hedef tag ile eşleşiyor mu ve çarptığımız şeyin Canı var mı?
        if (collision.CompareTag(targetTag))
        {
            CharacterStats targetStats = collision.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damageAmount);
            }
        }
    }
}