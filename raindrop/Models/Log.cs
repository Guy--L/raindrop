using NPoco;
using System;

namespace raindrop.Models
{
    [TableName("Log")]
    [PrimaryKey("LogId")]
    public class Log : Record<Log>
    {
        public int LogId { get; set; }
        public DateTime Stamp { get; set; }
        public int DeviceId { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; }

        [Ignore] public User User { get; set; }
    }
}
