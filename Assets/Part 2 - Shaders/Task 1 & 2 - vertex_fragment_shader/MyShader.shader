Shader "Custom/MyShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color ("Main color", Color) = (255, 255, 255, 255)
        _InflationAmount("Inflation Amount", Range(-0.02, 0.15)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct VertexIn
            {
                float4 objectSpacePosition : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct VertexToFragment
            {
                float2 uv : TEXCOORD0;
                float4 clipSpacePosition : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _InflationAmount;
            fixed4 _Color;

            VertexToFragment vert (VertexIn v)
            {
                VertexToFragment o;

                float3 objectSpaceVertexPosition = v.objectSpacePosition;
                objectSpaceVertexPosition += v.normal * _InflationAmount;
                o.clipSpacePosition = UnityObjectToClipPos(objectSpaceVertexPosition);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(VertexToFragment i) : SV_Target
            {

                fixed4 color = tex2D(_MainTex, i.uv);
                
                color.r = _Color;
                //color.b = _Color;
                
                return color;
            }
            ENDCG
        }
    }
}