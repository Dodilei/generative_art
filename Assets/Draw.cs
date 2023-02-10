using UnityEngine;

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

	protected static class ShaderIDs
	{
		public static int vertices = Shader.PropertyToID( "_Vertices" );
	}

    public Draw() {}

    public virtual void ApplyParamUpdate()
    {
        this.vertexCount = this._vertexCount;
    }

    public virtual void Awake()
    {
        this.ApplyParamUpdate();

        // Create new material containing main shader pass
		material = new Material( Shader.Find( shader_id ) );

        // Load Compute Shader
		computeShader = Instantiate( Resources.Load<ComputeShader>( compute_id ) );
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
}

public class DrawPoint : Draw
{

    public Vector4 _pointColor = new Vector4(1f,1f,1f,1f);

    protected Vector4 pointColor;

    public override void ApplyParamUpdate()
    {
        base.ApplyParamUpdate();

        this.pointColor = this._pointColor;
    }

    public override void Awake()
    {
        base.Awake();

        computeShader.SetVector("point_color", this.pointColor);
    }
}

public class DrawBlobPoint : DrawPoint
{
    public float minRadius = 0.2f;
    public float maxSpan = 0.35f;
    public float crispness = 3f;

    public Vector4 _scaleParameter = new Vector4(0.75f,1f,0.9f,0.85f);
    public Vector4 _phaseParameter = new Vector4(0f,0.35f,0.65f,0.7f);

    protected Vector4 configParameter;
    protected Vector4 scaleParameter;
    protected Vector4 phaseParameter;

    static DrawBlob()
    {
        vertexStride = 2*(4*sizeof(float));

        topology = MeshTopology.Points;

        shader_id = "Custom/PointShader";

        compute_id = "PointGenerators";
        cs_kernel_id = "Blob4Gen";
    }

    public DrawBlob()
    {
        this.ApplyParamUpdate();
    }

    public override void ApplyParamUpdate()
    {
        base.ApplyParamUpdate();

        this.vertexCount = this._vertexCount;

        this.configParameter = new Vector4(
            this.vertexCount,
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

		// Start CS (dim [vertexCount, 1, 1]) and fill vertex buffer
		computeShader.Dispatch( CSKernelMain, this.vertexCount, 1, 1 );

    }
}