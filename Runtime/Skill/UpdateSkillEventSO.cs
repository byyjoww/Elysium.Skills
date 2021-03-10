using Elysium.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Elysium.Skills
{
#endif

    [CreateAssetMenu(fileName = "SkillEventSO_", menuName = "Scriptable Objects/Event/Skill Event")]
    public class UpdateSkillEventSO : GenericEventSO<SkillContainerLevelWrapper, int>
    {

#if UNITY_EDITOR

        [Header("Editor Only")]
        [SerializeField] private SkillContainerLevelWrapper editorData;
        [SerializeField] private int editorData2;

        [CustomEditor(typeof(UpdateSkillEventSO), true)]
        public class UpdateSkillEventSOEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                if (GUILayout.Button("Raise"))
                {
                    var e = target as UpdateSkillEventSO;
                    e.Raise(e.editorData, e.editorData2);
                }
            }
        }
#endif
    }
}