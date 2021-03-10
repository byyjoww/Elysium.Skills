using Elysium.Combat;
using Elysium.Utils.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elysium.Skills
{
    public static class SkillUtils
    {
        public static GameObject CreateEffect(GameObject _effect, Transform _target, float _duration = 5f)
        {
            Vector3 offset = Camera.main.transform.position - _target.position;
            offset = offset.normalized * 0.5f;
            offset = new Vector3(offset.x, offset.y + 0.5f, offset.z);

            var effect = Object.Instantiate(_effect, _target.position + offset, _effect.transform.rotation, _target);
            effect.AddComponent<TimedDestroy>().Delay = _duration;
            return effect;
        }

        public static GameObject CreateEffect(GameObject _effect, Vector3 _position, float _duration = 5f, float _offset = 0.5f)
        {
            Vector3 offset = Camera.main.transform.position - _position;
            offset = offset.normalized * 0.5f;
            offset = new Vector3(0, _offset, 0);

            var effect = Object.Instantiate(_effect, _position + offset, _effect.transform.rotation);
            effect.AddComponent<TimedDestroy>().Delay = _duration;
            return effect;
        }

        public static GenericProjectile CreateEffect(GenericProjectile _effect, Vector3 _firepoint, float _duration = 5f)
        {
            var effect = Object.Instantiate(_effect, _firepoint, _effect.transform.rotation);
            effect.gameObject.AddComponent<TimedDestroy>().Delay = _duration;
            return effect;
        }

        public static void CreateDebugShape(Vector3 _origin, float _size, PrimitiveType _shape = PrimitiveType.Sphere, float _duration = 0.5f)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = _origin;
            sphere.transform.localScale = new Vector3(_size * 2, _size * 2, _size * 2);
            sphere.GetComponent<Collider>().enabled = false;
            sphere.GetComponent<MeshRenderer>().material.shader = Shader.Find("SuperSystems/Wireframe-Transparent");
            sphere.AddComponent<TimedDestroy>().Delay = _duration;
        }

        public static List<IDamageable> GetTargetsInAOE(float _size, DamageTeam[] _teams, Vector3 _origin)
        {
            if (_teams is null) { return null; }

            // Get colliders in a spherical area
            Collider[] colliders = Physics.OverlapSphere(_origin, _size);

            // Loop over colliders to check for damageable components & damage teams
            List<IDamageable> validTargets = new List<IDamageable>();
            for (int i = 0; i < colliders.Length; i++)
            {
                var damageableComponent = colliders[i].GetComponentInChildren<IDamageable>();
                if (damageableComponent == null) { continue; }
                if (_teams.Contains(damageableComponent.Team))
                {
                    // Can apply effect to this component
                    validTargets.Add(damageableComponent);
                }
            }

            return validTargets;
        }
    }
}