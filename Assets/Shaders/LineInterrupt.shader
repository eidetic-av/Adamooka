Shader "Unlit/LineInterrup"
{

	Properties
    {
        // material properties here
    }

    SubShader {

      Tags { "RenderType" = "Opaque" }

      CGPROGRAM

      #pragma surface surf Lambert

      struct Input {
          float4 color : COLOR;
      };

      void surf (Input input, inout SurfaceOutput output) {
          output.Albedo = 1;
      }

      ENDCG

    }

	
}
