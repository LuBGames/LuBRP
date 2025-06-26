using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace LubRP.Editor.ShaderGraph.Targets
{
    public enum MaterialType
    {
        Unlit
    }
    
    enum AlphaMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply,
    }
    
    enum SurfaceType
    {
        Opaque,
        Transparent,
    }
    
    enum ZWriteControl
    {
        Auto = 0,
        ForceEnabled = 1,
        ForceDisabled = 2
    }
    
    enum ZTestMode  // the values here match UnityEngine.Rendering.CompareFunction
    {
        Disabled = 0,
        Never = 1,
        Less = 2,
        Equal = 3,
        LEqual = 4,     // default for most rendering
        Greater = 5,
        NotEqual = 6,
        GEqual = 7,
        Always = 8,
    }
    
    enum ZTestModeForUI
    {
        Never = 1,
        Less = 2,
        Equal = 3,
        LEqual = 4,     // default for most rendering
        Greater = 5,
        NotEqual = 6,
        GEqual = 7,
        Always = 8,
    }
    
    enum RenderFace
    {
        Front = 2,      // = CullMode.Back -- render front face only
        Back = 1,       // = CullMode.Front -- render back face only
        Both = 0        // = CullMode.Off -- render both faces
    }
    
    sealed class LuBTarget : Target
    {
        static readonly GUID kSourceCodeGuid = new GUID("c04155d868334baeb6f6a6dc09929563"); // LuBTarget.cs
        
        public const string kUnlitMaterialTypeTag = "\"LuBMaterialType\" = \"Unlit\"";
        public const string kUberTemplatePath = "Packages/com.lub.rp/Editor/ShaderGraph/Templates/ShaderPass.template";
        public static readonly string[] kSharedTemplateDirectories = GenerationUtils.GetDefaultSharedTemplateDirectories().Union(new string[]
        {
            "Packages/com.lub.rp/Editor/ShaderGraph/Templates"
#if HAS_VFX_GRAPH
            , "Packages/com.unity.visualeffectgraph/Editor/ShaderGraph/Templates"
#endif
        }).ToArray();
        
        public string renderType
        {
            get
            {
                if (surfaceType == SurfaceType.Transparent)
                    return $"{RenderType.Transparent}";
                else
                    return $"{RenderType.Opaque}";
            }
        }
        
        public bool alphaClip
        {
            get => m_AlphaClip;
            set => m_AlphaClip = value;
        }
        
        // this sets up the default renderQueue -- but it can be overridden by ResetMaterialKeywords()
        public string renderQueue
        {
            get
            {
                if (surfaceType == SurfaceType.Transparent)
                    return $"{UnityEditor.ShaderGraph.RenderQueue.Transparent}";
                else if (alphaClip)
                    return $"{UnityEditor.ShaderGraph.RenderQueue.AlphaTest}";
                else
                    return $"{UnityEditor.ShaderGraph.RenderQueue.Geometry}";
            }
        }
        
        public bool allowMaterialOverride
        {
            get => m_AllowMaterialOverride;
            set => m_AllowMaterialOverride = value;
        }
        
        public ZWriteControl zWriteControl
        {
            get => m_ZWriteControl;
            set => m_ZWriteControl = value;
        }
        
        public SubTarget activeSubTarget
        {
            get => m_ActiveSubTarget.value;
            set => m_ActiveSubTarget = value;
        }
        
        public bool receiveShadows
        {
            get => m_ReceiveShadows;
            set => m_ReceiveShadows = value;
        }
        
        public string customEditorGUI
        {
            get => m_CustomEditorGUI;
            set => m_CustomEditorGUI = value;
        }
        
        public ZTestMode zTestMode
        {
            get => m_ZTestMode;
            set => m_ZTestMode = value;
        }
        
        public SurfaceType surfaceType
        {
            get => m_SurfaceType;
            set => m_SurfaceType = value;
        }
        
        public bool castShadows
        {
            get => m_CastShadows;
            set => m_CastShadows = value;
        }
        
        public AlphaMode alphaMode
        {
            get => m_AlphaMode;
            set => m_AlphaMode = value;
        }
        
        public RenderFace renderFace
        {
            get => m_RenderFace;
            set => m_RenderFace = value;
        }
        
        // SubTarget
        List<SubTarget> m_SubTargets;
        List<string> m_SubTargetNames;
        int activeSubTargetIndex => m_SubTargets.IndexOf(m_ActiveSubTarget);
        
        // Subtarget Data
        [SerializeField]
        List<JsonData<JsonObject>> m_Datas = new ();
        
        //View
        PopupField<string> m_SubTargetField;
        TextField m_CustomGUIField;
#if HAS_VFX_GRAPH
        Toggle m_SupportVFXToggle;
#endif
        
        [SerializeField]
        JsonData<SubTarget> m_ActiveSubTarget;
        
        [SerializeField]
        string m_CustomEditorGUI;
        
        [SerializeField]
        SurfaceType m_SurfaceType = SurfaceType.Opaque;
        
        [SerializeField]
        bool m_AlphaClip = false;
        
        [SerializeField]
        AlphaMode m_AlphaMode = AlphaMode.Alpha;
        
        [SerializeField]
        RenderFace m_RenderFace = RenderFace.Front;
        
        [SerializeField]
        ZTestMode m_ZTestMode = ZTestMode.LEqual;
        
        [SerializeField]
        bool m_SupportsLODCrossFade = false;
        
        // when checked, allows the material to control ALL surface settings (uber shader style)
        [SerializeField]
        bool m_AllowMaterialOverride = false;
        
        [SerializeField]
        bool m_CastShadows = true;
        
        [SerializeField]
        bool m_ReceiveShadows = true;
        
        [SerializeField]
        ZWriteControl m_ZWriteControl = ZWriteControl.Auto;

        public LuBTarget()
        {
            displayName = "LuB";
            m_SubTargets = TargetUtils.GetSubTargets(this);
            m_SubTargetNames = m_SubTargets.Select(x => x.displayName).ToList();
            TargetUtils.ProcessSubTargetList(ref m_ActiveSubTarget, ref m_SubTargets);
            ProcessSubTargetDatas(m_ActiveSubTarget.value);
        }
        
        void ProcessSubTargetDatas(SubTarget subTarget)
        {
            var typeCollection = TypeCache.GetTypesDerivedFrom<JsonObject>();
            foreach (var type in typeCollection)
            {
                if (type.IsGenericType)
                    continue;

                // Data requirement interfaces need generic type arguments
                // Therefore we need to use reflections to call the method
                var methodInfo = typeof(LuBTarget).GetMethod(nameof(SetDataOnSubTarget));
                var genericMethodInfo = methodInfo.MakeGenericMethod(type);
                genericMethodInfo.Invoke(this, new object[] { subTarget });
            }
        }
        
        public void SetDataOnSubTarget<T>(SubTarget subTarget) where T : JsonObject
        {
            if (!(subTarget is IRequiresData<T> requiresData))
                return;

            // Ensure data object exists in list
            T data = null;
            foreach (var x in m_Datas.SelectValue())
            {
                if (x is T y)
                {
                    data = y;
                    break;
                }
            }

            if (data == null)
            {
                data = Activator.CreateInstance(typeof(T)) as T;
                m_Datas.Add(data);
            }

            // Apply data object to SubTarget
            requiresData.data = data;
        }
        
        public override bool IsActive()
        {
            bool isUniversalRenderPipeline = GraphicsSettings.currentRenderPipeline is LubRenderPipelineAsset;
            return isUniversalRenderPipeline && activeSubTarget.IsActive();
        }

        public override void Setup(ref TargetSetupContext context)
        {
            // Setup the Target
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);

            // Override EditorGUI (replaces the URP material editor by a custom one)
            if (!string.IsNullOrEmpty(m_CustomEditorGUI))
                context.AddCustomEditorForRenderPipeline(m_CustomEditorGUI, typeof(LubRenderPipelineAsset));

            // Setup the active SubTarget
            TargetUtils.ProcessSubTargetList(ref m_ActiveSubTarget, ref m_SubTargets);
            m_ActiveSubTarget.value.target = this;
            ProcessSubTargetDatas(m_ActiveSubTarget.value);
            m_ActiveSubTarget.value.Setup(ref context);
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            var descs = context.blocks.Select(x => x.descriptor);
            // Core fields
            context.AddField(Fields.GraphVertex, descs.Contains(BlockFields.VertexDescription.Position) ||
                                                 descs.Contains(BlockFields.VertexDescription.Normal) ||
                                                 descs.Contains(BlockFields.VertexDescription.Tangent));
            context.AddField(Fields.GraphPixel);

            // SubTarget fields
            m_ActiveSubTarget.value.GetFields(ref context);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            // Core blocks
            if (!(m_ActiveSubTarget.value is UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenSubTarget<LuBTarget>))
            {
                context.AddBlock(BlockFields.VertexDescription.Position);
                context.AddBlock(BlockFields.VertexDescription.Normal);
                context.AddBlock(BlockFields.VertexDescription.Tangent);
                context.AddBlock(BlockFields.SurfaceDescription.BaseColor);
            }

            // SubTarget blocks
            m_ActiveSubTarget.value.GetActiveBlocks(ref context);
        }
        
        public void AddDefaultMaterialOverrideGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            // At some point we may want to convert this to be a per-property control
            // or Unify the UX with the upcoming "lock" feature of the Material Variant properties
            context.AddProperty("Allow Material Override", new Toggle() { value = allowMaterialOverride }, (evt) =>
            {
                if (Equals(allowMaterialOverride, evt.newValue))
                    return;

                registerUndo("Change Allow Material Override");
                allowMaterialOverride = evt.newValue;
                onChange();
            });
        }
        
        public void AddDefaultSurfacePropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo, bool showReceiveShadows)
        {
            context.AddProperty("Surface Type", new EnumField(SurfaceType.Opaque) { value = surfaceType }, (evt) =>
            {
                if (Equals(surfaceType, evt.newValue))
                    return;

                registerUndo("Change Surface");
                surfaceType = (SurfaceType)evt.newValue;
                onChange();
            });

            context.AddProperty("Blending Mode", new EnumField(AlphaMode.Alpha) { value = alphaMode }, surfaceType == SurfaceType.Transparent, (evt) =>
            {
                if (Equals(alphaMode, evt.newValue))
                    return;
            
                registerUndo("Change Blend");
                alphaMode = (AlphaMode)evt.newValue;
                onChange();
            });
            
            context.AddProperty("Render Face", new EnumField(RenderFace.Front) { value = renderFace }, (evt) =>
            {
                if (Equals(renderFace, evt.newValue))
                    return;
            
                registerUndo("Change Render Face");
                renderFace = (RenderFace)evt.newValue;
                onChange();
            });

            context.AddProperty("Depth Write", new EnumField(ZWriteControl.Auto) { value = zWriteControl }, (evt) =>
            {
                if (Equals(zWriteControl, evt.newValue))
                    return;

                registerUndo("Change Depth Write Control");
                zWriteControl = (ZWriteControl)evt.newValue;
                onChange();
            });

            context.AddProperty("Depth Test", new EnumField(ZTestModeForUI.LEqual) { value = (ZTestModeForUI)zTestMode }, (evt) =>
            {
                if (Equals(zTestMode, evt.newValue))
                    return;
            
                registerUndo("Change Depth Test");
                zTestMode = (ZTestMode)evt.newValue;
                onChange();
            });

            context.AddProperty("Alpha Clipping", "Avoid using when Alpha and AlphaThreshold are constant for the entire material as enabling in this case could introduce visual artifacts and will add an unnecessary performance cost when used with MSAA (due to AlphaToMask)", 0, new Toggle() { value = alphaClip }, (evt) =>
            {
                if (Equals(alphaClip, evt.newValue))
                    return;

                registerUndo("Change Alpha Clip");
                alphaClip = evt.newValue;
                onChange();
            });

            context.AddProperty("Cast Shadows", new Toggle() { value = castShadows }, (evt) =>
            {
                if (Equals(castShadows, evt.newValue))
                    return;
            
                registerUndo("Change Cast Shadows");
                castShadows = evt.newValue;
                onChange();
            });

            if (showReceiveShadows)
                context.AddProperty("Receive Shadows", new Toggle() { value = receiveShadows }, (evt) =>
                {
                    if (Equals(receiveShadows, evt.newValue))
                        return;
            
                    registerUndo("Change Receive Shadows");
                    receiveShadows = evt.newValue;
                    onChange();
                });
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<string> registerUndo)
        {
            // Core properties
            m_SubTargetField = new PopupField<string>(m_SubTargetNames, activeSubTargetIndex);
            context.AddProperty("Material", m_SubTargetField, (evt) =>
            {
                if (Equals(activeSubTargetIndex, m_SubTargetField.index))
                    return;

                registerUndo("Change Material");
                m_ActiveSubTarget = m_SubTargets[m_SubTargetField.index];
                ProcessSubTargetDatas(m_ActiveSubTarget.value);
                onChange();
            });

            // SubTarget properties
            m_ActiveSubTarget.value.GetPropertiesGUI(ref context, onChange, registerUndo);

            // Custom Editor GUI
            // Requires FocusOutEvent
            m_CustomGUIField = new TextField("") { value = customEditorGUI };
            m_CustomGUIField.RegisterCallback<FocusOutEvent>(s =>
            {
                if (Equals(customEditorGUI, m_CustomGUIField.value))
                    return;

                registerUndo("Change Custom Editor GUI");
                customEditorGUI = m_CustomGUIField.value;
                onChange();
            });
            context.AddProperty("Custom Editor GUI", m_CustomGUIField, (evt) => { });

#if HAS_VFX_GRAPH
            if (VFXViewPreference.generateOutputContextWithShaderGraph)
            {
                // VFX Support
                if (!(m_ActiveSubTarget.value is UniversalSubTarget))
                    context.AddHelpBox(MessageType.Info, $"The {m_ActiveSubTarget.value.displayName} target does not support VFX Graph.");
                else
                {
                    m_SupportVFXToggle = new Toggle("") { value = m_SupportVFX };
                    context.AddProperty("Support VFX Graph", m_SupportVFXToggle, (evt) =>
                    {
                        m_SupportVFX = m_SupportVFXToggle.value;
                    });
                }
            }
#endif
        }

        public override bool WorksWithSRP(RenderPipelineAsset scriptableRenderPipeline)
        {
            return scriptableRenderPipeline?.GetType() == typeof(LubRenderPipelineAsset);
        }
        
        public bool TrySetActiveSubTarget(Type subTargetType)
        {
            if (!subTargetType.IsSubclassOf(typeof(SubTarget)))
                return false;

            foreach (var subTarget in m_SubTargets)
            {
                if (subTarget.GetType().Equals(subTargetType))
                {
                    m_ActiveSubTarget = subTarget;
                    ProcessSubTargetDatas(m_ActiveSubTarget);
                    return true;
                }
            }

            return false;
        }
    }
    
    #region Pragmas

    static class CorePragmas
    {
        public static readonly PragmaCollection Base = new PragmaCollection
        {
            // { Pragma.Target(ShaderModel.Target20) },
            // { Pragma.MultiCompileInstancing },
            // { Pragma.MultiCompileFog },
            // { Pragma.InstancingOptions(InstancingOptions.RenderingLayer) },
            { Pragma.Vertex("vert") },
            { Pragma.Fragment("frag") },
        };
        
        public static readonly PragmaCollection Forward = new PragmaCollection
        {
            // { Pragma.Target(ShaderModel.Target20) },
            // { Pragma.MultiCompileInstancing },
            // { Pragma.MultiCompileFog },
            // { Pragma.InstancingOptions(InstancingOptions.RenderingLayer) },
            { Pragma.Vertex("vert") },
            { Pragma.Fragment("frag") },
        };
    }
    #endregion
    
    #region Passes
    static class CorePasses
    {
        /// <summary>
        ///  Automatically enables Alpha-To-Coverage in the provided opaque pass targets using alpha clipping
        /// </summary>
        /// <param name="pass">The pass to modify</param>
        /// <param name="target">The target to query</param>
        internal static void AddAlphaToMaskControlToPass(ref PassDescriptor pass, LuBTarget target)
        {
            if (target.allowMaterialOverride)
            {
                // When material overrides are allowed, we have to rely on the _AlphaToMask material property since we can't be
                // sure of the surface type and alpha clip state based on the target alone.
                pass.renderStates.Add(RenderState.AlphaToMask("[_AlphaToMask]"));
            }
            else if (target.alphaClip && (target.surfaceType == SurfaceType.Opaque))
            {
                pass.renderStates.Add(RenderState.AlphaToMask("On"));
            }
        }
        
        internal static void AddAlphaClipControlToPass(ref PassDescriptor pass, LuBTarget target)
        {
            if (target.allowMaterialOverride)
                pass.keywords.Add(CoreKeywordDescriptors.AlphaTestOn);
            else if (target.alphaClip)
                pass.defines.Add(CoreKeywordDescriptors.AlphaTestOn, 1);
        }
        
        internal static void AddTargetSurfaceControlsToPass(ref PassDescriptor pass, LuBTarget target, 
            bool blendModePreserveSpecular = false)
        {
            // the surface settings can either be material controlled or target controlled
            if (target.allowMaterialOverride)
            {
                // setup material control of via keyword
                pass.keywords.Add(CoreKeywordDescriptors.SurfaceTypeTransparent);
                pass.keywords.Add(CoreKeywordDescriptors.AlphaPremultiplyOn);
                pass.keywords.Add(CoreKeywordDescriptors.AlphaModulateOn);
            }
            else
            {
                // setup target control via define
                if (target.surfaceType == SurfaceType.Transparent)
                {
                    pass.defines.Add(CoreKeywordDescriptors.SurfaceTypeTransparent, 1);

                    // alpha premultiply in shader only needed when alpha is different for diffuse & specular
                    if ((target.alphaMode == AlphaMode.Alpha || target.alphaMode == AlphaMode.Additive) && blendModePreserveSpecular)
                        pass.defines.Add(CoreKeywordDescriptors.AlphaPremultiplyOn, 1);
                    else if (target.alphaMode == AlphaMode.Multiply)
                        pass.defines.Add(CoreKeywordDescriptors.AlphaModulateOn, 1);
                }
            }
            
            AddAlphaClipControlToPass(ref pass, target);
        }
        
        public static PassDescriptor ShadowCaster(LuBTarget target)
        {
            var result = new PassDescriptor()
            {
                // Definition
                displayName = "ShadowCaster",
                referenceName = "SHADERPASS_SHADOWCASTER",
                lightMode = "ShadowCaster",

                // Template
                passTemplatePath = LuBTarget.kUberTemplatePath,
                sharedTemplateDirectories = LuBTarget.kSharedTemplateDirectories,

                // Port Mask
                validVertexBlocks = CoreBlockMasks.Vertex,
                validPixelBlocks = CoreBlockMasks.FragmentAlphaOnly,

                // Fields
                structs = CoreStructCollections.Default,
                requiredFields = CoreRequiredFields.ShadowCaster,
                fieldDependencies = CoreFieldDependencies.Default,

                // Conditional State
                renderStates = CoreRenderStates.ShadowCaster(target),
                pragmas = CorePragmas.Base,
                defines = new DefineCollection(),
                keywords = new KeywordCollection { CoreKeywords.ShadowCaster },
                includes = new IncludeCollection { CoreIncludes.ShadowCaster },

                // Custom Interpolator Support
                customInterpolators = CoreCustomInterpDescriptors.Common
            };

            AddAlphaClipControlToPass(ref result, target);

            return result;
        }
    }
    #endregion
    
    #region StructCollections
    static class CoreStructCollections
    {
        public static readonly StructCollection Default = new StructCollection
        {
            { LuBStructs.Attributes },
            { LuBStructs.Varyings },
            { Structs.VertexDescriptionInputs },
            { Structs.SurfaceDescriptionInputs },
        };
    }
    #endregion
    
    #region FieldDependencies
    static class CoreFieldDependencies
    {
        public static readonly DependencyCollection Default = new DependencyCollection()
        {
            { LuBStructs.Default },
            new FieldDependency(LuBStructFields.Varyings.stereoTargetEyeIndexAsRTArrayIdx,    StructFields.Attributes.instanceID),
            new FieldDependency(LuBStructFields.Varyings.stereoTargetEyeIndexAsBlendIdx0,     StructFields.Attributes.instanceID),
        };
    }
    #endregion
    
    #region RenderStates
    static class CoreRenderStates
    {
        public static class Uniforms
        {
            public static readonly string srcBlend = "[" + Property.SrcBlend + "]";
            public static readonly string dstBlend = "[" + Property.DstBlend + "]";
            public static readonly string cullMode = "[" + Property.CullMode + "]";
            public static readonly string zWrite = "[" + Property.ZWrite + "]";
            public static readonly string zTest = "[" + Property.ZTest + "]";
        }
        
        public static Cull RenderFaceToCull(RenderFace renderFace)
        {
            switch (renderFace)
            {
                case RenderFace.Back:
                    return Cull.Front;
                case RenderFace.Front:
                    return Cull.Back;
                case RenderFace.Both:
                    return Cull.Off;
            }
            return Cull.Back;
        }
        
        // used by lit/unlit subtargets
        public static RenderStateCollection UberSwitchedRenderState(LuBTarget target, 
            bool blendModePreserveSpecular = false)
        {
            if (target.allowMaterialOverride)
            {
                return new RenderStateCollection
                {
                    RenderState.ZTest(Uniforms.zTest),
                    RenderState.ZWrite(Uniforms.zWrite),
                    RenderState.Cull(Uniforms.cullMode),
                    RenderState.Blend(Uniforms.srcBlend, Uniforms.dstBlend),
                };
            }
            var result = new RenderStateCollection();

            result.Add(RenderState.ZTest(target.zTestMode.ToString()));

            if (target.zWriteControl == ZWriteControl.Auto)
            {
                if (target.surfaceType == SurfaceType.Opaque)
                    result.Add(RenderState.ZWrite(ZWrite.On));
                else
                    result.Add(RenderState.ZWrite(ZWrite.Off));
            }
            else if (target.zWriteControl == ZWriteControl.ForceEnabled)
                result.Add(RenderState.ZWrite(ZWrite.On));
            else
                result.Add(RenderState.ZWrite(ZWrite.Off));

            result.Add(RenderState.Cull(RenderFaceToCull(target.renderFace)));

            if (target.surfaceType == SurfaceType.Opaque)
            {
                result.Add(RenderState.Blend(Blend.One, Blend.Zero));
            }
            else
            {
                // Lift alpha multiply from ROP to shader in preserve spec for different diffuse and specular blends.
                Blend blendSrcRGB = blendModePreserveSpecular ? Blend.One : Blend.SrcAlpha;

                switch (target.alphaMode)
                {
                    case AlphaMode.Alpha:
                        result.Add(RenderState.Blend(blendSrcRGB, Blend.OneMinusSrcAlpha, Blend.One, Blend.OneMinusSrcAlpha));
                        break;
                    case AlphaMode.Premultiply:
                        result.Add(RenderState.Blend(Blend.One, Blend.OneMinusSrcAlpha, Blend.One, Blend.OneMinusSrcAlpha));
                        break;
                    case AlphaMode.Additive:
                        result.Add(RenderState.Blend(blendSrcRGB, Blend.One, Blend.One, Blend.One));
                        break;
                    case AlphaMode.Multiply:
                        result.Add(RenderState.Blend(Blend.DstColor, Blend.Zero, Blend.Zero, Blend.One)); // Multiply RGB only, keep A
                        break;
                }
            }


            return result;
        }
        
        public static RenderStateDescriptor UberSwitchedCullRenderState(LuBTarget target)
        {
            if (target.allowMaterialOverride)
                return RenderState.Cull(Uniforms.cullMode);
            return RenderState.Cull(RenderFaceToCull(target.renderFace));
        }
        
        public static RenderStateCollection ShadowCaster(LuBTarget target)
        {
            var result = new RenderStateCollection
            {
                { RenderState.ZTest(ZTest.LEqual) },
                { RenderState.ZWrite(ZWrite.On) },
                { UberSwitchedCullRenderState(target) },
                { RenderState.ColorMask("ColorMask 0") },
            };
            return result;
        }
    }
    #endregion
    
    #region KeywordDescriptors

    static class CoreKeywordDescriptors
    {
        public static readonly KeywordDescriptor SurfaceTypeTransparent = new ()
        {
            displayName = ShaderKeywordStrings._SURFACE_TYPE_TRANSPARENT,
            referenceName = ShaderKeywordStrings._SURFACE_TYPE_TRANSPARENT,
            type = KeywordType.Boolean,
            definition = KeywordDefinition.ShaderFeature,
            scope = KeywordScope.Global,
            stages = KeywordShaderStage.Fragment,
        };
        
        public static readonly KeywordDescriptor AlphaPremultiplyOn = new ()
        {
            displayName = ShaderKeywordStrings._ALPHAPREMULTIPLY_ON,
            referenceName = ShaderKeywordStrings._ALPHAPREMULTIPLY_ON,
            type = KeywordType.Boolean,
            definition = KeywordDefinition.ShaderFeature,
            scope = KeywordScope.Local,
            stages = KeywordShaderStage.Fragment,
        };
        
        public static readonly KeywordDescriptor AlphaModulateOn = new ()
        {
            displayName = ShaderKeywordStrings._ALPHAMODULATE_ON,
            referenceName = ShaderKeywordStrings._ALPHAMODULATE_ON,
            type = KeywordType.Boolean,
            definition = KeywordDefinition.ShaderFeature,
            scope = KeywordScope.Local,
            stages = KeywordShaderStage.Fragment,
        };
        
        public static readonly KeywordDescriptor AlphaTestOn = new ()
        {
            displayName = ShaderKeywordStrings._ALPHATEST_ON,
            referenceName = ShaderKeywordStrings._ALPHATEST_ON,
            type = KeywordType.Boolean,
            definition = KeywordDefinition.ShaderFeature,
            scope = KeywordScope.Local,
            stages = KeywordShaderStage.Fragment,
        };
    }
    #endregion
    
    #region Includes

    static class CoreIncludes
    {
        const string kColor = "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl";
        const string kTexture = "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl";
        const string kCore = "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl";
        const string kInput = "Packages/com.lub.rp/ShaderLibrary/Input.hlsl";
        const string kGraphFunctions = "Packages/com.lub.rp/ShaderLibrary/ShaderGraphFunctions.hlsl";
        const string kShaderPass = "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl";
        const string kLighting = "Packages/com.lub.rp/ShaderLibrary/Lighting.hlsl";
        const string kShadowCasterPass = "Packages/com.lub.rp/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl";

        public static readonly IncludeCollection CorePregraph = new IncludeCollection
        {
            { kCore, IncludeLocation.Pregraph },
            { kTexture, IncludeLocation.Pregraph },
            { kColor, IncludeLocation.Pregraph },
            { kInput, IncludeLocation.Pregraph },
            { kLighting, IncludeLocation.Pregraph },
        };

        public static readonly IncludeCollection ShaderGraphPregraph = new IncludeCollection
        {
            { kGraphFunctions, IncludeLocation.Pregraph },
        };
        
        public static readonly IncludeCollection CorePostgraph = new IncludeCollection
        {
            { kShaderPass, IncludeLocation.Pregraph },
        };
        
        public static readonly IncludeCollection ShadowCaster = new IncludeCollection
        {
            { CorePregraph },
            { ShaderGraphPregraph },

            // Post-graph
            { CorePostgraph },
            { kShadowCasterPass, IncludeLocation.Postgraph },
        };
    }
        
    #endregion
    
    #region PortMasks
    class CoreBlockMasks
    {
        public static readonly BlockFieldDescriptor[] Vertex = new BlockFieldDescriptor[]
        {
            BlockFields.VertexDescription.Position,
            BlockFields.VertexDescription.Normal,
            BlockFields.VertexDescription.Tangent,
        };

        public static readonly BlockFieldDescriptor[] FragmentColorAlpha = new BlockFieldDescriptor[]
        {
            BlockFields.SurfaceDescription.BaseColor,
            BlockFields.SurfaceDescription.Alpha,
            BlockFields.SurfaceDescription.AlphaClipThreshold,
        };
        
        public static readonly BlockFieldDescriptor[] FragmentAlphaOnly = new BlockFieldDescriptor[]
        {
            BlockFields.SurfaceDescription.Alpha,
            BlockFields.SurfaceDescription.AlphaClipThreshold,
        };
    }
    #endregion
}