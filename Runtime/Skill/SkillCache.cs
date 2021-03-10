using Elysium.Combat;
using UnityEngine;

namespace Elysium.Skills
{
    public class SkillCache
    {
        // A SKILL CACHE SHOULD NEVER HAVE BOTH A TARGET AND A LOCATION
        // MAKE SURE THAT THE SKILL CACHE ADHERES TO THE REQUIREMENTS OF THE SKILL TYPE

        public SkillCache(GenericSkill _skill, IDamageDealer _caster, bool _look = true)
        {
            if (_skill == null) { throw new System.Exception("trying to create a skill cache with a null skill!"); }

            skill = _skill;
            caster = _caster;
            lookAtTarget = _look;
        }

        public SkillCache(GenericSkill _skill, IDamageDealer _caster, Vector3 _location, bool _look = true)
        {
            if (_skill == null) { throw new System.Exception("trying to create a skill cache with a null skill!"); }

            skill = _skill;
            caster = _caster;
            location = _location;
            lookAtTarget = _look;
        }

        public SkillCache(GenericSkill _skill, IDamageDealer _caster, IDamageable _target, bool _look = true)
        {
            if (_skill == null) { throw new System.Exception("trying to create a skill cache with a null skill!"); }

            skill = _skill;
            caster = _caster;
            target = _target;
            lookAtTarget = _look;
        }

        private GenericSkill skill;
        private IDamageDealer caster;
        private IDamageable target;
        private Vector3? location;
        private bool lookAtTarget;

        public GenericSkill Skill => skill;
        public IDamageDealer Caster => caster;
        public IDamageable Target => target;
        public Vector3? Location => location;
        public bool LookAtTarget => lookAtTarget;

        public Vector3 SkillTargetPosition
        {
            get
            {
                if (Target != null)
                {
                    return Target.DamageableObject.transform.position;
                }
                else if (Location.HasValue)
                {
                    return Location.Value;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }
    }
}