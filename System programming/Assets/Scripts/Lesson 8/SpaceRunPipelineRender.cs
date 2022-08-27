using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class SpaceRunPipelineRender : RenderPipeline
    {
        #region Fields

        private CameraRenderer _cameraRenderer = new CameraRenderer();

        #endregion

        #region Base methods

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            CamerasRender(context, cameras);
        }

        #endregion

        #region Methods

        private void CamerasRender(ScriptableRenderContext context, Camera[] cameras)
        {
            for(int i = 0; i < cameras.Length; i++)
            {
                _cameraRenderer.Render(context, cameras[i]);
            };
        }

        #endregion
    }
}
