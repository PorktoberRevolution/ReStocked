using UnityEngine;

namespace Restock
{
  public class ModuleRestockEnhancedLight: PartModule
  {
    // Path to the light cookie texture
    [KSPField]
    public string cookiePath;

    ModuleLight[] lightModules;
    Texture2D cookie;

    public override void OnAwake()
    {
      base.OnAwake();
    }
    public override void OnStart(StartState state)
    {
      base.OnStart(state);
      lightModules = base.GetComponentsInChildren<ModuleLight>();
      cookie = GameDatabase.Instance.GetTexture(cookiePath, false);
      cookie.wrapMode = TextureWrapMode.Clamp;

      if (cookie == null) { this.LogError($"Couldn't find light cookie at {cookiePath}"); return; }

      foreach (ModuleLight ml in lightModules)
      {
        foreach (Light l in ml.lights)
        {
          l.cookie = cookie;
        }
      }
    }


  }
}
