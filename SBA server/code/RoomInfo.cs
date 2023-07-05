using WatermelonDataTool.Serializer;

class RoomInfo : DataBase
{
    private static string rooms_path = "Room Infos.txt";
    public static Watermelon rooms = GetFromPath(rooms_path);
    /*
    * rooms
    {
        G06: {
            room_name:string "school office"
            type: "office"
            keywords:array ["office","school","principal"]
        }
        406: {
            room_name:string "3LK"
            type: "classroom"
            keywords:array ["class","room","Luke","3","路加","班","房","課室"]
        }
    }
     */
    private static object myLock = new object();//make sure no multi-thread access auth function at the same time.
    public static int maxResults = 10;
    public static List<(string id, string name)> Search(string search_string)//get session_id string
    {
        lock(myLock){

            //tidy search search string
            search_string = search_string.Replace(" ", string.Empty).Replace("_",string.Empty).Replace(" room", string.Empty).Replace("room ", string.Empty).ToLower();
            //store relevance to each room
            (string room_id,float rel)[] relevances_n_id = new (string room_id, float rel)[rooms.count];

            //calculate relevance with by weighting a*w + b*w + ...(NameOrId * 4 + RoomType * 2 + keyword1 * 1 + keyword2 * 1 + ...)
            for (int i = 0; i < rooms.count; i++)
            {
                Watermelon room = rooms.getObj<Watermelon>(i);
                relevances_n_id[i].room_id = rooms[i].FieldName;
                if (search_string.Contains(rooms[i].FieldName) || search_string.Contains(room.getObj<string>("room_name")))
                    relevances_n_id[i].rel+= 4;
                if (search_string.Contains(room.getObj<string>("room_type")))
                    relevances_n_id[i].rel += 2;

                string[] room_keywords = rooms.getObj<Watermelon>(i).getObj<string[]>("keywords");
                for (int j = 0;j< room_keywords.Length; j++)
                {
                    if (search_string.Contains(room_keywords[j].ToLower()))
                    {
                        relevances_n_id[i].rel ++;
                    }
                }
            }
            { }
            // use insertion sort to sort the array in descending order of relevance.
            for (int i = 1; i < relevances_n_id.Length; i++)
            {
                (string room_id, float rel) temp = relevances_n_id[i];
                int j = i - 1;
                while (j >= 0 && relevances_n_id[j].rel < temp.rel)
                {
                    relevances_n_id[j + 1] = relevances_n_id[j];
                    j--;
                }
                relevances_n_id[j + 1] = temp;
            }
            //generate result (ids and names)
            List<(string id, string name)> room_IdAndName_results = new List<(string id, string name)>();
            for(int i = 0; i < relevances_n_id.Length && i < maxResults; i++)
            {

                if (relevances_n_id[i].rel > 0)
                {
                    (string id, string name) current = new();

                    current.id = relevances_n_id[i].room_id;
                    current.name = rooms.getObj<string>(relevances_n_id[i].room_id + "/room_name");

                    room_IdAndName_results.Add(current);
                }
            }
            return room_IdAndName_results;
        }
    }//room_id_results[i] = relevances[i].room_id;
    public static void SetRoom(string id,string room_name, string room_type)
    {
        lock (myLock)
        {
            rooms.setobj(id + "/room_name", room_name);
            rooms.setobj(id + "/room_type", room_type);
            rooms.setobj(id+"/keywords", new string[0]);
            //the record: new DateTime(2023,5,15,15,30,0).ToBinary(), user_name,
        }
    }
    public static void SetKeywords(string id, params string[] keywords)
    {
        lock (myLock)
        {
            rooms.setobj(id + "/keywords",keywords);
        }
    }
    public static void AddKeyword(string id, string keyword)
    {
        lock (myLock)
        {
            rooms.setobj(id + "/keywords", new string[]{keyword}.Concat(rooms.getObj<string[]>(id + "/keywords")).ToArray());
        }
    }
    public static void RemoveRoom(string id)
    {
        lock (myLock)
        {
            rooms.removeField(id);
        }
    }
    public static void Save()
    {
        lock (myLock)
        {
            SaveToPath(rooms_path, rooms);
        }
    }
    public static string ViewRooms()
    {
        return rooms.ToString();
    }
}

