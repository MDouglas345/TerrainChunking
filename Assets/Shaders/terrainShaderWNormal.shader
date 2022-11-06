Shader "Unlit/terrainShaderWNormal"
{
    Properties
    {
        _HeightMap ("Height Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "white" {}
        _HeightDisplacement("Height Displacement", Float) = 50
        _Texture ("Texture", 2D) = "white" {}
        _terrainsize("Terrain size", Float) = 0
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _Texture;
            sampler2D _HeightMap;
            sampler2D _NormalMap;
            float _terrainsize;
            float4 _MainTex_ST;
            float4 _NormalMap_ST;
            float4 _Texture_ST;
            float _HeightDisplacement;

            StructuredBuffer<float4> _ChunkData;

            float4 color;

            float3 filterNormal(float2 uv, float texelSize, int terrainSize)
            {
                float4 h;
                h[0] = tex2D(_HeightMap, uv + texelSize*float2(0,-1)).r * _HeightDisplacement;
                h[1] = tex2D(_HeightMap, uv + texelSize*float2(-1,0)).r * _HeightDisplacement;
                h[2] = tex2D(_HeightMap, uv + texelSize*float2(1,0)).r * _HeightDisplacement;
                h[3] = tex2D(_HeightMap, uv + texelSize*float2(0,1)).r * _HeightDisplacement;
    
                float3 n;
                n.z = -(h[0] - h[3]);
                n.x = (h[1] - h[2]);
                n.y = 2 * texelSize * _terrainsize; // pixel space -> uv space -> world space
    
                return normalize(n);
            }

             float3 filterNormalOnV(float2 uv, float texelSize, int terrainSize)
            {
                float4 h;
                h[0] = tex2Dlod(_HeightMap, float4(uv + texelSize*float2(0,-1),0,0)).r * _HeightDisplacement;
                h[1] = tex2Dlod(_HeightMap, float4(uv + texelSize*float2(-1,0),0,0)).r * _HeightDisplacement;
                h[2] = tex2Dlod(_HeightMap, float4(uv + texelSize*float2(1,0),0,0)).r * _HeightDisplacement;
                h[3] = tex2Dlod(_HeightMap, float4(uv + texelSize*float2(0,1),0,0)).r * _HeightDisplacement;
    
                float3 n;
                n.z = -(h[0] - h[3]);
                n.x = (h[1] - h[2]);
                n.y = 2 * texelSize * _terrainsize; // pixel space -> uv space -> world space
    
                return normalize(n);
            }


            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;
                float4 ChunkData = _ChunkData[instanceID];
                v.vertex = v.vertex + float4(ChunkData.x * 256,0,ChunkData.y * 256,0);
                v.uv = v.uv + ChunkData.zw;

                float4 tex = tex2Dlod (_HeightMap, float4(v.uv,0,0));
                v.vertex.y = tex.r * _HeightDisplacement;
                //o.normal = filterNormalOnV(v.uv, 0.000008, _terrainsize);
                
               

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
               // return float4(i.normal, 1);
                fixed4 col = tex2D(_Texture, i.uv);
                fixed4 normal = tex2D(_NormalMap, i.uv);
                return normal;
                return float4(filterNormal(i.uv,0.00048828125, _terrainsize),1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return half4(i.normal,1);
            }
            ENDCG
        }
    }
}
