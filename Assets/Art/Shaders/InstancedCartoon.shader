Shader "Custom/CartoonOutlinedInstancedShader"
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
				#pragma target 4.5
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

				#pragma multi_compile_instancing
				/*#pragma instancing_options procedural:setup*/

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

				StructuredBuffer<float2> positionBuffer;
				StructuredBuffer<int> typeBuffer;


				v2f vert(appdata v, uint instanceID : SV_InstanceID)
				{

					float3 pos = float3(1.0, 1.0, 1.0);
					pos.xz = positionBuffer[instanceID];
					//pos = float3(0.0, 0.0, 0.0);
					float3 worldPosition = v.vertex + pos;

					v2f o;
					o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					o.world_normal = UnityObjectToWorldNormal(v.normal);
					o.view_direction = WorldSpaceViewDir(v.vertex);
					TRANSFER_SHADOW(o)
					UNITY_TRANSFER_FOG(o,o.pos);

					o.worldPos = mul(unity_ObjectToWorld, v.vertex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{

					//Sample texture
					fixed4 tex_sample = tex2D(_MainTex, i.uv);

					float3 view_direction = normalize(i.view_direction);
					float shadow = SHADOW_ATTENUATION(i);

					//Light incidence angle.
					float3 normal = normalize(i.world_normal);

					//Dot between normal and light
					float n_dot_l = dot(_WorldSpaceLightPos0, normal);


					float stepped_dot = smoothstep(_Step, _Step + _CoreShadowWidth, n_dot_l * shadow);


					float light_band = (min(max(stepped_dot, _MinStep), _MaxStep));

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
					UNITY_APPLY_FOG(i.fogCoord, col);

					fixed4 col = tex_sample * _Color * (_AmbientColor + directional_light + specular_shine + rim_band);
					col.a = 1.0;

					return col;
				}
			ENDCG
	}



		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
