# Vox.Core
Vox.Core is a library that provides a set of voxelization algorithms for meshes. 
It is written in pure C#. 
The library is designed to be easy to use.

> **Note:** This library is still in development and is not yet ready for production use.

## Algorithms

### Sparse Voxel Octree (SVO)
The Sparse Voxel Octree (SVO) algorithm is a hierarchical voxelization algorithm that uses an octree to represent the voxelized model. The octree is built recursively by subdividing the model into eight equal parts at each level of the tree.

```csharp
int depth = 4; // The depth of the octree
bool isSolid = false; // Whether the voxels should be solid or not
var voxelizer = new Voxelizer(); // Initialize the voxelizer
List<Voxel> voxels = voxelizer.VoxelizeSVO(mesh, depth, isSolid);

// voxel have:
// - Position
// - Size
// - State (Inside, Outside, Intersection)
```

### Signed Distance Field (SDF)
The Signed Distance Field (SDF) algorithm uses a 3D grid to represent the voxelized model. Each voxel in the grid stores the distance to the nearest surface.

>[!WARNING]
> Not yet implemented.


