Shader "Intel/Stereo Fading Edge"
{
	Properties
	{
		_MainTex ("Texture Image", 2D) = "black" {}
		_AlphaMap ("Alpha Map Image", 2D) = "black" {}
	}

	SubShader
	{
		// For more explanation about blending, refer to https://en.wikibooks.org/wiki/Cg_Programming/Unity/Transparency
		Tags { "Queue" = "Transparent" } 

		Pass
		{
			ZWrite Off // don't write to depth buffer in order not to occlude other objects

         	Blend SrcAlpha OneMinusSrcAlpha // use alpha blending

			CGPROGRAM

			// pragmas
			#pragma vertex vert
			#pragma fragment frag
			//#pragma nofog

			#include "UnityCG.cginc"

			// user defined variables
			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaMap;
			float4 _MainTex_ST;

			// base input structs
			struct vertexInput 
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				half2 tex : TEXCOORD0;
			};

			// vertex function
			vertexOutput vert(vertexInput input) 
			{
				vertexOutput output;

				output.pos = UnityObjectToClipPos(input.vertex);
				output.tex = TRANSFORM_TEX(input.texcoord, _MainTex);

				// Compute the offset value according to the eye index (0: Left Eye, 1: Right Eye) 
				output.tex.y = 0.5*(output.tex.y + 1 - unity_StereoEyeIndex);

				return output;
			}

			// fragment function
			fixed4 frag(vertexOutput input) : COLOR
			{
				fixed4 col = tex2D(_MainTex, input.tex);
				col.a = tex2D(_AlphaMap, input.tex).a;

				return col;
			}

			ENDCG
		}
	}

	// fallback commented out during development
	Fallback "Unlit/Texture"
}
