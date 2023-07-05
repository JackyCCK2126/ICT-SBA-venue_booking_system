using static EasyConsole;
using WatermelonDataTool.Serializer;

class Auth : DataBase
{
    public static string cred_path = "Auth/credentials.txt";
    
    public static Watermelon getCreds()
    {
        return GetFromPath(cred_path);
    }
    private static object myLock = new object();//make sure no multi-thread access auth function at the same time.
    public static string SignIn(string username, string password_or_session)//get session_id string
    {
        lock (myLock)
        {

            Watermelon creds = GetFromPath(cred_path);
            if (creds == null)
            {
                return "error: DB not found";
            }
            string pw_check = creds.getObj<string>(username + "/password");
            string session_check = creds.getObj<string>(username + "/session_id");
            string permission = creds.getObj<string>(username + "/permission");
            bool session_expired()
            {//prototype
                return false;
            }
            if (pw_check == null)
            {
                return "error: user not found";
            }
            //to login: client need to have correct pw or have a updated session_id
            else if (pw_check.ToString() == password_or_session || (session_check.ToString() == password_or_session && !session_expired()))
            {//logged in !

                if (session_expired())//prototype: if session expire, renew
                {
                    creds.setobj(username + "/session_id", GenerateSessionID(permission));
                    SaveToPath(cred_path, creds);
                }
                string session = creds.getObj<string>(username + "/session_id");

                return session;
            }
            else
            {//wrong credentials!
                return "error: wrong credentials";
            }

        }
    }

    /// <param name="permission"> t or s </param>
    public static string SignUp(string username, string password, string permission)
    {
        var creds = GetFromPath(cred_path);
        if (creds == null)
        {
            SaveToPath(cred_path, new());
            creds = GetFromPath(cred_path);
        }
        if (creds.exist(username))
        {
            return "error: username already exist";
        }
        else
        {//sign up available !
            creds.setobj(username + "/password", password);
            string s = GenerateSessionID(permission);
            creds.setobj(username + "/session_id", s);
            creds.setobj(username + "/permission", permission);
            SaveToPath(cred_path, creds);
            return s;
        }
    }
    /// <param name="permission"> teacher or student </param>
    public static string GenerateSessionID(string permission)
    {//create 15 digit SessionID string by random unicode from 97~122
        var random = new Random();
        string sessionId = permission + "-";
        for (int i = 0; i < 64; i++)
        {
            sessionId += random.Next(0, 9).ToString();
        }
        return sessionId;
    }
}

