Shader "Custom/Planet"
{
	Properties
	{
		_MainColor ("Main color", Color) = (1,1,1,1)
		_OceanColor ("Ocean color", Color) = (1,1,1,1)
		_TerrainColor ("Terrain color", Color) = (1,1,1,1)
		_MountainColor ("Mountain color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Emission("Emission", COLOR) = (1,1,1,1)
		_Height("Height", Range(0, 10)) = 0
		_Seed("Seed", Range(0, 10000)) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0
		
		sampler2D _MainTex;
		
		struct Input
		{
			float2 uv_MainTex;
			float4 color: COLOR;
		};
		
		fixed4 _MainColor;
		fixed4 _OceanColor;
		fixed4 _TerrainColor;
		fixed4 _MountainColor;
		float4 _Emission;
		float _Height;
		float _Seed;
		
		float hash(float2 st)
		{
			return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
		}
		
		float2 noise(float2 p, float size)
		{
			float result = 0;
			
			p*= size;
			float2 i = floor(p + _Seed);
			float2 f = frac(p + _Seed / 739);
			float2 e = float2(0, 1);
			
			float z0 = hash((i + e.xx) % size);
			float z1 = hash((i + e.yx) % size);
			float z2 = hash((i + e.xy) % size);
			float z3 = hash((i + e.yy) % size);
			float2 u = smoothstep(0, 1, f);
			
			result = lerp(z0, z1, u.x) + (z2 - z0) * u.y * (1 - u.x) + (z3 - z1) * u.x * u.y;
			
			return result;
		}
		
		void vert(inout appdata_full v)
		{
			float height = noise(v.texcoord, 5) * 0.75 + noise(v.texcoord, 30) * 0.125 + noise(v.texcoord, 50) * 0.125;
			
			v.color.r = height + _Height;
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 color = tex2D (_MainTex, IN.uv_MainTex);
			
			float height = IN.color.r;
			if(height < 0.45)
			{
				color = _OceanColor * _MainColor;
			}
			else if(height < 0.75)
			{
				color = _TerrainColor * _MainColor;
			}
			else
			{
				color = _MountainColor * _MainColor;
			};
			
			o.Albedo = color.rgb;
			o.Alpha = color.a;
			o.Emission = _Emission.xyz;
		}
		
		ENDCG
	}
	
	FallBack "Diffuse"
}
