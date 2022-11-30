Shader "Unlit/Tmp" {
    Properties { }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _Col;

            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color: COLOR;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                o.color = float4(0.5,0.5,0.5, 1);
                o.vertex = UnityObjectToClipPos(val.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
    }
}