Shader "VectorField/TangentFieldViewer" {
    Properties { }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha 
        Pass {
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define PI 3.14159265
            StructuredBuffer<float3> _Line;
            StructuredBuffer<float2> _Dist;
            float _T;
            float _C;
            int   _M;
            float4 _C0;
            float4 _C1;
            float4 _C2;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv:   TEXCOORD0;
                float2 dist: TEXCOORD1;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Line[vid];
                uint iid = vid / 6;
                uint uid = vid % 6; 
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.uv   = float2(uid == 0 ? 1 : 0, (float)iid);
                o.dist = _Dist[vid].xy;
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float  v = pow(sin(_T * 5 + i.uv.x * PI + i.uv.y) * 0.5 + 0.5, 2);
                float4 o = _C0;
                o = _M != 2 ? lerp(o, _C1, i.dist.x * _C) : o;
                o = _M != 1 ? lerp(o, _C2, i.dist.y * _C) : o;
                //o.w =  1 - v * 0.8;
                o.w =  1;
                return clamp(o, 0, 1);
            }
            ENDCG
        }
    }
}
