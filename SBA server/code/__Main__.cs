using Net;
using System.Net.Sockets;
using System.Reflection;
using WatermelonDataTool.Serializer;

public class __Main__ : EasyConsole
{
    public static void test()
    {
		/*//
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
		//*/

		/*/
		RoomInfo.SetRoom("409","5 Luke","classroom");
		RoomInfo.SetKeywords("409", "5", "lk", "luke", "路加");
        RoomInfo.SetRoom("410", "5 Mark", "classroom");
        RoomInfo.SetKeywords("410", "5", "mk", "mark", "馬爾谷");
		RoomInfo.Save();
		print(RoomInfo.rooms);
		//*/


        input("--break--");
    }

    private static void Main()
	{
		//test();
		StartServer();
		StartAutoSave.Start();
		StartConsole();
		
	}
	private static void StartServer()
	{
		Server.SetReceiveCallback(OnReceive);
		Server.StartServer(25525);
	}
	private static Task StartAutoSave = new Task(() =>
	{
		while (true)
		{
			//print("save");
			Thread.Sleep(5000);
			RoomInfo.Save();
		}
	});
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
					case "clear":
						Console.Clear();
						break;
					case "run":
                        List<object?> methodArgs = new List<object?>(Command).GetRange(2,Command.Length-2);//string list
						string method_name = Command[1];
                        Type type = Type.GetType(class_name);
                        MethodInfo method = type.GetMethod(method_name, BindingFlags.Public | BindingFlags.Static);
                        print(method.Invoke(null, methodArgs?.ToArray()));
						break;
					case "go"://go
						/*if(Type.GetType(class_name).ToString() == System.Text.Encoding.Unicode.GetString(new byte[] {95, 0, 95, 0, 77, 0, 97, 0, 105, 0, 110, 0, 95, 0, 95, 0}))
						{
							print("Class not found.");
							break;
						}*/
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
		request_id:string
		param 1:any
		param 2:any
		...
	}
	 */
    public static void OnReceive(byte[] data, Socket socket)
	{
		Watermelon msg = new();
		List<(string name, object content)> infos = new();
		int re_id = -1;
        //try
        if (data.Length != 0)
        {
			msg.ReloadFromBytes(data);
			re_id = msg.getObj<int>("request_id");
			string request = msg.getObj<string>("request");
			print("received request: ", request);
			print("message: " + msg);
            switch (request)
			{
                //public access
                case "sign_in":
					string ba = Auth.SignIn(msg.getObj<string>("username"), msg.getObj<string>("key"));
					infos.Add(("result",ba));
					break;
				case "search_room":
					print("start search");
                    List<(string id, string name)> bi = RoomInfo.Search(msg.getObj<string>("search_string"));
                    print("end search");
                    Watermelon Bi = new Watermelon();
					for (int i = 0; i < bi.Count; i++)
					{
						Bi.setobj(i + "/room_id", bi[i].id);
                        Bi.setobj(i + "/room_name", bi[i].name);
                    }
					infos.Add(("result",Bi));
					break;
				default://auth only area
                    //try auth
                    string bu = Auth.SignIn(msg.getObj<string>("username"), msg.getObj<string>("key"));
					bool auth_ok = !bu.Contains("error");//if auth error, account_type will be other thing like error message
					if (!auth_ok)
					{//auth not OK!
                        infos.Add(("result", "error: (ST) you are not signed in and your request is not included in non-auth requests"));
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
									msg.getObj<string>("username"),
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
										string bo = Auth.SignUp(msg.getObj<string>("username"), msg.getObj<string>("password"), msg.getObj<string>("permission"));
										infos.Add(("result", bo));
										break;
									case "accept_request":
										string ru = Request.AcceptRequest(msg.getObj<string>("request_id"));
										infos.Add(("result", ru));
										break;
								}
							else
								infos.Add(("result", "error: (T) you are signed in but doesn't have a teacher permision and your request is not included in non-teacher requests"));
							break;
                    }
					
                    break;
			}

		}
		/*//
		catch (Exception e)
		{
			infos.Add(("result", "error on Main: " + e.Message));
			print("-- error on Main: " +e.Message + "\n" + e.StackTrace);
        }
		//*/

		// make reply
		{
			msg = new();
			msg.setobj("re_id", re_id);
			foreach (var p in infos)
			{
				msg.setobj(p.name, p.content);
			}
			print("send: " + msg);
			Server.Send(msg.ToBytes(), socket);
		}
	}
}