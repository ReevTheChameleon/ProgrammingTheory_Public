Shader "Test/Stencil"{
	Properties{
		[IntRange] matStencilID("stencilID",Range(0,255)) = 0
	}
	SubShader{
		LOD 100
		Tags{"Queue"="Geometry"}
		Pass{
			Name "Main"
			Blend Zero One
			ZWrite Off
			stencil{
				Ref 5
				Comp Always
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex vertexShader
			#pragma fragment fragmentShader
			#include "UnityCG.cginc"
			//for Universal Render Pipelines
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct VertToFrag{
				float4 clipPos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			//sampler2D matTexture;
			//float4 matTexture_ST;
			half4 matTint;

			VertToFrag vertexShader(
				float4 objectPos : POSITION,
				float2 uv : TEXCOORD0
			){
				VertToFrag v2f;
				v2f.clipPos = UnityObjectToClipPos(objectPos);
				//For Universal Render Pipelines
				//float3 worldPos = TransformObjectToWorld(objectPos.xyz);
				//v2f.clipPos = TransformWorldToHClip(worldPos);
				//v2f.uv = uv*matTexture_ST.xy + matTexture_ST.zw;
				v2f.uv = uv;
				return v2f;
			}
			half4 fragmentShader(
				VertToFrag v2f
			) : SV_TARGET
			{
				if(v2f.uv.x > 0.7f)
					discard;
				return half4(v2f.uv,0,1);
			}
			ENDHLSL
		}
	}
}
