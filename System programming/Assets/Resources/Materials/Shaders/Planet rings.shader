Shader "Custom/Planet rings"
{
	Properties
	{
		_DefaultTexture ("Default texture", 2D) = "white" {} // Текстура 1
		_Ring1("Ring 1", Vector) = (1,1,1,0.5)
		_Ring1Color("Ring 1 Color", COLOR) = (1,1,1,1)
		_Ring2("Ring 2", Vector) = (1,1,1,0.5)
		_Ring2Color("Ring 2 Color", COLOR) = (1,1,1,1)
		_Ring3("Ring 3", Vector) = (1,1,1,0.5)
		_Ring3Color("Ring 3 Color", COLOR) = (1,1,1,1)
		_Ring4("Ring 4", Vector) = (1,1,1,0.5)
		_Ring4Color("Ring 4 Color", COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"ForceNoShadowCasting" = "True"
        }
		
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _DefaultTexture;
			float4 _DefaultTexture_ST;
			
			float2 _Ring1;
			float2 _Ring2;
			float2 _Ring3;
			float2 _Ring4;
			
			float4 _Ring1Color;
			float4 _Ring2Color;
			float4 _Ring3Color;
			float4 _Ring4Color;
			
			float vectorRadiusSqrMagnitude(float2 vectorRadius)
			{
				return (0.5 - vectorRadius.x) * (0.5 - vectorRadius.x) + (0.5 - vectorRadius.y) * (0.5 - vectorRadius.y);
			}
			
			bool magnitudeWithinRange(float magnitude, float2 range)
			{
				float minRadius = (0.5 - range.x) * (0.5 - range.x);
				float maxRadius = (0.5 - range.y) * (0.5 - range.y);
				
				return (magnitude >= minRadius && magnitude <= maxRadius);
			}
			
			struct v2f
			{
				float2 uv : TEXCOORD0; // UV-координаты вершины
				float4 vertex : SV_POSITION; // координаты вершины
			};
			
			v2f vert(appdata_full v)
			{
				v2f result;
				
				result.uv		= TRANSFORM_TEX(v.texcoord, _DefaultTexture);
				result.vertex	= UnityObjectToClipPos(v.vertex);
				
				return result;
			}
			
			fixed4 frag(v2f vertexData) : SV_Target
			{
				fixed4 color = fixed4(0,0,0,0);
				
				fixed VSM = vectorRadiusSqrMagnitude(vertexData.uv);
				
				if(magnitudeWithinRange(VSM, _Ring1))
				{
					color = _Ring1Color;
				}
				else if(magnitudeWithinRange(VSM, _Ring2))
				{
					color = _Ring2Color;
				}
				else if(magnitudeWithinRange(VSM, _Ring3))
				{
					color = _Ring3Color;
				}
				else if(magnitudeWithinRange(VSM, _Ring4))
				{
					color = _Ring4Color;
				};
				
				return color;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
