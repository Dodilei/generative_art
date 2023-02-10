using UnityEngine;
using System.Collections.Generic;


public abstract class Draw : MonoBehaviour
{
    public int _vertexCount;
    protected int vertexCount;

    // Material will contain the main shader pass
    protected static Material material;

    // Compute Shader will generate vertices
    protected static ComputeShader mainCompShader;
    protected int CSKernelMain;

    // Buffer to store vertices
    protected static ComputeBuffer vertexBuffer;

    // NameID of unique Shader Pass
    protected static string shader_id;
    // NameID of main* compute shader
    protected static string main_cshader;

    // ID of main* cs kernel
    protected static string cs_kernel_id;

    // Size of each vertex
    protected static int vertexStride;
    // Topology kind
    protected static MeshTopology topology;

    // List to store ComputeShader names
    protected static List<string> CSList = new List<string>();
    // Dictionary to relate CS names and objects after instancing
    protected static Dictionary<string, ComputeShader> computeShaders = new Dictionary<string, ComputeShader>();

    // class to store shader information, this is allegedly faster IDK
	protected static class ShaderIDs
	{
		public static int vertices = Shader.PropertyToID( "_Vertices" );
	}

    // Constructor
    // Adds main* ComputeShader to CSList
    public Draw()
    {
        Debug.Log("Starting Draw");
        CSList.Add(main_cshader);
    }

    // This function applies external parameters
    public virtual void ApplyParamUpdate()
    {
        this.vertexCount = this._vertexCount;
    }

    // Awake runs when started
    public virtual void Awake()
    {
        // Apply external params
        this.ApplyParamUpdate();

        // Create vertex buffer
		vertexBuffer = new ComputeBuffer( vertexCount, vertexStride );

        // Create new material containing main shader pass
		material = new Material( Shader.Find( shader_id ) );

        // Instantiate all compute shaders in CSList and add them to the Dictionary
        this.InstantiateComputeShaders();

        // Get main* shader from dictionary
        mainCompShader = computeShaders[main_cshader];

        // Load main* kernel
		this.CSKernelMain = mainCompShader.FindKernel( cs_kernel_id );

        // Link vertex buffer to CS main kernel
		mainCompShader.SetBuffer( CSKernelMain, ShaderIDs.vertices, vertexBuffer );

        // Link vertex buffer to main shader in material
		material.SetBuffer( ShaderIDs.vertices, vertexBuffer );
    }

    // This runs when drawing to the camera
    public virtual void OnRenderObject()
    {
        // Use first defined pass on Shader
		material.SetPass( 0 );

        // Dispatch the Shader
		Graphics.DrawProceduralNow( topology, vertexBuffer.count);
        // instance count will mirror vertexBuffer size
        // mesh topology influences how vertex info is sent to VS,
        //:therefore GS input topology must match it
    }

    public virtual void OnDisable()
    {
        this.OnDestroy();
    }

    public virtual void OnDestroy()
    {
        vertexBuffer.Release();
		Destroy( mainCompShader );
		Destroy( material );
    }

    // Instantiate all compute Shaders on Dictionary
    public virtual void InstantiateComputeShaders()
    {
        foreach (string key in CSList)
        {
            computeShaders[key] = Instantiate( Resources.Load<ComputeShader>( key ) );
        }
    }
}

public class DrawLine : Draw
{
    public float _lineWidth = 0.05f;
    protected Vector4 _lineWidthPoly;

    public Vector4 _lineColor = new Vector4(1f,1f,1f,1f);

    protected bool isWidthPoly = false;

    protected Vector4 lineWidth;
    protected Vector4 lineColor;

    public override void ApplyParamUpdate()
    {
        base.ApplyParamUpdate();

        if (this.isWidthPoly)
        {
            this.lineWidth = this._lineWidthPoly;
        }
        else
        {
            this.lineWidth = new Vector4(this._lineWidth, 0f, 0f, 0f);
        }

        this.lineColor = this._lineColor;
    }

    public void SetWidthPoly(Vector4 width)
    {
        this._lineWidthPoly = width;
        this.isWidthPoly = true;
    }

    public override void Awake()
    {
        base.Awake();

        mainCompShader.SetVector("line_width", this.lineWidth);
        mainCompShader.SetVector("line_color", this.lineColor);
    }
}

public class DrawBlob : DrawLine
{
    public float minRadius = 0.2f;
    public float maxSpan = 0.35f;
    public float crispness = 3f;

    public Vector4 _scaleParameter = new Vector4(0.75f,1f,0.9f,0.85f);
    public Vector4 _phaseParameter = new Vector4(0f,0.35f,0.65f,0.7f);

    protected Vector4 configParameter;
    protected Vector4 scaleParameter;
    protected Vector4 phaseParameter;

    protected static string cs_helper_id = "BisecCalc";

    protected static string cs_loop_id = "LoopCloser";

    static DrawBlob()
    {
        Debug.Log("Starting static DrawBlob");

        vertexStride = 2*(4*sizeof(float)) + (2*sizeof(float)) + sizeof(float);

        topology = MeshTopology.LineStrip;

        shader_id = "Custom/LineShader";

        main_cshader = "BlobCompute";
        cs_kernel_id = "Blob4Gen";
    }

    public DrawBlob()
    {
        Debug.Log("Starting DrawBlob");
        
        this.ApplyParamUpdate();
    }

    public override void ApplyParamUpdate()
    {
        base.ApplyParamUpdate();

        this.vertexCount = this._vertexCount + 1;

        this.configParameter = new Vector4(
            this.vertexCount-1,
            this.minRadius,
            this.maxSpan,
            this.crispness
        );

        this.scaleParameter = this._scaleParameter;
        this.phaseParameter = this._phaseParameter;
    }

    public override void Awake()
    {
        base.Awake();

        // change this to IDs
        mainCompShader.SetVector( "parameter", this.configParameter );
        mainCompShader.SetVector( "scale",     this.scaleParameter  );
        mainCompShader.SetVector( "phase",     this.phaseParameter  );

        int CSHelperKernel = mainCompShader.FindKernel( cs_helper_id );
        mainCompShader.SetBuffer( CSHelperKernel, ShaderIDs.vertices, vertexBuffer );

        int CSLoopKernel = mainCompShader.FindKernel( cs_loop_id );
        mainCompShader.SetBuffer( CSLoopKernel, ShaderIDs.vertices, vertexBuffer );

		// Start CS (dim [vertexCount, 1, 1]) and fill vertex buffer
		mainCompShader.Dispatch( CSKernelMain, this.vertexCount-1, 1, 1 );

        // Start bisection calculator
        mainCompShader.Dispatch( CSHelperKernel, this.vertexCount-1, 1, 1 );

        // Start loop maker
        mainCompShader.Dispatch( CSLoopKernel, 1, 1, 1 );

    }
}