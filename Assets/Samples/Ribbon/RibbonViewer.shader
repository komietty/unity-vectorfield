Shader "VectorField/RibbonViewer" {
    Properties { }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            StructuredBuffer<float3> _Line;
            StructuredBuffer<float3> _Color;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float4 colour : TEXCOORD0;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(_Line[vid], 1));
                o.colour = float4(_Color[vid], 1);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return clamp(i.colour, 0, 1);
            }
            ENDCG
        }
    }
}
