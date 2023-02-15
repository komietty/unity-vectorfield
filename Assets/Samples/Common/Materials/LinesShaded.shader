Shader "VectorField/LinesShaded" {
    Properties {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
    }
    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry"
        }
        Pass {
            Tags {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            StructuredBuffer<float3> _Line;
            StructuredBuffer<float3> _Norm;
            StructuredBuffer<float3> _Col;

            // Universal Render Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // Unity keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 vertexColor : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirWS : TEXCOORD5;
                half fogFactor : TEXCOORD6;
                half3 vertexLight   : TEXCOORD7;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord : TEXCOORD8;
                #endif
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 9);
            };

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float4 _NormalMap_ST;
            float _NormalScale;
            float _Metallic;
            float _Smoothness;
            CBUFFER_END

            Varyings vert(uint vid: SV_VertexID, Attributes input) {
                Varyings output;
                
                output.vertexColor = _Col[vid];
                output.positionWS = _Line[vid];
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                output.normalWS = _Norm[vid];
                output.tangentWS = TransformObjectToWorldDir(input.tangent.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangent.w;
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                output.fogFactor = ComputeFogFactor(output.positionHCS.z);
                output.vertexLight = VertexLighting(output.positionWS, output.normalWS);
                
                // Shadow
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                #endif
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                // SurfaceDataを作成
                SurfaceData surfaceData;
                surfaceData.normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, float2(0, 0)));
                half4 col = half4(input.vertexColor, 1);
                surfaceData.albedo = col.rgb;
                surfaceData.alpha = col.a;
                surfaceData.emission = 0.0;                
                surfaceData.metallic = _Metallic;
                surfaceData.occlusion = 1.0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.specular = 0.0;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                // InputDataを作成
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = TransformTangentToWorld(surfaceData.normalTS, half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = SafeNormalize(input.viewDirWS);
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = input.vertexLight;
                inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionHCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

                // PBRのライティング計算
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);

                // if the color doesn't show up, try toggle wire/shaded on the scene view
                return color;
            }
            ENDHLSL
        }
    } 
}
