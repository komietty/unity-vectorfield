Shader "Unlit/VectorFiledView" {
    Properties { }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _Nrm;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Nrm[vid];
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.color = vid % 2 == 0? (1).xxxx : float4(1, 0, 0, 1);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _Tng;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Tng[vid];
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.color = vid % 2 == 0? (1).xxxx : float4(1, 0, 0, 1);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
    }
}
