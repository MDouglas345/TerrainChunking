using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LODSorterImpl : ILODSorter
{
    /*
        This class will be a data store for things related to LOD sorting
        The calculation itself will be done on the GPU via a compute buffer

        The compute shader (Sorter) will have the following inputs :
                    * StructuredBuffer (ChunkData) that contains a chunk data segment (POS, UVoffset) for every chunk in the world
                    * A List of ranges (Ranges) for which each chunk will be tested against for sorting
                    * Append Compute Buffers (LODBuffers) that will hold data from ChunkData dependent on the result from Range testing
                    * The Vector2 position of the player on the XZ plane
        
        You can get the amount in an Append Compute Buffer using ComputeBuffer.CopyCount : https://docs.unity3d.com/ScriptReference/ComputeBuffer.CopyCount.html
        Copy the count directly into certain locations in the ArgsBuffer to show how many instances are required.

        The ArgsBuffer is a compute buffer that will store sizeof(int) * 5 * LodLevels. This will hold the draw arguements for how many instances
        to draw for every lod mesh level.
        specifically, the instance count (the only thing that will be changing) is always the [1] element in a 5 arguement array

        Manual LOD buffers, disregard the dynamic creation, too complicated. 3 LOD buffers for the highest, medium, and lowest quality meshes to be
        drawn
        Each BufferLOD will be essentially duplicates of the ChunkData buffer, but with varying amount of actual data.

        Personal curiousity : Can you avoid setting the buffers and other variables to the comoute shader every frame?
        The buffers are not to be released until game closes so that shouldnt be an issue, if I can just set these buffers at startup
        and just call Dispatch every frame that would be amazing. To test after I get it working
        ASIDE : Well, if this was to work, it would work for everything except the player position, which will be updated every frame.
    */
    int ViewDistance;

    int ChunksInWorld;
    float[] Ranges; // in ascending order
    
    ComputeBuffer[] LODBuffers; // In Descending order ; Highest buffer first
    ComputeBuffer ChunkData;
    ComputeShader   Sorter;

    List<LODChunkData> LocalChunkData;
    int LodLevels;

    public LODSorterImpl(Transform reference, int viewdistance, int lodlevels, int chunksinworld, int chunksperside, List<LODChunkData> chunks) : base(reference){
        LodLevels = lodlevels;
        ChunksInWorld = chunksinworld;
        LocalChunkData = chunks;

        Ranges = new float[lodlevels];

        ViewDistance = viewdistance;

        calcRanges();

        
        Enable();

        Sorter =  Resources.Load("LODSorter", typeof(ComputeShader)) as ComputeShader;

    }
    public override ComputeBuffer[] GetBuffers(){
        return LODBuffers;
    }

    public override void update(){
        setupDispatchSorter();
        dispatchSorter();
        //debugBuffers();
    }

    private void debugBuffers(){
        LODChunkData[] bufferdata = new LODChunkData[ChunksInWorld];

        ComputeBuffer countbuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        

        for (int i = 0; i < LODBuffers.Length; i++){
            LODBuffers[i].GetData(bufferdata);
            ComputeBuffer.CopyCount(LODBuffers[i], countbuffer,0);
            int[] count = new int[1]{0};
            countbuffer.GetData(count);
        }

        countbuffer.Release();
    }
    

    private void setupDispatchSorter(){
        foreach(ComputeBuffer buffer in LODBuffers){
            buffer.SetCounterValue(0);
        }
        
        Sorter.SetBuffer(0, "ChunkData", ChunkData);
        Sorter.SetBuffer(0, "LODBufferHigh", LODBuffers[0]);
        Sorter.SetBuffer(0, "LODBufferMed", LODBuffers[1]);
        Sorter.SetBuffer(0, "LODBufferLow", LODBuffers[2]);
        Sorter.SetFloats("Reference",   new float[]{referencePoint.position.x, referencePoint.position.z});
        Sorter.SetFloats("Ranges", Ranges);
        Sorter.SetInt("ViewDistance", ViewDistance);
        Sorter.SetInt("TotalChunks", ChunksInWorld);
    }


    private void dispatchSorter(){
        Sorter.Dispatch(0,(int)Math.Ceiling(ChunksInWorld/64f),1,1);
    }

    public override void Enable(){
        ChunkData = new ComputeBuffer(ChunksInWorld, sizeof(float) * 4);
        ChunkData.SetData(LocalChunkData);

        LODBuffers = new ComputeBuffer[LodLevels+1];

        for (int i = 0; i <LodLevels+1; i++){
            LODBuffers[i] = new ComputeBuffer(ChunksInWorld, sizeof(float) * 4, ComputeBufferType.Append);
        }
    }

    public override void Disable(){
        ChunkData.Release();
        ChunkData = null;
        foreach(ComputeBuffer buffer in LODBuffers){
            buffer.Release();
        }
    }

    private void calcRanges(){
        Ranges[0] = 0.3f * ViewDistance;
        Ranges[1] = 0.6f * ViewDistance;
    }
}

   

