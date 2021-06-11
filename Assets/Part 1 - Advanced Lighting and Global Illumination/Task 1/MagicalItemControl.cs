using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicalItemControl : MonoBehaviour
{
    public GameObject projector;
    public GameObject aimLine;
    public Material bumpMaterial;

    bool projectorIsActive = false;
    bool aimLineIsActive = false;
    bool changed = false;


    // Start is called before the first frame update
    void Start()
    {
        projector.SetActive(false);
        aimLine.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!projectorIsActive)
            {
                projector.SetActive(true);
                projectorIsActive = true;

                aimLine.SetActive(false);
                aimLineIsActive = false;
            }
            else
            {
                projector.SetActive(false);
                projectorIsActive = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!aimLineIsActive)
            {
                projector.SetActive(false);
                projectorIsActive = false;

                aimLine.SetActive(true);
                aimLineIsActive = true;
            }
            else
            {
                aimLine.SetActive(false);
                aimLineIsActive = false;
            }
        }


        if (projectorIsActive)
        {

            if (Input.GetKey(KeyCode.Mouse0))
            {
                projector.transform.Rotate(new Vector3(0, 0, 50));
            }

        }

        if (aimLineIsActive)
        {

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (bumpMaterial != null)
                {
                    if (!changed)
                    {
                        bumpMaterial.SetFloat("_BumpScale", 1.5f);
                        changed = true;
                    }
                    else
                    {
                        bumpMaterial.SetFloat("_BumpScale", 0f);
                        changed = false;
                    }
                }
            }


        }




    }

}
