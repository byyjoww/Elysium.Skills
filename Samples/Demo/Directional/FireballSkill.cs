using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Directional/Fireball")]
public class FireballSkill : DirectionalSkill
{
    [Header("EFFECTS")]
    public GenericProjectile FireballPrefab;
    public GameObject explosionVFX;

    public override IEnumerator Execute(SkillCache _cache)
    {
        Vector3 firepoint = _cache.Caster.DamageDealerObject.transform.position + _cache.Caster.DamageDealerObject.transform.forward + _cache.Caster.DamageDealerObject.transform.up;

        GenericProjectile fb = SkillUtils.CreateEffect(FireballPrefab, firepoint, 3f);
        fb.Setup(_cache.Location.Value, _cache.Caster.DealsDamageToTeams, (_target) => OnHit(_cache, _target));

        yield return null;
    }

    public virtual void OnHit(SkillCache _cache, IDamageable _damageable)
    {
        Debug.Log("on hit");
        if (_damageable != null) { _damageable.TakeDamage(_cache.Caster, _cache.Caster.Damage.Value); }        
        SkillUtils.CreateEffect(explosionVFX, _damageable.DamageableObject.transform.position, 2f);
    }
}
