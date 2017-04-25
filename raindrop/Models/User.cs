using NPoco;

namespace raindrop.Models
{
    [TableName("User")]
    [PrimaryKey("UserId")]
    public class User : Record<User>
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
