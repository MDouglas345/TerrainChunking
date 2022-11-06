using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LODChunkData{
    public LODChunkData(Vector2 pos, Vector2 uv){
        Pos = pos;
        UV = uv;
    }
    public Vector2 Pos;
    public Vector2 UV;
}