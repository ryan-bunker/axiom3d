#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Axiom.Core;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.MathLib;

namespace Axiom.Graphics {
    /// <summary>
    ///    Defines the functionality of a 3D API
    /// </summary>
    ///	<remarks>
    ///		The RenderSystem class provides a base class
    ///		which abstracts the general functionality of the 3D API
    ///		e.g. Direct3D or OpenGL. Whilst a few of the general
    ///		methods have implementations, most of this class is
    ///		abstract, requiring a subclass based on a specific API
    ///		to be constructed to provide the full functionality.
    ///		<p/>
    ///		Note there are 2 levels to the interface - one which
    ///		will be used often by the caller of the engine library,
    ///		and one which is at a lower level and will be used by the
    ///		other classes provided by the engine. These lower level
    ///		methods are marked as internal, and are not accessible outside
    ///		of the Core library.
    ///	</remarks>
    public abstract class RenderSystem : IDisposable {
        #region Fields

        protected RenderTargetList renderTargets = new RenderTargetList();
        protected TextureManager textureMgr;
        protected HardwareBufferManager hardwareBufferManager;
        protected CullingMode cullingMode;
        protected bool isVSync;
        protected bool depthWrite;
        protected int numCurrentLights;

        // Stored options
        protected EngineConfig engineConfig = new EngineConfig();

        // Active viewport (dest for future rendering operations) and target
        protected Viewport activeViewport;
        protected RenderTarget activeRenderTarget;

        protected int numFaces, numVertices;

        // used to determine capabilies of the hardware
        protected HardwareCaps caps = new HardwareCaps();

        /// Saved set of world matrices
        protected Matrix4[] worldMatrices = new Matrix4[256];

        /// <summary>
        ///     Flag for whether vertex winding needs to be inverted, useful for reflections.
        /// </summary>
        protected bool invertVertexWinding;

        #endregion Fields

        #region Constructor

