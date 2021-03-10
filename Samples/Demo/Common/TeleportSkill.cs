using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Teleportation/Teleport")]
public class TeleportSkill : SelfCastSkill
{
    [Header("DESTINATION")]
    [SerializeField] public Vector3? destination = null;

    public override IEnumerator Execute(SkillCache _cache)
    {
        //SkillUtils.CreateEffect(WarpEffect, _cache.Caster.DamageDealerObject.transform, 2f);
        if (destination.HasValue)
            Teleport(_cache, destination.Value);
        AdditionalEffect?.Invoke();
        yield return null;
    }

    protected void Teleport(SkillCache _cache, Vector3 _destination)
    {
        Transform unit = _cache.Caster.DamageDealerObject.transform;
        unit.position = _destination;
    }
}
