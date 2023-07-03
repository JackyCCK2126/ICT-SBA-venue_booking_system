using Net;
using System.Net.Sockets;
using System.Reflection;
using WatermelonDataTool.Serializer;

public class __Main__ : EasyConsole
{
    public static void test()
    {
		/*
		print(Auth.SignUp("jacky", "123", "student"));//pass
        string session_id = Auth.SignIn("jacky", "123");//pass
		print(session_id);//pass
		print("t-id: ", Auth.SignIn("jacky", session_id));//pass
		print("t-error: ", Auth.SignIn("jacky", "1234567"));
        string request_1 = Request.AddRequest("409", 2023, 7, 1, 12f, 12.5f, "jacky", "play 傳說對決");//pass
        string request_2 = Request.AddRequest("409", 2023, 7, 1, 12f, 12.5f, "jacky", "play COC");//pass
		print("both should ok first time ->>", request_1, request_2);//pass
		print("should be OK first time ->>" + Request.AcceptRequest(request_1));//pass
		print("should be error ->>" + Request.AcceptRequest(request_2));//pass
		print("should be error ->>" + Request.AddRequest("409", 2023, 7, 1, 12f, 12.5f, "jacky", "play 傳說對決"));//pass
		*/

        input("--break--");
    }

    private static void Main()
	{
		test();
		//start_server();
		Task a = new Task(()=>{ StartAutoSave(); });
		a.Start();
		StartConsole();
		
	}
	private static void StartServer()
	{
		server.SetReceiveCallback(OnReceive);
		server.StartServer(25525);
	}
	private static Task StartAutoSave()
	{
		while (true)
		{
			Thread.Sleep(5000);
			RoomInfo.Save();
        }
	}
    private static void StartConsole()
	{
		string class_name = "__Main__";

        while (true)
		{
			try
			{
				string[] Command = input().Split(' ');
                string request = Command[0].ToLower();
				switch (request)
				{
					case "run":
                        List<object?> methodArgs = new List<object?>(Command).GetRange(2,Command.Length-2);//string list
						string method_name = Command[1];
                        Type type = Type.GetType(class_name);
                        MethodInfo method = type.GetMethod(method_name, BindingFlags.Public | BindingFlags.Static);
                        print(method.Invoke(null, methodArgs?.ToArray()));
						break;
					case "go"://go 
						class_name = Command[1];
						print("switched to class " + Type.GetType(class_name));
                        break;
                }

            }catch(Exception e)
			{
				print(e.Message);
			}

        }
	}
    /*
	msg
	{
		request:string
		action_id:string
		param 1:any
		param 2:any
		...
	}
	 */
    public static void OnReceive(byte[] data, Socket socket)
	{
		Watermelon msg = new();
		List<(string name, object content)> infos = new();
        try
		{
			msg.ReloadFromBytes(data);
			string request = msg.getObj<string>("request");
            switch (request)
			{
                //public access
                case "sign_in":
					string ba = Auth.SignIn(msg.getObj<string>("user_name"), msg.getObj<string>("credential"));
					infos.Add(("result",ba));
					break;
				case "search_room":
					string[] bi = RoomInfo.Search(msg.getObj<string>("search_string"));
					infos.Add(("result",bi));
					break;
				default://auth only area
                    //try auth
                    string bu = Auth.SignIn(msg.getObj<string>("user_name"), msg.getObj<string>("credential"));
					bool auth_ok = bu.Contains("error");//if auth error, account_type will be other thing like error message
					if (!auth_ok)
					{//auth not OK!
                        infos.Add(("result", "error: sign in error"));
						break;
                    }
					//auth OK!
					switch (request)
                    {
						case "request_book":
							string ra = Request.AddRequest(
                                    msg.getObj<string>("room_id"),
									msg.getObj<int>("year"),
									msg.getObj<int>("month"),
									msg.getObj<int>("day"),
									msg.getObj<float>("start_hour"),
									msg.getObj<float>("end_hour"),
									msg.getObj<string>("user_name"),
									msg.getObj<string>("reason")
								);
							infos.Add(("result",ra));
							break;
						case "view_request_by_ids":
							string[] ids = msg.getObj<string[]>("request_ids");
							Watermelon extracted_requests = new();
							foreach(string id in ids)
							{
								extracted_requests.setobj(id,Request.GetRequestByID(id));
							}
							infos.Add(("result",extracted_requests));

							break;
						default:
							//teacher only area
							if (bu.Contains("teacher"))
								switch (request)
								{
									case "sign_up":
										string bo = Auth.SignUp(msg.getObj<string>("user_name"), msg.getObj<string>("password"), msg.getObj<string>("permission"));
										infos.Add(("result", bo));
										break;
									case "accept_request":
										string ru = Request.AcceptRequest(msg.getObj<string>("request_id"));
                                        infos.Add(("result", ru));
                                        break;
								}
							break;
                    }
					
                    break;
			}

		}
		catch (Exception e)
		{
			server.Disconnect(socket);
			infos.Add(("result", "error: " + e.Message));
        }

		// make reply
		{
			msg = new();
			msg.setobj("re_id", msg.getObj<string>("action_id"));
			foreach (var p in infos)
			{
				msg.setobj(p.name, p.content);
			}
			server.Send(msg.ToBytes(), socket);
		}
	}
}