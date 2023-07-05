using UnityEngine;

public class blue_icon : MonoBehaviour
{
    private void Start()
    {
        transform.Rotate(transform.forward*Random.value*Random.value*360);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward*10);
    }
}
