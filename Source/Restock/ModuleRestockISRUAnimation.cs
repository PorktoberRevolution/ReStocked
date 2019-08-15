using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockISRUAnimation: PartModule
    {
        [KSPField] public string activeAnimationName = "";

        [KSPField] public string deployAnimationName = "";

        public bool isDeployed = false;
        
        public Animation DeployAnimation
        {
            get;
            private set;
        }

        private List<BaseConverter> _modules;

        private bool _deployAnimationPresent = false;
        
        public void Start()
        {
            if (base.vessel == null) return;
            _modules = base.part.FindModulesImplementing<BaseConverter>();

            _deployAnimationPresent = (deployAnimationName != string.Empty);
            DeployAnimation = ((_deployAnimationPresent) ? base.part.FindModelAnimators(deployAnimationName)[0] : null);

            foreach (var a in base.part.FindModelAnimators()) a.Stop(); 
            
        }

        public override void OnLoad(ConfigNode node)
        {
            if (isDeployed)
            {
                PlayDeployAnimation(1000);
            }
            else
            {
                PlayDeployAnimation(-1000);
            }
        }
        
        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || base.vessel == null) return;
            try
            {
                if (!CheatOptions.InfiniteElectricity)
                {
                    var ecHash = PartResourceLibrary.ElectricityHashcode;
                    base.vessel.GetConnectedResourceTotals(ecHash, out var ecAmount, out _, true);
                    Debug.Log(ecAmount);
                    if (ecAmount < 0.1)
                    {
                        if (isDeployed) RetractModule();
                        return;
                    }
                }

                int enabledCount = 0;
                foreach (var m in _modules)
                {
                    if (m.ModuleIsActive()) enabledCount++;
                }
                if (isDeployed && enabledCount == 0) RetractModule();
                else if (!isDeployed && enabledCount != 0) DeployModule();
            }
            catch (Exception e)
            {
                this.LogException("Failed to update animation module", e);
            }
        }

        public void DeployModule()
        {
            isDeployed = true;
            PlayDeployAnimation(1);
        }

        public void RetractModule()
        {
            isDeployed = false;
            PlayDeployAnimation(-1);
        }

        private void PlayDeployAnimation(int speed)
        {
            if (_deployAnimationPresent)
            {
                var deployAnimationState = DeployAnimation[deployAnimationName];
                if (speed < 0 && deployAnimationState.time < Mathf.Epsilon)
                {
                    deployAnimationState.time = deployAnimationState.length;
                }
                else if (speed > 0 && deployAnimationState.time > deployAnimationState.length - Mathf.Epsilon)
                {
                    deployAnimationState.time = 0.0f;
                }
                deployAnimationState.speed = speed;
                DeployAnimation.Play(deployAnimationName);
            }
        }
    }
}