using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ILODSorter : ISystemComponent
{
    protected Transform referencePoint;

    public ILODSorter(Transform reference){
        referencePoint = reference;
    }

  public abstract void Enable();

  public abstract void Disable();

  public abstract void update();

  public abstract ComputeBuffer[] GetBuffers();
} 
