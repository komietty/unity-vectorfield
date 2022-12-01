Shader "ddg/Quads" {
    Properties { }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass {
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define PI 3.14159265
            StructuredBuffer<float3> _Line;
            float _T;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 uvw: TEXCOORD0;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Line[vid];
                uint iid = vid / 4;
                uint uvid = vid % 4;
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.uvw = float3((uvid == 0 || uvid == 3) ? 1 : 0, uvid < 2 ? 1 : 0, (float)iid);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float  v = pow(sin(_T * 5 + i.uvw.x * PI + i.uvw.z) * 0.5 + 0.5, 2);
                return float4(v, 0.6 + v * 0.4, 1, 1);
            }
            ENDCG
        }
    }
}
