Shader "GPU/Asteroids"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Smoothness("Smoothness", Range(0,1)) = 0.5
		_Texture("Texture", 2D) = "white"{}
	}
	SubShader
	{
		CGPROGRAM
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		
		#pragma target 4.5
		
		#include "FractalGPU.hlsl"
		
		float4 _BaseColor;
		float _Smoothness;
		
		sampler2D _Texture;
		
		struct Input
		{
			float2 uv_Texture;
			float3 worldPos;
		};
		
		void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
		{
			fixed4 c = tex2D(_Texture, input.uv_Texture);
			
			surface.Albedo = _BaseColor.rgb;
			surface.Albedo *= c.rgb;
			surface.Smoothness = _Smoothness;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}