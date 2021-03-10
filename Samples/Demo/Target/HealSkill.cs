using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Heal")]
public class HealSkill : TargetSkill
{
    [Header("EFFECTS")]
    public GameObject HealEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        _cache.Target.Heal(_cache.Caster, _cache.Caster.Damage.Value);
        SkillUtils.CreateEffect(HealEffect, _cache.Target.DamageableObject.transform, 2f);
        yield return null;
    }
}
