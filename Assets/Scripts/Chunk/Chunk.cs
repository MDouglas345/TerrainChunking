using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    public static uint ChunkSize = 256;
    public static float Density = 1f;

    public static uint ChunkSizeInVerts = 257; // must equal to factor(ChunkSize) + 1
    
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public GameObject gameObject;

    public Vector2 ChunkSpaceLocation;
    public Vector2 WorldSpaceLocation;

    public Vector2 ChunkUV;

    public Chunk(Transform parent, Vector2 location, uint ChunkPerSide){
        ChunkSpaceLocation = location;
        WorldSpaceLocation = new Vector2(location.x * ChunkPerSide, location.y * ChunkPerSide);

        gameObject = new GameObject();

        gameObject.transform.position = new Vector3(location.x * ChunkSize, 0,location.y * ChunkSize );
        //gameObject = Object.Instantiate(gameObject, new Vector3(location.x * ChunkSize, 0,location.y * ChunkSize ), Quaternion.identity);

        gameObject.transform.parent = parent;
        gameObject.layer = 3;
        
        meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        meshCollider.sharedMesh = GenerateMesh(location, ChunkPerSide);
        
        meshCollider.enabled =false;
        
        //meshFilter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        //meshFilter.mesh = meshCollider.sharedMesh;
        //meshFilter.mesh = GenerateMesh(location, ChunkPerSide);

        //meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        //meshRenderer.allowOcclusionWhenDynamic = false;

        //Material mat = Resources.Load("basicTerrainMaat", typeof(Material)) as Material;
        //meshRenderer.sharedMaterial = mat;

        uint TotallVertexPerSide = ChunkPerSide * (ChunkSizeInVerts-1);

        ChunkUV = new Vector2(
          (location.x * (ChunkSizeInVerts-1) ) / TotallVertexPerSide,
          (location.y * (ChunkSizeInVerts-1) ) / TotallVertexPerSide
        );

        

        
    }

    public LODChunkData GetLODChunkData(){
        return new LODChunkData(ChunkSpaceLocation, ChunkUV);
    }

    // Start is called before the first frame update
     public static Mesh GenerateLODMesh( uint ChunksPerSide,uint verts){
        /**
            instead of generating new mesh for each chunk, copy them into each chunk's meshdata
        */
        Mesh mesh = new Mesh();
        Density = (ChunkSize) / (float)(verts-1);

        uint TotallVertexPerSide = ChunksPerSide * (ChunkSizeInVerts-1);
        float UVDifferential = ((float)TotallVertexPerSide / ChunksPerSide) / (float)TotallVertexPerSide;



        /*
            generate the verticies
                during vertex generation, calculate the UV = {vertex.x / width, vertex.y / height}
            
            generate triangle indexes


        */
        Vector3[] verticies = new Vector3[verts * verts];
        Vector2[] UV = new Vector2[verts * verts];
        int[] triangles = new int[((verts-1) * (verts-1)) * 6];

        for (int x = 0; x < verts; x++){
            for (int z = 0; z < verts; z++){
                verticies[x + z * verts] = new Vector3(x*Density,0,z*Density);
                //UV[x + z * ChunkSizeInVerts] = new Vector2((float)x /(float) (ChunkSizeInVerts-1), (float)z / (float)(ChunkSizeInVerts-1));
                UV[x + z * verts]  = new Vector2(
                   ((float)x / (verts-1) * UVDifferential),
                   ((float)z / (verts-1) * UVDifferential)
                );
            }
        }

        for (int x = 0, index = 0; x < verts-1; x++){
            for (int z = 0; z < verts-1; z++){
                triangles[index]        = x + z * (int)verts;
                triangles[index + 1]    = x + (z+1) * (int)verts;
                triangles[index + 2]    = (x + 1) + z * (int)verts;

                triangles[index + 3]    = (x + 1) + z * (int)verts; 
                triangles[index + 4]    = x + (z+1) * (int)verts ;
                triangles[index + 5]    = (x+ 1) + (z + 1) * (int)verts;
                index+= 6;
            }
        }

        mesh.vertices = verticies;
        mesh.uv = UV;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }



    public static Mesh GenerateMesh(Vector2 ChunkPos, uint ChunksPerSide){
        /**
            instead of generating new mesh for each chunk, copy them into each chunk's meshdata
        */
        Mesh mesh = new Mesh();
        Density = ChunkSize / (ChunkSizeInVerts-1);

        uint TotallVertexPerSide = ChunksPerSide * (ChunkSizeInVerts-1);
        float UVDifferential = ((float)TotallVertexPerSide / ChunksPerSide) / (float)TotallVertexPerSide;

        Vector2 ChunkUV = new Vector2(
          (ChunkPos.x * (ChunkSizeInVerts-1) ) / TotallVertexPerSide,
          (ChunkPos.y * (ChunkSizeInVerts-1) ) / TotallVertexPerSide
        );

        /*
            generate the verticies
                during vertex generation, calculate the UV = {vertex.x / width, vertex.y / height}
            
            generate triangle indexes


        */
        Vector3[] verticies = new Vector3[ChunkSizeInVerts * ChunkSizeInVerts];
        Vector2[] UV = new Vector2[ChunkSizeInVerts * ChunkSizeInVerts];
        int[] triangles = new int[((ChunkSizeInVerts-1) * (ChunkSizeInVerts-1)) * 6];

        for (int x = 0; x < ChunkSizeInVerts; x++){
            for (int z = 0; z < ChunkSizeInVerts; z++){
                verticies[x + z * ChunkSizeInVerts] = new Vector3(x*Density,0,z*Density);
                //UV[x + z * ChunkSizeInVerts] = new Vector2((float)x /(float) (ChunkSizeInVerts-1), (float)z / (float)(ChunkSizeInVerts-1));
                UV[x + z * ChunkSizeInVerts]  = new Vector2(
                    ChunkUV.x + ((float)x / (ChunkSizeInVerts-1) * UVDifferential),
                    ChunkUV.y + ((float)z / (ChunkSizeInVerts-1) * UVDifferential)
                );
            }
        }

        for (int x = 0, index = 0; x < ChunkSizeInVerts-1; x++){
            for (int z = 0; z < ChunkSizeInVerts-1; z++){
                triangles[index]        = x + z * (int)ChunkSizeInVerts;
                triangles[index + 1]    = x + (z+1) * (int)ChunkSizeInVerts;
                triangles[index + 2]    = (x + 1) + z * (int)ChunkSizeInVerts;

                triangles[index + 3]    = (x + 1) + z * (int)ChunkSizeInVerts; 
                triangles[index + 4]    = x + (z+1) * (int)ChunkSizeInVerts ;
                triangles[index + 5]    = (x+ 1) + (z + 1) * (int)ChunkSizeInVerts;
                index+= 6;
            }
        }

        mesh.vertices = verticies;
        mesh.uv = UV;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}
