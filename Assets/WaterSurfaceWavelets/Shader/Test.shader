Shader "Ocean/Ocean"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _PositionMap("Position Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess("Shininess", Range(0.1, 256)) = 32
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert
            #pragma target 4.0

            sampler2D _MainTex;
            sampler2D _PositionMap;
            sampler2D _NormalMap;
            fixed4 _SpecularColor;
            float _Shininess;

            struct Input
            {
                float2 uv_MainTex;
            };

            void vert(inout appdata_full v) 
            {
                float3 worldPos = tex2Dlod(_PositionMap, v.texcoord).xyz;
                v.vertex.xyz += mul(unity_WorldToObject, worldPos);
                //v.vertex += float4(worldPos,1.0);
                //v.pos = UnityObjectToClipPos(v.vertex);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                // Sample the position and normal maps
                
                float3 worldNormal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));

                // Transform the position and normal to object space
                //float4 localPos = mul(unity_ObjectToWorld, float4(worldPos, 1.0));
                float3 localNormal = normalize(mul((float3x3)unity_WorldToObject, worldNormal));

                // Sample the base texture
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

                // Use the transformed normal for lighting calculations
                o.Normal = localNormal;

                // Calculate diffuse lighting (Lambert)
                //half3 lightDir = normalize(_WorldSpaceLightPos0.xyz - localPos.xyz);
                //half NdotL = max(0, dot(localNormal, lightDir));
                o.Albedo = c.rgb ;

                // Calculate specular lighting (Blinn-Phong)
                //half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - localPos.xyz);
                //half3 halfDir = normalize(lightDir + viewDir);
                //half spec = pow(max(0, dot(localNormal, halfDir)), _Shininess);
                o.Specular = _SpecularColor.rgb;
                //o.Gloss = spec; // Store specular intensity in Gloss channel for Shader Graph compatibility

                o.Alpha = c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}