using WatermelonDataTool.Serializer;

class BookingRecords : DataBase
{
	public static string record_path = "Booking Records";
	public static Watermelon current_records = new();
	/*
	path: Booking Records/room_id/year/month/day.nex:nexus
	{
		@record_id{
			start_hour:float(16.0),
			end_hour:float(19.5),
			user_name:string,
			reason:string
		}//this is a booking request from 4:00 to 7:30
	}
	 */
	private static object myLock = new object();//make sure no multi-thread access auth function at the same time.

	/// <summary> for internal use </summary>
    public static string AddBookingRecord(string room_id, int year, int month, int day, float start_hour, float end_hour, string user_name, string reason)
	{//returns record id
		lock (myLock)
		{
			string current_path = Path.Combine(
				record_path,
				room_id,
				year.ToString(),
				month.ToString(),
				day.ToString()
			);
			current_records = GetFromPath(current_path);
            //ps. latest_record_id also represents no. of booking records of the room at that day.
            int new_record_id = current_records.TouchGet<int>("latest_record_id", 0) + 1;


            current_records.setobj(new_record_id.ToString()+ "/start_hour", start_hour);
			current_records.setobj(new_record_id.ToString() + "/end_hour", end_hour);
			current_records.setobj(new_record_id.ToString() + "/user_name", user_name);
			current_records.setobj(new_record_id.ToString() + "/reason", reason);

			current_records.setobj("latest_record_id", new_record_id);
			Save(current_path);
			return new_record_id.ToString();
		}
	}
	/// <summary> for internal use. (indirectly used by bookers and admin) </summary>
	public static void RemoveBookingRecord(string room_id, int year, int month, int day, string record_id)
	{
		string path = Path.Combine(record_path, room_id, year.ToString(), month.ToString(), day.ToString());
		current_records = GetFromPath(path);
		current_records.removeField(record_id);
	}
	public static void Save(string path)
	{
		lock (myLock)
		{
			SaveToPath(path, current_records);
		}
	}
}