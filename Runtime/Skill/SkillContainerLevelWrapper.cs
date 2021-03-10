namespace Elysium.Skills
{
    [System.Serializable]
    public class SkillContainerLevelWrapper
    {
        public SkillContainer Skill;
        public int Level = 1;
        public int Index => Level - 1;
        public string Name
        {
            get
            {
                if (Skill == null) { return ""; }
                else return Skill.SkillName;
            }
        }

        public SkillContainerLevelWrapper(SkillContainer _skill, int _level)
        {
            Skill = _skill;
            Level = _level;
        }
    }
}