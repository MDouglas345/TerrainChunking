using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    /*
        The chunk manager is responsible for initalizing all chunks and updating the quadtree 
        

        In a frame, this is the order of operations :

        Update the quad tree
                This involves iterating over all chunks and assigning them to the correct quadtrant
                If a chunk is in the lowest level of the quadrant (tbd), consider it out of view range and must be deactivated
                if the chunk is higher than the lowest level, add it to the VisibleChunks list
        
        Iterate over the visible chunk list and check its currentLOD == neededLOD, if they are not the same
                Change the mesh to the corresponding neededLOD level
                ??? Also iterate over gameobject in the chunk's list to also change their LOD mesh
        

    */
    public Transform Player;


    public List<Chunk> VisibleChunks;

    public List<GameObject> ChunksMeshes;

    public Dictionary<Vector2,Chunk> Chunks;

    

    public int ViewDistance = 100;

    public int ChunksPerSide = 200;

    public int LODs = 2;

    

    public ILODSorter LODSorter;

    public IChunkRenderer ChunkRenderer;

    public List<List<Chunk>> LODSortedChunks;

    // Start is called before the first frame update
    void Awake()
    {
        VisibleChunks = new List<Chunk>();
        Chunks = new Dictionary<Vector2, Chunk>();
        ChunksMeshes = new List<GameObject>();

        CreateChunks();
        
        List<LODChunkData> ChunkData = new List<LODChunkData>();

        foreach(KeyValuePair<Vector2,Chunk> chunk in Chunks){
            ChunkData.Add(chunk.Value.GetLODChunkData());
            //ChunksMeshes.Add(chunk.Value.gameObject);
        }

        //StaticBatchingUtility.Combine(ChunksMeshes.ToArray(), gameObject);

        
        LODSorter = new LODSorterImpl(Player, ViewDistance, LODs, ChunksPerSide * ChunksPerSide,ChunksPerSide,  ChunkData);
        ChunkRenderer = new ChunkRendererImpl(LODs, ChunksPerSide, LODSorter.GetBuffers());
    }

    private void CreateChunks(){
        for (int x = 0; x < ChunksPerSide; x++){
            for (int y = 0; y < ChunksPerSide; y++){
                Vector2 loc = new Vector2(x,y);
                Chunks.Add(loc, new Chunk(gameObject.transform, loc,(uint)ChunksPerSide ));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        LODSorter.update();

        ChunkRenderer.render();
    }

    void OnDisable(){
        LODSorter.Disable();
        ChunkRenderer.Disable();
    }

    
}
