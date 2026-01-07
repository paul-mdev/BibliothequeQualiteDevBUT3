using System.ComponentModel.DataAnnotations;

namespace BibliothequeQualiteDev.Server.Models
{
    public class RightsModel
    {
        [Key]
        public int right_id { get; set; }
        public string right_name { get; set; } = string.Empty;

        public ICollection<RoleRightsModel> role_rights { get; set; } = new List<RoleRightsModel>();
    }
}
