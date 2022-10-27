using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DBScriptDeployment.Models
{
    public class User
    {
        [Required]
        [JsonProperty("server")]
        public string Server { get; set; }
        [Required]
        [JsonProperty("username")]
        public string Username { get; set; }
        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
