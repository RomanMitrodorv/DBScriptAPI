using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFS.ClientApi.Models
{
    public class TaskPath
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}