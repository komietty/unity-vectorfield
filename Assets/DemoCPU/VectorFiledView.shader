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
            struct v2f { float4 vertex : SV_POSITION; };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Nrm[vid];
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return float4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}
