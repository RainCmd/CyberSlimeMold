Shader "Unlit/Particle"
{
    Properties
    {
        [PerRendererData]  _MainTex ("Sprite Texture", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "CanUseSpriteAtlas"="true" "PreviewType"="Plane" }
        LOD 100

        Pass
        {
            Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "CanUseSpriteAtlas"="true" "PreviewType"="Plane" }
  
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            // 模板测试设置：标记需要产生Bloom的物体
            Stencil
            {
                Ref 1
                Comp Always     // 总是通过深度测试
                Pass Replace    // 通过时将模板值设为参考值
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - .1);
                return col;
            }
            ENDCG
        }
    }
}
