Shader "Unlit/Outline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _SmoothStepRanges("Smooth Ranges", Vector) = (0,1,0,1)
        _T("T", Vector) = (0,1,0,1)
        _Color("SonarColor", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _CameraDepthTexture;
                float4 _CameraDepthTexture_TexelSize;

                sampler2D _CameraDepthNormalsTexture;
                float4 _CameraDepthNormalsTexture_TexelSize;

                float4 _MainTex_ST;
                float2 _T;
                float4 _SmoothStepRanges;
                float4 _Color;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                float D(float2 uv, float2 offset) 
                {
                    float4 d = tex2D(_CameraDepthTexture, uv + offset * _CameraDepthTexture_TexelSize.xy);
                    return LinearEyeDepth(d);
                }


                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    float4 col = tex2D(_MainTex, i.uv);



                    float4 depth_tex = tex2D(_CameraDepthTexture, i.uv);
                    float base_depth = LinearEyeDepth(depth_tex);

                    float depth_dx = max(abs(D(i.uv, float2(1, 0)) - base_depth), abs(D(i.uv, float2(-1, 0)) - base_depth));
                    float depth_dy = max(abs(D(i.uv, float2(0, 1)) - base_depth), abs(D(i.uv, float2(0, -1)) - base_depth));



                    if (max(depth_dx, depth_dy) > _T.x / 1000.0f)
                        return _Color;
                    else
                        return col;

                    //return float4(depth_dx, depth_dy, 0, 1);
                    //float4 r = _SmoothStepRanges;




                    //depth2 = min(depth2, 1000);
                    //depth2 = depth2 * (depth2 != 1000) - 10;
                    //float s = smoothstep(_T.x - _T.y, _T.x + _T.y, depth2);
                    //float s2 = smoothstep(_T.x + _T.y, 10000, depth2);
                    //float s3 = lerp(s, s2, depth2 > (_T.x + _T.y));
                    ////return s3;
                    ////float s2 = smoothstep(s, r.z, _T);
                    ////float4 color = smoothstep()
                    //return lerp(col, _Color, s3);

                }
                ENDCG
            }
        }
}
