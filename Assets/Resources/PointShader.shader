
Shader "Custom/PointShader"
{
	CGINCLUDE
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
			
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
		float2 polar : TEXCOORD2;
	};

	float random(float2 p){return frac(cos(dot(p,float2(23.14069263277926,2.665144142690225)))*12345.6789);}

	float perlin(float2 p, float cell_size) 
	{
		float2 base = p / cell_size;

		float4x2 corners;

		corners[0] = floor(base);
		corners[3] = ceil(base);

		corners[1] = float2(ceil(base.x), floor(base.y));
		corners[2] = float2(floor(base.x), ceil(base.y));

		float4 values;
		values.x = random(corners[0]);
		values.y = random(corners[1]);
		values.z = random(corners[2]);
		values.w = random(corners[3]);

		float2 first_interp = lerp(values.xy, values.zw, frac(base.y));
		float sec_interp = lerp(first_interp.x, first_interp.y, frac(base.x));

		return sec_interp;
	}

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
		o.polar = float2(0,0);

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

		o.polar = input[0].vertex.xy + float2(-0.05, 0.05);
		o.vertex = Polar2Cartesian(float4(o.polar, input[0].vertex.zw));
		o.color = float4(1,1,1,1);
		outstream.Append(o);

		o.polar = input[0].vertex.xy + float2(0.05, 0.05);
		o.vertex = Polar2Cartesian(float4(o.polar, input[0].vertex.zw));
		o.color = float4(0.2,0.2,0.2,1);
		outstream.Append(o);

		o.polar = input[0].vertex.xy + float2(-0.05, -0.05);
		o.vertex = Polar2Cartesian(float4(o.polar, input[0].vertex.zw));
		o.color = float4(1,1,1,1);
		outstream.Append(o);

		o.polar = input[0].vertex.xy + float2(0.05, -0.05);
		o.vertex = Polar2Cartesian(float4(o.polar, input[0].vertex.zw));
		o.color = float4(0.2,0.2,0.2,1);
		outstream.Append(o);

		// vertex order is important to represent TriStrips


		outstream.RestartStrip();
	}

	// Fragment Shader main function
	fixed4 Frag( g2f i ) : SV_Target
	{
		float4 nseR50 = 0.5*float4(1,1,1,0)*perlin(i.vertex.xy, 20);
		//float4 nseR50 = 0.25*float4(1,1,1,0)*random(trunc(i.polar.xx*50));
		//float4 nseR200 = 0.07*float4(1,1,1,0)*random(trunc(i.polar.xx*200));
		//float4 nseC50 = 0.2*float4(1,1,1,0)*random(trunc(i.vertex.xy / 10));
		//float4 nseC250 = 0.2*float4(1,1,1,0)*random(trunc(i.polar.xx / 2));
        return fixed4(float4(0.5,0.5,0.5,1) + nseR50);
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