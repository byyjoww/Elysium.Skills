using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Lightning Blast")]
public class LightningBlastSkill : TargetSkill
{
    [Header("EFFECTS")]
    public GameObject blastEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        _cache.Target.TakeDamage(_cache.Caster, _cache.Caster.Damage.Value);
        SkillUtils.CreateEffect(blastEffect, _cache.Target.DamageableObject.transform);
        yield return null;
    }
}
