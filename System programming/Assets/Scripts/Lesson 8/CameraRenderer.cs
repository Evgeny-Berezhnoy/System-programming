using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public partial class CameraRenderer
    {
        #region Fields

        private readonly List<ShaderTagId> _drawingShaderTagIds;

        private CommandBuffer _commandBuffer;
        private CullingResults _cullingResults;

        private ScriptableRenderContext _context;
        private Camera _camera;

        #endregion

        #region Constructors

        public CameraRenderer()
        {
            _drawingShaderTagIds = new List<ShaderTagId>();
            _drawingShaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        #endregion

        #region Methods

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _context    = context;
            _camera     = camera;

            #if UNITY_EDITOR

            EmitWorldGeometryForCamera();

            #endif

            if (!Cull(out var parameters))
            {
                return;
            };

            SetCameraSettings(parameters);

            StartRecordingCommandBuffer();

            DrawVisible();

            #if UNITY_EDITOR

            DrawUnsupportedShaders();
            DrawGizmos();

            #endif

            FinishRecordingCommandBuffer();

            _context.Submit();
        }

        private void SetCameraSettings(ScriptableCullingParameters parameters)
        {
            _cullingResults = _context.Cull(ref parameters);
            
            _context.SetupCameraProperties(_camera);
        }

        private bool Cull(out ScriptableCullingParameters parameters)
        {
            return _camera.TryGetCullingParameters(out parameters);
        }

        private void DrawVisible()
        {
            DrawRenderers(SortingCriteria.CommonOpaque, RenderQueueRange.opaque);

            _context.DrawSkybox(_camera);

            DrawRenderers(SortingCriteria.CommonTransparent, RenderQueueRange.transparent);
        }

        private void DrawRenderers(
            SortingCriteria sortingCriteria,
            RenderQueueRange renderQueueRange)
        {
            var drawingSettings =
                CreateDrawingSettings(
                    _drawingShaderTagIds,
                    sortingCriteria,
                    out var sortingSettings);

            var filteringSettings = new FilteringSettings(renderQueueRange);

            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private DrawingSettings CreateDrawingSettings(
            List<ShaderTagId> shaderTags,
            SortingCriteria sortingCriteria,
            out SortingSettings sortingSettings)
        {
            sortingSettings = new SortingSettings(_camera) { criteria = sortingCriteria };

            var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);

            for(int i = 1; i < shaderTags.Count; i++)
            {
                drawingSettings.SetShaderPassName(i, shaderTags[i]);
            };

            return drawingSettings;
        }

        private void StartRecordingCommandBuffer()
        {
            _commandBuffer = new CommandBuffer() { name = _camera.name };
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            _commandBuffer.BeginSample(_commandBuffer.name);

            ExecuteCommandBuffer();
        }

        private void FinishRecordingCommandBuffer()
        {
            _commandBuffer.EndSample(_commandBuffer.name);

            ExecuteCommandBuffer();
        }

        private void ExecuteCommandBuffer()
        {
            _context.ExecuteCommandBuffer(_commandBuffer);

            _commandBuffer.Clear();
        }

        #endregion
    }
}
