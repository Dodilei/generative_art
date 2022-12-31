
Shader "Meta/MainShader"
{
	CGINCLUDE
			
	#include "UnityCG.cginc"
	
	// struct from CS
	struct c2v
	{
		float4 vertex : SV_POSITION;
		float width : TEXCOORD0;
		float4 color : TEXCOORD1;
	};

	// struct used between VS and GS
	struct v2g
	{
		float4 vertex : SV_POSITION;
		float width : TEXCOORD0;
		float4 color : TEXCOORD1;
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
		o.width = _Vertices[ vi ].width;
		o.color = _Vertices[ vi ].color;

		return o;
	}
	
	// Geometry Shader main function
	[MaxVertexCount(4)]
	void Geom( line v2g input[2], inout TriangleStream<g2f> outstream)
	{
		// calculate normal

		float2 vec = input[1].vertex.xy - input[0].vertex.xy;
		float rdst = rsqrt(vec.x*vec.x + vec.y*vec.y);

		float4 normal = float4(0,0,0,0);
		normal.x = -vec.y * rdst;
		normal.y = vec.x * rdst;

		// initialize output

		g2f o;
		o.color = input[0].color;

		// calculate four vertices of a quad to draw a
		//:linear-variable width line

		o.vertex = input[0].vertex - input[0].width*normal;
		outstream.Append(o);
		o.vertex = input[0].vertex + input[0].width*normal;
		outstream.Append(o);

		o.vertex = input[1].vertex - input[1].width*normal;
		outstream.Append(o);
		o.vertex = input[1].vertex + input[1].width*normal;
		outstream.Append(o);

		// vertex order is important to represent TriStrips

		outstream.RestartStrip();
	}

	// Fragment Shader main function
	fixed4 Frag( g2f i ) : SV_Target
	{
		return fixed4(i.color);
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