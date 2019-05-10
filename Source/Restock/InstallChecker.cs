using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Restock
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class InstallChecker : MonoBehaviour
    {
        public readonly IEnumerable<string> UNEXPECTED_URLS = Array.AsReadOnly(new[] {
            "Squad/Parts/FuelTank/fuelTankT100",
            "Squad/Parts/FuelTank/fuelTankT200",
            "Squad/Parts/FuelTank/fuelTankT400",
            "Squad/Parts/FuelTank/fuelTankT800",
        });

        private void Start()
        {
            string[] errorMessages = CheckInstallLocation().Concat(CheckUnexpectedUrls()).ToArray();

            if (errorMessages.Length > 0) CreateWarningDialog(errorMessages);
        }

        private IEnumerable<string> CheckInstallLocation()
        {
            UrlDir gameData = GameDatabase.Instance.root.children.First(dir => dir.type == UrlDir.DirectoryType.GameData);
            AssemblyLoader.LoadedAssembly loadedAssembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly());

            if (loadedAssembly == null)
            {
                Debug.LogError("[Restock] Error checking install location - could not find loaded assembly");
                yield break;
            }

            UrlDir assemblyDir = GetDirectory(gameData, loadedAssembly.url);

            if (assemblyDir == null)
            {
                Debug.LogError("[Restock] Error checking install location - could not find assembly url directory: " + loadedAssembly.url);
                yield break;
            }

            string observedInstallPath = Path.GetFullPath(assemblyDir.parent.path);
            string expectedInstallPath = Path.GetFullPath(Path.Combine(KSPUtil.ApplicationRootPath, Path.Combine("GameData", "ReStock")));

            if (observedInstallPath != expectedInstallPath)
            {
                Debug.LogError($"[Restock] Install found at '{observedInstallPath}'");
                yield return $"Expected Restock to be installed at\n{expectedInstallPath}\nbut actually installed at\n{observedInstallPath}";
            }
        }

        private IEnumerable<string> CheckUnexpectedUrls()
        {
            UrlDir gameData = GameDatabase.Instance.root.children.First(dir => dir.type == UrlDir.DirectoryType.GameData);
            foreach (string unexpectedUrl in UNEXPECTED_URLS)
            {
                if (!(gameData.GetDirectory(unexpectedUrl) is UrlDir unexpectedDir)) continue;
                Debug.LogError("[ReStock] Found unexpected directory " + unexpectedDir.path);
                yield return $"Found unexpected directory, likely left over from an older version of KSP:\n" + unexpectedDir.path;
            }
        }

        private void CreateWarningDialog(params string[] allMessages)
        {
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog(
                    "RestockInstallWarning",
                    $"Restock has detected installation issues:\n\n{string.Join("\n\n", allMessages)}",
                    "Restock - installation issues detected",
                    HighLogic.UISkin,
                    new Rect(0.5f, 0.5f, 500f, 60f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIHorizontalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("OK", delegate () { }, 140.0f, 30.0f, true),
                        new DialogGUIFlexibleSpace()
                    )
                ),
                true,
                HighLogic.UISkin);
        }

        private UrlDir GetDirectory(UrlDir baseUrl, string url)
        {
            string[] splitUrl = url.Trim().Split('/');

            UrlDir currentDir = baseUrl;
            foreach (string dirName in splitUrl)
            {
                currentDir = currentDir.children.FirstOrDefault(dir => dir.name == dirName);
                if (currentDir == null) return null;
            }
            return currentDir;
        }
    }
}
