using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Fire Missiles")]
public class FireMissilesSkill : TargetSkill
{
    [Header("DETAILS")]
    public int numOfMissiles = 3;
    public float intervalBetweenMissiles = 0.5f;

    [Header("EFFECTS")]
    public GenericProjectile FireballPrefab;
    public GameObject explosionVFX;

    public override IEnumerator Execute(SkillCache _cache)
    {
        Vector3 firepoint = _cache.Caster.DamageDealerObject.transform.position + _cache.Caster.DamageDealerObject.transform.forward + _cache.Caster.DamageDealerObject.transform.up;
        var fallbackPos = _cache.Target.DamageableObject.transform.position + _cache.Target.DamageableObject.transform.up;

        for (int i = 0; i < numOfMissiles; i++)
        {
            if (!_cache.Target.IsDead) { fallbackPos = _cache.Target.DamageableObject.transform.position + _cache.Target.DamageableObject.transform.up; }

            GenericProjectile fb = SkillUtils.CreateEffect(FireballPrefab, firepoint, 3f);
            fb.Setup(_cache.Target, fallbackPos, _cache.Caster.DealsDamageToTeams, (_target) => OnHit(_cache, _target, fb));
            yield return new WaitForSeconds(intervalBetweenMissiles);
        }        

        yield return null;
    }

    public virtual void OnHit(SkillCache _cache, IDamageable _damageable, GenericProjectile _projectile)
    {
        if (_damageable != null) { _damageable.TakeDamage(_cache.Caster, _cache.Caster.Damage.Value); }        
        SkillUtils.CreateEffect(explosionVFX, _projectile.transform.position, 2f);
    }
}
