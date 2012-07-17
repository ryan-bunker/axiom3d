﻿#region MIT/X11 License

//Copyright © 2003-2011 Axiom 3D Rendering Engine Project
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

#endregion License

using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Math;
using Axiom.Media;

namespace Axiom.Samples.Core
{
	internal class VolumeTextureSample : SdkSample
	{
		protected Texture ptex;
		protected SimpleRenderable vrend;
		protected SimpleRenderable trend;
		protected SceneNode snode, fnode;
		protected AnimationState animState;
		protected float globalReal, globalImag, globalTheta, xtime;

		public VolumeTextureSample()
		{
			Metadata[ "Title" ] = "Volume Textures";
			Metadata[ "Description" ] = "Demonstrates the use of volume textures.";
			Metadata[ "Thumbnail" ] = "thumb_voltex.png";
			Metadata[ "Category" ] = "Unsorted";
		}

		public override void TestCapabilities( RenderSystemCapabilities capabilities )
		{
			if( !capabilities.HasCapability( Capabilities.Texture3D ) )
			{
				throw new AxiomException( "Your card does not support 3D textures, so cannot run this demo. Sorry!" );
			}
		}

		protected override void SetupView()
		{
			base.SetupView();

			Camera.Position = new Vector3( 220, -2, 176 );
			Camera.LookAt( Vector3.Zero );
			Camera.Near = 5;
			Camera.PolygonMode = PolygonMode.Wireframe;
		}

		public override bool FrameRenderingQueued( FrameEventArgs evt )
		{
			xtime += evt.TimeSinceLastFrame;
			xtime = (float)System.Math.IEEERemainder( xtime, 10 );
			( (ThingRendable)trend ).AddTime( evt.TimeSinceLastFrame * 0.05f );
			animState.AddTime( evt.TimeSinceLastFrame );
			return base.FrameRenderingQueued( evt );
		}

		protected override void SetupContent()
		{
			ptex = TextureManager.Instance.CreateManual( "DynaTex", ResourceGroupManager.DefaultResourceGroupName,
			                                             TextureType.ThreeD, 64, 64, 64, 0, Media.PixelFormat.A8R8G8B8, TextureUsage.Default, null );

			SceneManager.AmbientLight = new ColorEx( 0.6f, 0.6f, 0.6f );
			SceneManager.SetSkyBox( true, "Examples/MorningSkyBox", 50 );

			Light light = SceneManager.CreateLight( "VolumeTextureSampleLight" );
			light.Diffuse = new ColorEx( 0.75f, 0.75f, 0.80f );
			light.Specular = new ColorEx( 0.9f, 0.9f, 1 );
			light.Position = new Vector3( -100, 80, 50 );
			SceneManager.RootSceneNode.AttachObject( light );

			// Create volume renderable
			snode = SceneManager.RootSceneNode.CreateChildSceneNode( Vector3.Zero );

			vrend = new VolumeRendable( 32, 750, "DynaTex" );
			snode.AttachObject( vrend );

			trend = new ThingRendable( 90, 32, 7.5f );
			trend.Material = (Material)MaterialManager.Instance.GetByName( "Examples/VTDarkStuff" );
			trend.Material.Load();
			snode.AttachObject( trend );

			fnode = SceneManager.RootSceneNode.CreateChildSceneNode();
			Entity head = SceneManager.CreateEntity( "head", "ogrehead.mesh" );
			fnode.AttachObject( head );

			Animation anim = SceneManager.CreateAnimation( "OgreTrack", 10 );
			anim.InterpolationMode = InterpolationMode.Spline;

			NodeAnimationTrack track = anim.CreateNodeTrack( 0, fnode );
			TransformKeyFrame key = track.CreateNodeKeyFrame( 0 );
			key.Translate = new Vector3( 0, -15, 0 );
			key = track.CreateNodeKeyFrame( 5 );
			key.Translate = new Vector3( 0, 15, 0 );
			key = track.CreateNodeKeyFrame( 10 );
			key.Translate = new Vector3( 0, -15, 0 );
			animState = SceneManager.CreateAnimationState( "OgreTrack" );
			animState.IsEnabled = true;

			globalReal = 0.4f;
			globalImag = 0.6f;
			globalTheta = 0.0f;

			CreateControls();

			DragLook = true;

			Generate();
		}

