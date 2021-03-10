using Elysium.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elysium.Skills
{
    public abstract class TargetSkill : GenericSkill
    {
        public override CastType CastingType => CastType.Target;

        public bool AllyTarget = false;

        public override SkillCache CanCast(IDamageDealer _caster, IRaycastHitResults _results, List<DamageTeam> _opposingTeams)
        {
            // TODO: Check if target is still alive

            if (AllyTarget)
            {
                List<IDamageable> possibleTargets = new List<IDamageable>();
                if (_results.Player != null) { possibleTargets.Add(_results.Player); }

                foreach (var unit in _results.Units)
                {
                    if (_opposingTeams.Contains(unit.Team)) { continue; }
                    possibleTargets.Add(unit);
                }

                if (possibleTargets.Count > 0)
                {
                    return new SkillCache(this, _caster, possibleTargets[0]);
                }
            }
            else if (_results.Units.Length > 0)
            {
                foreach (var unit in _results.Units)
                {
                    if (!_opposingTeams.Contains(unit.Team)) { continue; }
                    return new SkillCache(this, _caster, unit);
                }
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
            if (_cache.Target.IsDead) { return false; }
            if (!_cache.Target.DamageableObject) { return false; }

            return true;
        }
    }
}