using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restock
{
  public class ModuleRestockLinkedColorChanger : ModuleColorChanger
  {

    // The module to link this ColorChanger to
    [KSPField] public string parentModuleID = "moduleName";

    public ModuleColorChanger parentModuleColorChanger;

    public override void Start()
    {
      base.Start();

      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        
        foreach (ModuleColorChanger mcc in part.GetComponents<ModuleColorChanger>())
        {
          Debug.Log(mcc.moduleID);
          if (mcc.moduleID == parentModuleID)
          {
            parentModuleColorChanger = mcc;
          }
        }
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!parentModuleColorChanger)
        return;
      
      SetScalar(parentModuleColorChanger.GetScalar);
      
    }
  }
}