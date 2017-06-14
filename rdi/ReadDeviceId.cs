using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace rdi
{
    class ReadDeviceId
    {
        public static int Get(string connection)
        {
            int deviceid = 0;
            using (SqlConnection con = new SqlConnection(connection))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(
                        "select max(ServerCabinetID) from dbo.Cabinet", con))
                    {
                        deviceid = (int) command.ExecuteScalar();
                    }
                }
                catch
                {
                    Program.ErrorMesg("Could not read deviceid from "+connection);
                }
            }
            return deviceid;
        }
    }
}
