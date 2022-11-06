using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IChunkRenderer : ISystemComponent
{
    protected Mesh[]   LODMeshes;
    protected int  LODs;

    public IChunkRenderer(int lods, int ChunksPerSide){
        LODs = lods;

        List<int> factors = Util.getFactors((int)Chunk.ChunkSize);
        int length = factors.Count;
        factors[length-1] = factors[length-1]-1;

        LODMeshes = new Mesh[LODs+1];

        for (int i = 0; i < LODs+1; i++){
            int verts;

           
                verts = factors[length - (i+1)];
            

          
            
            LODMeshes[i] = Chunk.GenerateLODMesh((uint)ChunksPerSide, (uint)verts+1);
        }
    }

    public abstract void render();

    //public abstract void setupRender(ComputeBuffer[] LODBuffers);


    
  public abstract void Enable();

  public abstract void Disable();
}
