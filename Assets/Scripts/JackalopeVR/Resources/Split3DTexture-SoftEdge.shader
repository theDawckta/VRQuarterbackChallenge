Shader "VOKE/Split 3D Texture-SoftEdge"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_AlphaMap ("Alpha Map Texture", 2D) = "black" {}
		_Offset ("Offset", Vector) = (0,0,1,1)
		_ImageSize("ImageSize", Vector) = (1024,1024,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		//Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100

		Pass
		{
			ZWrite Off // don't write to depth buffer in order not to occlude other objects

         	//Blend SrcAlpha OneMinusSrcAlpha // use alpha blending

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// user defined variables
			sampler2D _MainTex;
			sampler2D _AlphaMap;
			float4 _MainTex_ST;
			float4 _Offset;
			float4 _ImageSize;
			

			// base input structs
			struct vertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float2 tex : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			// vertex function
			vertexOutput vert (vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);				
				float2 uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.tex = (uv * _Offset.zw) + _Offset.xy;
				return output;
			}

			// fragment function
			fixed4 frag (vertexOutput input) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, input.tex);
				col.a = tex2D(_AlphaMap, input.tex).a;
				return col;
			}
			ENDCG
		}
	}
}
