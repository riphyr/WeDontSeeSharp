Shader "Custom/UVShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1,0,1,1)
        _UVIntensity ("UV Light Intensity", Range(0,5)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            fixed4 _EmissionColor;
            float _UVIntensity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float intensity = saturate(_UVIntensity);

                // Objet invisible par défaut
                if (intensity <= 0.01)
                    discard;

                // Affichage progressif sous UV
                fixed4 col = _Color;
                col.rgb += _EmissionColor.rgb * intensity;
                col.a = intensity;

                return col;
            }
            ENDCG
        }
    }
}
