// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Asteroids/Enviroment/Background Space"
{
	Properties
	{
		[StyledHeader(Sky)]_Sky("Sky", Float) = 0
		_SkyFirstColor("Sky First Color", Color) = (0,0,0,1)
		_SkySecondColor("Sky Second Color", Color) = (1,0,0,1)
		_SphereCenter("Sphere Center", Vector) = (0,0,0,0)
		_Radius("Radius", Float) = 0
		_Hardness("Hardness", Float) = 0
		[StyledHeader(Stars)]_Stars("Stars", Float) = 1
		_StarsColor("Stars Color", Color) = (1,1,1,1)
		_OverallScale("Overall Scale", Float) = 5
		_ClipThreshold("Clip Threshold", Range( 0.9 , 0.99)) = 0.95
		_StarsScale("Stars Scale", Range( 1 , 50)) = 20
		_Randomness("Randomness", Range( 0 , 5)) = 5
		_BrightnessVariationScale("Brightness Variation Scale", Range( 0.01 , 0.2)) = 0.05
		_Brightnesspower("Brightness power", Range( 1 , 4)) = 1
		_Brightness("Brightness", Range( 0 , 5)) = 1
		_StarsSpeed("Stars Speed", Vector) = (0,0,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		[HideInInspector]_MainTex("MainTex", 2D) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform float _Stars;
		uniform sampler2D _MainTex;
		uniform half _Sky;
		uniform float4 _SkySecondColor;
		uniform float4 _SkyFirstColor;
		uniform float2 _SphereCenter;
		uniform float _Radius;
		uniform float _Hardness;
		uniform float _Brightness;
		uniform float2 _Offset;
		uniform float _OverallScale;
		uniform float2 _StarsSpeed;
		uniform float _BrightnessVariationScale;
		uniform float _Randomness;
		uniform float _StarsScale;
		uniform float _ClipThreshold;
		uniform float _Brightnesspower;
		uniform float4 _StarsColor;


		float2 UnityGradientNoiseDir( float2 p )
		{
			p = fmod(p , 289);
			float x = fmod((34 * p.x + 1) * p.x , 289) + p.y;
			x = fmod( (34 * x + 1) * x , 289);
			x = frac( x / 41 ) * 2 - 1;
			return normalize( float2(x - floor(x + 0.5 ), abs( x ) - 0.5 ) );
		}
		
		float UnityGradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 ip = floor( p );
			float2 fp = frac( p );
			float d00 = dot( UnityGradientNoiseDir( ip ), fp );
			float d01 = dot( UnityGradientNoiseDir( ip + float2( 0, 1 ) ), fp - float2( 0, 1 ) );
			float d10 = dot( UnityGradientNoiseDir( ip + float2( 1, 0 ) ), fp - float2( 1, 0 ) );
			float d11 = dot( UnityGradientNoiseDir( ip + float2( 1, 1 ) ), fp - float2( 1, 1 ) );
			fp = fp * fp * fp * ( fp * ( fp * 6 - 15 ) + 10 );
			return lerp( lerp( d00, d01, fp.y ), lerp( d10, d11, fp.y ), fp.x ) + 0.5;
		}


		inline float2 UnityVoronoiRandomVector( float2 UV, float offset )
		{
			float2x2 m = float2x2( 15.27, 47.63, 99.41, 89.98 );
			UV = frac( sin(mul(UV, m) ) * 46839.32 );
			return float2( sin(UV.y* +offset ) * 0.5 + 0.5, cos( UV.x* offset ) * 0.5 + 0.5 );
		}
		
		//x - Out y - Cells
		float3 UnityVoronoi( float2 UV, float AngleOffset, float CellDensity, inout float2 mr )
		{
			float2 g = floor( UV * CellDensity );
			float2 f = frac( UV * CellDensity );
			float t = 8.0;
			float3 res = float3( 8.0, 0.0, 0.0 );
		
			for( int y = -1; y <= 1; y++ )
			{
				for( int x = -1; x <= 1; x++ )
				{
					float2 lattice = float2( x, y );
					float2 offset = UnityVoronoiRandomVector( lattice + g, AngleOffset );
					float d = distance( lattice + offset, f );
		
					if( d < res.x )
					{
						mr = f - lattice - offset;
						res = float3( d, offset.x, offset.y );
					}
				}
			}
			return res;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - float3( _SphereCenter ,  0.0 ) ) / _Radius );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float4 lerpResult85 = lerp( _SkySecondColor , _SkyFirstColor , pow( saturate( dotResult8_g1 ) , _Hardness ));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float4 appendResult11 = (float4(( ( _ScreenParams.x / _ScreenParams.y ) * ase_screenPosNorm.x ) , ase_screenPosNorm.y , 0.0 , 0.0));
			float4 temp_output_68_0 = (( appendResult11 * ( 200.0 / _OverallScale ) )*1.0 + float4( ( _StarsSpeed * _Time.y ), 0.0 , 0.0 ));
			float gradientNoise28 = UnityGradientNoise(( float4( _Offset, 0.0 , 0.0 ) + temp_output_68_0 ).xy,_BrightnessVariationScale);
			float4 color35 = IsGammaSpace() ? float4(1,0,0,1) : float4(1,0,0,1);
			float2 uv24 = 0;
			float3 unityVoronoy24 = UnityVoronoi(temp_output_68_0.xy,_Randomness,( 1.0 / _StarsScale ),uv24);
			float temp_output_47_0 = pow( ( _Brightness * saturate( ( saturate( gradientNoise28 ) * (0.0 + (( color35.r - unityVoronoy24.x ) - _ClipThreshold) * (1.0 - 0.0) / (1.0 - _ClipThreshold)) ) ) ) , _Brightnesspower );
			float4 stars59 = ( ( ( 1.0 + temp_output_47_0 ) * _StarsColor ) * ( _StarsColor.a * saturate( temp_output_47_0 ) ) );
			o.Emission = ( lerpResult85 + stars59 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18707
0;0;1920;1059;5807.592;1674.72;3.371062;True;False
Node;AmplifyShaderEditor.CommentaryNode;54;-3692.845,365.9602;Inherit;False;3654.015;1063.848;;41;58;53;50;49;48;51;52;47;45;46;43;44;42;29;41;39;38;28;35;24;20;25;19;22;23;21;12;11;13;8;14;9;7;2;59;68;69;70;71;72;73;Stars;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenParams;2;-3642.845,415.9602;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;7;-3412.845,439.9602;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;9;-3524.845,592.9603;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-3300.845,803.9603;Inherit;False;Property;_OverallScale;Overall Scale;9;0;Create;True;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-3255.845,519.9603;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;69;-3111.804,893.0844;Inherit;False;Property;_StarsSpeed;Stars Speed;16;0;Create;True;0;0;False;0;False;0,0;0.6,0.7;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;70;-3111.905,1018.284;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;-3101.845,609.9603;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;-3096.845,785.9603;Inherit;False;2;0;FLOAT;200;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2886.589,1112.38;Inherit;False;Property;_StarsScale;Stars Scale;11;0;Create;True;0;0;False;0;False;20;4;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-2910.802,903.0842;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-2916.845,709.9604;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-2725.916,989.8104;Inherit;False;Property;_Randomness;Randomness;12;0;Create;True;0;0;False;0;False;5;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;68;-2722.92,709.267;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;19;-2630.845,553.9602;Inherit;False;Property;_Offset;Offset;17;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;-2585.517,1095.111;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2425.448,765.0977;Inherit;False;Property;_BrightnessVariationScale;Brightness Variation Scale;13;0;Create;True;0;0;False;0;False;0.05;0.05;0.01;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;24;-2386.616,971.2104;Inherit;True;0;0;1;0;1;False;1;True;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.ColorNode;35;-2404.491,1260.497;Inherit;False;Constant;_Color0;Color 0;7;0;Create;True;0;0;False;0;False;1,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-2386.845,660.9603;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;38;-2153.491,1090.497;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-2175.774,1328.895;Inherit;False;Property;_ClipThreshold;Clip Threshold;10;0;Create;True;0;0;False;0;False;0.95;0.9655;0.9;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;28;-2018.491,652.4974;Inherit;True;Gradient;False;True;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;41;-1879.135,1090.691;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;29;-1756.491,656.4974;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-1592.605,860.9505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1556.605,745.9505;Inherit;False;Property;_Brightness;Brightness;15;0;Create;True;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;43;-1440.605,860.9505;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-1403.605,947.9505;Inherit;False;Property;_Brightnesspower;Brightness power;14;0;Create;True;0;0;False;0;False;1;1;1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-1260.605,798.9505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;47;-1090.605,799.9505;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;52;-929.605,941.9505;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;51;-835.605,980.9505;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;49;-888.605,780.9505;Inherit;False;Property;_StarsColor;Stars Color;8;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-892.605,664.9505;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;87;-3694.685,-478.5856;Inherit;False;1079.562;673.864;;9;79;86;81;83;74;85;60;61;63;Sky Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-649.605,665.9505;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-645.605,925.9505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-3637.892,-19.86671;Inherit;False;Property;_Radius;Radius;5;0;Create;True;0;0;False;0;False;0;9.85;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-3638,69.63256;Inherit;False;Property;_Hardness;Hardness;6;0;Create;True;0;0;False;0;False;0;0.14;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;86;-3644.685,-153.5806;Inherit;False;Property;_SphereCenter;Sphere Center;4;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-459.8663,781.6707;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;74;-3410.421,-57.72168;Inherit;True;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT2;0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-289.6646,776.9679;Inherit;False;stars;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;79;-3340.677,-255.42;Inherit;False;Property;_SkySecondColor;Sky Second Color;3;0;Create;True;0;0;False;0;False;1,0,0,1;0.03674623,0.03377537,0.3113208,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;60;-3367.37,-428.5856;Inherit;False;Property;_SkyFirstColor;Sky First Color;2;0;Create;True;0;0;False;0;False;0,0,0,1;0.05515092,0.0255429,0.1320755,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;85;-3057.901,-273.9444;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;90;-1869.037,-318.0023;Inherit;False;399.783;166.1285;;2;89;88;Drawers;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-3060.785,-142.1402;Inherit;False;59;stars;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1819.037,-268.0023;Half;False;Property;_Sky;Sky;1;0;Create;True;0;0;True;1;StyledHeader(Sky);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-2850.123,-224.1209;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-2161.464,854.3656;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;73;-2399.464,880.3656;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-1645.254,-266.8738;Inherit;False;Property;_Stars;Stars;7;0;Create;True;0;0;True;1;StyledHeader(Stars);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;91;-2923.477,-794.5711;Inherit;True;Property;_MainTex;MainTex;18;1;[HideInInspector];Create;True;0;0;True;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-2446.067,-270.2174;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Asteroids/Enviroment/Background Space;False;False;False;False;True;True;True;False;True;True;True;True;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;2;1
WireConnection;7;1;2;2
WireConnection;8;0;7;0
WireConnection;8;1;9;1
WireConnection;11;0;8;0
WireConnection;11;1;9;2
WireConnection;13;1;14;0
WireConnection;71;0;69;0
WireConnection;71;1;70;0
WireConnection;12;0;11;0
WireConnection;12;1;13;0
WireConnection;68;0;12;0
WireConnection;68;2;71;0
WireConnection;22;1;21;0
WireConnection;24;0;68;0
WireConnection;24;1;23;0
WireConnection;24;2;22;0
WireConnection;20;0;19;0
WireConnection;20;1;68;0
WireConnection;38;0;35;1
WireConnection;38;1;24;0
WireConnection;28;0;20;0
WireConnection;28;1;25;0
WireConnection;41;0;38;0
WireConnection;41;1;39;0
WireConnection;29;0;28;0
WireConnection;42;0;29;0
WireConnection;42;1;41;0
WireConnection;43;0;42;0
WireConnection;45;0;44;0
WireConnection;45;1;43;0
WireConnection;47;0;45;0
WireConnection;47;1;46;0
WireConnection;52;0;47;0
WireConnection;51;0;52;0
WireConnection;48;1;47;0
WireConnection;50;0;48;0
WireConnection;50;1;49;0
WireConnection;53;0;49;4
WireConnection;53;1;51;0
WireConnection;58;0;50;0
WireConnection;58;1;53;0
WireConnection;74;15;86;0
WireConnection;74;14;81;0
WireConnection;74;12;83;0
WireConnection;59;0;58;0
WireConnection;85;0;79;0
WireConnection;85;1;60;0
WireConnection;85;2;74;0
WireConnection;63;0;85;0
WireConnection;63;1;61;0
WireConnection;72;0;25;0
WireConnection;72;1;73;0
WireConnection;0;2;63;0
ASEEND*/
//CHKSM=AEDECE6EA2FB5938C830FC8665F9808DBD979E0C