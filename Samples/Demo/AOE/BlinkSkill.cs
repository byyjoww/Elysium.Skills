using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Teleportation/Blink")]
public class BlinkSkill : AOESkill
{
    [Header("EFFECTS")]
    public GameObject WarpEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        SkillUtils.CreateEffect(WarpEffect, _cache.Caster.DamageDealerObject.transform, 2f);
        _cache.Caster.DamageDealerObject.transform.position = _cache.Location.Value;
        yield return null;
    }
}
