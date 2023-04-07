Shader "Unlit/NormalOutline"
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

                float _CameraZoomT;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                float4 D(float2 uv, float2 offset) 
                {
                    float4 d = tex2D(_CameraDepthNormalsTexture, uv + offset * _CameraDepthNormalsTexture_TexelSize.xy);

                    float dep;
                    float3 n;

                    DecodeDepthNormal(d, dep, n);

                    return float4(n.x, n.y, n.z, dep);
                }
                

                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    float4 col = tex2D(_MainTex, i.uv);



                    float4 depth_tex = tex2D(_CameraDepthNormalsTexture, i.uv);

                    float depth_normal;
                    float depth;
                    float3 base_normal;

                    DecodeDepthNormal(depth_tex, depth, base_normal);


                    float n_dotx = min(dot(D(i.uv, float2(1, 0)), base_normal), dot(D(i.uv, float2(-1, 0)), base_normal));
                    float n_doty = min(dot(D(i.uv, float2(0, 1)), base_normal), dot(D(i.uv, float2(0, -1)), base_normal));


                    float3 m_dot = min(n_dotx, n_doty);
                    float m = min(m_dot.x, max(m_dot.y, m_dot.z));

                    float depth_dx = max(abs(D(i.uv, float2(1, 0)).w - depth), abs(D(i.uv, float2(-1, 0)).w - depth));
                    float depth_dy = max(abs(D(i.uv, float2(0, 1)).w - depth), abs(D(i.uv, float2(0, -1)).w - depth));

                    //return float4(n_dotx, n_doty, 0, 0);

                    if (m < _T.x | max(depth_dx, depth_dy) > _CameraZoomT)
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
