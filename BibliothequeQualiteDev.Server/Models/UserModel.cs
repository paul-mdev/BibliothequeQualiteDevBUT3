using System.Reflection.Metadata;

public class UserModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPswd { get; set; }
    public string UserMail { get; set; }

    public int RoleID { get; set; }
   
}
