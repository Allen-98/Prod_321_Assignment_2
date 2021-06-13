using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MySurfaceShaderControl : MonoBehaviour
{
    public Material mt;

    [Range(0, 1)]
    public float smooth;
    [Range(0, 1)]
    public float metallic;

    public Slider smoothSlider;
    public Slider metallicSlider;
    public Texture texture;

    private Texture originTexture;
    private Texture secondaryTexture;
    bool isChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        mt.SetTexture("_MainTex", texture);
        originTexture = texture;
        secondaryTexture = mt.GetTexture("_SecondaryTex");
    }

    // Update is called once per frame
    void Update()
    {
        mt.SetFloat("_Glossiness", smooth);
        mt.SetFloat("_Metallic", metallic);
    }


    public void SmoothChange()
    {
        smooth = smoothSlider.value;
    }

    public void MetallicChange()
    {
        metallic = metallicSlider.value;
    }

    public void ChangeTexture()
    {
        if (!isChanged)
        {

            mt.SetTexture("_MainTex", secondaryTexture);
            isChanged = true;
        }
        else
        {
            mt.SetTexture("_MainTex", originTexture);
            isChanged = false;
        }
    }



}
