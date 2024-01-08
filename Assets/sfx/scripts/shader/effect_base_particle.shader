Shader "ArtCenter/Effect/Base_Particle" {
    Properties {
        _MainTex ("Main Tex", 2D) = "white" {}
		_FixColor("FixColor", COLOR) = (1,1,1,1)
        _Brightness ("Brightness", Float ) = 1
        _MainTexPannerX ("Main Tex Panner X", Float ) = 0
        _MainTexPannerY ("Main Tex Panner Y", Float ) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.BlendMode)] SrcBlend ("SrcBlend", Float) = 5//SrcAlpha
		[Enum(UnityEngine.Rendering.BlendMode)] DstBlend ("DstBlend", Float) = 1//One
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend [SrcBlend] [DstBlend]
            Cull Off
            ZWrite Off
            ZTest [_ZTest]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma skip_variants DIRECTIONAL DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SCREEN SHADOWS_SHADOWMASK VERTEXLIGHT_ON

            //#pragma target 2.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform fixed4 _FixColor;
            uniform float _Brightness;
            uniform float _MainTexPannerX;
            uniform float _MainTexPannerY;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = (TRANSFORM_TEX(v.texcoord0,_MainTex) + half2(_MainTexPannerX,_MainTexPannerY) * _Time.y);
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _MainTex_var = tex2D(_MainTex,i.uv0);
                float3 emissive = _Brightness * _MainTex_var.rgb * i.vertexColor.rgb * _FixColor.rgb;
                return fixed4(emissive,i.vertexColor.a * _MainTex_var.a * _FixColor.a);
            }
            ENDCG
        }
    }
    //FallBack "Diffuse"
    //CustomEditor "ShaderForgeMaterialInspector"
}
