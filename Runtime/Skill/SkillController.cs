using Elysium.Combat;
using Elysium.Utils;
using Elysium.Utils.Attributes;
using Elysium.Utils.Timers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Elysium.Skills
{
    public class SkillController : MonoBehaviour, IDamageDealer
    {
        public const float INITIAL_SKILL_DELAY = 1f;
        public const float PENDING_SKILL_TIME_LIMIT = 2f;
        public const int RUNTIME_SKILL_LENGTH = 8;

        // EVENTS
        [SerializeField] private UpdateSkillEventSO updateSkillChannel = default;

        // DEBUG
        [SerializeField] private GenericSkill DebugSelectedSkill = null;
        [SerializeField] private GenericSkill DebugPendingSkill = null;
        [SerializeField] private GenericSkill DebugRunningSkill = null;

        // SELECTED SKILL
        public GenericSkill SelectedSkill { get; private set; }
        public SkillCache PendingSkill { get; private set; }

        [HideInInspector] public TimerInstance pendingTimer;

        // CASTING & EXECUTION
        public enum ExecutionPhase { STANDBY = 0, CASTING = 1, EXECUTING = 2 }
        [ReadOnly] public ExecutionPhase Phase = default;
        public SkillCache RunningSkill = null;

        [HideInInspector] public TimerInstance castTimer;

        // EQUIPPED SKILLS
        public IReadOnlyList<SkillContainerLevelWrapper> RuntimeSkills => runtimeSkills;
        [SerializeField, ReadOnly] private SkillContainerLevelWrapper[] runtimeSkills;

        // DELAY
        private bool isDelay => !delayTimer.IsEnded;
        [HideInInspector] public TimerInstance delayTimer;

        // SETTINGS
        public bool Disabled { get; private set; }
        public RefValue<int> Damage { get; set; }
        public List<DamageTeam> DealsDamageToTeams { get; set; }
        public IResource SkillResource { get; set; }
        public IAnimationEvents AnimationEvents { get; set; }

        public GameObject DamageDealerObject
        {
            get
            {
                if (!gameObject || !transform.parent) { return null; }
                return transform.parent.gameObject;
            }
        }

        // EVENTS
        public event UnityAction<SkillContainerLevelWrapper, int> OnEquippedSkillChanged;

        public event UnityAction<GenericSkill> OnSkillSelected;
        public event UnityAction OnSkillDeselected;

        public event UnityAction<Transform, float> OnOutOfTargetRange;
        public event UnityAction<Vector3, float> OnOutOfLocationRange;

        public event UnityAction<SkillCache> OnSkillCastStarted;
        public event UnityAction<SkillCache> OnSkillCastCancelled;
        public event UnityAction<SkillCache> OnSkillCastEnded;

        public event UnityAction<SkillCache> OnSkillExecutionStarted;
        public event UnityAction<SkillCache> OnSkillExecutionLanded;
        public event UnityAction<SkillCache> OnSkillExecutionEnded;

        public event UnityAction<float> OnSkillDelayStarted;
        public event UnityAction OnSkillDelayEnded;

        #region SETUP
        public void Awake()
        {
            if (AnimationEvents == null) { AnimationEvents = DamageDealerObject.GetComponentInChildren<IAnimationEvents>(); }
            if (updateSkillChannel != null) { updateSkillChannel.OnRaise += UpdateRuntimeSkill; }
            CreateTimers();
        }

        private void OnDestroy()
        {
            if (updateSkillChannel != null) { updateSkillChannel.OnRaise -= UpdateRuntimeSkill; }
        }

        private void CreateTimers()
        {
            // CREATE PENDING TIMER
            pendingTimer = Timer.CreateTimer(0, () => !this, false);
            pendingTimer.OnTimerEnd += ClearPendingSkill;

            // CREATE CAST TIMER
            castTimer = Timer.CreateTimer(0, () => !this, false);

            // CREATE DELAY TIMER        
            delayTimer = Timer.CreateTimer(INITIAL_SKILL_DELAY, () => !this, true);
            void EndDelay() => OnSkillDelayEnded?.Invoke();
            delayTimer.OnTimerEnd += EndDelay;
        }

        public void InitializeRuntimeSkills(SkillContainerLevelWrapper[] _skills)
        {
            runtimeSkills = new SkillContainerLevelWrapper[RUNTIME_SKILL_LENGTH];

            for (int i = 0; i < _skills.Length; i++)
            {
                UpdateRuntimeSkill(_skills[i], i);
            }
        }

        public void UpdateRuntimeSkill(SkillContainerLevelWrapper _skill, int _index)
        {
            if (_index >= runtimeSkills.Length) { throw new System.Exception("attempting to set a skill with higher index than runtimeskill length | " + _index); }
            runtimeSkills[_index] = CreateRuntimeSkill(_skill);
            OnEquippedSkillChanged?.Invoke(runtimeSkills[_index], _index);
        }

        public SkillContainerLevelWrapper CreateRuntimeSkill(SkillContainerLevelWrapper _skill)
        {
            if (_skill == null || _skill.Skill == null) { return new SkillContainerLevelWrapper(null, 0); }
            return new SkillContainerLevelWrapper(_skill.Skill, _skill.Level);
        }
        #endregion

        #region SELECT_SKILL
        public void TrySelectSkill(int _index)
        {
            if (TryGetSkillFromRuntimeSkills(_index, out SkillContainerLevelWrapper _skillContainer))
            {
                GenericSkill skill = _skillContainer.Skill.GetSkillAtLevel(_skillContainer.Level);
                TrySelectSkill(skill);
                return;
            }

            DeselectSkill();
            return;
        }

        public void TrySelectSkill(GenericSkill _skill)
        {
            if (_skill == SelectedSkill) { return; }

            DeselectSkill();
            SelectSkill(_skill);

            if (_skill.CastingType == GenericSkill.CastType.Self)
            {
                TrySetPendingSkill(_skill, null);
            }
        }

        private bool TryGetSkillFromRuntimeSkills(int _index, out SkillContainerLevelWrapper _skillContainer)
        {
            if (_index == -1 || _index >= runtimeSkills.Length)
            {
                _skillContainer = null;
                return false;
            }

            _skillContainer = runtimeSkills[_index];
            if (_skillContainer == null)
            {
                return false;
            }

            return true;
        }

        private void SelectSkill(GenericSkill _skill)
        {
            SelectedSkill = _skill;
            DebugSelectedSkill = _skill;
            OnSkillSelected?.Invoke(_skill);
        }

        private void DeselectSkill()
        {
            if (SelectedSkill == null) { return; }

            SelectedSkill = null;
            DebugSelectedSkill = null;
            OnSkillDeselected?.Invoke();
        }
        #endregion

        #region CACHE_PENDING_SKILL
        public bool TrySetPendingSkill(GenericSkill _skill, IRaycastHitResults _results)
        {
            var cache = _skill.CanCast(this, _results, DealsDamageToTeams);
            if (cache != null)
            {
                SetPendingSkill(cache);
                DeselectSkill();
                return true;
            }

            DeselectSkill();
            return false;
        }

        public void ForceSetPendingSkill(SelfCastSkill _skill)
        {
            ForceSetPendingSkill(new SkillCache(_skill, this));
        }

        public void ForceSetPendingSkill(AOESkill _skill, Vector3 _location)
        {
            ForceSetPendingSkill(new SkillCache(_skill, this, _location));
        }

        public void ForceSetPendingSkill(TargetSkill _skill, IDamageable _target)
        {
            ForceSetPendingSkill(new SkillCache(_skill, this, _target));
        }

        public void ForceSetPendingSkill(SkillCache _cache)
        {
            SetPendingSkill(_cache);
            DeselectSkill();
        }

        public void SetPendingSkill(SkillCache _cache)
        {
            PendingSkill = _cache;
            DebugPendingSkill = _cache.Skill;
            pendingTimer.SetTime(PENDING_SKILL_TIME_LIMIT);
        }

        public void ClearPendingSkill()
        {
            if (PendingSkill == null) { return; }

            PendingSkill = null;
            DebugPendingSkill = null;
            pendingTimer.EndTimer();
        }
        #endregion

        #region CASTING
        public bool TryCastCachedSkill()
        {
            if (TryCastSkill(PendingSkill))
            {
                return true;
            }

            return false;
        }

        private bool TryCastSkill(SkillCache _cache)
        {
            if (_cache == null) { return false; }

            // REMOVE CLEARPENDINGSKILL() IF YOU WANT THE PLAYER TO BE ABLE TO CACHE SKILLS WHILE CASTING OR DURING SKILL DELAY AND WHILE CALLING TRYCASTSKILL DURING UPDATE
            if (isDelay) { Debug.Log("skill controller is still on delay"); ClearPendingSkill(); return false; }
            // REMOVE CLEARPENDINGSKILL() IF YOU WANT THE PLAYER TO BE ABLE TO CACHE SKILLS WHILE CASTING OR DURING SKILL COOLDOWN AND WHILE CALLING TRYCASTSKILL DURING UPDATE
            if (Phase != ExecutionPhase.STANDBY) { Debug.Log("skill controller is not on standby"); ClearPendingSkill(); return false; }
            // REMOVE CLEARPENDINGSKILL() IF YOU WANT THE PLAYER TO BE ABLE TO CACHE SKILLS WHILE THE CONTROLLER IS DISABLED AND WHILE CALLING TRYCASTSKILL DURING UPDATE
            if (Disabled) { Debug.Log("skill controller is disabled"); ClearPendingSkill(); return false; }

            if (!_cache.Skill.RevalidateTargets(_cache)) { Debug.Log("targets for cached skill aren't valid anymore"); ClearPendingSkill(); return false; }

            if (!_cache.Skill.InTargetRange(DamageDealerObject.transform.position, _cache.SkillTargetPosition))
            {
                if (_cache.Target != null)
                {
                    OnOutOfTargetRange?.Invoke(_cache.Target.DamageableObject.transform, _cache.Skill.Range);
                }

                if (_cache.Location.HasValue)
                {
                    OnOutOfLocationRange?.Invoke(_cache.Location.Value, _cache.Skill.Range);
                }

                return false;
            }

            SetRunningSkill(PendingSkill);
            ClearPendingSkill();

            // DONT CAST 0 CAST TIME ABILITIES        
            if (_cache.Skill.CastTime <= 0) { StartSkill(); }
            else { StartCasting(); }
            return true;
        }

        private void StartCasting() // ---> TRANSITION TO CASTING
        {
            Phase = ExecutionPhase.CASTING;

            castTimer.OnTimerEnd += EndCasting;
            castTimer.SetTime(RunningSkill.Skill.CastTime);

            OnSkillCastStarted?.Invoke(RunningSkill);
        }

        private void EndCasting()
        {
            OnSkillCastEnded?.Invoke(RunningSkill);
            castTimer.OnTimerEnd -= EndCasting;
            StartSkill();
        }

        public void CancelCasting() // ---> TRANSITION TO STANDBY
        {
            if (Phase != ExecutionPhase.CASTING) { return; }
            if (!RunningSkill.Skill.CanInterrupt) { return; }

            // CANCEL CASTING        
            castTimer.ClearOnEnd();
            castTimer.SetTime(0);
            OnSkillCastCancelled?.Invoke(RunningSkill);
            ClearRunningSkill();
        }
        #endregion

        #region EXECUTING
        private void StartSkill() // ---> TRANSITION TO EXECUTING
        {
            // CHECK FOR ENOUGH RESOURCES
            if (RunningSkill.Skill.Cost > 0 && !SkillResource.TryLoseResource(RunningSkill.Skill.Cost))
            {
                Debug.Log("not enough mana");
                ClearRunningSkill();
                return;
            }

            // START EXECUTING THE SKILL
            Phase = ExecutionPhase.EXECUTING;
            AnimationEvents.OnAttackHit += ExecuteSkill;
            AnimationEvents.OnAttackEnd += EndSkill;

            StartDelay(RunningSkill.Skill);
            OnSkillExecutionStarted?.Invoke(RunningSkill);

            if (RunningSkill.Skill.ExecuteTriggerStart == "")
            {
                // Debug.LogError("no valid attack triggers");
                ExecuteSkill();
                EndSkill();
                return;
            }
        }

        public void ExecuteSkill()
        {
            // WHEN THE ANIMATION TRIGGERS THE HIT
            StartCoroutine(RunningSkill.Skill.Execute(RunningSkill));
            AnimationEvents.OnAttackHit -= ExecuteSkill;
            OnSkillExecutionLanded?.Invoke(RunningSkill);
        }

        public void EndSkill()  // ---> TRANSITION TO STANDBY
        {
            // WHEN THE ANIMATION FINISHES
            OnSkillExecutionEnded?.Invoke(RunningSkill);


            AnimationEvents.OnAttackEnd -= EndSkill;
            ClearRunningSkill();
        }

        private void SetRunningSkill(SkillCache _cache)
        {
            DebugRunningSkill = _cache.Skill;
            RunningSkill = _cache;
        }

        private void ClearRunningSkill()
        {
            Phase = ExecutionPhase.STANDBY;
            DebugRunningSkill = null;
            RunningSkill = null;
        }
        #endregion

        #region DELAY
        private void StartDelay(GenericSkill _skill)
        {
            float delayTime = _skill.Delay > 0 ? _skill.Delay : 0.01f;
            delayTimer.SetTime(delayTime);
            OnSkillDelayStarted?.Invoke(delayTime);
        }
        #endregion

        #region UTILS
        public void DisableController(bool _status)
        {
            Disabled = _status;
            if (Disabled)
            {
                ClearPendingSkill();
                CancelCasting();
            }

            string s = Disabled ? "disabled" : "enabled";
            // Debug.Log($"Skill controller for {DamageDealerObject.name} was {s}");
        }

        public void ResetToDefaultValues()
        {
            Phase = ExecutionPhase.STANDBY;

            DeselectSkill();
            ClearPendingSkill();
            CancelCasting();

            AnimationEvents.OnAttackHit -= ExecuteSkill;
            AnimationEvents.OnAttackEnd -= EndSkill;

            DisableController(false);
        }

        private void FixedUpdate()
        {
            Vector3? _targetPos = null;

            if (Phase == ExecutionPhase.STANDBY) { return; }

            if (RunningSkill != null)
            {
                if (!RunningSkill.LookAtTarget && Phase != ExecutionPhase.CASTING) { return; }

                if (RunningSkill.Target != null)
                {
                    var target = RunningSkill.Target.DamageableObject.transform.position;
                    if (target != DamageDealerObject.transform.position) { _targetPos = target; }
                }

                if (RunningSkill.Location.HasValue) { _targetPos = RunningSkill.Location.Value; }
            }

            if (!_targetPos.HasValue) { return; }

            float speed = 30f;
            Vector3 direction = _targetPos.Value - DamageDealerObject.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(direction, DamageDealerObject.transform.up);
            DamageDealerObject.transform.rotation = Quaternion.Lerp(DamageDealerObject.transform.rotation, toRotation, speed * Time.deltaTime);
        }
        #endregion
    }
}