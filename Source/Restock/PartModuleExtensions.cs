using System;
using UnityEngine;

namespace Restock
{
    public static class PartModuleExtensions
    {
        public static void Log(this PartModule module, string message) => Debug.Log(FormatMessage(module, message));
        public static void LogWarning(this PartModule module, string message) => Debug.LogWarning(FormatMessage(module, message));
        public static void LogError(this PartModule module, string message) => Debug.LogError(FormatMessage(module, message));
        public static void LogException(this PartModule module, string message, Exception exception) => Debug.LogException(new Exception(FormatMessage(module, message), exception));

        private static string FormatMessage(PartModule module, string message) => $"[{GetPartName(module.part)} {module.GetType()}] {message}";
        private static string GetPartName(Part part) => part.partInfo?.name ?? part.name;
    }
}