        public RenderSystem() {		
            // default to true
            isVSync = true;

            // default to true
            depthWrite = true;

            // This means CULL clockwise vertices, i.e. front of poly is counter-clockwise
            // This makes it the same as OpenGL and other right-handed systems
            this.cullingMode = Axiom.Graphics.CullingMode.Clockwise; 
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the name of this RenderSystem based on it's assembly attribute Title.
        /// </summary>
        public virtual string Name {
            get {
                AssemblyTitleAttribute attribute = 
                    (AssemblyTitleAttribute)Attribute.GetCustomAttribute(this.GetType().Assembly, typeof(AssemblyTitleAttribute), false);

                if(attribute != null)
                    return attribute.Title;
                else
                    return "Not Found";
            }
        }

        /// <summary>
        ///     Sets whether or not vertex windings set should be inverted; this can be important
        ///     for rendering reflections.
        /// </summary>
        public bool InvertVertexWinding {
            get {
                return invertVertexWinding;
            }
            set {
                invertVertexWinding = value;
            }
        }

        /// <summary>
        /// Gets/Sets a value that determines whether or not to wait for the screen to finish refreshing
        /// before drawing the next frame.
        /// </summary>
        public bool IsVSync {
            get { 
                return isVSync; 
            }
            set { 
                isVSync = value; 
            }
        }

        /// <summary>
        ///		Gets a set of hardware capabilities queryed by the current render system.
        /// </summary>
        public HardwareCaps Caps {
            get { 
                return caps; 
            }
        }

        /// <summary>
        /// Gets a dataset with the options set for the rendering system.
        /// </summary>
        public EngineConfig ConfigOptions {
            get { 
                return this.engineConfig; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int FacesRendered {
            get { 
                return numFaces; 
            }
        }

        #endregion

        #region Abstract properties

        /// <summary>
        ///		Sets the color & strength of the ambient (global directionless) light in the world.
        /// </summary>
        public abstract ColorEx AmbientLight { set; }

        /// <summary>
        ///    
        /// </summary>
        public abstract CullingMode CullingMode { get; set; }

        /// <summary>
        ///		Sets the type of light shading required (default = Gouraud).
        /// </summary>
        public abstract Shading ShadingMode { set; }

        /// <summary>
        ///		Sets whether or not dynamic lighting is enabled.
        ///		<p/>
        ///		If true, dynamic lighting is performed on geometry with normals supplied, geometry without
        ///		normals will not be displayed. If false, no lighting is applied and all geometry will be full brightness.
        /// </summary>
        public abstract bool LightingEnabled { set; }

        /// <summary>
        ///    Sets whether or not normals are to be automatically normalized.
        /// </summary>
        /// <remarks>
        ///    This is useful when, for example, you are scaling SceneNodes such that
        ///    normals may not be unit-length anymore. Note though that this has an
        ///    overhead so should not be turn on unless you really need it.
        ///    <p/>
        ///    You should not normally call this direct unless you are rendering
        ///    world geometry; set it on the Renderable because otherwise it will be
        ///    overridden by material settings. 
        /// </remarks>
        public abstract bool NormalizeNormals { set; }

        /// <summary>
        ///		Turns stencil buffer checking on or off. 
        /// </summary>
        ///	<remarks>
        ///		Stencilling (masking off areas of the rendering target based on the stencil 
        ///		buffer) can be turned on or off using this method. By default, stencilling is
        ///		disabled.
        ///	</remarks>
        public abstract bool StencilCheckEnabled { set; }

        #endregion

        #region Overridable virtual methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoCreateWindow"></param>
        public abstract RenderWindow Initialize(bool autoCreateWindow);

        /// <summary>
        ///		Shuts down the RenderSystem.
        /// </summary>
        public virtual void Shutdown() {
            // destroy each render window
            foreach(RenderTarget target in renderTargets) {
                target.Destroy();
            }

            // Clear the render window list
            renderTargets.Clear();

            // dispose of the render system
            this.Dispose();
        }

		/// <summary>
		///		Utility method to notify all render targets that a camera has been removed, 
		///		incase they were referring to it as their viewer. 
		/// </summary>
		/// <param name="camera">Camera being removed.</param>
		internal virtual void NotifyCameraRemoved(Camera camera) {
			for(int i = 0; i < renderTargets.Count; i++) {
				RenderTarget target = (RenderTarget)renderTargets[i];
				target.NotifyCameraRemoved(camera);
			}
		}

        #endregion

        #region Abstract methods

        /// <summary>
        ///    Creates and registers a render texture object.
        /// </summary>
        /// <param name="name">The name for the new render texture. Note that names must be unique.</param>
        /// <param name="width">Requested width for the render texture.</param>
        /// <param name="height">Requested height for the render texture.</param>
        /// <returns>
        ///    On success, a reference to a new API-dependent, RenderTexture-derived
        ///    class is returned. On failure, null is returned.
        /// </returns>
        /// <remarks>
        ///    Because a render texture is basically a wrapper around a texture object,
        ///    the width and height parameters of this method just hint the preferred
        ///    size for the texture. Depending on the hardware driver or the underlying
        ///    API, these values might change when the texture is created.
        /// </remarks>
        public abstract RenderTexture CreateRenderTexture(string name, int width, int height);

		/// <summary>
		///		Creates a new render window.
		/// </summary>
		/// <remarks>
		///		This method creates a new rendering window as specified
		///		by the paramteters. The rendering system could be
		///		responible for only a single window (e.g. in the case
		///		of a game), or could be in charge of multiple ones (in the
		///		case of a level editor). The option to create the window
		///		as a child of another is therefore given.
		///		This method will create an appropriate subclass of
		///		RenderWindow depending on the API and platform implementation.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colorDepth"></param>
		/// <param name="isFullscreen"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="depthBuffer"></param>
		/// <param name="target">
		///		A handle to a pre-created window to be used for the rendering target.
		///	</param>
		/// <returns></returns>
        public abstract RenderWindow CreateRenderWindow(string name, int width, int height, int colorDepth,
            bool isFullscreen, int left, int top, bool depthBuffer, object target);

		/// <summary>
		///		Requests an API implementation of a hardware occlusion query used to test for the number
		///		of fragments rendered between calls to <see cref="IHardwareOcclusionQuery.Begin"/> and 
		///		<see cref="IHardwareOcclusionQuery.End"/> that pass the depth buffer test.
		/// </summary>
		/// <returns>An API specific implementation of an occlusion query.</returns>
		public abstract IHardwareOcclusionQuery CreateHardwareOcclusionQuery();

		/// <summary>
		///		Builds a perspective projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix3 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <returns></returns>
		public Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far) {
			// create without consideration for Gpu programs by default
			return MakeProjectionMatrix(fov, aspectRatio, near, far, false);
		}

        /// <summary>
        ///		Builds a perspective projection matrix suitable for this render system.
        /// </summary>
        /// <remarks>
        ///		Because different APIs have different requirements (some incompatible) for the
        ///		projection matrix, this method allows each to implement their own correctly and pass
        ///		back a generic Matrix3 for storage in the engine.
        ///	 </remarks>
        /// <param name="fov">Field of view angle.</param>
        /// <param name="aspectRatio">Aspect ratio.</param>
        /// <param name="near">Near clipping plane distance.</param>
        /// <param name="far">Far clipping plane distance.</param>
        /// <param name="forGpuProgram"></param>
        /// <returns></returns>
        public abstract Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far, bool forGpuProgram);

		/// <summary>
		///		Builds a orthographic projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix3 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <returns></returns>
		public Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far) {
			return MakeOrthoMatrix(fov, aspectRatio, near, far, false);
		}

