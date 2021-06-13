using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorControl : MonoBehaviour
{

    public GameObject projector;
    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        projector.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!isActive)
            {
                projector.SetActive(true);
                isActive = true;
            } else
            {
                projector.SetActive(false);
                isActive = false;
            }
        }


        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (isActive)
            {
                projector.transform.Rotate(new Vector3(0, 0, 50));
            }
        }





    }

}
