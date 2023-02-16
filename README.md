# Generative Art

This is an exploratory project in generative art. The objective is to implement a GPU focused framework for creating procedurally generated parametric shapes. It is developed in Unity, utilizes C# for lifecycle management and shaders (Compute, Vertex, and Geometry shaders) for vertex generation and rendering. 

---

## Structure

The system consists of a modular structure focused on flexibility and inheritance for future extensions (new shapes and styles).

### Class Hierarchy
* **Draw.cs**: Acts as the base class. It manages the lifecycle of `ComputeBuffer` objects and handles the initialization of materials and shaders. This class defines the template for dispatching compute kernels and executing procedural draw calls via `Graphics.DrawProceduralNow()`.
* **DrawLine.cs**: An implementation that handles line-based shapes. It uses compute kernels for bisection field calculations and loop closure, ensuring that path geometry remains continuous and correctly oriented.
* **DrawBlob.cs**: Abstracts from the line functionality to implement organic shapes. It binds the shape parameters to the compute buffers and executes the vertex generation kernels.

---

## Blob Generation

Vertex positions are calculated within the `BlobCompute.compute` file using the `Blob4Gen` kernel. The shape is defined by a polar coordinate system where the radius $R$ at an angle $\theta$ is determined by the sum of four harmonic oscillations.

The generation follows this expression:

$$R = r_0 + 0.5k + \frac{k}{8} \cdot ({S} \cdot \cos(\theta \cdot [1, 2, 3, 4] + 2\pi \cdot {\phi})^{{c}})$$

Where:
* $r_0$: Represents the base radius.
* $k$: Represents the maximum span or amplitude envelope.
* **$$S$$ (scale)**: A Vector4 defining the amplitude weight of each of the four harmonic layers.
* **$$\phi$$ (phase)**: A Vector4 defining the phase offset for each harmonic, allowing for shape rotation and asymmetry.
* **$$c$$ (crisp)**: A factor that determines the sharpness of the resulting lobes.



---

## GPU Pipeline

### 1. Compute Stage (`LineUtils.compute`)
Before rendering, two kernels process the data:
* **BisecCalc**: Calculates perpendicular bisection vectors at each vertex. These vectors are stored in the vertex buffer and used later to determine the direction of line expansion.
* **LoopCloser**: Ensures the first and last vertices are synchronized to create a seamless closed loop for the blob.

### 2. Shader Stage (`LineShader.shader`)
The rendering uses a multi-stage shader:
* **Vertex Shader**: Collects the generated positions from the `StructuredBuffer`.
* **Geometry Shader**: Receives the line strip data and expands each segment into a variable-width quad. It uses the calculated bisection vectors to ensure smooth joints at every corner.
* **Fragment Shader**: Handles the final rasterization and color application.



---

## Technical Parameters

The `MainScript.cs` serves as the entry point, allowing for the configuration of three primary `Vector4` parameters that drive the `Blob4Gen` kernel:

| Parameter | Component Mapping | Description |
| :--- | :--- | :--- |
| **Config** | `(x: count, y: r0, z: k, w: crisp)` | Defines vertex resolution, base size, and harmonic sharpness. |
| **Scale** | `(x, y, z, w)` | Adjusts the strength of the 1st, 2nd, 3rd, and 4th harmonics. |
| **Phase** | `(x, y, z, w)` | Controls the phase shift ($0$ to $2\pi$) for each harmonic layer. |

---

## Memory and Performance

Data is stored in a `StructuredBuffer` with a stride of 32 bytes per vertex. This buffer contains:
* `float4`: Position data.
* `float4`: Extra data reserved for future features.
* `float2`: Bisection vectors.
* `float`: Line width.

All vertex data is maintained within GPU memory, avoiding data transfer overhead. Everything is processed within the GPU's parallel architecture.