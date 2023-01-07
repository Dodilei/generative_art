using UnityEngine;
using System.Collections.Generic;


public abstract class Draw : MonoBehaviour
{
    public int _vertexCount;
    protected int vertexCount;

    // Material will contain the main shader pass
    protected static Material material;

    // Compute Shader will generate vertices
    protected static ComputeShader computeShader;
    protected int CSKernelMain;

    // Buffer to store vertices
    protected static ComputeBuffer vertexBuffer;

    protected static string shader_id;
    protected static string compute_id;

    protected static string cs_kernel_id;

    protected static int vertexStride;
    protected static MeshTopology topology;

    protected static List<string> CSList = new List<string>();
    protected static Dictionary<string, ComputeShader> computeShaders = new Dictionary<string, ComputeShader>();

	protected static class ShaderIDs
	{
		public static int vertices = Shader.PropertyToID( "_Vertices" );
	}

    public Draw()
    {
        Debug.Log("Starting Draw");
        CSList.Add(compute_id);
    }

    public virtual void ApplyParamUpdate()
    {
        this.vertexCount = this._vertexCount;
    }

    public virtual void Awake()
    {
        this.ApplyParamUpdate();

        // Create new material containing main shader pass
		material = new Material( Shader.Find( shader_id ) );

        this.InstantiateComputeShaders();

        // Load Compute Shader
		computeShader = computeShaders[compute_id];
		this.CSKernelMain = computeShader.FindKernel( cs_kernel_id );

        // Create vertex buffer
		vertexBuffer = new ComputeBuffer( vertexCount, vertexStride );

        // Link vertex buffer to CS main kernel
		computeShader.SetBuffer( CSKernelMain, ShaderIDs.vertices, vertexBuffer );

        // Link vertex buffer to main shader in material
		material.SetBuffer( ShaderIDs.vertices, vertexBuffer );
    }

    public virtual void OnRenderObject()
    {

        // Use first defined pass on main shaders
		material.SetPass( 0 );

        // Dispatch the main shaders
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
		Destroy( computeShader );
		Destroy( material );
    }

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

        computeShader.SetVector("line_width", this.lineWidth);
        computeShader.SetVector("line_color", this.lineColor);
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

        compute_id = "BlobCompute";
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
        computeShader.SetVector( "parameter", this.configParameter );
        computeShader.SetVector( "scale",     this.scaleParameter  );
        computeShader.SetVector( "phase",     this.phaseParameter  );

        int CSHelperKernel = computeShader.FindKernel( cs_helper_id );
        computeShader.SetBuffer( CSHelperKernel, ShaderIDs.vertices, vertexBuffer );

        int CSLoopKernel = computeShader.FindKernel( cs_loop_id );
        computeShader.SetBuffer( CSLoopKernel, ShaderIDs.vertices, vertexBuffer );

		// Start CS (dim [vertexCount, 1, 1]) and fill vertex buffer
		computeShader.Dispatch( CSKernelMain, this.vertexCount-1, 1, 1 );

        // Start bisection calculator
        computeShader.Dispatch( CSHelperKernel, this.vertexCount-1, 1, 1 );

        // Start loop maker
        computeShader.Dispatch( CSLoopKernel, 1, 1, 1 );

    }
}