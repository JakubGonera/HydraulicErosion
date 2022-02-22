Shader "Unlit/TerrainWithVert"
{
    Properties
    {
        _GrassColour("Grass", Color) = (1,1,1,1)
        _RockColour("Rock", Color) = (1,1,1,1)
        _MaxSlope("Max slope", Range(0, 1)) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : TEXCOORD0;
            };

            fixed4 _GrassColour;
            fixed4 _RockColour;
            float _MaxSlope;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float slope = acos(saturate(i.worldNormal.y));
                slope /= (3.14 / 2);
                slope /= _MaxSlope;
                slope = saturate(slope);
                return lerp(_GrassColour, _RockColour, slope);
                //return _GrassColour;

                //// sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
            }
            ENDCG
        }
    }
}
