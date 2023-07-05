using UnityEngine;
using WatermelonDataTool.Serializer;

public class search_result_manager : MonoBehaviour
{
    public GameObject room_btn_prefab;
    // Update is called once per frame
    public void ShowResults(Watermelon results)
    {
        clear();
        for (int i = 0; i < results.count; i++)
        {
            GameObject current = Instantiate(room_btn_prefab, transform);
            current.GetComponent<room_button>().room_name = results.getObj<string>(i+"/room_name");
            current.GetComponent<room_button>().room_id = results.getObj<string>(i + "/room_id");

        }
    }
    public void clear()
    {
        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
    }
}
