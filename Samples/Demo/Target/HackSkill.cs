using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Hack")]
public class HackSkill : TargetSkill
{
    [Header("EFFECTS")]
    public GameObject HackEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        _cache.Target.TakeDamage(_cache.Caster, _cache.Caster.Damage.Value);
        SkillUtils.CreateEffect(HackEffect, _cache.Caster.DamageDealerObject.transform, 2f);
        yield return null;
    }
}
