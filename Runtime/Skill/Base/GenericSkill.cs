using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Elysium.Combat;

namespace Elysium.Skills
{
    public abstract class GenericSkill : ScriptableObject
    {
        public enum CastType { Target, AOE, Self, Directional }

        [Range(0, 100)] public int Cost = 0;
        [Range(0, 10)] public float CastTime = 0f;
        [Range(0, 5)] public float Delay = 5f;
        [Range(0, 20)] public float Range = 5f;
        public bool CanInterrupt = true;

        public virtual CastType CastingType { get; }
        public string CastTriggerStart { get; set; }
        public string CastTriggerEnd { get; set; }
        public string ExecuteTriggerStart { get; set; }

        public Action AdditionalEffect;

        // CHECK CONDITIONS
        public abstract SkillCache CanCast(IDamageDealer _caster, IRaycastHitResults _results, List<DamageTeam> _opposingTeams);
        public abstract bool InTargetRange(Vector3 _self, Vector3 _target);
        public abstract bool RevalidateTargets(SkillCache _cache);

        // EXECUTE SKILL
        public abstract IEnumerator Execute(SkillCache _cache);
    }
}