namespace INVENTO_FLOW.Models
{
    public class User
    {
        int id { get; set; }
        string ?name { get; set; }

        string ?PasswordHash { get; set; }

        string ?Role { get; set; }
    }
}