		/// <summary>
		///		Builds an orthographic projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix3 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <param name="forGpuProgram"></param>
		/// <returns></returns>
		public abstract Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far, bool forGpuPrograms);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="func"></param>
        /// <param name="val"></param>
        protected internal abstract void SetAlphaRejectSettings(int stage, CompareFunction func, byte val);

        /// <summary>
        ///    Sets whether or not color buffer writing is enabled, and for which channels. 
        /// </summary>
        /// <remarks>
        ///    For some advanced effects, you may wish to turn off the writing of certain color
        ///    channels, or even all of the color channels so that only the depth buffer is updated
        ///    in a rendering pass. However, the chances are that you really want to use this option
        ///    through the Material class.
        /// </remarks>
        /// <param name="red">Writing enabled for red channel.</param>
        /// <param name="green">Writing enabled for green channel.</param>
        /// <param name="blue">Writing enabled for blue channel.</param>
        /// <param name="alpha">Writing enabled for alpha channel.</param>
        protected internal abstract void SetColorBufferWriteEnabled(bool red, bool green, bool blue, bool alpha);

        /// <summary>
        ///		Sets the fog with the given params.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="color"></param>
        /// <param name="density"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        protected abstract internal void SetFog(FogMode mode, ColorEx color, float density, float start, float end);

        /// <summary>
        ///		Converts the System.Drawing.Color value to a int.  Each API may need the 
        ///		bytes of the packed color data in different orders. i.e. OpenGL - ABGR, D3D - ARGB
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public abstract int ConvertColor(ColorEx color);

        /// <summary>
        ///		Sets the global blending factors for combining subsequent renders with the existing frame contents.
        ///		The result of the blending operation is:</p>
        ///		<p align="center">final = (texture * src) + (pixel * dest)</p>
        ///		Each of the factors is specified as one of a number of options, as specified in the SceneBlendFactor
        ///		enumerated type.
        /// </summary>
        /// <param name="src">The source factor in the above calculation, i.e. multiplied by the texture color components.</param>
        /// <param name="dest">The destination factor in the above calculation, i.e. multiplied by the pixel color components.</param>
        protected abstract internal void SetSceneBlending(SceneBlendFactor src, SceneBlendFactor dest);

        /// <summary>
        ///     Sets the 'scissor region' ie the region of the target in which rendering can take place.
        /// </summary>
        /// <remarks>
        ///     This method allows you to 'mask off' rendering in all but a given rectangular area
        ///     as identified by the parameters to this method.
        ///     <p/>
        ///     Not all systems support this method. Check the <see cref="Axiom.Graphics.Capabilites"/> enum for the
        ///     ScissorTest capability to see if it is supported.
        /// </remarks>
        /// <param name="enabled">True to enable the scissor test, false to disable it.</param>
        /// <param name="left">Left corner (in pixels).</param>
        /// <param name="top">Top corner (in pixels).</param>
        /// <param name="right">Right corner (in pixels).</param>
        /// <param name="bottom">Bottom corner (in pixels).</param>
        public abstract void SetScissorTest(bool enable, int left, int top, int right, int bottom);

