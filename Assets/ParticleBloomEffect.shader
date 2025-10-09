Shader "Unlit/ParticleBloomEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BloomBlurSize ("Blur Size", Range(0, 10)) = 2.0        // 亮斑扩散范围
        _BloomColorTint ("Color Tint", Color) = (1, 1, 1, 1)    // 亮斑颜色 tint
    }

    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        half4 _MainTex_TexelSize;
        half _BloomBlurSize;
        half4 _BloomColorTint;

        // 高斯模糊处理
        struct v2f_blur
        {
            float4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
            half2 blurSize : TEXCOORD1;
        };

        v2f_blur vert_blur(appdata_img v)
        {
            v2f_blur o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            o.blurSize = _MainTex_TexelSize.xy * _BloomBlurSize;
            return o;
        }

        // 高斯模糊片段着色器 (权重基于高斯分布)
        fixed4 frag_blur(v2f_blur i) : SV_Target
        {
            fixed4 col = tex2D(_MainTex, i.uv) * 0.4;
            half w1 = .25;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * 1, i.blurSize.y * 1)) * w1;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * 1, i.blurSize.y * -1)) * w1;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * -1, i.blurSize.y * -1)) * w1;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * -1, i.blurSize.y * 1)) * w1;
            half w2 = .05;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * 2, i.blurSize.y * 2)) * w2;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * 2, i.blurSize.y * -2)) * w2;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * -2, i.blurSize.y * -2)) * w2;
            col += tex2D(_MainTex, i.uv + half2(i.blurSize.x * -2, i.blurSize.y * 2)) * w2;
            return col * _BloomColorTint;
        }
        ENDCG

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
            ENDCG
        }
    }
    FallBack Off
}
