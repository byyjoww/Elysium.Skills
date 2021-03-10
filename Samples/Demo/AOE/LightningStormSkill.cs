using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Lightning Storm")]
public class LightningStormSkill : AOESkill
{
    public float RepeatDelay = 0.2f;
    public int RepeatTimes = 5;

    [Header("EFFECTS")]
    public GameObject centerBallEffect;
    public GameObject hitEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        // MAIN EFFECT
        // SkillUtils.CreateDebugShape(_target, _skill.AreaOfEffect, _duration: repeatDelay * repeatTimes);

        var go = SkillUtils.CreateEffect(centerBallEffect, _cache.Location.Value, RepeatDelay * RepeatTimes, 0.5f);
        // go.GetComponentInChildren<AreaOfEffectIndicator>().size = AreaOfEffect;

        int repeatCounter = 0;
        while (repeatCounter < RepeatTimes)
        {
            var validTargets = SkillUtils.GetTargetsInAOE(AreaOfEffect, _cache.Caster.DealsDamageToTeams.ToArray(), _cache.Location.Value);

            // Do damage to all valid targets
            for (int i = 0; i < validTargets.Count; i++)
            {
                validTargets[i].TakeDamage(_cache.Caster, _cache.Caster.Damage.Value);

                // INDIVIDUAL EFFECT
                SkillUtils.CreateEffect(hitEffect, validTargets[i].DamageableObject.transform, 1f);
            }

            yield return new WaitForSeconds(RepeatDelay);
            repeatCounter++;
        }

        yield return null;
    }
}
