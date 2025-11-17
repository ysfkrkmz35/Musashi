using UnityEngine;

namespace Musashi.Core.Combat
{
    /// <summary>
    /// 4-directional attack/defense system - For Honor inspired
    /// Yukarı, Aşağı, Sol, Sağ yönlü saldırı ve savunma
    /// </summary>
    public enum AttackDirection
    {
        None = 0,
        Up = 1,      // Yukarı - Kafa seviyesi
        Down = 2,    // Aşağı - Bacak seviyesi
        Left = 3,    // Sol - Oyuncunun solu
        Right = 4    // Sağ - Oyuncunun sağı
    }

    /// <summary>
    /// Directional combat event data
    /// </summary>
    public class DirectionalAttackData
    {
        public AttackDirection Direction { get; set; }
        public bool IsHeavyAttack { get; set; }
        public float Damage { get; set; }
        public float FocusCost { get; set; }
        public GameObject Attacker { get; set; }

        public DirectionalAttackData(AttackDirection dir, bool heavy, float damage, float focus, GameObject attacker)
        {
            Direction = dir;
            IsHeavyAttack = heavy;
            Damage = damage;
            FocusCost = focus;
            Attacker = attacker;
        }
    }

    /// <summary>
    /// Combat result after directional check
    /// </summary>
    public enum CombatResult
    {
        Blocked,           // Savunma başarılı - aynı yön
        ParrySuccess,      // Parry başarılı - karşı saldırı penceresi
        ParryFailed,       // Parry başarısız - ekstra hasar
        Dodged,            // Dodge ile kaçındı
        Hit,               // Vurdu
        Missed             // Kaçırdı
    }

    /// <summary>
    /// Helper methods for directional combat
    /// </summary>
    public static class DirectionalHelper
    {
        public static AttackDirection GetOppositeDirection(AttackDirection dir)
        {
            switch (dir)
            {
                case AttackDirection.Up: return AttackDirection.Down;
                case AttackDirection.Down: return AttackDirection.Up;
                case AttackDirection.Left: return AttackDirection.Right;
                case AttackDirection.Right: return AttackDirection.Left;
                default: return AttackDirection.Up;
            }
        }

        public static AttackDirection GetRandomDirection()
        {
            int random = Random.Range(1, 5);
            return (AttackDirection)random;
        }

        public static AttackDirection[] GetAllDirections()
        {
            return new AttackDirection[] {
                AttackDirection.Up,
                AttackDirection.Down,
                AttackDirection.Left,
                AttackDirection.Right
            };
        }
    }
}
