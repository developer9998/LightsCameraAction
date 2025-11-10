using BepInEx.Logging;
using System;
using System.Diagnostics;

namespace LightsCameraAction.Tools
{
    public static class Logging
    {
        private static ManualLogSource logger;
        public static void Init()
        {
            logger = Logger.CreateLogSource("LightsCameraAction");
        }

        public static void Exception(Exception e)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogWarning($"({methodInfo.ReflectedType.Name}.{methodInfo.Name}()) " + string.Join(" ", e.Message, e.StackTrace));
        }

        public static void Fatal(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogFatal($"({methodInfo.ReflectedType.Name}.{methodInfo.Name}()) " + string.Join(" ", content));
        }

        public static void Warning(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogWarning($"({methodInfo.ReflectedType.Name}.{methodInfo.Name}()) " + string.Join(" ", content));
        }

        public static void Info(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogInfo($"({methodInfo.ReflectedType.Name}.{methodInfo.Name}()) " + string.Join(" ", content));

        }

        public static void Debug(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogDebug($"({methodInfo.ReflectedType.Name}.{methodInfo.Name}()) " + string.Join("  ", content));
        }
    }
}
