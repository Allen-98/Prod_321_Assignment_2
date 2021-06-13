using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeometryShaderControl : MonoBehaviour
{

    public Material mt;
    [Range(-0.001f, 0.05f)]
    public float furFactor;
    [Range(0, 0.1f)]
    public float outlineFactor;
    public Color color;
    public Slider furSlider;
    public Slider outlineSlider;
    public Slider R;
    public Slider G;
    public Slider B;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mt.SetFloat("_FurFactor", furFactor);
        mt.SetFloat("_OutlineFactor", outlineFactor);
        mt.SetColor("_OutlineColor", color);
    }


    public void ChangeFurFactor()
    {
        furFactor = furSlider.value;
    }

    public void ChangeOutlineFactor()
    {
        outlineFactor = outlineSlider.value;
    }

    public void ChangeOutlineColorR()
    {
        color.r = R.value;
    }

    public void ChangeOutlineColorG()
    {
        color.g = G.value;
    }

    public void ChangeOutlineColorB()
    {
        color.b = B.value;
    }


}
