Shader "Custom/TestSinglePassRendering_DrawProceduralBase"
{
    SubShader {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _VrtsBuff;
            StructuredBuffer<float3> _NrmsBuff;

            struct appdata {
                uint vrtId : SV_VertexID;
                uint idcId : SV_InstanceID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            float3 getVertexPos(appdata v) {
                //float3 camPos = _WorldSpaceCameraPos;
                //float3 toCamDir = normalize(camPos - pos);
                //float3 sideDir = normalize(cross(toCamDir, dir));

                float3 ret = (0).xxx;
                switch(v.vrtId) {
                    case 0: ret = float3(-0.01, -0.01, 0.0); break;
                    case 1: ret = float3( 0.01, -0.01, 0.0); break;
                    case 2: ret = float3( 0.01,  0.01, 0.0); break;
                    case 3: ret = float3(-0.01,  0.01, 0.0); break;
                }
                float3 offset = _VrtsBuff[v.idcId];
                //uint pi = (v.idcId) * 12 * 4;
                //float3 offset = asfloat(_MeshBuff.Load3(pi));
                return ret + offset;
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(getVertexPos(v));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return fixed4(1,0,0,1);
            }
            ENDCG
        }
    }
}