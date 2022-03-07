Shader "Custom/TerrainShader"
{
    Properties
    {
        //Colour properties for terrain colouring
        _GrassColour("Grass", Color) = (1,1,1,1)
        _RockColour("Rock", Color) = (1,1,1,1)
        _SnowColour("Snow colour", Color) = (1,1,1,1)
        //Max slope - used for tweaking the slope that counts as grass
        _MaxSlope("Max slope", Range(0, 1)) = 0.4
        //Height (in %) from which snow appears
        _SnowCap("Snow cap", Range(0, 1)) = 0.7
        
        _Size("Size of map", Int) = 256
        //Margin to cut off
        _Margin("Size of margin", Int) = 10
        //Height regulator
        _HeightMult("Height multiplier", Range(10, 300)) = 100
        //Comparison ratio - sourced from UI slider 
        _ComparisonRatio("Comparison ratio", Range(0, 1)) = 0
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

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        //Properties from the material
        fixed4 _GrassColour;
        fixed4 _RockColour;
        fixed4 _SnowColour;
        float _MaxSlope;
        float _SnowCap;
        int _Size;
        int _Margin;
        float _HeightMult;
        float _ComparisonRatio;

        //Vertex part
        #ifdef SHADER_API_D3D11
        //Heightmap
        StructuredBuffer<float> _Map;
        //Heightmap before erosion
        StructuredBuffer<float> _OriginalMap;

        //Get value from the map (accounting for the margin), if outside return default (def)
        float getVal(int x, int y, float def) {
            if (x >= 0 && x < _Size && y >= 0 && y < _Size) {
                return lerp(_Map[(y + _Margin) * (_Size + 2 * _Margin) + x + _Margin],
                    _OriginalMap[(y + _Margin) * (_Size + 2 * _Margin) + x + _Margin], _ComparisonRatio);
            }
            else {
                return def;
            }
        }
        #endif

        //Change the vertex data
        void vert(inout appdata_full v) {
            #ifdef SHADER_API_D3D11

            //Get coordinates of vertex and sample the height from the maps with linear interpolation between the two heightmaps
            int x = (int)v.vertex.x;
            int y = (int)v.vertex.z;
            float height = lerp(_Map[(y + _Margin) * (_Size + _Margin * 2) + x + _Margin], 
                _OriginalMap[(y + _Margin) * (_Size + _Margin * 2) + x + _Margin], _ComparisonRatio);
            v.vertex.xyz = float3(x, height * _HeightMult, y);
                
            //Sample the height map
            float fx0 = getVal(x - 1, y, height) * _HeightMult, fx1 = getVal(x + 1, y, height) * _HeightMult;
            float fy0 = getVal(x, y - 1, height) * _HeightMult, fy1 = getVal(x, y + 1, height) * _HeightMult;

            //Calculate the new normal
            float3 n = normalize(float3((fx0 - fx1) / 2, 1, (fy0 - fy1) / 2));

            v.normal = n;

            #endif
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        //Pixel part
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Calculate the slope and blend between grass and rock accordingly
            float slope = acos(saturate(IN.worldNormal.y));
            slope /= (3.14 / 2);
            slope /= _MaxSlope;
            slope = saturate(slope);
            fixed4 grass = lerp(_GrassColour, _RockColour, slope);
            //If passes snow threashold - blend with snow
            if (IN.worldPos.y >= _HeightMult * _SnowCap) {
                float ratio = (IN.worldPos.y - _HeightMult * _SnowCap) / (_HeightMult * (1 - _SnowCap) / 2);
                o.Albedo = lerp( grass, _SnowColour, saturate(ratio));
            }
            else {
                o.Albedo = grass;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
