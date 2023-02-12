using UnityEngine;
using static Draw;

public class MainScript : DrawBlobPoint
{
    public override void Awake()
    {
        this.minRadius = 0.08f;
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
        this._phaseParameter = this._phaseParameter + new Vector4(0.025f, 0.025f, -0.03f, 0.01f);
        this.RunComplete();
        this.minRadius = this.minRadius + 0.1f;
        this._phaseParameter = this._phaseParameter + new Vector4(0.05f, -0.025f, 0.04f, 0.025f);
        this.RunComplete();
        this.enabled = false;
    }
}
