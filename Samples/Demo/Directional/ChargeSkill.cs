using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Directional/Charge")]
public class ChargeSkill : DirectionalSkill
{
    public float Speed = 10f;

    [Header("EFFECTS")]
    public GameObject OnHitEffect;

    public override IEnumerator Execute(SkillCache _cache)
    {
        // GET COMPONENTS
        Transform myTransform = _cache.Caster.DamageDealerObject.transform;
        if (!myTransform.TryGetComponent(out ICollisionDetector _collisionDetector))
        {
            Debug.LogError("no collision detection component");
            yield break;
        }

        Vector3 origin = myTransform.position;
        bool hasCollided = false;

        // TURN OFF NAVMESHAGENT
        if (myTransform.TryGetComponent(out NavMeshAgent agent))
        {
            agent.enabled = false;
        }

        // SETUP COLLISION EVENTS
        void OnCollidedWithEnemy(IDamageable _enemy)
        {
            hasCollided = true;
            _collisionDetector.OnEnemyCollision -= OnCollidedWithEnemy;
            OnHit(_cache, _enemy);
        }

        void OnCollidedWithAnything(Collider _collider)
        {
            hasCollided = true;
            _collisionDetector.OnAnyCollision -= OnCollidedWithAnything;
            OnHit(_cache, null);
        }

        _collisionDetector.OnEnemyCollision += OnCollidedWithEnemy;
        _collisionDetector.OnAnyCollision += OnCollidedWithAnything;

        // START CHARGE
        myTransform.LookAt(_cache.Location.Value);        
        while (!hasCollided && Vector3.Distance(myTransform.position, origin) < Range)
        {
            Move(myTransform, _cache.Location.Value, origin, Speed);
            yield return new WaitForFixedUpdate();
        }

        // TURN OFF COLLISION EVENTS
        _collisionDetector.OnEnemyCollision -= OnCollidedWithEnemy;
        _collisionDetector.OnAnyCollision -= OnCollidedWithAnything;

        // TURN ON NAVMESHAGENT
        if (agent != null) { agent.enabled = true; }

        yield return null;
    }

    private void Move(Transform _myTransform, Vector3 _targetPos, Vector3 _origin, float _speed)
    {
        Vector3 direction = _targetPos - _origin;
        Debug.DrawRay(_myTransform.position, direction, Color.red);

        Vector3 desiredTarget = direction + _myTransform.position;
        desiredTarget.y = _myTransform.position.y;

        _myTransform.Translate(_myTransform.InverseTransformDirection(_myTransform.forward) * Time.deltaTime * _speed);
    }

    public virtual void OnHit(SkillCache _cache, IDamageable _damageable)
    {
        if (_damageable != null) 
        {
            _damageable.TakeDamage(_cache.Caster, _cache.Caster.Damage.Value);
            SkillUtils.CreateEffect(OnHitEffect, _damageable.DamageableObject.transform.position, 2f);
        }        
    }

    public override SkillCache CanCast(IDamageDealer _caster, IRaycastHitResults _results, List<DamageTeam> _opposingTeams)
    {
        if (_results.Ground.HasValue)
        {
            return new SkillCache(this, _caster, _results.Ground.Value, false);
        }

        return null;
    }
}
