using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyController : MonoBehaviour
{

    public float Speed = 200;
    public Text modeText; 

    private Animator anim;
	private bool isMoving = false;
    private bool superAttack = false;

    float translationX, translationZ;


    // Start is called before the first frame update
    void Start()
    {
        modeText.text = "Stand Mode";
        anim = GetComponent<Animator>();
        translationX = 0; translationZ = 0;
    }

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!isMoving)
            {
                anim.SetBool("isMoving", true);
                isMoving = true;
                modeText.text = "Normal Fight Mode";
            }
            else
            {
                anim.SetBool("isMoving", false);
                isMoving = false;
                modeText.text = "Stand Mode";
            }
        }

        translationX = Mathf.MoveTowards(translationX, Input.GetAxis("Horizontal"), Time.deltaTime * Speed);
        translationZ = Mathf.MoveTowards(translationZ, Input.GetAxis("Vertical"), Time.deltaTime * Speed);

        //if (translationX == 0 && translationZ == 0)
        //    isMoving = false;
        //else
        //    isMoving = true;

        //anim.SetBool("isMoving", isMoving);
        anim.SetFloat("Horizontal", translationX);
        anim.SetFloat("Vertical", translationZ);



        if (Input.GetKeyDown(KeyCode.Mouse0) && isMoving)
        {
            anim.SetTrigger("UpperAttack1");

        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && isMoving)
        {
            anim.SetTrigger("LowerAttack1");
        }

        if (Input.GetKeyDown(KeyCode.G) && isMoving)
        {
            anim.SetBool("SuperAttack", true);
            anim.SetTrigger("SuperAttackTriggle");
            superAttack = true;
            modeText.text = "Super Attack Mode";
        }

        if (superAttack && Input.GetKeyDown(KeyCode.F))
        {
            anim.SetTrigger("Combo1");
        }

        if (superAttack && Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("Combo2");
        }

        if (superAttack && Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("FinishAttack");
            superAttack = false;
            modeText.text = "Normal Fight Mode";
        }




    }
}
