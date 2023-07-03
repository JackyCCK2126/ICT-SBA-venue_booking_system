using WatermelonDataTool.Serializer;

class Request : DataBase
{
	private static string request_path = "Booking Requests.txt";
	/*
	* Requests
	{
		(@request_id):{

			month:int, day:int, start_hour:float (eg.16.0), end_hour:float (eg.18.5),
			user_name:string,
			room_id:string,
			reason:string,

			acceptability:bool? (e.g null/True/False)

		}#this is a booking request from 4:00 to 6:30
	}
	 */
	private static object myLock = new object();//make sure no multi-thread access auth function at the same time.
	public static Watermelon requests { get { if (_requests == null) { _requests = GetFromPath(request_path); } return _requests/*ref*/; } }
	private static Watermelon _requests = null;

	public static string AddRequest(string room_id, int year, int month, int day, float start_hour, float end_hour, string user_name, string reason)
	{
		lock(myLock) {
			if (!available(room_id, year, month, day, start_hour, end_hour)) return "error: unavailable";
			EasyConsole.print(requests.exist("latest_request_id"));
			int new_id = requests.TouchGet<int>("latest_request_id",0) + 1;
			Watermelon request_info = new();
			request_info.setobj("room_id", room_id);
			request_info.setobj("year", year);
			request_info.setobj("month", month);
			request_info.setobj("day", day);
			request_info.setobj("start_hour", start_hour);
			request_info.setobj("end_hour", end_hour);
			request_info.setobj("user_name", user_name);
			request_info.setobj("reason", reason);
			request_info.setobj("acceptability", null);

			requests.setobj(new_id.ToString(), request_info);
			requests.setobj("latest_request_id", new_id);
			Save();
			return new_id.ToString();//request id
		}
	}

	public static string AcceptRequest(string request_id)
	{
		Watermelon current = requests.getObj<Watermelon>(request_id);
		if (current == null) return "error: request not found";

		//extract current request info c1-7
		var c1 = current.getObj<string>("room_id");
		var c2 = current.getObj<int>("year");
		var c3 = current.getObj<int>("month");
		var c4 = current.getObj<int>("day");
		var c5 = current.getObj<float>("start_hour");
		var c6 = current.getObj<float>("end_hour");
		var c7 = current.getObj<string>("user_name");
		var c8 = current.getObj<string>("reason");

		if (!available(c1, c2, c3, c4, c5, c6)) return "error: unavailable";
		BookingRecords.AddBookingRecord(c1,c2,c3,c4,c5,c6,c7,c8);//this function will also save new record.
		requests.setobj(request_id + "/acceptability", true);
		return "OK";
	}

	public static Watermelon GetRequestByID(string request_id)
	{
        return requests.getObj<Watermelon>(request_id);
    }
		

    static bool available(string room_id, int year, int month, int day, float start_hour, float end_hour)
	{//
        string current_path = Path.Combine(
                BookingRecords.record_path,
                room_id,
                year.ToString(),
                month.ToString(),
                day.ToString()
            );
		Watermelon current_recs = GetFromPath(current_path);

		foreach(Melon recf in current_recs)
		{
			if (recf.FieldName == "latest_record_id") continue;
			Watermelon rec = (Watermelon)recf.obj;

			if (
				start_hour <= rec.getObj<float>("start_hour") && end_hour > rec.getObj<float>("start_hour")
				||
				end_hour >= rec.getObj<float>("end_hour") && start_hour < rec.getObj<float>("end_hour")
				||
				end_hour == start_hour
				)
				return false;
		}
        return true;
	}

	public static void Save()
	{
		lock (myLock)
		{
			SaveToPath(request_path, requests);
		}
	}
}