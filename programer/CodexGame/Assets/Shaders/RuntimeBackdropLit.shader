Shader "CodexGame/RuntimeBackdropLit"
{
  Properties
  {
    _MainTex ("Backdrop", 2D) = "white" {}
    _Color ("Tint", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }
    LOD 150

    CGPROGRAM
    #pragma surface surf Lambert
    #pragma target 2.0

    sampler2D _MainTex;
    fixed4 _Color;

    struct Input
    {
      float2 uv_MainTex;
    };

    void surf(Input input, inout SurfaceOutput output)
    {
      fixed4 sampled = tex2D(_MainTex, input.uv_MainTex) * _Color;
      output.Albedo = sampled.rgb;
      output.Alpha = 1;
    }
    ENDCG
  }

  Fallback "Unlit/Texture"
}
