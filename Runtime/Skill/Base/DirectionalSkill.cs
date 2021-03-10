using Elysium.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elysium.Skills
{
    public abstract class DirectionalSkill : GenericSkill
    {
        public override CastType CastingType => CastType.Directional;

        public float Width = 1f;

        public override SkillCache CanCast(IDamageDealer _caster, IRaycastHitResults _results, List<DamageTeam> _opposingTeams)
        {
            if (_results.Ground.HasValue)
            {
                return new SkillCache(this, _caster, _results.Ground.Value);
            }

            return null;
        }

        public override bool InTargetRange(Vector3 _self, Vector3 _target)
        {
            if (Vector3.Distance(_self, _target) > Range)
            {
                return false;
            }

            return true;
        }

        public override bool RevalidateTargets(SkillCache _cache)
        {
            return true;
        }
    }
}