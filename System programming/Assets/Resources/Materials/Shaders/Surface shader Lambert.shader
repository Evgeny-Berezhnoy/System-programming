Shader "Custom/Surface shader Lambert"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Emission("Emission", COLOR) = (1,1,1,1)
		_Height("Height", Range(0, 10)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd noshadow
		#pragma target 3.0
		
		sampler2D _MainTex;
		
		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
		};
		
		fixed4 _Color;
		float4 _Emission;
		float _Height;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Emission = _Emission.xyz;
		}
		
		ENDCG
	}
	
	FallBack "Diffuse"
}
