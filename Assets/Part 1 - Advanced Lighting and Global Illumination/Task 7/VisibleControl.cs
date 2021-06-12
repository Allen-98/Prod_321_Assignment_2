using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisibleControl : MonoBehaviour
{

    public Material mt;
    [Range(0, 1)]
    public float alpha;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        mt.SetFloat("_Metallic", alpha);
    }

    public void ChangeValue()
    {
        alpha = slider.value;

    }

}
