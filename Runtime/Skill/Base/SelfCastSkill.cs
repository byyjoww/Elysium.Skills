using Elysium.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elysium.Skills
{
    public abstract class SelfCastSkill : GenericSkill
    {
        public override CastType CastingType => CastType.Self;

        public override SkillCache CanCast(IDamageDealer _caster, IRaycastHitResults _results, List<DamageTeam> _opposingTeams)
        {
            return new SkillCache(this, _caster);
        }

        public override bool InTargetRange(Vector3 _self, Vector3 _target)
        {
            return true;
        }

        public override bool RevalidateTargets(SkillCache _cache)
        {
            return true;
        }
    }
}