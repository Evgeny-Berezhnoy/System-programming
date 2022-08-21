Shader "Custom/Fog"
{
    Properties
    {
        _Altitude("Altitude", Range(0, 0.3)) = 5
        _Color("Color", COLOR) = (1,1,1,0.5)
        _Emission ("Emission", Range(0,0.5)) = 0
    }
    SubShader
    {
        Tags
        {   
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
        
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Altitude;
            fixed4 _Color;
            fixed _Emission;

            struct v2f
			{
				float4 vertex : SV_POSITION; // координаты вершины
			};

            v2f vert(appdata_full v)
            {
                v2f result;

                v.vertex.xyz += v.normal * _Altitude;

                result.vertex   = UnityObjectToClipPos(v.vertex);

                return result;
            }

            fixed4 frag(v2f vertexData) : SV_Target
            {
                fixed4 color;
                fixed4 emission;

                emission = (fixed4)_Emission * _Color.w;

                color = _Color + emission;

                return color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
