using UnityEngine;

public static class DamageSystem
{
    public static float ProcessDamage(DamageData dmg, CoreStats attacker, CoreStats target)
    {
        float finalDamage = dmg.baseDamage;

        finalDamage *= attacker.damage;

        // 2. Crit
        if (dmg.canCrit && Random.value < dmg.critChance)
        {
            finalDamage *= dmg.critMultiplier;
        }

        // 3. Armor (placeholder)
        float armor = 0f;
        finalDamage = finalDamage * (100f / (100f + armor));

        // 4. Apply damage
        target.TakeDamage(finalDamage);

        // 5. Lifesteal
        float heal = finalDamage * attacker.lifeSteal;
        attacker.Heal(heal);

        return finalDamage;
    }
}