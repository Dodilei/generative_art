using UnityEngine;
using static Draw;

public class MainScript : DrawBlob
{
    public override void Awake()
    {
        this.minRadius = 0.05f;
    }

    public void RunComplete()
    {
        base.Awake();
        base.OnRenderObject();
        base.OnDestroy();
    }

    public override void OnRenderObject()
    {
        this.RunComplete();
        this.minRadius = this.minRadius + 0.1f;
        this.RunComplete();
        this.enabled = false;
    }
}
