using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyController : MonoBehaviour
{



    private Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyDown(KeyCode.T))
		{
			anim.SetFloat("Vertical", 0.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);
		}
		//Run forward
		if (Input.GetKeyDown(KeyCode.W))
		{
			anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);
		}
		//Run 45 up right
		if (Input.GetKeyDown(KeyCode.E))
		{
			anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
		}
		//Run strafe right
		if (Input.GetKeyDown(KeyCode.D))
		{
			anim.SetFloat("Vertical", 0.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
		}
		//Run 45 back right
		if (Input.GetKeyDown(KeyCode.X))
		{
			anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
		}
		//Run backwards
		if (Input.GetKeyDown(KeyCode.S))
		{
			anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);
		}
		//Run 45 back left
		if (Input.GetKeyDown(KeyCode.Z))
		{
			anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);
		}
		//Run strafe left
		if (Input.GetKeyDown(KeyCode.A))
		{
			anim.SetFloat("Vertical", 0.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);
		}
		//Run 45 up left
		if (Input.GetKeyDown(KeyCode.Q))
		{
			anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
			anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);


		}

	}
}
