using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using WatermelonDataTool.Serializer;

public class search : MonoBehaviour
{
    public search_result_manager Search_Result_Manager;
    public TMPro.TMP_InputField Search_Field;

    private bool SearchRequested = false;
    public float searchingCD = 1;
    private float lastSearchingTime = 0;
    private bool isSearching = false;

    public void requestSearch()
    {
        SearchRequested = true;
    }

    private bool CD_OK
    {
        get { return (Time.timeSinceLevelLoad - lastSearchingTime) >= searchingCD; }
    }
    private void Update()
    {
        if (SearchRequested)
        {
            if(!isSearching && CD_OK)
            {
                SearchRequested = false;
                lastSearchingTime = Time.timeSinceLevelLoad;
                doSearch();
            }
        }
    }

    public void doSearch()
    {
        print("start seachring");
        isSearching = true;
        Search_Result_Manager.clear();
        Watermelon p = new Watermelon();
        p.setobj("search_string", Search_Field.text);
        try
        {
            communicator.instance.Communicate("search_room", OnReceive, OnError, p);
        }
        catch (Exception) { }
     }
    private void OnReceive(Watermelon msg)
    {
        isSearching = false;
        Search_Result_Manager.ShowResults(msg.getObj<Watermelon>("result"));
    }
    private void OnError(string e)
    {
        isSearching = false;
        print("Error on search: " + e);
    }
}
