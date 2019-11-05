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

        // should the module wait until a current looping animation completes before changing state?
        [KSPField] public bool waitForComplete = false;

        public bool IsDeployed => (CurrentState == State.InactiveWaiting ||
                                   CurrentState == State.Active ||
                                   CurrentState == State.Deploying);

        private enum State
        {
            Inactive,
            InactiveWaiting,
            Deploying,
            Active,
            ActiveWaiting,
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

            if (_deployAnimationPresent)
            {
                DeployAnimation = base.part.FindModelAnimators(deployAnimationName)[0];
                if (DeployAnimation == null)
                {
                    _deployAnimationPresent = false;
                    this.LogError($"Can't find deploy animation named {deployAnimationName}");
                }
            }
            else DeployAnimation = null;

            if (_retractAnimationPresent)
            {
                RetractAnimation = base.part.FindModelAnimators(retractAnimationName)[0];
                if (RetractAnimation == null)
                {
                    _retractAnimationPresent = false;
                    this.LogError($"Can't find retract animation named {retractAnimationName}");
                }
            }
            else RetractAnimation = null;

            if (_activeAnimationPresent)
            {
                ActiveAnimation = base.part.FindModelAnimators(activeAnimationName)[0];
                if (ActiveAnimation == null)
                {
                    _activeAnimationPresent = false;
                    this.LogError($"Can't find active animation named {activeAnimationName}");
                }
            }
            else ActiveAnimation = null;

            if (_inactiveAnimationPresent)
            {
                InactiveAnimation = base.part.FindModelAnimators(inactiveAnimationName)[0];
                if (InactiveAnimation == null)
                {
                    _inactiveAnimationPresent = false;
                    this.LogError($"Can't find inactive animation named {inactiveAnimationName}");
                }
            }
            else InactiveAnimation = null;

            foreach (var a in base.part.FindModelAnimators()) a.Stop();

            if (!HighLogic.LoadedSceneIsFlight) return;

            if (ConvertersEnabled())
            {
                DeployStart(1000f);
            }
            else
            {
                RetractStart(1000f);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            try
            {
                switch (CurrentState)
                {
                    // System is inactive, and playing the inactive animation if present
                    case State.Inactive:
                        if (ConvertersEnabled())
                        {
                            if (waitForComplete)
                            {
                                DeployWait();
                            }
                            else
                            {
                                DeployStart();
                            }
                        }

                        break;

                    // System is inactive, but waiting for the animation to end before deploying
                    case State.InactiveWaiting:
                        if (!waitForComplete || !_inactiveAnimationPresent)
                        {
                            this.LogError(
                                "Invalid state! waitForComplete not enabled or inactive animation not present.");
                            CurrentState = State.Inactive;
                        }
                        else if (!ConvertersEnabled())
                        {
                            RetractEnd();
                        }
                        else if (!InactiveAnimation.IsPlaying(inactiveAnimationName))
                        {
                            DeployStart();
                        }

                        break;

                    // System is deploying
                    case State.Deploying:
                        if (!_deployAnimationPresent)
                        {
                            this.LogError("Invalid state! Deploying without an animation present.");
                            CurrentState = State.Active;
                        }
                        else if (!ConvertersEnabled())
                        {
                            RetractStart();
                        }
                        else if (!DeployAnimation.IsPlaying(deployAnimationName))
                        {
                            DeployEnd();
                        }

                        break;

                    // System is active, and playing the active animation if present
                    case State.Active:
                        if (!ConvertersEnabled())
                        {
                            if (waitForComplete)
                            {
                                RetractWait();
                            }
                            else
                            {
                                RetractStart();
                            }
                        }

                        break;

                    // System is active, but waiting for the animation to finish before retracting
                    case State.ActiveWaiting:
                        if (!waitForComplete || !_activeAnimationPresent)
                        {
                            this.LogError(
                                "Invalid state! waitForComplete not enabled or active animation not present.");
                            CurrentState = State.Active;
                        }
                        else if (ConvertersEnabled())
                        {
                            DeployEnd();
                        }
                        else if (!ActiveAnimation.IsPlaying(activeAnimationName))
                        {
                            RetractStart();
                        }

                        break;

                    // System is retracting
                    case State.Retracting:
                        if (!_retractAnimationPresent && !_deployAnimationPresent)
                        {
                            this.LogError("Invalid state! Retracting without an animation present.");
                            CurrentState = State.Inactive;
                        }
                        else if (ConvertersEnabled())
                        {
                            DeployStart();
                        }
                        else if (_retractAnimationPresent)
                        {
                            if (!RetractAnimation.IsPlaying(retractAnimationName))
                            {
                                RetractEnd();
                            }
                        }
                        else if (_deployAnimationPresent)
                        {
                            if (!DeployAnimation.IsPlaying(deployAnimationName))
                            {
                                RetractEnd();
                            }
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

        private void DeployWait()
        {
            if (_inactiveAnimationPresent)
            {
                CurrentState = State.InactiveWaiting;
                PlayAnimation(InactiveAnimation, inactiveAnimationName, loop: false);
            }
            else
            {
                DeployStart();
            }
        }

        private void DeployStart(float speed = 1f)
        {
            if (_deployAnimationPresent)
            {
                if (_retractAnimationPresent && RetractAnimation.IsPlaying(retractAnimationName))
                {
                    RetractAnimation.Stop(retractAnimationName);
                }

                CurrentState = State.Deploying;
                PlayAnimation(DeployAnimation, deployAnimationName, speed * deploySpeed);
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
                PlayAnimation(ActiveAnimation, activeAnimationName, loop: true);
            }
        }

        private void RetractWait()
        {
            if (_activeAnimationPresent)
            {
                CurrentState = State.ActiveWaiting;
                PlayAnimation(ActiveAnimation, activeAnimationName, loop: false);
            }
            else
            {
                RetractStart();
            }
        }

        private void RetractStart(float speed = 1f)
        {
            if (_retractAnimationPresent)
            {
                if (_deployAnimationPresent && DeployAnimation.IsPlaying(deployAnimationName))
                {
                    DeployAnimation.Stop(deployAnimationName);
                }

                CurrentState = State.Retracting;
                PlayAnimation(RetractAnimation, retractAnimationName, speed * retractSpeed);
            }
            else if (_deployAnimationPresent)
            {
                CurrentState = State.Retracting;
                PlayAnimation(DeployAnimation, deployAnimationName, speed * retractSpeed * -1);
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
                PlayAnimation(InactiveAnimation, inactiveAnimationName, loop: true);
            }
        }

        private bool ConvertersEnabled()
        {
            if (needsEC && !CheatOptions.InfiniteElectricity)
            {
                var ecHash = PartResourceLibrary.ElectricityHashcode;
                base.vessel.GetConnectedResourceTotals(ecHash, out var ecAmount, out _, true);
                if (ecAmount < 0.1)
                {
                    return false;
                }
            }

            for (var i = 0; i < _modules.Count; i++)
            {
                if (_modules[i].ModuleIsActive())
                {
                    return true;
                }
            }

            return false;
        }

        private static void PlayAnimation(Animation anim, string name, float speed = 1f, bool loop = false)
        {
            var animState = anim[name];

            if (animState.wrapMode != WrapMode.Loop)
            {
                if (speed < 0 && animState.time < Mathf.Epsilon)
                {
                    animState.time = animState.length;
                }
                else if (speed > 0 && animState.time > animState.length - Mathf.Epsilon)
                {
                    animState.time = 0.0f;
                }
            }

            animState.speed = speed;
            animState.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

            //if (!anim.IsPlaying(name)) 
            anim.Play(name);
        }
    }
}