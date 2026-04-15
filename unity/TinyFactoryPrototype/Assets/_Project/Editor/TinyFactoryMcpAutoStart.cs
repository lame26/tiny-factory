using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace TinyFactory.Editor
{
    [InitializeOnLoad]
    internal static class TinyFactoryMcpAutoStart
    {
        private const string UvxPath =
            @"C:\Users\lame2\AppData\Local\Microsoft\WinGet\Packages\astral-sh.uv_Microsoft.Winget.Source_8wekyb3d8bbwe\uvx.exe";

        private static bool s_startedThisDomain;

        static TinyFactoryMcpAutoStart()
        {
            EditorApplication.delayCall += ConfigureAndStart;
        }

        private static void ConfigureAndStart()
        {
            if (s_startedThisDomain)
            {
                return;
            }

            s_startedThisDomain = true;

            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
            EditorPrefs.SetString("MCPForUnity.HttpTransportScope", "local");
            EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://localhost:8080");
            EditorPrefs.SetString("MCPForUnity.UvxPath", UvxPath);
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            EditorConfigurationCache.Instance.Refresh();

            _ = StartServerAsync();
        }

        private static async Task StartServerAsync()
        {
            try
            {
                if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
                {
                    MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
                }

                for (var attempt = 0; attempt < 30; attempt++)
                {
                    if (MCPServiceLocator.Server.IsLocalHttpServerReachable())
                    {
                        await MCPServiceLocator.Bridge.StartAsync();
                        Debug.Log("[TinyFactory] MCP for Unity requested on http://localhost:8080/mcp.");
                        return;
                    }

                    await Task.Delay(attempt < 6 ? 500 : 3000);
                }

                Debug.LogWarning("[TinyFactory] MCP for Unity server did not become reachable.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TinyFactory] MCP for Unity auto-start failed: {ex.Message}");
            }
        }
    }
}
