using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    [CreateAssetMenu(fileName = "Space run pipeline render asset", menuName = "Rendering/Space run pipeline render asset")]
    public class SpaceRunPipelineRenderAsset : RenderPipelineAsset
    {
        #region Base methods

        protected override RenderPipeline CreatePipeline()
        {
            return new SpaceRunPipelineRender();
        }

        #endregion
    }
}