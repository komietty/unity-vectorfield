Shader "VectorField/TangentSpaceViewer" {
    Properties { 
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha 
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _Lines;

            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 colour : TEXCOORD0;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                uint iid = ceil((vid % 6 + 1) * 0.5); 
                o.vertex = UnityObjectToClipPos(float4(_Lines[vid], 1));
                o.colour = float3(
                    iid == 1 ? 1 : 0,
                    iid == 2 ? 1 : 0,
                    iid == 3 ? 1 : 0);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return float4(i.colour, 1);
            }
            ENDCG
        }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define PI 3.14159265
            float4 _Color;
            float _T;
            StructuredBuffer<float3> _Lines;

            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 colour : TEXCOORD1;
            };

            v2f vert (uint vid: SV_VertexID) {
                v2f o;
                uint iid = vid / 6;
                uint uid = vid % 6;
                o.vertex = UnityObjectToClipPos(float4(_Lines[vid], 1));
                o.uv = float2(uid == 0 ? 1 : 0, (float)iid);
                o.colour = _Color; 
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                //float v = pow(sin(_T * 5 + i.uv.x * PI + i.uv.y) * 0.5 + 0.5, 2);
                //return float4(i.colour, 1 - pow(v * 0.8, 2));
                return float4(i.colour, 1);
            }
            ENDCG
        }
    }
}
