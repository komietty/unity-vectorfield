Shader "VectorField/SimpleLines" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
     }
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
            float4 _Color;

            struct appdata { float4 vertex : POSITION; };
            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert (uint vid: SV_VertexID, appdata val) {
                v2f o;
                float3 p = _Line[vid];
                o.vertex = UnityObjectToClipPos(float4(p, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}
