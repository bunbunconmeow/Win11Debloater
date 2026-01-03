using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecVers_Debloat.Schemas
{
    public class ScriptAddon
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("downloaded")]
        public bool Downloaded { get; set; }

        [JsonIgnore]
        public string Extension
        {
            get
            {
                if (!string.IsNullOrEmpty(Title) && Title.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    return "JS";
                }
                return "PS1";
            }
        }
    }

    public class ScriptAddonsCollection
    {
        [JsonPropertyName("scripts")]
        public List<ScriptAddon> Scripts { get; set; } = new List<ScriptAddon>();
    }
}
