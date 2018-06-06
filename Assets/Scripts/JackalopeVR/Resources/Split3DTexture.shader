Shader "VOKE/Split 3D Texture"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_Offset ("Offset", Vector) = (0,0,1,1)
		_ImageSize("ImageSize", Vector) = (1024,1024,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZWrite Off // don't write to depth buffer in order not to occlude other objects

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// user defined variables
			sampler2D _MainTex;
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
				float2 tex_org : TEXCOORD1;
			};

			// vertex function
			vertexOutput vert (vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);				
				float2 uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.tex_org = uv * (_ImageSize.xy + 2) / _ImageSize.xy - (1 / _ImageSize.xy);
				output.tex = (output.tex_org * _Offset.zw) + _Offset.xy;
				return output;
			}

			// fragment function
			fixed4 frag (vertexOutput input) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, input.tex);
				float2 a = sign(0.5 - abs(0.5 - input.tex_org)) + 1;
				col *= saturate(a.x * a.y);
				return col;
			}
			ENDCG
		}
	}
}
