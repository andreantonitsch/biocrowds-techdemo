Shader "Custom/InstancedSurfaceShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_CoreShadowWidth("CoreShadowWidth", Float) = 0.01
		_Step("Step", Range(-0.8,0.8)) = 0.15
		_MinStep("ShadowedValue", Range(0,0.5)) = 0.1
		_MaxStep("IlluminatedValue", Range(0.5,1)) = 1.0

		[HDR] _AmbientColor("AmbientColor", Color) = (0.5, 0.5, 0.5, 1)
		_Glossiness("Glossiness", Float) = 20.0
		_SpecularColor("SpecularColor", Color) = (1, 1, 1, 1)
		_SpecularFuziness("SpecularFuziness", Float) = 0.01
		_SpecularFuzinessMin("SpecularFuzinessMin", Float) = 0.005
		_SpecularMaxBrightness("SpecularMaxBrightness", Float) = 0.13

		_RimColor("RimColor", Color) = (1, 1, 1, 1)
		_RimLighting("RimLighting", Float) = 0.8
		_RimFuzziness("RimFuzziness", Float) = 0.05
		_RimWidth("RimWidth", Range(0, 1)) = 0.05

		_OutlineColor("OutlineColor", Color) = (0, 0, 0, 0)
		_OutlineWidth("OutlineWidth", Float) = 0.3


	}
		SubShader
		{
			Tags {
				"RenderType" = "Opaque"
				"LightMode" = "Lambert"
			}

			LOD 200

			CGPROGRAM

			#pragma target 4.5
			#pragma surface surf Standard addshadow fullforwardshadows
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup


			sampler2D _MainTex;
			float4 _Color;
			float4 _AmbientColor;

			float _Step;
			float _MinStep;
			float _MaxStep;

			float _CoreShadowWidth;
			float _Glossiness;
			float4 _SpecularColor;
			float _SpecularFuziness;
			float _SpecularFuzinessMin;

			float _RimLighting;
			float _RimFuzziness;
			float _RimWidth;

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<int> typeBuffer;
			StructuredBuffer<float2> positionBuffer;
	#endif

			struct Input {
				float2 uv_MainTex;
				float3 viewDir;
				float3 worldNormal;
			};

			void setup()
			{
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
					float2 data = positionBuffer[unity_InstanceID];

					float4 position = float4(data.x, 1.1, data.y, 1.0);

					unity_ObjectToWorld._11_21_31_41 = float4(0.8, 0, 0, 0);
					unity_ObjectToWorld._12_22_32_42 = float4(0, 1.75, 0, 0);
					unity_ObjectToWorld._13_23_33_43 = float4(0, 0, 0.8, 0);
					unity_ObjectToWorld._14_24_34_44 = position;
					unity_WorldToObject = unity_ObjectToWorld;
					unity_WorldToObject._14_24_34 *= -1;
					unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
				#endif
			}



			void surf(Input i, inout SurfaceOutputStandard o) {

				//Sample texture
				fixed4 tex_sample = tex2D(_MainTex, i.uv_MainTex);

				float3 view_direction = normalize(i.viewDir);

				//Light incidence angle.
				float3 normal = normalize(i.worldNormal);

				//Dot between normal and light
				float n_dot_l = dot(_WorldSpaceLightPos0, normal);

				float stepped_dot = smoothstep(_Step, _Step + _CoreShadowWidth, n_dot_l);

				float light_band = (min(max(stepped_dot, _MinStep), _MaxStep));

				//Dot between normal and midvector between view direction and light
				float3 mid = normalize(_WorldSpaceLightPos0 + view_direction);
				float n_dot_v = dot(normal, mid);
				float specular_brightness = pow(n_dot_v * stepped_dot, _Glossiness * _Glossiness);
				float specular_band = smoothstep(_SpecularFuzinessMin, _SpecularFuziness, specular_brightness);
				float4 specular_shine = specular_band * _SpecularColor;

				float4 directional_light = light_band * _LightColor0;

				//Rim lighting intensity and band
				float rim_dot = 1 - dot(view_direction, normal);
				float rim_intense = rim_dot * pow(n_dot_l, _RimWidth);

				float rim_band = smoothstep(_RimLighting - _RimFuzziness, _RimLighting + _RimFuzziness, rim_intense);

				fixed4 c = tex_sample * _Color * (_AmbientColor + directional_light + specular_shine + rim_band);
				c.a = 1.0;

				o.Albedo = c.rgb;
				o.Metallic = 0;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
				o.Normal = o.Normal + float3(0.0, 0.0, 0.0);
			}

		ENDCG
		}
}
