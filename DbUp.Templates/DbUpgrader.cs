using System;
using System.Collections.Generic;
using System.Text;
using DbUp.Engine;
using DbUp.Engine.Preprocessors;

namespace DbUp.Templates
{
    public static class UpgradeEngineExtensions
    {
        public static string DryRun(this UpgradeEngine upgradeEngine, IDictionary<string, string> variables)
        {
            var buffer = new StringBuilder($"-- DbUp generated script at {DateTimeOffset.Now}\n");
            var variableReplacer = new VariableSubstitutionPreprocessor(variables);
            upgradeEngine.GetScriptsToExecute().ForEach(s =>
            {
                buffer.AppendLine($"-- Script name: {s.Name}");
                buffer.AppendLine(variableReplacer.Process(s.Contents));
            });
            return buffer.ToString();
        }
    }
}