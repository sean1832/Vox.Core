# Vox.Core

Vox.Core is a C# library that provides a collection of voxelization algorithms for 3D meshes. It focuses on performance and ease of use while offering flexibility through different voxelization techniques. It is not as performant as native C++ libraries, however, it is suitable for prototyping, research, and educational purposes.

> **Note:** This library is under active development and not yet ready for production use.

## Features
- **Sparse Voxel Octree (SVO)**
  - **Bounding Volume Hierarchy (BVH) Optimizations**
- **Signed Distance Field (SDF)** (Coming Soon)

## Algorithms

### Sparse Voxel Octree (SVO)

The [Sparse Voxel Octree](https://eisenwave.github.io/voxel-compression-docs/svo/svo.html) (SVO) is a hierarchical voxelization algorithm that represents a 3D model as an octree. At each level, the model is recursively subdivided into eight smaller regions, making the algorithm space-efficient. SVO only stores information about voxels at the surface of the model, reducing memory usage compared to a full grid-based representation.

#### Basic Usage (Without Optimization)
This example demonstrates how to voxelize a mesh using SVO without additional optimizations.

```csharp
int depth = 4; // Specifies the depth of the octree (more depth = finer detail)
bool isSolid = false; // Whether voxels should be treated as solid
var voxelizer = new Voxelizer(); // Initialize the voxelizer
List<Voxel> voxels = voxelizer.VoxelizeSVO(mesh, depth, isSolid);

// Voxel has the following properties:
// - Position: The 3D position of the voxel (x, y, z)
// - Size: The size of the voxel (x, y, z)
// - State: Inside, Outside, or Intersection with the model's surface
```

#### BVH Optimization
The [Bounding Volume Hierarchy](https://en.wikipedia.org/wiki/Bounding_volume_hierarchy) (BVH) is a data structure that improves voxelization performance by spatially organizing the mesh's triangles. This reduces the number of triangles processed during voxelization, speeding up computation without sacrificing accuracy.

##### Example of BVH Optimized Voxelization:

1. **BVH Setup:** First, construct a BVH from the mesh. This is a one-time operation.
2. **Voxelization Process:** Use the BVH to guide the SVO voxelization.

```csharp
// Step 1: Create a BVH for the mesh (One-time setup)
int maxTrianglesPerLeaf = 16; // Max number of triangles per leaf node
int bucketSize = 4; // Bucket size for SAH heuristic
mesh.ComputeTriangleBounds(); // Precompute triangle bounds
BoundingVolumeHierarchy bvh = new BoundingVolumeHierarchy(mesh, maxTrianglesPerLeaf, bucketSize); // Create BVH

// Step 2: Voxelize using the BVH structure
int depth = 4; // Octree depth for voxelization
bool isSolid = false; // Whether the voxels should be treated as solid
var voxelizer = new Voxelizer(); // Initialize voxelizer
List<Voxel> voxels = voxelizer.VoxelizeSVO(bvh, depth, isSolid);
```

### Signed Distance Field (SDF) (Coming Soon)

The Signed Distance Field (SDF) algorithm voxelizes a 3D model by generating a grid where each voxel stores the distance to the closest surface of the model. This approach allows for smooth and continuous representations of the surface, often used for advanced rendering techniques like ray marching.

> **Note:** SDF voxelization is not yet implemented.