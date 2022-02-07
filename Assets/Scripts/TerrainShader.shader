Shader "Custom/TerrainShader"
{
    Properties
    {
        _GrassColour("Grass", Color) = (1,1,1,1)
        _HalfGrassColour("Half grass", Color) = (1,1,1,1)
        _RockColour("Rock", Color) = (1,1,1,1)
        _FirstThreashold("First threashold", Range(0, 1)) = 0.3
        _SecondThreashold("Second threashold", Range(0, 1)) = 0.7
        _BlendRange("Blend range", Range(0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _HalfGrassColour;
        fixed4 _GrassColour;
        fixed4 _RockColour;
        float _FirstThreashold;
        float _SecondThreashold;
        float _BlendRange;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float slope = acos(saturate(IN.worldNormal.y));
            slope /= (3.14 / 2);
            slope /= 0.4;
            slope = saturate(slope);
            /*if (slope < _FirstThreashold) {
                if (slope > _FirstThreashold - _BlendRange) {
                    o.Albedo = lerp(_GrassColour, _HalfGrassColour, (slope - (_FirstThreashold - _BlendRange))/_BlendRange);
                }
                else {
                    o.Albedo = _GrassColour;
                }
            }
            else if (slope < _SecondThreashold) {
                if (slope > _SecondThreashold - _BlendRange) {
                    o.Albedo = lerp(_HalfGrassColour, _RockColour, (slope - (_SecondThreashold - _BlendRange)) / _BlendRange);
                }
                else {
                    o.Albedo = _HalfGrassColour;
                }
            }
            else {
                o.Albedo = _RockColour;
            }*/
            o.Albedo = lerp(_GrassColour, _RockColour, slope);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
