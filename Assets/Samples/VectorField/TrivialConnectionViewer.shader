Shader "ddg/TangentFieldViewer" {
    Properties { }
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
            #define PI 3.14159265
            StructuredBuffer<float3> _Line;
            float _C;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv:   TEXCOORD0;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Line[vid];
                uint iid = vid / 6;
                uint uid = vid % 6; 
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.uv   = float2(uid == 0 ? 1 : 0, (float)iid);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float4 o = float4(_C, 0, 0, 1);
                return clamp(o, 0, 1);
            }
            ENDCG
        }
    }
}
