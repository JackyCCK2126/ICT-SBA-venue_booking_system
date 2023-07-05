using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomPage : MonoBehaviour
{
    public GameObject SearchPage, RoomPanel;
    public TMP_Text RoomName,RoomID;
    public void LoadRoom(string room_id, string room_name)
    {
        RoomPanel.SetActive(true);
        SearchPage.SetActive(false);
        //show room info
        RoomName.SetText(room_name);
        RoomID.SetText(room_id);
    }
    public void Close()
    {
        RoomPanel.SetActive(false);
        SearchPage.SetActive(true);
    }
    public static RoomPage instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
}
