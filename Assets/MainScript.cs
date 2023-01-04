using UnityEngine;
using static Draw;

public class MainScript : DrawBlob
{
    public override void OnRenderObject()
    {
        base.OnRenderObject();
        this.minRadius = this.minRadius + 0.1f;
        this.OnDestroy();
        this.Awake();
        base.OnRenderObject(); 
        this.enabled = false;
    }
}
