Shader "Custom/TerrainShader"
{
    Properties
    {
        _GrassColour("Grass", Color) = (1,1,1,1)
        _RockColour("Rock", Color) = (1,1,1,1)
        _MaxSlope("Max slope", Range(0, 1)) = 0.4
        _Height("Height", Range(1, 50)) = 10
        _Size("Size of map", Int) = 256
        _HeightMult("Height multiplier", Range(10, 300)) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

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
        fixed4 _Debug;
        float _MaxSlope;
        int _Size;
        float _HeightMult;

        #ifdef SHADER_API_D3D11
            StructuredBuffer<float> _Map;
        
        float getVal(int x, int y, float def) {
            if (x >= 0 && x < _Size && y >= 0 && y < _Size) {
                return _Map[y * _Size + x];
            }
            else {
                return def;
            }
        }
        #endif

        void vert(inout appdata_full v) {
            #ifdef SHADER_API_D3D11
            int x = (int)v.vertex.x;
            int y = (int)v.vertex.z;
            float height = _Map[(int)v.vertex.z * _Size + (int)v.vertex.x];
            v.vertex.xyz = float3(v.vertex.x, height * _HeightMult, v.vertex.z);
                
            // sample the height map:
            float fx0 = getVal(x - 1, y, height) * _HeightMult, fx1 = getVal(x + 1, y, height) * _HeightMult;
            float fy0 = getVal(x, y - 1, height) * _HeightMult, fy1 = getVal(x, y + 1, height) * _HeightMult;

            // the spacing of the grid in same units as the height map
            float eps = 1.0;

            // plug into the formulae above:
            float3 n = normalize(float3((fx0 - fx1) / (2 * eps), 1, (fy0 - fy1) / (2 * eps)));

            v.normal = n;

            #endif
        }

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
