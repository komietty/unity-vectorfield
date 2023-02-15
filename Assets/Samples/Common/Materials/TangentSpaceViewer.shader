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
            float4 _Color;
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
                o.vertex = UnityObjectToClipPos(float4(_Lines[vid], 1));
                o.colour = vid < 6 ? float4(1, 0, 0, 1) : _Color; 
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return float4(i.colour, 1);
            }
            ENDCG
        }
    }
}
