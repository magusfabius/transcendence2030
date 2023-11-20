using UnityEngine;
using UnityEngine.Rendering;using UnityEngine.Rendering.HighDefinition;

[System.Serializable, VolumeComponentMenu("Letterbox")]
public sealed class LetterBox : CustomPass
{
    [SerializeField] public Vector2Int aspect = new(239, 100);
    [SerializeField] public Color color = Color.black;
    [SerializeField] public bool writeDepth = true;
    [SerializeField] public bool writeColor = true;
    [SerializeField] public bool showDebug;

    const float kDepth = 0f;
    const byte kStencil = 0;
    
    protected override void Execute(CustomPassContext ctx)
    {
        if(ctx.hdCamera.camera.cameraType != CameraType.Game)
            return;
        
        var currentViewportSize = ctx.cameraDepthBuffer.rtHandleProperties.currentViewportSize * ctx.cameraDepthBuffer.scaleFactor;;
        
        if (ctx.cameraDepthBuffer.useScaling)
            currentViewportSize = ctx.cameraDepthBuffer.GetScaledSize(ctx.cameraDepthBuffer.rtHandleProperties.currentViewportSize);
        else
            currentViewportSize = new Vector2Int(ctx.cameraDepthBuffer.rt.width, ctx.cameraDepthBuffer.rt.height);
        
        var cameraAspect = currentViewportSize.x / (float)currentViewportSize.y;
        var wantedAspect = aspect.x / (float) aspect.y;
        var aspectRatio = cameraAspect / wantedAspect;
        
        var c1 = showDebug ? Color.red : color;
        var c2 = showDebug ? Color.green : color;
        var flags = (writeDepth ? RTClearFlags.Depth | RTClearFlags.Stencil : RTClearFlags.None) | (writeColor ? RTClearFlags.Color : RTClearFlags.None); 

        if (aspectRatio < 1f)
        {
            var halfDelta = (Vector2)currentViewportSize * (1f - aspectRatio) * 0.5f;
            var vp1 = new Rect(0f, 0f, currentViewportSize.x, halfDelta.y);
            var vp2 = new Rect(0f, currentViewportSize.y - halfDelta.y, currentViewportSize.x, halfDelta.y);

            ctx.cmd.SetViewport(vp1);
            ctx.cmd.ClearRenderTarget(flags, c1, kDepth, kStencil);
            ctx.cmd.SetViewport(vp2);
            ctx.cmd.ClearRenderTarget(flags, c2, kDepth, kStencil);
        }
        else if (aspectRatio > 1f)
        {
            var halfDelta = ((Vector2)currentViewportSize - (Vector2)currentViewportSize / aspectRatio) * 0.5f;
            var vp1 = new Rect(0f, 0f, halfDelta.x, currentViewportSize.y);
            var vp2 = new Rect(currentViewportSize.x - halfDelta.x, 0f, halfDelta.x, currentViewportSize.y);
            ctx.cmd.SetViewport(vp1);
            ctx.cmd.ClearRenderTarget(flags, c1, kDepth, kStencil);
            ctx.cmd.SetViewport(vp2);
            ctx.cmd.ClearRenderTarget(flags, c2, kDepth, kStencil);
        }
    }

    protected override void Cleanup()
    {
        var cam = Camera.main;
        if (cam)
        {
            var hdCamera = HDCamera.GetOrCreate(cam);
            hdCamera.Reset();
        }
    }
}
