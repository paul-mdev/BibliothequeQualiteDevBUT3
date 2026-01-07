using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BibliothequeQualiteDev.Server.Models
{
    public class UsersModel
    {
        public int user_id { get; set; }
        public string user_pswd { get; set; } = string.Empty;
        public string user_name { get; set; } = string.Empty;
        public string user_mail { get; set; } = string.Empty;

        public int role_id { get; set; }

        [ForeignKey("role_id")]
        [JsonIgnore]
        public RolesModel? role { get; set; } = null;
    }
}