Shader "Custom/TerrainShader"
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
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        fixed4 _GrassColour;
        fixed4 _RockColour;
        float _MaxSlope;

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
            slope /= _MaxSlope;
            slope = saturate(slope);
            o.Albedo = lerp(_GrassColour, _RockColour, slope);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
