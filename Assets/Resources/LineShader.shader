
Shader "Custom/LineShader"
{
	CGINCLUDE
			
	#include "UnityCG.cginc"
	
	// struct from CS
	struct c2v
	{
		float4 vertex : SV_POSITION;
		float width : TEXCOORD0;
		float4 color : TEXCOORD1;
		float2 bisection : TEXCOORD2;
	};

	// struct used between VS and GS
	struct v2g
	{
		float4 vertex : SV_POSITION;
		//float width : TEXCOORD0;
		float4 color : TEXCOORD1;
		float2 bisection : TEXCOORD2;
	};
	
	// struct used between GS and FS
	struct g2f
	{
		float4 vertex : SV_POSITION;
		float4 color : TEXCOORD1;
	};

	// buffer which will be read inside VS
	StructuredBuffer<c2v> _Vertices;
	
	// Vertex Shader main function
	v2g Vert( uint vi : SV_VertexID )
	{
		v2g o;

		o.vertex = UnityObjectToClipPos( _Vertices[ vi ].vertex );
		o.color = (vi % 2)*float4(1, 1, 1, 1);//_Vertices[ vi ].color;
		o.bisection = _Vertices[ vi ].bisection * _Vertices[ vi ].width;

		return o;
	}
	
	// Geometry Shader main function
	[MaxVertexCount(4)]
	void Geom( point v2g input[1], inout TriangleStream<g2f> outstream)
	{
		// initialize output

		g2f o;
		o.color = float4(1,1,1,1);

		// calculate four vertices of a quad to draw a
		//:linear-variable width line

		o.vertex = input[0].vertex + float4(-0.05,0.05,0,0);
		outstream.Append(o);
		o.vertex = input[0].vertex + float4(0.05, 0.05,0,0);
		outstream.Append(o);

		o.vertex = input[0].vertex + float4(-0.05, -0.05,0,0);
		outstream.Append(o);
		o.vertex = input[0].vertex + float4(0.05, -0.05,0,0);
		outstream.Append(o);

		// vertex order is important to represent TriStrips

		outstream.RestartStrip();
	}

	// Fragment Shader main function
	fixed4 Frag( g2f i ) : SV_Target
	{
        return fixed4(1,1,1,1);
	}

	ENDCG
	

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma geometry Geom
			#pragma fragment Frag
			ENDCG
		}
	}
}