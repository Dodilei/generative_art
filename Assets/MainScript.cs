using UnityEngine;
using static Draw;

public class MainScript : DrawBlob
{
    public override void Awake()
    {
        this.minRadius = 0.08f;
        this._lineWidth = 0.065f;
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
        this._lineWidth = 0.05f;
        this.minRadius = this.minRadius + 0.1f;
        this.RunComplete();
        this._lineWidth = 0.035f;
        this.minRadius = this.minRadius + 0.1f;
        this.RunComplete();
        this.enabled = false;
    }
}
