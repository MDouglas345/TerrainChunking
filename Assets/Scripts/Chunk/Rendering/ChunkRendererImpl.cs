using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ChunkRendererImpl : IChunkRenderer
{
    ComputeBuffer ArgsBuffer;
    Bounds MeshBounds;
    Material[] MeshMats;

    ComputeBuffer[] LODBuffers;

    Color[] colors;

    int chunksperside;
    
    public ChunkRendererImpl(int lods, int chunksperside, ComputeBuffer[] lodbuffers) : base(lods, chunksperside){
        Enable();
       
        this.chunksperside = chunksperside;
        MeshBounds = new Bounds(new Vector3(0,-128,0),new Vector3(chunksperside*256 * 2, 256 , chunksperside*256 * 2));
        LODBuffers = lodbuffers;
        colors = new Color[3];

        colors[0] = Color.green;
        colors[1] = Color.blue;
        colors[2] = Color.red;

         initMats();
    }

    private void initMats(){
        MeshMats =  new Material[LODs+1];

        for(int i = 0; i < MeshMats.Length; i++){
            MeshMats[i] = new Material(Resources.Load("terrainMaterial", typeof(Material)) as Material);
            int terrainsize = MeshMats[i].GetInt("_terrainsize");
            MeshMats[i].SetBuffer("_ChunkData",LODBuffers[i]);
            MeshMats[i].SetColor("color", colors[i]);
            MeshMats[i].SetFloat("_texelSize", 1.0f / (chunksperside * 256.0f));
            
        }
        
    }   

    private  void setupRender(){
        for (int i = 0; i < LODBuffers.Length; i++){
            ComputeBuffer.CopyCount(LODBuffers[i], ArgsBuffer, 4 + i * 5 * 4);
        }
    }

    private void debugSetupRender(){
         int[] args = new int[5*(LODs+1)]; // does this initalize them to 0??
         ArgsBuffer.GetData(args);
    }

    public override void render(){
        setupRender();
        //debugSetupRender();
        
        for (int i = 0; i < LODMeshes.Length; i++){
            
            Graphics.DrawMeshInstancedIndirect(LODMeshes[i], 0, MeshMats[i], MeshBounds, ArgsBuffer, i * 5 * 4);
        }
        
        /*
        int x = 0;
        MeshMat.SetBuffer("_ChunkData", LODBuffers[x]);
        Graphics.DrawMeshInstancedIndirect(LODMeshes[x], 0, MeshMat, MeshBounds, ArgsBuffer, x * 5 * 4);
        */
    }

    private void initalizeArgs(){
        int[] args = new int[5*(LODs+1)]; // does this initalize them to 0??
        for (int i = 0; i < LODMeshes.Length; i++){
            args[i * 5] = (int)LODMeshes[i].GetIndexCount(0);
        }
        ArgsBuffer.SetData(args);
    }

    public override void Enable(){
        ArgsBuffer = new ComputeBuffer(LODs+1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
        // === int[5]{0,0,0,0,0} * lods more detail here -> https://docs.unity3d.com/2023.1/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
        initalizeArgs();
    }

    public override void Disable(){
        ArgsBuffer.Release();
        ArgsBuffer = null;
    }
}
