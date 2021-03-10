using Elysium.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Heal Nova")]
public class HealNovaSkill : AOESkill
{
    public float RepeatDelay = 0f;
    public int RepeatTimes = 1;

    [Header("EFFECTS")]
    public GameObject healAreaEffect;
    public GameObject individualHealEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        // MAIN EFFECT
        // CreateDebugShape(_target, _skill.AreaOfEffect, _duration: 2f);

        var go = SkillUtils.CreateEffect(healAreaEffect, _cache.Location.Value, 2f, 0.05f);

        int repeatCounter = 0;
        while (repeatCounter < RepeatTimes)
        {
            var validTargets = SkillUtils.GetTargetsInAOE(AreaOfEffect, EnumTools.GetAllEnumsExcept<DamageTeam>(_cache.Caster.DealsDamageToTeams.ToArray()), _cache.Location.Value);

            // Do damage to all valid targets
            for (int i = 0; i < validTargets.Count; i++)
            {
                validTargets[i].Heal(_cache.Caster, _cache.Caster.Damage.Value);

                // INDIVIDUAL EFFECT
                SkillUtils.CreateEffect(individualHealEffect, validTargets[i].DamageableObject.transform, 1f);
            }

            yield return new WaitForSeconds(RepeatDelay);
            repeatCounter++;
        }

        yield return null;
    }
}
