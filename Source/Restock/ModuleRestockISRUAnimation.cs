using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockISRUAnimation : PartModule
    {
        // name of the deploy animation to use
        [KSPField] public string deployAnimationName = "";

        // name of the retract animation to use
        // will default to using the deploy animation in reverse
        [KSPField] public string retractAnimationName = "";

        // name of the active animation to use
        [KSPField] public string activeAnimationName = "";
        
        // name of the inactive animation to use
        [KSPField] public string inactiveAnimationName = "";

        // speed to run the animation when deploying
        [KSPField] public float deploySpeed = 1.0f;

        // speed to run the animation when retracting
        [KSPField] public float retractSpeed = 1.0f;

        // does this module need electric charge to be enabled?
        [KSPField] public bool needsEC = false;

        public bool IsDeployed
        {
            get { return (CurrentState == State.Active || CurrentState == State.Deploying); }
        }
        
        private enum State
        {
            Inactive,
            Deploying,
            Active,
            Retracting
        }

        private Animation DeployAnimation { get; set; }
        private Animation RetractAnimation { get; set; }
        private Animation ActiveAnimation { get; set; }
        private Animation InactiveAnimation { get; set; }

        private State CurrentState { get; set; }
        
        private bool _deployAnimationPresent = false;
        private bool _retractAnimationPresent = false;
        private bool _activeAnimationPresent = false;
        private bool _inactiveAnimationPresent = false;
        
        private List<BaseConverter> _modules;

        public void Start()
        {
            _modules = base.part.FindModulesImplementing<BaseConverter>();

            _deployAnimationPresent = (deployAnimationName != string.Empty);
            _retractAnimationPresent = (retractAnimationName != string.Empty);
            _activeAnimationPresent = (activeAnimationName != string.Empty);
            _inactiveAnimationPresent = (inactiveAnimationName != string.Empty);
            
            DeployAnimation = ((_deployAnimationPresent) ? base.part.FindModelAnimators(deployAnimationName)[0] : null);
            RetractAnimation = ((_retractAnimationPresent) ? base.part.FindModelAnimators(retractAnimationName)[0] : null);
            ActiveAnimation = ((_activeAnimationPresent) ? base.part.FindModelAnimators(activeAnimationName)[0] : null);
            InactiveAnimation = ((_inactiveAnimationPresent) ? base.part.FindModelAnimators(inactiveAnimationName)[0] : null);
            
            CurrentState = State.Inactive;
            
            foreach (var a in base.part.FindModelAnimators()) a.Stop();
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!HighLogic.LoadedSceneIsFlight || base.vessel == null)
            {
                CurrentState = State.Inactive;
                return;
            }
            
            if (IsDeployed)
            {
                DeployEnd();
            }
            else
            {
                RetractEnd();
            }
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            
            try
            {
                if (needsEC && !CheatOptions.InfiniteElectricity)
                {
                    var ecHash = PartResourceLibrary.ElectricityHashcode;
                    base.vessel.GetConnectedResourceTotals(ecHash, out var ecAmount, out _, true);
                    if (ecAmount < 0.1)
                    {
                        if (IsDeployed) RetractStart();
                        return;
                    }
                }

                int enabledCount = 0;
                foreach (var m in _modules)
                {
                    if (m.ModuleIsActive()) enabledCount++;
                }

                switch (CurrentState)
                {
                    case State.Active:
                        if (enabledCount == 0)
                        {
                            DeployStart();
                        }
                        else if (_activeAnimationPresent && !ActiveAnimation.IsPlaying(activeAnimationName))
                        {
                            PlayAnimation(ActiveAnimation, activeAnimationName);
                        }
                        break;
                    
                    case State.Deploying:
                        if (!_deployAnimationPresent)
                        {
                            this.LogError("Invalid state!");
                            CurrentState = State.Active;
                        }
                        if (enabledCount == 0)
                        {
                            RetractStart();
                        }
                        else if (!DeployAnimation.IsPlaying(deployAnimationName))
                        {
                            DeployEnd();
                        }
                        break;
                    
                    case State.Inactive:
                        if (enabledCount != 0)
                        {
                            DeployStart();
                        }
                        else if (_inactiveAnimationPresent && !InactiveAnimation.IsPlaying(inactiveAnimationName))
                        {
                            PlayAnimation(InactiveAnimation, inactiveAnimationName);
                        }
                        break;
                    
                    case State.Retracting:
                        if (!_retractAnimationPresent && !_deployAnimationPresent)
                        {
                            this.LogError("Invalid state!");
                            CurrentState = State.Inactive;
                        }
                        if (enabledCount != 0)
                        {
                            DeployStart();
                        }
                        else if (!RetractAnimation.IsPlaying(retractAnimationName))
                        {
                            RetractEnd();
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                this.LogException("Failed to update animation module", e);
            }
        }

        private void DeployStart()
        {
            if (_deployAnimationPresent)
            {
                CurrentState = State.Deploying;
                PlayAnimation(DeployAnimation, deployAnimationName, deploySpeed);
            }
            else
            {
                DeployEnd();
            }
        }

        private void DeployEnd()
        {
            CurrentState = State.Active;
            if (_activeAnimationPresent)
            {
                PlayAnimation(ActiveAnimation, activeAnimationName);
            }
        }

        private void RetractStart()
        {
            if (_retractAnimationPresent)
            {
                CurrentState = State.Retracting;
                PlayAnimation(RetractAnimation, retractAnimationName, retractSpeed);
            } else if (_deployAnimationPresent)
            {
                CurrentState = State.Retracting;
                PlayAnimation(DeployAnimation, deployAnimationName, retractSpeed * -1);
            }
            else
            {
                RetractEnd();
            }
        }

        private void RetractEnd()
        {
            CurrentState = State.Inactive;
            if (_inactiveAnimationPresent)
            {
                PlayAnimation(InactiveAnimation, inactiveAnimationName);
            }
        }

        private void PlayAnimation(Animation anim, string name, float speed = 1f)
        {
            var animState = anim[name];
            if (speed < 0 && animState.time < Mathf.Epsilon)
            {
                animState.time = animState.length;
            }
            else if (speed > 0 && animState.time > animState.length - Mathf.Epsilon)
            {
                animState.time = 0.0f;
            }

            animState.speed = speed;
            anim.Play(name);            
        }
    }
}