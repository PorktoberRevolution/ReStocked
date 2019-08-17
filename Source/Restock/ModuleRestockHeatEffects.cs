using System;
using UnityEngine;
using System.Collections.Generic;

namespace Restock
{
    public class ModuleRestockHeatEffects : PartModule
    {
        [KSPField] public bool enableHeatEmissive = false;

        [KSPField] public string shaderProperty = string.Empty;

        [KSPField] public FloatCurve redCurve= new FloatCurve();

        [KSPField] public FloatCurve greenCurve = new FloatCurve();

        [KSPField] public FloatCurve blueCurve = new FloatCurve();

        [KSPField] public FloatCurve alphaCurve = new FloatCurve();

        [KSPField] public double draperPoint = 798.0;
        
        [KSPField] public double lerpMax = double.NaN;
        
        [KSPField] public double lerpMin = 0.0;

        [KSPField] public bool useCoreTemp = false;
        
        [KSPField] public bool useSkinTemp = false;

        [KSPField] public bool disableBlackbody = false;
        
        [KSPField] public List<Renderer> renderers = new List<Renderer>();

        private readonly string _shaderBlackbody = "_TemperatureColor";

        private ModuleCoreHeat _coreHeatModule = null;
        
        private int _shaderPropertyID;

        private int _shaderBlackbodyID;

        private double _lerpRange;

        private Color _emissiveColor = new Color();
        private MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();

        public void Start()
        {
            Debug.Log("Start()");
            if (base.vessel == null) return;
            
            if (enableHeatEmissive)
            {
                if (useCoreTemp)
                {
                    _coreHeatModule = base.part.FindModuleImplementing<ModuleCoreHeat>();
                    if (_coreHeatModule == null)
                    {
                        this.LogError("Part has no Core Heat module, skipping");
                        useCoreTemp = false;
                    }
                }
                
                if (double.IsNaN(lerpMax))
                {
                    if (useCoreTemp)
                    {
                        lerpMax = _coreHeatModule.CoreShutdownTemp;
                    }
                    else
                    {
                        lerpMax = useSkinTemp ? part.skinMaxTemp : part.maxTemp;
                    }
                }

                _lerpRange = lerpMax - lerpMin - draperPoint;

                _shaderPropertyID = Shader.PropertyToID(shaderProperty);
            }

            if (disableBlackbody)
            {
                _shaderBlackbodyID = Shader.PropertyToID(_shaderBlackbody);
            }
        }
        
        public override void OnLoad(ConfigNode node)
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) return;
            
            Debug.Log("OnLoad()");
            Debug.Log(node.ToString());
            
            renderers = base.part.FindModelComponents<Renderer>();
            
            if (node.HasValue("excludedRenderer"))
            {
                var excludedRenderers = new List<string>();
                
                excludedRenderers.AddRange(node.GetValues("excludedRenderer"));

                for (int i = renderers.Count - 1; i >= 0; i--)
                {
                    if (renderers[i] == null || excludedRenderers.Contains(renderers[i].name))
                    {
                        renderers.RemoveAt(i);
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            if (enableHeatEmissive)
            {
                var temp = 0.0;
                if (useCoreTemp)
                {
                    temp = _coreHeatModule.CoreTemperature;
                }
                else
                {
                    temp = useSkinTemp ? base.part.skinTemperature : base.part.temperature;
                }
                
                var temp2 = (float) ((temp - draperPoint) / _lerpRange);
                temp2 = Mathf.Clamp01(temp2);
                
                _emissiveColor.r = redCurve.Evaluate(temp2);
                _emissiveColor.g = greenCurve.Evaluate(temp2);
                _emissiveColor.b = blueCurve.Evaluate(temp2);
                _emissiveColor.a = alphaCurve.Evaluate(temp2);

                _propertyBlock.SetColor(_shaderPropertyID, _emissiveColor);
            }

            if (disableBlackbody)
            {
                _propertyBlock.SetColor(_shaderBlackbodyID, Color.black);

            }
            
            foreach (var r in renderers)
            {
                r.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}