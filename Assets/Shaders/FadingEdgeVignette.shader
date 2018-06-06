Shader "VOKE/Fading Edge Vignette"
 {
 Properties {
     _MainTex ("Base (RGB) Trans (A)", 2D) = "black" {}
     _AlphaMap ("Additional Alpha Map (Greyscale)", 2D) = "black" {}
 }
 
 SubShader {
     Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
     LOD 200
 
 CGPROGRAM
 #pragma surface surf Lambert alpha
 
 sampler2D _MainTex;
 sampler2D _AlphaMap;
 float4 _Color;
 
 struct Input {
     float2 uv_MainTex;
 };
 
 void surf (Input IN, inout SurfaceOutput o) {
     half4 c = tex2D(_MainTex, IN.uv_MainTex);
     o.Emission = c.rgb;
     o.Alpha = tex2D(_AlphaMap, IN.uv_MainTex).a;
 }
 ENDCG
 }
 
 Fallback "Transparent/VertexLit"
 }
