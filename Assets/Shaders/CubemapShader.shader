// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "CubemapShader"  
{
   Properties 
   {
	  _Color ("Color", Color) = (1,1,1,1)
	  _MainTex ("Albedo (RGB)", 2D) = "white" {}	
      _Cube("Reflection Map", Cube) = "" {}
      _Opacity ("Opacity", Range(0,1)) = 1
      _RefIntensity ("Reflection Intensity", float) = 1
   }
   SubShader 
   {
   	  Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
   	  Blend SrcAlpha OneMinusSrcAlpha

      Pass 
      {   
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 

         #include "UnityCG.cginc"

         // User-specified uniforms
         sampler2D _MainTex;
         uniform samplerCUBE _Cube;   

         struct vertexInput 
         {
            float4 vertex 		: POSITION;
            float3 normal 		: NORMAL;
            float2 uv_MainTex   : TEXCOORD0;
         };

         struct vertexOutput 
         {

            float4 pos 			: POSITION;
            float2 uv_MainTex   : TEXCOORD0;
            float3 normalDir 	: TEXCOORD1;
            float3 viewDir 		: TEXCOORD2;
         };


		 half _RefIntensity;
		 half _Opacity;
		 fixed4 _Color;

         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;

            output.viewDir = mul(unity_ObjectToWorld, input.vertex).xyz - _WorldSpaceCameraPos;
            output.normalDir = normalize(mul(float4(input.normal, 0.0), unity_WorldToObject).xyz);
            output.pos = UnityObjectToClipPos(input.vertex);

            output.uv_MainTex = input.uv_MainTex;

            return output;
         }
 
         fixed4 frag(vertexOutput input) : COLOR
         {
         	float refractiveIndex = 1.5;

			fixed4 finalColor = fixed4(1,1,1,1);

			// Diffuse Texture
			finalColor = tex2D (_MainTex, input.uv_MainTex) * _Color;

			// Reflection
			float3 reflectedDir = reflect(input.viewDir, normalize(input.normalDir));
            finalColor.rgb *= texCUBE(_Cube, reflectedDir) * _RefIntensity;

            // Alpha Component.
            finalColor.a *= _Opacity;

            return finalColor;
         }
 
         ENDCG
      }
   }
}