        /// <summary>
        ///		Sets the surface parameters to be used during rendering an object.
        /// </summary>
        /// <param name="ambient"></param>
        /// <param name="diffuse"></param>
        /// <param name="specular"></param>
        /// <param name="emissive"></param>
        /// <param name="shininess"></param>
        protected abstract internal void SetSurfaceParams(ColorEx ambient, ColorEx diffuse, ColorEx specular, ColorEx emissive, float shininess);

        /// <summary>
        ///		Tells the hardware how to treat texture coordinates.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="texAddressingMode"></param>
        protected abstract internal void SetTextureAddressingMode(int stage, TextureAddressing texAddressingMode);

        /// <summary>
        ///    Tells the rendersystem to use the attached set of lights (and no others) 
        ///    up to the number specified (this allows the same list to be used with different
        ///    count limits).
        /// </summary>
        /// <param name="lightList">List of lights.</param>
        /// <param name="limit">Max number of lights that can be used from the list currently.</param>
        protected abstract internal void UseLights(LightList lightList, int limit);

		#region SetStencilBufferParams Overloads 

		/// <summary>
		///		This method allows you to set all the stencil buffer parameters in one call.
		/// </summary>
		/// <remarks>
		///		<para>
		///		The stencil buffer is used to mask out pixels in the render target, allowing
		///		you to do effects like mirrors, cut-outs, stencil shadows and more. Each of
		///		your batches of rendering is likely to ignore the stencil buffer, 
		///		update it with new values, or apply it to mask the output of the render.
		///		The stencil test is:<PRE>
		///		(Reference Value & Mask) CompareFunction (Stencil Buffer Value & Mask)</PRE>
		///		The result of this will cause one of 3 actions depending on whether the test fails,
		///		succeeds but with the depth buffer check still failing, or succeeds with the
		///		depth buffer check passing too.</para>
		///		<para>
		///		Unlike other render states, stencilling is left for the application to turn
		///		on and off when it requires. This is because you are likely to want to change
		///		parameters between batches of arbitrary objects and control the ordering yourself.
		///		In order to batch things this way, you'll want to use OGRE's separate render queue
		///		groups (see RenderQueue) and register a RenderQueueListener to get notifications
		///		between batches.</para>
		///		<para>
		///		There are individual state change methods for each of the parameters set using 
		///		this method. 
		///		Note that the default values in this method represent the defaults at system 
		///		start up too.</para>
		/// </remarks>
		/// <param name="function">The comparison function applied.</param>
		/// <param name="refValue">The reference value used in the comparison.</param>
		/// <param name="mask">
		///		The bitmask applied to both the stencil value and the reference value 
		///		before comparison.
		/// </param>
		/// <param name="stencilFailOp">The action to perform when the stencil check fails.</param>
		/// <param name="depthFailOp">
		///		The action to perform when the stencil check passes, but the depth buffer check still fails.
		/// </param>
		/// <param name="passOp">The action to take when both the stencil and depth check pass.</param>
		/// <param name="twoSidedOperation">
		///		If set to true, then if you render both back and front faces 
		///		(you'll have to turn off culling) then these parameters will apply for front faces, 
		///		and the inverse of them will happen for back faces (keep remains the same).
		/// </param>
		public abstract void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp, bool twoSidedOperation);
	
		public void SetStencilBufferParams() {
			SetStencilBufferParams(CompareFunction.AlwaysPass, 0, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}
		
		public void SetStencilBufferParams(CompareFunction function) {
			SetStencilBufferParams(function, 0, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue) {
			SetStencilBufferParams(function, refValue, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask) {
			SetStencilBufferParams(function, refValue, mask, 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp, StencilOperation depthFailOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, depthFailOp, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, depthFailOp, passOp, false);
		}

		#endregion SetStencilBufferParams Overloads 

		#region ClearFrameBuffer Overloads

		/// <summary>
		///		Clears one or more frame buffers on the active render target.
		/// </summary>
		/// <param name="buffers">
		///		Combination of one or more elements of <see cref="FrameBuffer"/>
		///		denoting which buffers are to be cleared.
		/// </param>
		/// <param name="color">The color to clear the color buffer with, if enabled.</param>
		/// <param name="depth">The value to initialize the depth buffer with, if enabled.</param>
		/// <param name="stencil">The value to initialize the stencil buffer with, if enabled.</param>
		public abstract void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth, int stencil);

		public void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth) {
			ClearFrameBuffer(buffers, color, depth, 0);
		}

		public void ClearFrameBuffer(FrameBuffer buffers, ColorEx color) {
			ClearFrameBuffer(buffers, color, 1.0f, 0);
		}

		public void ClearFrameBuffer(FrameBuffer buffers) {
			ClearFrameBuffer(buffers, ColorEx.Black, 1.0f, 0);
		}

		#endregion ClearFrameBuffer Overloads

        #endregion Abstract Methods

        #region Object overrides

        /// <summary>
        /// Returns the name of this RenderSystem.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return this.Name;
        }

        #endregion

        #region Abstract methods and properties

        /// <summary>
        /// 
        /// </summary>
        protected abstract internal bool DepthWrite { set; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract internal bool DepthCheck { set; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract internal CompareFunction DepthFunction { set; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract internal int DepthBias { set; }

        /// <summary>Sets the current view matrix.</summary>
        public abstract Matrix4 ViewMatrix	{ set; }

        /// <summary>Sets the current world matrix.</summary>
        public abstract Matrix4 WorldMatrix { set; }

        /// <summary>Sets the current projection matrix.</summary>
        public abstract Matrix4 ProjectionMatrix { set; }

        /// <summary>
        ///		Sets how to rasterise triangles, as points, wireframe or solid polys.
        /// </summary>
        protected abstract internal SceneDetailLevel RasterizationMode { set; }

        /// <summary>
        ///		Signifies the beginning of a frame, ie the start of rendering on a single viewport. Will occur
        ///		several times per complete frame if multiple viewports exist.
        /// </summary>
        protected abstract internal void BeginFrame();

        /// <summary>
        ///    Binds a given GpuProgram (but not the parameters). 
        /// </summary>
        /// <remarks>
        ///    Only one GpuProgram of each type can be bound at once, binding another
        ///    one will simply replace the existing one.
        /// </remarks>
        /// <param name="program"></param>
        public abstract void BindGpuProgram(GpuProgram program);

        /// <summary>
        ///    Bind Gpu program parameters.
        /// </summary>
        /// <param name="parms"></param>
        public abstract void BindGpuProgramParameters(GpuProgramType type, GpuProgramParameters parms);

        /// <summary>
        ///    Unbinds the current GpuProgram of a given GpuProgramType.
        /// </summary>
        /// <param name="type"></param>
        public abstract void UnbindGpuProgram(GpuProgramType type);

        /// <summary>
        ///		Ends rendering of a frame to the current viewport.
        /// </summary>
        protected abstract internal void EndFrame();

        /// <summary>
        ///    Internal method for updating all render targets attached to this rendering system.
        /// </summary>
        public void UpdateAllRenderTargets() {
            // Update all in order of priority
            // This ensures render-to-texture targets get updated before render windows
            for(int i = 0; i < renderTargets.Count; i++) {
                RenderTarget target = (RenderTarget)renderTargets[i];

                // only update if it is active
                if(target.IsActive) {
                    target.Update();
                }
            }
        }

        /// <summary>
        ///		Sets the details of a texture stage, to be used for all primitives
        ///		rendered afterwards. User processes would
        ///		not normally call this direct unless rendering
        ///		primitives themselves - the SubEntity class
        ///		is designed to manage materials for objects.
        ///		Note that this method is called by SetMaterial.
        /// </summary>
        /// <param name="stage">The index of the texture unit to modify. Multitexturing hardware
        //		can support multiple units (see NumTextureUnits)</param>
        /// <param name="enabled">Boolean to turn the unit on/off</param>
        /// <param name="textureName">The name of the texture to use - this should have
        ///		already been loaded with TextureManager.Load.</param>
        protected abstract internal void SetTexture(int stage, bool enabled, string textureName);

        /// <summary>
        ///		Sets a method for automatically calculating texture coordinates for a stage.
        /// </summary>
        /// <param name="stage">Texture stage to modify.</param>
        /// <param name="method">Calculation method to use</param>
        protected abstract internal void SetTextureCoordCalculation(int stage, TexCoordCalcMethod method);

        /// <summary>
        ///		Sets the index into the set of tex coords that will be currently used by the render system.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="index"></param>
        protected abstract internal void SetTextureCoordSet(int stage, int index);

        /// <summary>
        ///    Sets the filtering options for a given texture unit.
        /// </summary>
        /// <param name="unit">
        ///    The texture unit to set the filtering options for.
        /// </param>
        /// <param name="minFilter">
        ///    The filter used when a texture is reduced in size.
        /// </param>
        /// <param name="magFilter">
        ///    The filter used when a texture is magnified.
        /// </param>
        /// <param name="mipFilter">
        ///    The filter used between mipmap levels, FilterOptions.None disables mipmapping
        /// </param>
        protected void SetTextureUnitFiltering(int unit, FilterOptions minFilter, FilterOptions magFilter, FilterOptions mipFilter) {
            SetTextureUnitFiltering(unit, FilterType.Min, minFilter);
            SetTextureUnitFiltering(unit, FilterType.Mag, magFilter);
            SetTextureUnitFiltering(unit, FilterType.Mip, mipFilter);
        }

        /// <summary>
        ///    Sets a single filter for a given texture unit.
        /// </summary>
        /// <param name="unit">
        ///    The texture unit to set the filtering options for.
        /// </param>
        /// <param name="type">
        ///    The filter type.
        /// </param>
        /// <param name="filter">
        ///    The filter to be used.
        /// </param>
        protected abstract void SetTextureUnitFiltering(int unit, FilterType type, FilterOptions filter);

        /// <summary>
        ///		Sets the maximal anisotropy for the specified texture unit.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="index">maxAnisotropy</param>
        protected abstract internal void SetTextureLayerAnisotropy(int stage, int maxAnisotropy);

        /// <summary>
        ///		Sets the texture matrix for the specified stage.  Used to apply rotations, translations,
        ///		and scaling to textures.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="xform"></param>
        protected abstract internal void SetTextureMatrix(int stage, Matrix4 xform);

        /// <summary>
        ///		Sets the current viewport that will be rendered to.
        /// </summary>
        /// <param name="viewport"></param>
        protected abstract internal void SetViewport(Viewport viewport);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// DOC
        public virtual void Render(RenderOperation op) {
            int val;

            if(op.useIndices)
                val = op.indexData.indexCount;
            else
                val = op.vertexData.vertexCount;

            // calculate faces
            switch(op.operationType) {
                case RenderMode.TriangleList:
                    numFaces += val / 3;
                    break;
                case RenderMode.TriangleStrip:
                case RenderMode.TriangleFan:
                    numFaces += val - 2;
                    break;
                case RenderMode.PointList:
                case RenderMode.LineList:
                case RenderMode.LineStrip:
                    break;
            }

            // increment running vertex count
            numVertices += op.vertexData.vertexCount;
        }

        /// <summary>
        ///		Utility function for setting all the properties of a texture unit at once.
        ///		This method is also worth using over the individual texture unit settings because it
        ///		only sets those settings which are different from the current settings for this
        ///		unit, thus minimising render state changes.
        /// </summary>
        /// <param name="textureUnit">Index of the texture unit to configure</param>
        /// <param name="layer">Reference to a TextureLayer object which defines all the settings.</param>
        protected virtual internal void SetTextureUnit(int unit, TextureUnitState unitState) {
            // set the texture if it is different from the current
            SetTexture(unit, true, unitState.TextureName);

            // Tex Coord Set
            SetTextureCoordSet(unit, unitState.TextureCoordSet);

            // Texture layer filtering
            SetTextureUnitFiltering(
                unit, 
                unitState.GetTextureFiltering(FilterType.Min),
                unitState.GetTextureFiltering(FilterType.Mag),
                unitState.GetTextureFiltering(FilterType.Mip));

            // Texture layer anistropy
            SetTextureLayerAnisotropy(unit, unitState.TextureAnisotropy);

            // set the texture blending mode
            SetTextureBlendMode(unit, unitState.ColorBlendMode);

            // set the texture blending mode
            SetTextureBlendMode(unit, unitState.AlphaBlendMode);

            // this must always be set for OpenGL.  DX9 will ignore dupe render states like this (observed in the
            // output window when debugging with high verbosity), so there is no harm
            SetTextureAddressingMode(unit, unitState.TextureAddressing);

            bool anyCalcs = false;

            for(int i = 0; i < unitState.NumEffects; i++) {
                TextureEffect effect = unitState.GetEffect(i);

                switch(effect.type) {
                    case TextureEffectType.EnvironmentMap:
                        if((EnvironmentMap)effect.subtype == EnvironmentMap.Curved) {
                            SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMap);
                            anyCalcs = true;
                        }
                        else if((EnvironmentMap)effect.subtype == EnvironmentMap.Planar) {
                            SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapPlanar);
                            anyCalcs = true;
                        }
                        else if((EnvironmentMap)effect.subtype == EnvironmentMap.Reflection) {
                            SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapReflection);
                            anyCalcs = true;
                        }
                        else if((EnvironmentMap)effect.subtype == EnvironmentMap.Normal) {
                            SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapNormal);
                            anyCalcs = true;
                        }
                        break;

                    case TextureEffectType.Scroll:
                    case TextureEffectType.Rotate:
                    case TextureEffectType.Transform:
                        break;
                } // switch
            } // for

            // Ensure any previous texcoord calc settings are reset if there are now none
            if(!anyCalcs) {
                SetTextureCoordCalculation(unit, TexCoordCalcMethod.None);
                SetTextureCoordSet(unit, unitState.TextureCoordSet);
            }

            // set the texture matrix to that of the current layer for any transformations
            SetTextureMatrix(unit, unitState.TextureMatrix);

            // set alpha rejection
            SetAlphaRejectSettings(unit, unitState.AlphaRejectFunction, unitState.AlphaRejectValue);
        }

        /// <summary>
        ///		Sets the texture blend modes from a TextureLayer record.
        ///		Meant for use internally only - apps should use the Material
        ///		and TextureLayer classes.
        /// </summary>
        /// <param name="stage">Texture unit.</param>
        /// <param name="blendMode">Details of the blending modes.</param>
        public abstract void SetTextureBlendMode(int unit, LayerBlendModeEx blendMode);

        /// <summary>
        ///		Turns off a texture unit if not needed.
        /// </summary>
        /// <param name="textureUnit"></param>
        protected virtual internal void DisableTextureUnit(int unit) {
            SetTexture(unit, false, "");
        }

        /// <summary>
        ///	
        /// </summary>
        /// <param name="matrices"></param>
        /// <param name="count"></param>
        virtual internal void SetWorldMatrices(Matrix4[] matrices, ushort count) {
            if(!caps.CheckCap(Capabilities.VertexBlending)) {
                // save these for later during software vertex blending
                for(int i = 0; i < count; i++)
                    worldMatrices[i] = matrices[i];

                // reset the hardware world matrix to identity
                WorldMatrix = Matrix4.Identity;
            }

            // TODO: Implement hardware vertex blending in the API's
        }

        /// <summary>
        /// 
        /// </summary>
        internal void BeginGeometryCount() {
            numFaces = 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///    Attaches a render target to this render system.
        /// </summary>
        /// <param name="target"></param>
        public void AttachRenderTarget(RenderTarget target) {
            if(target.Priority == RenderTargetPriority.High) {
                // insert at the front of the list
                renderTargets.Insert(0, target);
            }
            else {
                // add to the end
                renderTargets.Add(target);
            }
        }

        // TODO: Implement
        public void DetachRenderTarget(string name) {}
        public void DetachRenderTarget(RenderTarget target) {}

        #endregion Public Methods

        #region IDisposable Members

        public void Dispose() {
            //if(textureMgr != null)
            //	textureMgr.Dispose();

        }

        #endregion
    }

}