Shader "Custom/CartoonOutlinedShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_RampTex("RampTexture", 2D) = "white" {}
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

		_NoiseTexture("NoiseTexture", 2D) = "white" {}
		_TargetPosition("TargetPosition", Vector) = (0, 0, 0, 0)
		_OcclusionRadius("OcclusionRadius", Float) = 0.05
		_OcclusionScale("OcclusionScale", Float) = 0.01
		_OcclusionBackPlaneOffsets("OcclusionBackPlaneOffsets", Vector) = (0.1, 0, -0.1, 0)
		_MinAlpha("MinAlpha", Float) = 0.3

			//Texture Pattern
			_PatternTexture("PatternTexture", 2D) = "white" {}
			_PatternShadow("PatternShadow", Color) = (1,1,1,1)
			_PatternScale("PatternScale", Float) = 1.0
				//_NoiseTexture("NoiseTexture", 2D) = "white" {}
				_NoiseScale("NoiseScale", Float) = 1.0
				_NoiseCutoff("NoiseCutoff", Vector) = (0.5, 0.5, 0.5, 0.5)
				_PatternRotation("PatternRotation", Range(0.0, 3.15)) = 0.0

	}
		SubShader
			{
				Tags {
					"LightMode" = "ForwardBase"
					"PassFlags" = "OnlyDirectional"
					"RenderType" = "Opaque"
				}

				LOD 100
				Blend SrcAlpha OneMinusSrcAlpha
				Pass
				{
					Name "Toon"
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

				#pragma shader_feature RAMP_TEXTURE_ON RAMP_TEXTURE_OFF
				#pragma shader_feature __ PATTERN_TEXTURE_ON
				#pragma shader_feature __ NON_OCCLUSION_ON


				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float3 world_normal : NORMAL;
					UNITY_FOG_COORDS(1)
					float4 pos : SV_POSITION;
					float3 view_direction : TEXCOORD3;
					SHADOW_COORDS(2)
					float4 worldPos : TEXCOORD4;

				#ifdef NON_OCCLUSION_ON
					float4 screenPos : TEXCOORD5;
					float4 targetScreenPos : TEXCOORD6;
				#endif

				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
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
				sampler2D _NoiseTexture;


			#ifdef NON_OCCLUSION_ON
				float3 _TargetPosition;
				float _OcclusionRadius;
				float4 _OcclusionBackPlaneOffsets;
				float _OcclusionScale;
				float _MinAlpha;
			#endif

			#ifdef RAMP_TEXTURE_ON
				sampler2D _RampTex;
			#endif

			#ifdef PATTERN_TEXTURE_ON
				float _PatternScale;
				sampler2D _PatternTexture;
				float4 _PatternShadow;
				//sampler2D _NoiseTexture;
				float2 _NoiseCutoff;
				float _NoiseScale;
				float _PatternRotation;
			#endif

				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					o.world_normal = UnityObjectToWorldNormal(v.normal);
					o.view_direction = WorldSpaceViewDir(v.vertex);
					TRANSFER_SHADOW(o)
					UNITY_TRANSFER_FOG(o,o.pos);

					o.worldPos = mul(unity_ObjectToWorld, v.vertex);

			#ifdef NON_OCCLUSION_ON
					o.screenPos = ComputeScreenPos(o.pos);
					//Computes the screen space of the target's world position
					o.targetScreenPos = ComputeScreenPos(UnityObjectToClipPos(mul(unity_WorldToObject, float4(_TargetPosition,1))));
			#endif

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{

				#ifdef NON_OCCLUSION_ON
					float center_ofscreen = distance(float2(i.screenPos.xy), i.targetScreenPos.xy);
					float alpha = smoothstep(_OcclusionScale, _OcclusionRadius, center_ofscreen);
				#endif



					//Sample texture
					fixed4 tex_sample = tex2D(_MainTex, i.uv);

				#ifdef PATTERN_TEXTURE_ON
					float s = sin(_PatternRotation);
					float c = cos(_PatternRotation);

					float2x2 rot_matrix = float2x2(c, -s, s, c);

					rot_matrix *= 0.5;
					rot_matrix += 0.5;
					rot_matrix = rot_matrix * 2 - 1;
					fixed4 noise_color = tex2D(_NoiseTexture, i.worldPos.xz * float2(_NoiseScale, _NoiseScale) + float2(_Time.x * 5 + _SinTime.x, 0)).r;
					float NoiseStrength = smoothstep(_NoiseCutoff.x, _NoiseCutoff.y, noise_color);
					//Pattern Texture
					fixed4 pattern_texture = tex2D(_PatternTexture, (mul(i.worldPos.xz, rot_matrix) + 0.5) * float2(_PatternScale, _PatternScale));
					fixed4 pattern_color = (1 - pattern_texture.r) * (NoiseStrength * NoiseStrength);
					tex_sample = tex_sample - pattern_color;
				#endif


					float3 view_direction = normalize(i.view_direction);
					float shadow = SHADOW_ATTENUATION(i);

					//Light incidence angle.
					float3 normal = normalize(i.world_normal);

					//Dot between normal and light
					float n_dot_l = dot(_WorldSpaceLightPos0, normal);




					float stepped_dot = smoothstep(_Step, _Step + _CoreShadowWidth, n_dot_l * shadow);

#ifdef RAMP_TEXTURE_ON
					float light_band = tex2D(_RampTex, n_dot_l * shadow);
#endif
#ifdef RAMP_TEXTURE_OFF
					float light_band = (min(max(stepped_dot, _MinStep), _MaxStep));
#endif

					//Dot between normal and midvector between view direction and light
					float3 mid = normalize(_WorldSpaceLightPos0 + view_direction);
					float n_dot_v = dot(normal, mid);
					float specular_brightness = pow(n_dot_v * stepped_dot, _Glossiness * _Glossiness);
					float specular_band = smoothstep(_SpecularFuzinessMin, _SpecularFuziness , specular_brightness);
					float4 specular_shine = specular_band * _SpecularColor;


					float4 directional_light = light_band * _LightColor0;

					//Rim lighting intensity and band
					float rim_dot = 1 - dot(view_direction, normal);
					float rim_intense = rim_dot * pow(n_dot_l, _RimWidth);

					float rim_band = smoothstep(_RimLighting - _RimFuzziness, _RimLighting + _RimFuzziness, rim_intense);

					// apply fog
					//UNITY_APPLY_FOG(i.fogCoord, col);

					fixed4 col = tex_sample * _Color * (_AmbientColor + directional_light + specular_shine + rim_band);
					col.a = 1.0;
				#ifdef NON_OCCLUSION_ON
					col.a = max(alpha, _MinAlpha);
				#endif
					return col;
				}
				ENDCG
			}

			

				UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
			}
}