		protected override void CleanupContent()
		{
			if( vrend != null )
			{
				if( !vrend.IsDisposed )
				{
					vrend.Dispose();
				}

				vrend = null;
			}

			if( trend != null )
			{
				if( !trend.IsDisposed )
				{
					trend.Dispose();
				}

				trend = null;
			}

			if( ptex != null )
			{
				TextureManager.Instance.Remove( "DynaTex" );
				ptex = null;
			}

			base.CleanupContent();
		}

		protected void CreateControls()
		{
			TrayManager.CreateLabel( TrayLocation.TopLeft, "JuliaParamLabel", "Julia Parameters", 200 );
			Slider sl = TrayManager.CreateThickSlider( TrayLocation.TopLeft, "RealSlider", "Real", 200, 80, -1, 1, 50 );
			sl.SetValue( globalReal, false );
			sl.SliderMoved += new SliderMovedHandler( sl_SliderMoved );
			sl = TrayManager.CreateThickSlider( TrayLocation.TopLeft, "ImagSlider", "Imag", 200, 80, -1, 1, 50 );
			sl.SetValue( globalImag, false );
			sl.SliderMoved += new SliderMovedHandler( sl_SliderMoved );
			sl = TrayManager.CreateThickSlider( TrayLocation.TopLeft, "ThetaSlider", "Theta", 200, 80, -1, 1, 50 );
			sl.SetValue( globalTheta, false );
			sl.SliderMoved += new SliderMovedHandler( sl_SliderMoved );

			TrayManager.ShowCursor();
		}

		private void sl_SliderMoved( object sender, Slider slider )
		{
			switch( slider.Name )
			{
				case "RealSlider":
					globalReal = slider.Value;
					break;
				case "ImagSlider":
					globalImag = slider.Value;
					break;
				case "ThetaSlider":
					globalTheta = slider.Value;
					break;
			}
			Generate();
		}

		protected void Generate()
		{
			Julia julia = new Julia( globalReal, globalImag, globalTheta );
			float scale = 2.5f;
			float vcut = 29.0f;
			float vscale = 1.0f / vcut;

			HardwarePixelBuffer buffer = ptex.GetBuffer( 0, 0 );

			LogManager.Instance.Write( "Volume Texture Sample [info]: HardwarePixelBuffer " + buffer.Width + "x" + buffer.Height );

			buffer.Lock( BufferLocking.Normal );
			PixelBox pb = buffer.CurrentLock;

			LogManager.Instance.Write( "Volume Texture Sample [info]: PixelBox " + pb.Width + "x" + pb.Height + "x" + pb.Depth );

			unsafe
			{
				uint* pbptr = (uint*)pb.Data;
				for( int z = pb.Front; z < pb.Back; z++ )
				{
					for( int y = pb.Top; y < pb.Bottom; y++ )
					{
						for( int x = pb.Left; x < pb.Right; x++ )
						{
							if( z == pb.Front || z == ( pb.Back - 1 ) || y == pb.Top || y == ( pb.Bottom - 1 ) ||
							    x == pb.Left || x == ( pb.Right - 1 ) )
							{
								pbptr[ x ] = 0;
							}
							else
							{
								float val = julia.Eval( ( (float)x / pb.Width - 0.5f ) * scale,
								                        ( (float)y / pb.Height - 0.5f ) * scale,
								                        ( (float)z / pb.Depth - 0.5f ) * scale );
								if( val > vcut )
								{
									val = vcut;
								}

								PixelConverter.PackColor( (float)x / pb.Width, (float)y / pb.Height, (float)z / pb.Depth, ( 1.0f - ( val * vscale ) ) * 0.7f, PixelFormat.A8R8G8B8, (System.IntPtr)( &pbptr[ x ] ) );
							}
						}
						pbptr += pb.RowPitch;
					}
					pbptr += pb.SliceSkip;
				}
				buffer.Unlock();
			}
		}
	}
}