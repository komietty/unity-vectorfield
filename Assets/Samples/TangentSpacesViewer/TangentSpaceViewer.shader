Shader "VectorField/TangentSpaceViewer" {
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
            StructuredBuffer<float3> _Lines;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 colour : TEXCOORD0;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Lines[vid];
                uint iid = vid % 6; 
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                o.colour = iid < 2 ? float3(1, 0, 0) : (iid < 4 ? float3(0, 1, 0) : float3(0, 0, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return float4(i.colour, 1);
            }
            ENDCG
        }
    }
}
