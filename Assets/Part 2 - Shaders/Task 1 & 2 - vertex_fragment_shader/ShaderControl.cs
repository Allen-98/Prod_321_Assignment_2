using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderControl : MonoBehaviour
{
    public Material mt;
    [Range(-0.02f, 0.15f)]
    public float inflationAmount;
    public Slider inflationSlider;
    public Button colorButton;

    bool rIsChanged = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mt.SetFloat("_InflationAmount", inflationAmount);
    }


    public void InflationAmountValueChange()
    {
        inflationAmount = inflationSlider.value;
    }

    public void MainColorChange()
    {
        if (!rIsChanged)
        {
            Color color = new Color(0, 0, 0);
            mt.SetColor("_Color", color);
            rIsChanged = true;
        }
        else
        {
            Color color = new Color(255, 0, 0);
            mt.SetColor("_Color", color);
            rIsChanged = false;
        }

    }


}
