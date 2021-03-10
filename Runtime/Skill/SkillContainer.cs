using UnityEngine;
using System;

namespace Elysium.Skills
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Skills/Skill Container")]
    public class SkillContainer : ScriptableObject
    {
        public Sprite SkillIcon = null;
        public string SkillName = "Skill";
        public string SkillDescription = "";
        public string CastTriggerStart = "cast2";
        public string CastTriggerEnd = "cast2End";
        public string ExecuteTriggerStart = "attack1";

        [Flags] public enum AISkillBehaviour { DAMAGE = 1, HEAL = 2, UTIL = 4 }
        public AISkillBehaviour SkillBehaviour = default;

        [Space]
        [SerializeField] private GenericSkill[] Skills = new GenericSkill[10];

        public int MaxLevel => Skills.Length;

        public GenericSkill GetSkillAtLevel(int _level)
        {
            if (_level == 0) { throw new Exception("this function shouldnt receive a 0 value for level. If you want to get the skill based on the array index, use GetSkillAtPosition instead!"); }
            return GetSkillAtPosition(_level - 1);
        }

        public GenericSkill GetSkillAtPosition(int _index)
        {
            if (Skills.Length <= _index) { throw new Exception($"index {_index} is out of the bounds of the array"); }
            var clone = Instantiate(Skills[_index]);
            clone.CastTriggerStart = CastTriggerStart;
            clone.CastTriggerEnd = CastTriggerEnd;
            clone.ExecuteTriggerStart = ExecuteTriggerStart;
            return clone;
        }

        public static T CreateGenericSkill<T>(string _castTriggerStart, string _castTriggerEnd, string _executeTriggerStart) where T : GenericSkill
        {
            var clone = CreateInstance<T>();
            clone.CastTriggerStart = _castTriggerStart;
            clone.CastTriggerEnd = _castTriggerEnd;
            clone.ExecuteTriggerStart = _executeTriggerStart;
            return clone;
        }

        private void OnValidate()
        {
            if (SkillBehaviour == 0) { Debug.LogError($"skill {SkillName} doesnt have an AI Skill Behaviour set"); }
        }
    }
}