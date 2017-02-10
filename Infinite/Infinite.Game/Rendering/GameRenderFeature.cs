using Infinite.Shaders;
using SiliconStudio.Xenko.Rendering;

namespace Infinite.Rendering
{
    class GameRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> RenderEffectKey;

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(RenderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;
                var renderMesh = (RenderMesh)renderObject;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    // Generate shader permuatations
                    //renderEffect.EffectValidator.ValidateParameter(GameParameters.EnableColorEffect, renderMesh.Mesh.Parameters.Get(GameParameters.EnableColorEffect));
                }
            }
        }
    }
}
