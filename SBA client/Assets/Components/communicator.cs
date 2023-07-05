using UnityEngine;
using Net;
using WatermelonDataTool.Serializer;
using System;
using System.Collections;

public class communicator : MonoBehaviour
{
    private Client client = new Client();
    // Start is called before the first frame update
    public bool debug;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        client.setUp(25525, "127.0.0.1");
    }

    int next_action_id = 100;
    //<returns> decoded reply </returns>
    private object myLock = new object();
    public void Communicate (string request, Action<Watermelon> callback, Action<string> OnError = null, Watermelon param_s = null, bool write_auth_info = true)
    {
        StartCoroutine(communicate(request, callback, OnError, param_s, write_auth_info));
    }

    IEnumerator communicate(string request, Action<Watermelon> callback, Action<string> OnError = null, Watermelon param_s = null, bool write_auth_info = true)
    {
        lock (myLock)
        {
            //LoadingIconManager.instance.set_loading_icon(true, 0);
            int action_id = next_action_id;
            next_action_id++;
            try
            {
                Watermelon msg = new();
                msg.setobj("request_id", action_id);
                if (write_auth_info)
                {
                    msg.setobj("username", auth.instance.username);
                    msg.setobj("key", auth.instance.session_id);
                }
                msg.setobj("request", request);
                if (param_s != null)
                {
                    foreach (Melon p in param_s)
                    {
                        msg.setobj(p.FieldName, p.obj);
                    }
                }
                client.Connect();
                if (debug) print("send: " + msg);
                client.SendBytes(msg.ToBytes());
                byte[] b = client.ReceiveBytes();
                if (b != null)
                {
                    msg.ReloadFromBytes(b);
                    if (debug) print("reply: " + msg);
                    if (msg.getObj<int>("re_id") == action_id)
                    {
                        callback(msg);

                    }
                    else OnError?.Invoke("wrong reply! (next to impossible on properly run)");
                }
                else
                {
                    OnError?.Invoke("error on communicate: network problem!");
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
            //LoadingIconManager.instance.set_loading_icon(false, 0);
            client.DisconnectAndRenew();
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;
    }

    public static communicator instance;
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
