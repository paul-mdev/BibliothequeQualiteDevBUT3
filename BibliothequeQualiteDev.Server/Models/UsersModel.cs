using System.Reflection.Metadata;

public class UsersModel
{
    public int user_id { get; set; }
    public string user_name { get; set; } = string.Empty;
    public string user_pswd { get; set; }
    public string user_mail { get; set; }

    public int role_id { get; set; }
   
}
