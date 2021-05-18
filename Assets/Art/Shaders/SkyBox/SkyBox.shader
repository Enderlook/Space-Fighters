// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Asteroids/Enviroment/Sky Box"
{
	Properties
	{
		[StyledHeader(Sky)]_Sky("Sky", Float) = 0
		_SkyColor("Sky Color", Color) = (0,0,0,1)
		[StyledHeader(Stars)]_Stars("Stars", Float) = 0
		_StarsSpeed("Stars Speed", Vector) = (0.007,0,0,0)
		_StarsAngle("Stars Angle", Float) = 20
		_StarsScale("Stars Scale", Vector) = (8,2,0,0)
		_StarsDensity("Stars Density", Float) = 10
		_StartsQuantity("Starts Quantity", Float) = 100
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IsEmissive" = "true"  }
		Cull Off
		ZWrite On
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
		};

		uniform float _Sky;
		uniform float _Stars;
		uniform float4 _SkyColor;
		uniform float2 _StarsScale;
		uniform float2 _StarsSpeed;
		uniform float _StarsAngle;
		uniform float _StarsDensity;
		uniform float _StartsQuantity;


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
			float3 normalizeResult2_g5 = normalize( ase_worldPos );
			float3 break4_g5 = normalizeResult2_g5;
			float2 appendResult13_g5 = (float2(( atan2( break4_g5.x , break4_g5.z ) / 6.28318548202515 ) , ( asin( break4_g5.y ) / ( UNITY_PI / 2.0 ) )));
			float2 uv8 = 0;
			float3 unityVoronoy8 = UnityVoronoi((appendResult13_g5*_StarsScale + ( _StarsSpeed * _Time.y )),_StarsAngle,_StarsDensity,uv8);
			float stars22 = pow( ( 1.0 - saturate( unityVoronoy8.x ) ) , _StartsQuantity );
			o.Emission = ( _SkyColor + stars22 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18301
0;0;1920;1059;2031.927;272.491;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;14;-1377.406,-277.8852;Inherit;False;1914.521;460.2023;;14;22;13;12;11;8;18;9;5;17;7;4;1;24;25;Stars;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;1;-1138.099,91.35394;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;24;-1141.495,-59.6163;Inherit;False;Property;_StarsSpeed;Stars Speed;4;0;Create;True;0;0;False;0;False;0.007,0;0.004,0.005;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-911.0997,10.35394;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;7;-925.0997,-145.646;Inherit;False;Property;_StarsScale;Stars Scale;6;0;Create;True;0;0;False;0;False;8,2;8,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;17;-928.0997,-234.6461;Inherit;False;SkyBoxUV;-1;;5;3e303dafe61a43341bd183aea371dcd8;0;0;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-675.871,-28.18741;Inherit;False;Property;_StarsAngle;Stars Angle;5;0;Create;True;0;0;False;0;False;20;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-676.0997,69.35394;Inherit;False;Property;_StarsDensity;Stars Density;7;0;Create;True;0;0;False;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;5;-688.0997,-162.6461;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VoronoiNode;8;-442.0996,-163.6461;Inherit;True;0;0;1;0;1;False;1;True;False;4;0;FLOAT2;0,0;False;1;FLOAT;20;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SaturateNode;11;-254.9875,-162.704;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;12;-103.9875,-162.704;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-159.457,-71.95032;Inherit;False;Property;_StartsQuantity;Starts Quantity;8;0;Create;True;0;0;False;0;False;100;115;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;13;54.45847,-162.6666;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;311.2769,-167.9968;Inherit;False;stars;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;-1236,-950.6622;Inherit;False;Property;_SkyColor;Sky Color;2;0;Create;True;0;0;False;0;False;0,0,0,1;0.06607824,0.02803488,0.1698113,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;28;-1330.27,286.509;Inherit;False;382.3436;165.9833;;2;27;26;Drawers;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-1216.086,-748.3169;Inherit;False;22;stars;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-1123.927,336.509;Inherit;False;Property;_Stars;Stars;3;0;Create;True;0;0;True;1;StyledHeader(Stars);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-950.6002,-825.441;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1280.27,337.4923;Inherit;False;Property;_Sky;Sky;1;0;Create;True;0;0;True;1;StyledHeader(Sky);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-641.16,-870.5637;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Asteroids/Enviroment/Sky Box;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;1;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Background;;Background;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;24;0
WireConnection;4;1;1;0
WireConnection;5;0;17;0
WireConnection;5;1;7;0
WireConnection;5;2;4;0
WireConnection;8;0;5;0
WireConnection;8;1;18;0
WireConnection;8;2;9;0
WireConnection;11;0;8;0
WireConnection;12;0;11;0
WireConnection;13;0;12;0
WireConnection;13;1;25;0
WireConnection;22;0;13;0
WireConnection;21;0;20;0
WireConnection;21;1;23;0
WireConnection;0;2;21;0
ASEEND*/
//CHKSM=FB9497811CD60764D03DBC7463E3DA09A7BED85D