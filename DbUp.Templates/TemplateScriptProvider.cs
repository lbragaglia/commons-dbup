using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Transactions;

namespace DbUp.Templates
{
    public class TemplateScriptProvider : IScriptProvider
    {
        private readonly string _variable;
        private readonly IEnumerable<string> _values;
        private readonly IDictionary<string, string> _scripts = new Dictionary<string, string>();

        public TemplateScriptProvider(string variable, IEnumerable<string> values)
            : this(variable, values, resourceName => resourceName.EndsWith(".tpl.sql"))
        {
        }

        public TemplateScriptProvider(string variable, IEnumerable<string> values, Func<string, bool> resourceFilter)
        {
            _variable = $"${variable}$";
            _values = values;

            var thisAssembly = GetType().Assembly;
            foreach (var resourceName in thisAssembly.GetManifestResourceNames().Where(resourceFilter))
            {
                var resource = thisAssembly.GetManifestResourceStream(resourceName);
                if (resource == null) continue;

                _scripts[resourceName] = GetResourceText(resource);
            }
        }

        private static string GetResourceText(Stream resource)
        {
            using (var sr = new StreamReader(resource))
            {
                return sr.ReadToEnd();
            }
        }

        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            return _scripts.SelectMany(tmpl =>
                _values.Select(v => new LazySqlScript(GetScriptName(tmpl.Key, v), GetScriptContent(tmpl.Value, v))));
        }

        private static string GetScriptName(string scriptName, string value)
        {
            return scriptName.Replace(".tpl.sql", $".{value}.sql");
        }

        private Func<string> GetScriptContent(string scriptContent, string variableValue)
        {
            return () => scriptContent.Replace(_variable, variableValue);
        }
    }
}