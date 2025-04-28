Shader "Custom/UVShaderTexture_Corrected"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (1, 0, 1, 1)
        _UVIntensity ("UV Light Intensity", Range(0, 5)) = 0
        _HitPosition ("Hit Position", Vector) = (0,0,0,0)
        _RevealRadius ("Reveal Radius", Float) = 0.5
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _EmissionColor;
            float _UVIntensity;
            float3 _HitPosition;
            float _RevealRadius;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Si le pixel de base est totalement transparent, on le discard
                if (texColor.a <= 0.01)
                    discard;

                // Appliquer la révélation
                float intensity = saturate(_UVIntensity);
                float distanceFromHit = distance(i.worldPos, _HitPosition);
                float reveal = saturate(1.0 - (distanceFromHit / _RevealRadius));

                if (intensity <= 0.01 || reveal <= 0.01)
                    discard;

                texColor.rgb *= reveal;
                texColor.rgb += _EmissionColor.rgb * intensity * reveal;
                texColor.a *= reveal;

                return texColor;
            }
            ENDCG
        }
    }
}
