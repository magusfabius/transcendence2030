using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    // A project-specific shader stripper for passes and permutations that are never used in the Enemies project.
    // Don't forget to change this if making changes to the feature set used by the project.
    class ProjectSpecificShaderPreprocessor : BaseShaderPreprocessor
    {
        protected override bool DoShadersStripper(HDRenderPipelineAsset hdrpAsset, Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
        {
            // We never use PathTracing in player builds (it's only used for reference checking visual in Editor).
            if (snippet.passName == "PathTracingDXR")
            {
                if (LOG) Debug.Log($"Stripping PathTracingDXR pass from shader '{shader.name}'.");
                return true;
            }

            // We never use ray-traced sub-surface scattering.
            if (snippet.passName == "SubSurfaceDXR")
            {
                if (LOG) Debug.Log($"Stripping SubSurfaceDXR pass from shader '{shader.name}'.");
                return true;
            }

            // We have no (intentional) use of previous digital human shaders
            if (shader.name.StartsWith("Shader Graphs/DigitalHuman") && shader.name.EndsWith("(2019)"))
            {
                if (LOG) Debug.Log($"Stripping all 2019 digital human shaders ('{shader.name}').");
                return true;
            }

            // These VFX shaders don't need to interact with ray-tracing
            if (kVFXShadersNoDXR.Contains(shader.name) && snippet.passName.Contains("DXR"))
            {
                if (LOG) Debug.Log($"Stripping all DXR passes from shader '{shader.name}' / '{snippet.passName}'.");
                return true;
            }

            // These MSAA hair shaders are no longer used (TODO: remove from project)
            if (kMSAAHairShaders.Contains(shader.name))
            {
                if (LOG) Debug.Log($"Stripping all passes from shader '{shader.name}'.");
                return true;
            }

            return false;
        }

        static bool LOG = false;

        static readonly string[] kVFXShadersNoDXR = { "Teaser/Flame", "Teaser/Smoke" };
        static readonly string[] kMSAAHairShaders = { "Hair/DeferredHairShadingMarschnerBasic", "Hair/DeferredHairShadingMarschnerVolumetric", "Hair/WriteVisibility", "Hidden/MultiSampleHair"};
    }
    
    class ProjectSpecificComputePreprocessor : /*IComputeShaderVariantStripper,*/ IComputeShaderVariantStripperSkipper
    {
        public bool active => true;
        internal bool StripShader(HDRenderPipelineAsset hdAsset, ComputeShader shader, string kernelName, ShaderCompilerData inputData) => false;
        public bool CanRemoveVariant(ComputeShader shader, string shaderVariant, ShaderCompilerData shaderCompilerData) => false;

        public bool SkipShader(ComputeShader shader, string shaderVariant)
        {
            // These MSAA hair shaders are no longer used (TODO: remove from project)
            if (kMSAAHairShaders.Contains(shader.name))
            {
                if (LOG) Debug.Log($"Skipping compute shader '{shader.name}'.");
                return true;
            }

            return false;
        }

        static bool LOG = false;

        static readonly string[] kMSAAHairShaders = { "MultiSampleHairKernels", "CullHairStrandKernels", "PrefixSumUtility" };
    }
}
