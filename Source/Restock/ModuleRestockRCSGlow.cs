using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restock
{
  public class ModuleRestockRCSGlow: PartModule
  {
    [KSPField(isPersistant = false)]
    public FloatCurve alphaCurve = new FloatCurve();

    [KSPField(isPersistant = false)]
    public FloatCurve blueCurve = new FloatCurve();

    [KSPField(isPersistant = false)]
    public FloatCurve greenCurve = new FloatCurve();

    [KSPField(isPersistant = false)]
    public FloatCurve redCurve = new FloatCurve();

    [KSPField(isPersistant = false)]
    public string shaderColorParameter = "_EmissiveColor";

    ModuleRCSFX rcs;
    List<Material> thrustMaterials;

    public void Start()
    {
      rcs = part.GetComponent<ModuleRCSFX>();

      thrustMaterials = new List<Material>();
      foreach (Transform t in rcs.thrusterTransforms)
      {
        thrustMaterials.Add(t.GetComponentInChildren<MeshRenderer>().material);
      }
    }

    public void FixedUpdate()
    {

      if (HighLogic.LoadedSceneIsFlight)
      {
        for (int i = 0; i < thrustMaterials.Count; i++)
        {
          float thrust = rcs.thrustForces[i] / rcs.thrusterPower;
          Color c;
          c = new Color(redCurve.Evaluate(thrust),
                        greenCurve.Evaluate(thrust),
                        blueCurve.Evaluate(thrust),
                        alphaCurve.Evaluate(thrust));
          thrustMaterials[i].SetColor(shaderColorParameter, c);

        }
      }
    }
  }
}

