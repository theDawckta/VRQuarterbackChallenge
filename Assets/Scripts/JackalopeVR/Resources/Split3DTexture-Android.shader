Shader "VOKE/Split 3D Texture-OEM"
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
		Lighting Off
		Cull Off

		Pass
		{
				ZWrite Off // don't write to depth buffer in order not to occlude other objects

			GLSLPROGRAM

			#pragma only_renderers gles gles3
			#extension GL_OES_EGL_image_external : enable
			#extension GL_OES_EGL_image_external_essl3 : enable
			
			precision mediump float;

			uniform samplerExternalOES _MainTex;
			uniform vec4 _MainTex_ST;
			uniform vec4 _Offset;
			uniform vec4 _ImageSize;

			#ifdef VERTEX

			#include "UnityCG.glslinc"
		
			varying vec2 texVal;

			void main()
			{
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

				texVal = gl_MultiTexCoord0.xy;

				// Flip the image vertically
				texVal.y = 1.0 - texVal.y;

				// Apply the scale and offset values;
				//texVal = texVal.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				texVal = texVal.xy * _Offset.zw + _Offset.xy;
            }
            #endif  

			#ifdef FRAGMENT

			varying vec2 texVal;

            void main()
            {          
#if defined(SHADER_API_GLES) | defined(SHADER_API_GLES3)
				vec4 pixel_color = texture2D(_MainTex, texVal.xy);
				pixel_color.w = 1.0;

				gl_FragColor = pixel_color;
#else
				gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
#endif
			}
            #endif       
				
			ENDGLSL

		}
	}
}
