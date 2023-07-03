using System.Runtime.InteropServices;

namespace game_server_1.Data
{
    internal class DataStructs
    {
        public struct Vector3
        {
            public float x, y, z;
            public Vector3([Optional] float X, [Optional] float Y, [Optional] float Z)
            {
                x = X;
                y = Y;
                z = Z;
            }
            public override string ToString()
            {
                return "(" + x + "," + y + "," + z + ")";
            }
            public static Vector3 One()
            {
                return new Vector3(1,1,1);
            }
        }
        public struct UserData
        {
            public string Name, IPAddress, UserID;
            public Vector3 Pos;
            public UserData([Optional] string name, [Optional] string IP ,[Optional] string user_id,[Optional] Vector3 pos)
            {
                Name = name;
                IPAddress = IP;
                Pos = pos;
                UserID = user_id;
            }
            public override string ToString()
            {
                return "(" + UserID + "," + Name + "," + IPAddress + "," + Pos + ")";
            }
        }
    }
}
