using UnityEngine;
using static Draw;

public class MainScript : DrawBlob
{
    public override void Awake()
    {
        this.minRadius = 0.08f;
        this._lineWidth = 0.05f;
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
        this.enabled = false;
    }
}
