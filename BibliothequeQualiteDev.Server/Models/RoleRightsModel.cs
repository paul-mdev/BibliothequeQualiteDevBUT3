using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeQualiteDev.Server.Models
{
    public class RoleRightsModel
    {
        public int role_id { get; set; }
        public int right_id { get; set; }

        [ForeignKey("role_id")]
        public RolesModel role { get; set; } = null!;

        [ForeignKey("right_id")]
        public RightsModel right { get; set; } = null!;
    }
}
