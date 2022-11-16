Shader "Custom/Tmp"
{
    SubShader {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _MeshBuff;

            struct appdata {
                uint vrtId : SV_VertexID;
                uint idcId : SV_InstanceID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            float3 getVertexPos(appdata v) {
                return _MeshBuff[v.vrtId];
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