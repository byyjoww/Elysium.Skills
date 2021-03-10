using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Invulnerable")]
public class InvulnerableSkill : SelfCastSkill
{
    public override IEnumerator Execute(SkillCache _cache)
    {
        yield return null;
    }
}
