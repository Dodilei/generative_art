
Shader "Custom/PointShader"
{
	CGINCLUDE
			
	#include "UnityCG.cginc"
	
	// struct from CS
	struct c2v
	{
		float4 vertex : SV_POSITION;
		float4 color : TEXCOORD1;
	};

	// struct used between VS and GS
	struct v2g
	{
		float4 vertex : SV_POSITION;
		float4 color : TEXCOORD1;
	};
	
	// struct used between GS and FS
	struct g2f
	{
		float4 vertex : SV_POSITION;
		float4 color : TEXCOORD1;
	};

	float4 Polar2Cartesian( float4 point_vertex )
	{
		float4 o;
		o = point_vertex;
		o.xy = float2(o.x*cos(o.y), o.x*sin(o.y));
		return o;
	}

	// buffer which will be read inside VS
	StructuredBuffer<c2v> _Vertices;
	
	// Vertex Shader main function
	v2g VertCartesian( uint vi : SV_VertexID )
	{
		v2g o;

		o.vertex = UnityObjectToClipPos( _Vertices[ vi ].vertex );
		o.color = _Vertices[ vi ].color;

		return o;
	}

	v2g VertPolar( uint vi : SV_VertexID )
	{
		v2g o;

		float4 transformation = _Vertices[ vi ].vertex;
		transformation.y = 0;

		o.vertex = UnityObjectToClipPos( transformation );
		o.vertex.y = _Vertices[ vi ].vertex.y;
		o.color = _Vertices[ vi ].color;

		return o;
	}
	
	// Geometry Shader main function
	[MaxVertexCount(4)]
	void Square( point v2g input[1], inout TriangleStream<g2f> outstream)
	{
		// initialize output
		input[0].vertex = Polar2Cartesian(input[0].vertex);
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

	// Geometry Shader main function
	[MaxVertexCount(4)]
	void PolarSquare( point v2g input[1], inout TriangleStream<g2f> outstream)
	{
		// initialize output
		g2f o;
		o.color = input[0].color;

		// calculate four vertices of a quad to draw a
		//:linear-variable width line

		o.vertex = Polar2Cartesian(input[0].vertex + float4(-0.05,0.05,0,0));
		outstream.Append(o);
		o.vertex = Polar2Cartesian(input[0].vertex + float4(0.05, 0.05,0,0));
		outstream.Append(o);

		o.vertex = Polar2Cartesian(input[0].vertex + float4(-0.05, -0.05,0,0));
		outstream.Append(o);
		o.vertex = Polar2Cartesian(input[0].vertex + float4(0.05, -0.05,0,0));
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
			#pragma vertex VertCartesian
			#pragma geometry Square
			#pragma fragment Frag
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex VertPolar
			#pragma geometry PolarSquare
			#pragma fragment Frag
			ENDCG			
		}
	}
}