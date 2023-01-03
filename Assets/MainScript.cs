using UnityEngine;


public class MainScript : MonoBehaviour
{
    // Set public variables

    public string compute_id = "MainComputeShader";
    public string shader_id = "Meta/MainShader";

    public string cs_kernel_name = "TestKernel";

    public int vertexCount = 9;
    const int vertexStride = 3*(4*sizeof(float)) + sizeof(float);
    

    // Initialize variables and objects

    // Material will contain the main shader pass
    Material _material;

    // Compute Shader will generate vertices
    ComputeShader _computeShader;
    int CSKernel;

    // Buffer to store vertices
    ComputeBuffer vertexBuffer;

	//Create struct ShaderIDs (shader property ids)
	//Property IDs are used to get properties faster than name-strings
	//But how are the properties with same name in all shaders linked?
	static class ShaderIDs
	{
		public static int vertices = Shader.PropertyToID( "_Vertices" );
	}

    // Awake is always called before Start
    void Awake()
    {
        // Create new material containing main shader pass
		_material = new Material( Shader.Find( shader_id ) );

        // Load Compute Shader
		_computeShader = Instantiate( Resources.Load<ComputeShader>( compute_id ) );
		CSKernel = _computeShader.FindKernel( cs_kernel_name );

        // Create vertex buffer
		vertexBuffer = new ComputeBuffer( vertexCount, vertexStride );

        // Link vertex buffer to CS main kernel
		_computeShader.SetBuffer( CSKernel, ShaderIDs.vertices, vertexBuffer );

        if ( cs_kernel_name == "Blob4Gen" )
        {
            // change this to IDs
            _computeShader.SetVector( "_f4parameter1", new Vector4(vertexCount,0.25f,0.25f,1f) );
            _computeShader.SetVector( "_f4parameter2", new Vector4(0.75f,1f,0.9f,0.85f)     );
            _computeShader.SetVector( "_f4parameter3", new Vector4(0f,0.35f,0.65f,0.7f)     );
        }

		// Link vertex buffer to main shader in material
		_material.SetBuffer( ShaderIDs.vertices, vertexBuffer );

		// Start CS (dim [vertexCount, 1, 1]) and fill vertex buffer
		_computeShader.Dispatch( CSKernel, vertexCount, 1, 1 );
    }

    void OnDestroy()
	{
		vertexBuffer.Release();
		Destroy( _computeShader );
		Destroy( _material );
	}

	void OnRenderObject()
	{
		// Use first defined pass on main shaders
		_material.SetPass( 0 );

        // Dispatch the main shaders
		Graphics.DrawProceduralNow( MeshTopology.LineStrip, vertexBuffer.count);
        // instance count will mirror vertexBuffer size
        // mesh topology influences how vertex info is sent to VS,
        //:therefore GS input topology must match it
	}
}
