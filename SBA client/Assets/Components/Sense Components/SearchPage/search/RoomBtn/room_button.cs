using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class room_button : MonoBehaviour
{
    public string room_id = null;
    public string room_name { set { GetComponentInChildren<TMP_Text>().text = value; } get { return GetComponentInChildren<TMP_Text>().text; } }

    public void OpenRoomPage()
    {
        RoomPage.instance.LoadRoom(room_id, room_name);
    }
}
