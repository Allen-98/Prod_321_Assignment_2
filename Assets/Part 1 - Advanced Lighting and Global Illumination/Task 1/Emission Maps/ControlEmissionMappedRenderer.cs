using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlEmissionMappedRenderer : MonoBehaviour
{
    private RawImage img;

    private float m_Time = 1f;

    public Text text;

    public ParticleSystem particle;

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {

        m_Time -= Time.deltaTime;
        text.text = "Next Color: " + m_Time.ToString();
        
        if(m_Time <= 0)
        {
            ChangeColor();
            particle.Play();
            m_Time = 1f;
        }

    }


    public void ChangeColor()
    {
        //img.color = new Color(1, 0, 0,(int)Random.Range(0, 255));
        img.color = Random.ColorHSV();
        
    }


}
