Shader "Unlit/terrainShader"
{
    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    StructuredBuffer<float4> _ChunkData;
    sampler2D _Texture;
    sampler2D _HeightMap;
    float _HeightDisplacement;
    ENDCG
    Properties
    {
        _HeightMap ("Height Map", 2D) = "white" {}
        _HeightDisplacement("Height Displacement", Float) = 50
        _Texture ("Texture", 2D) = "white" {}
        _terrainsize("Terrain size", Float) = 0
    }

    
    SubShader
    {
        

        Pass
        {
            Tags {"LightMode"="ForwardBase" "DisableBatching"="True"}

            CGPROGRAM
           
            // make fog work
            #pragma multi_compile_fog
          
            #pragma multi_compile_fwdbase

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #include "AutoLight.cginc"
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 ouv : TEXCOORD1;
                float4 _ShadowCoord : TEXCOORD2;

                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                fixed4 diff : COLOR0;
                fixed4 ambient : COLOR1;

                
            };

            
     
            float _terrainsize;
            float4 _MainTex_ST;
            float4 _Texture_ST;
            

            

            float4 color;

            float _texelSize;

            

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
                n.y = 1.15 * texelSize * _terrainsize; // pixel space -> uv space -> world space
    
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
                o.ouv = v.uv * 16;
                v.uv = v.uv + ChunkData.zw;
                

                float4 tex = tex2Dlod (_HeightMap, float4(v.uv,0,0));
                v.vertex.y = tex.r * _HeightDisplacement;
                //o.normal = filterNormalOnV(v.uv,_texelSize, _terrainsize);
                
               

                //o.ambient =ShadeSH9(half3(worldNormal1));
                

                
               

                o.pos =mul(UNITY_MATRIX_VP, v.vertex);
                o._ShadowCoord = ComputeScreenPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture);

                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
               // return float4(i.normal, 1);
                fixed4 col = tex2D(_Texture, i.uv);
                fixed4 col2 = tex2D(_Texture, i.ouv);

                float3 normal = normalize(filterNormal(i.uv,_texelSize, _terrainsize));

                float3 worldNormal = UnityObjectToWorldNormal(normal);

                float3 cameraforward = normalize(mul((float3x3)unity_CameraToWorld, float3(0,0,1)));

                float3 cfReflected = reflect(-cameraforward, normal);


                float spec = max(0,dot(_WorldSpaceLightPos0.xyz, cfReflected) * _LightColor0 * 0.5f);

                float nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                float diff = nl * _LightColor0;



                float attenuation = SHADOW_ATTENUATION(i);
                //float4 ambient = (col * 0.7 + col2 * 0.3);
                float ambient = _LightColor0 * 0.4f;

                // calculated as  1 / (heightmap length * 256)
                //return (float4(filterNormal(i.uv,_texelSize, _terrainsize),1) * 0.5 + col * 0.1f) * (i.diff * 0.9)  * attenuation  + ambient ;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                 //fixed shadow = SHADOW_ATTENUATION(i);
                
                return (ambient + (diff + spec) *(attenuation)) * (col * 0.7 + col2 * 0.3)   ;
            }
            ENDCG
        }

         Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }                 
            CGPROGRAM
                                                       
            void vert (inout float4 vertex:POSITION,inout float2 uv:TEXCOORD0,uint i:SV_InstanceID)
            {              
                float4 ChunkData = _ChunkData[i];
                vertex = vertex + float4(ChunkData.x * 256,0,ChunkData.y * 256,0);
                float2 iuv = uv* 5;
                iuv = uv + ChunkData.zw;

                float4 tex = tex2Dlod (_HeightMap, float4(iuv,0,0));
                vertex.y = tex.r * _HeightDisplacement;

                vertex = mul(UNITY_MATRIX_VP, vertex);
                
               
            }
           
            float4 frag (float4 vertex:POSITION, float2 uv:TEXCOORD0) : SV_Target
            {
                float4 color = tex2D( _Texture,uv);              
                return 0;
            }
            ENDCG
        }          

       
    }
}
