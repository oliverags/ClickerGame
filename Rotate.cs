using UnityEngine;

public class Rotate : MonoBehaviour
{
   [SerializeField] float speed = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0,0,1) * Time.deltaTime * speed);
                        
    }
}
