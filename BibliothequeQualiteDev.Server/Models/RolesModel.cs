namespace BibliothequeQualiteDev.Server.Models
{
    public class RolesModel
    {
        public int role_id { get; set; }
        public string role_name { get; set; } = string.Empty;

        // navigation
        public ICollection<RoleRightsModel> role_rights { get; set; } = new List<RoleRightsModel>();
        public ICollection<UsersModel> users { get; set; } = new List<UsersModel>();
    }
}