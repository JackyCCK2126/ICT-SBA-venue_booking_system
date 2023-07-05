using UnityEngine;

public class LoadingIconManager : MonoBehaviour
{
    public GameObject[] icons_prefabs;
    private GameObject[] icons;
    GameObject canvas()
    {
      return GameObject.Find("Canvas");
    }
    public void set_loading_icon(bool active , int icon_index){
        
        if (active)
        {
            if (icons[icon_index] == null)
                icons[icon_index] = Instantiate(icons_prefabs[icon_index],canvas().transform);
            //print("the icon is now: " + icons[icon_index].ToString());
        }
        else
        {
            Destroy(icons[icon_index]);
            //print("the icon is now: " + icons[icon_index].ToString());
        } 
    }

    public static LoadingIconManager instance;
    void Awake()
    {
        icons = new GameObject[icons_prefabs.Length];
        if (instance==null)instance=this;else if(instance!=this)Debug.Log("Instance already exists, destroying object!");}
}
