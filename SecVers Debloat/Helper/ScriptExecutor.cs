using Jint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Helper
{
    internal class ScriptExecutor
    {
        // Event to send log messages to the UI
        public event EventHandler<string> OnLogMessage;

        public async Task ExecuteScriptFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log($"Error: File not found: {filePath}");
                return;
            }

            string scriptContent = File.ReadAllText(filePath);

            await ExecuteScriptAsync(scriptContent);
        }

        public async Task ExecuteScriptAsync(string scriptContent)
        {
            await Task.Run(() =>
            {
                try
                {
                    var engine = new Engine(cfg =>
                    {
                        cfg.LimitRecursion(20);
                    });

                    var bridge = new SystemBridge((msg) => Log(msg));
                    engine.SetValue("Sys", bridge);

                    Log("--- Script Start ---");

                    // Execute script
                    engine.Execute(scriptContent);

                    Log("--- Script End ---");
                }
                catch (Jint.Runtime.JavaScriptException jsEx)
                {
                    Log($"[SCRIPT ERROR] Location {jsEx.Location}: {jsEx.Message}");
                }
                catch (Exception ex)
                {
                    Log($"[CRITICAL ERROR] {ex.Message}");
                }
            });
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke(this, message);
        }
    }
}
