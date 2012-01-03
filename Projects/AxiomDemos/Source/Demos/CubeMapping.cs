#region Namespace Declarations

using System;
using System.Collections;
using System.ComponentModel.Composition;
using Axiom.Collections;
using Axiom.Core;
using Axiom.CrossPlatform;
using Axiom.Overlays;
using Axiom.Input;
using Axiom.Math;
using Axiom.Graphics;
using System.Collections.Generic;

#endregion Namespace Declarations


namespace Axiom.Demos
{
	/// <summary>
	/// 	Summary description for EnvMapping.
	/// </summary>
#if !(WINDOWS_PHONE || XBOX || XBOX360)
    [Export(typeof(TechDemo))]
#endif
    public class CubeMapping : TechDemo
	{
		#region Perlin noise data and algorithms

		private float Lerp( float t, float a, float b )
		{
			return ( ( a ) + ( t ) * ( ( b ) - ( a ) ) );
		}

		private float Fade( float t )
		{
			return ( t ) * ( t ) * ( t ) * ( t ) * ( ( t ) * ( ( t ) * 6 - 15 ) + 10 );
		}

		private float Grad( int hash, float x, float y, float z )
		{
			int h = hash & 15;                      // CONVERT LO 4 BITS OF HASH CODE
			float u = h < 8 || h == 12 || h == 13 ? x : y,   // INTO 12 GRADIENT DIRECTIONS.
				v = h < 4 || h == 12 || h == 13 ? y : z;
			return ( ( h & 1 ) == 0 ? u : -u ) + ( ( h & 2 ) == 0 ? v : -v );
		}

		private float Noise3( float x, float y, float z )
		{
			int X = ( (int)System.Math.Floor( x ) ) & 255,                  // FIND UNIT CUBE THAT
				Y = ( (int)System.Math.Floor( y ) ) & 255,                  // CONTAINS POINT.
				Z = ( (int)System.Math.Floor( z ) ) & 255;
			x -= (float)System.Math.Floor( x );                                // FIND RELATIVE X,Y,Z
			y -= (float)System.Math.Floor( y );                                // OF POINT IN CUBE.
			z -= (float)System.Math.Floor( z );
			float u = Fade( x ),                                // COMPUTE FADE CURVES
				v = Fade( y ),                                // FOR EACH OF X,Y,Z.
				w = Fade( z );
			int A = p[ X ] + Y, AA = p[ A ] + Z, AB = p[ A + 1 ] + Z,      // HASH COORDINATES OF
				B = p[ X + 1 ] + Y, BA = p[ B ] + Z, BB = p[ B + 1 ] + Z;      // THE 8 CUBE CORNERS,

			return Lerp( w, Lerp( v, Lerp( u, Grad( p[ AA ], x, y, z ),  // AND ADD
				Grad( p[ BA ], x - 1, y, z ) ), // BLENDED
				Lerp( u, Grad( p[ AB ], x, y - 1, z ),  // RESULTS
				Grad( p[ BB ], x - 1, y - 1, z ) ) ),// FROM  8
				Lerp( v, Lerp( u, Grad( p[ AA + 1 ], x, y, z - 1 ),  // CORNERS
				Grad( p[ BA + 1 ], x - 1, y, z - 1 ) ), // OF CUBE
				Lerp( u, Grad( p[ AB + 1 ], x, y - 1, z - 1 ),
				Grad( p[ BB + 1 ], x - 1, y - 1, z - 1 ) ) ) );
		}

		// constant table
		int[] p = {
		151,160,137,91,90,15,
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,

		151,160,137,91,90,15,
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
	};

		#endregion Perlin noise data and algorithms

		#region Fields

		private bool noiseOn;
		private float keyDelay = 0.0f;
		private string[] meshes =
			{   "ogrehead.mesh",
				"razor.mesh",
				"geosphere12500.mesh",
				"knot.mesh",
				"geosphere19220.mesh",
				"geosphere1000.mesh",
				"geosphere8000.mesh",
				"sphere.mesh"
			};
		private string[] cubeMaps = { "cubescene.jpg", "cubemap.jpg", "early_morning.jpg", "cloudy_noon.jpg", "evening.jpg", "morning.jpg", "stormy.jpg" };
		private string[] blendModes = { "Add", "Modulate", "ModulateX2", "ModulateX4", "Source1" };
		private int currentMeshIndex = -1;
		private int currentLbxIndex = -1;
		private LayerBlendOperationEx currentLbx;
		private int currentCubeIndex = 0;
		private Mesh originalMesh;
		private Mesh clonedMesh;
		private Entity objectEntity = null;
		private SceneNode objectNode;
		private Material material;
		private List<Material> clonedMaterials = new List<Material>();
		private float displacement = 0.1f;
		private float density = 50.0f;
		private float timeDensity = 5.0f;
		private float tm = 0.0f;

		const string ENTITY_NAME = "CubeMappedEntity";
		const string MESH_NAME = "CubeMappedMesh";
		const string MATERIAL_NAME = "Examples/SceneCubeMap2";
		const string SKYBOX_MATERIAL = "Examples/SceneSkyBox2";

		#endregion Fields

		#region Constructors

		public CubeMapping()
		{

		}

		#endregion Constructors

		#region Methods

		public override void CreateScene()
		{
			scene.AmbientLight = new ColorEx( 1.0f, 0.5f, 0.5f, 0.5f );

			// create a default point light
			Light light = scene.CreateLight( "MainLight" );
			light.Position = new Vector3( 20, 80, 50 );

			// set the initial skybox
			scene.SetSkyBox( true, SKYBOX_MATERIAL, 2000.0f );

			// create a node that will be used to attach the objects to
			objectNode = scene.RootSceneNode.CreateChildSceneNode();

			// show overlay
			Overlay overlay = OverlayManager.Instance.GetByName( "Example/CubeMappingOverlay" );
			overlay.Show();
		}

		/// <summary>
		///
		/// </summary>
		private void ClearEntity()
		{
			// clear all cloned materials
			for ( int i = 0; i < clonedMaterials.Count; i++ )
			{
				MaterialManager.Instance.Unload( (Material)clonedMaterials[ i ] );
			}

			clonedMaterials.Clear();

			// detach and remove entity
			objectNode.DetachAllObjects();
			scene.RemoveEntity( objectEntity );

			// unload current cloned mesh
			MeshManager.Instance.Unload( clonedMesh );

			objectEntity = null;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="meshName"></param>
		private void PrepareEntity( string meshName )
		{
			if ( objectEntity != null )
			{
				ClearEntity();
			}

			// load mesh if necessary
			originalMesh = (Mesh)MeshManager.Instance.GetByName( meshName );

			// load mesh with shadow buffer so we can do fast reads
			if ( originalMesh == null )
			{
				originalMesh = (Mesh)MeshManager.Instance.Load(
					meshName, ResourceGroupManager.DefaultResourceGroupName,
					BufferUsage.StaticWriteOnly,
					BufferUsage.StaticWriteOnly,
					true, true, 1 );

				if ( originalMesh == null )
				{
					throw new Exception( string.Format( "Can't find mesh named '{0}'.", meshName ) );
				}
			}

			PrepareClonedMesh();

			// create a new entity based on the cloned mesh
			objectEntity = scene.CreateEntity( ENTITY_NAME, MESH_NAME );

			// setting the material here propogates it down to cloned sub entites, no need to clone them
			objectEntity.MaterialName = material.Name;

			Pass pass = material.GetTechnique( 0 ).GetPass( 0 );

			// add original sub mesh texture layers after the new cube map recently added
			for ( int i = 0; i < clonedMesh.SubMeshCount; i++ )
			{
				SubMesh subMesh = clonedMesh.GetSubMesh( i );
				SubEntity subEntity = objectEntity.GetSubEntity( i );

				// does this mesh have its own material set?
				if ( subMesh.IsMaterialInitialized )
				{
					string matName = subMesh.MaterialName;
					Material subMat = (Material)MaterialManager.Instance.GetByName( matName );

					if ( subMat != null )
					{
						subMat.Load();

						// Clone the sub entities material
						Material cloned = subMat.Clone( string.Format( "CubeMapTempMaterial#{0}", i ) );
						Pass clonedPass = cloned.GetTechnique( 0 ).GetPass( 0 );

						// add global texture layers to the existing material of the entity
						for ( int j = 0; j < pass.TextureUnitStageCount; j++ )
						{
							TextureUnitState orgLayer = pass.GetTextureUnitState( j );
							TextureUnitState newLayer = clonedPass.CreateTextureUnitState( orgLayer.TextureName );
							orgLayer.CopyTo( newLayer );
							newLayer.SetColorOperationEx( currentLbx );
						}

						// set the new material for the subentity and cache it
						subEntity.MaterialName = cloned.Name;
						clonedMaterials.Add( cloned );
					}
				}
			}

			// attach the entity to the scene
			objectNode.AttachObject( objectEntity );

			// update noise if currently set to on
			if ( noiseOn )
			{
				UpdateNoise();
			}
		}

		/// <summary>
		///
		/// </summary>
		private void PrepareClonedMesh()
		{
			// create a new mesh based on the original, only with different BufferUsage flags (inside PrepareVertexData)
			clonedMesh = MeshManager.Instance.CreateManual( MESH_NAME, ResourceGroupManager.DefaultResourceGroupName, null );
			clonedMesh.BoundingBox = (AxisAlignedBox)originalMesh.BoundingBox.Clone();
			clonedMesh.BoundingSphereRadius = originalMesh.BoundingSphereRadius;

			// clone the actual data
			clonedMesh.SharedVertexData = PrepareVertexData( originalMesh.SharedVertexData );

			// clone each sub mesh
			for ( int i = 0; i < originalMesh.SubMeshCount; i++ )
			{
				SubMesh orgSub = originalMesh.GetSubMesh( i );
				SubMesh newSub = clonedMesh.CreateSubMesh( string.Format( "ClonedSubMesh#{0}", i ) );

				if ( orgSub.IsMaterialInitialized )
				{
					newSub.MaterialName = orgSub.MaterialName;
				}

				// prepare new vertex data
				newSub.useSharedVertices = orgSub.useSharedVertices;
				newSub.vertexData = PrepareVertexData( orgSub.vertexData );

				// use existing index buffer as is since it wont be modified anyway
				newSub.indexData.indexBuffer = orgSub.indexData.indexBuffer;
				newSub.indexData.indexStart = orgSub.indexData.indexStart;
				newSub.indexData.indexCount = orgSub.indexData.indexCount;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="vertexData"></param>
		/// <returns></returns>
		private VertexData PrepareVertexData( VertexData orgData )
		{
			if ( orgData == null )
			{
				return null;
			}

			VertexData newData = new VertexData();
			// copy things that do not change
			newData.vertexCount = orgData.vertexCount;
			newData.vertexStart = orgData.vertexStart;

			// copy vertex buffers
			VertexDeclaration newDecl = newData.vertexDeclaration;
			VertexBufferBinding newBinding = newData.vertexBufferBinding;

			// prepare buffer for each declaration
			for ( int i = 0; i < orgData.vertexDeclaration.ElementCount; i++ )
			{
				VertexElement element = orgData.vertexDeclaration.GetElement( i );
				VertexElementSemantic ves = element.Semantic;
				short source = element.Source;
				HardwareVertexBuffer orgBuffer = orgData.vertexBufferBinding.GetBuffer( source );

				// check usage for the new buffer
				bool dynamic = false;

				switch ( ves )
				{
					case VertexElementSemantic.Normal:
					case VertexElementSemantic.Position:
						dynamic = true;
						break;
				}

				if ( dynamic )
				{
					HardwareVertexBuffer newBuffer =
						HardwareBufferManager.Instance.CreateVertexBuffer( orgBuffer.VertexDeclaration, orgBuffer.VertexCount, BufferUsage.DynamicWriteOnly, true );

					// copy and bind the new dynamic buffer
					newBuffer.CopyTo( orgBuffer, 0, 0, orgBuffer.Size, true );
					if ( newBinding.BindingCount > 0 && newBinding.GetBuffer( source ) != null )
					{
						source = (short)newBinding.BindingCount;
					}
					newBinding.SetBinding( source, newBuffer );
				}
				else
				{
					// use the existing buffer
					if ( newBinding.BindingCount > 0 && newBinding.GetBuffer( source ) != null )
					{
						source = (short)newBinding.BindingCount;
					}
					newBinding.SetBinding( source, orgBuffer );
				}

				// add the new element to the declaration
				newDecl.AddElement( source, element.Offset, element.Type, ves, element.Index );
			} // foreach

			return newData;
		}

		/// <summary>
		///
		/// </summary>
#if !AXIOM_SAFE_ONLY
unsafe
#endif
		private void UpdateNoise()
		{
#if AXIOM_SAFE_ONLY
			ITypePointer<float> sharedNormals = null;
#else
			float* sharedNormals = null;
#endif

			for ( int i = 0; i < clonedMesh.SubMeshCount; i++ )
			{
				SubMesh subMesh = clonedMesh.GetSubMesh( i );
				SubMesh orgSubMesh = originalMesh.GetSubMesh( i );

				if ( subMesh.useSharedVertices )
				{
					if ( sharedNormals == null )
					{
						sharedNormals = NormalsGetCleared( clonedMesh.SharedVertexData );
					}

					UpdateVertexDataNoiseAndNormals(
						clonedMesh.SharedVertexData,
						originalMesh.SharedVertexData,
						subMesh.indexData,
						sharedNormals );
				}
				else
				{
					var normals = NormalsGetCleared( subMesh.vertexData );

					UpdateVertexDataNoiseAndNormals(
						subMesh.vertexData,
						orgSubMesh.vertexData,
						subMesh.indexData,
						normals );

					NormalsSaveNormalized( subMesh.vertexData, normals );
				}
			} // for

			if ( sharedNormals != null )
			{
				NormalsSaveNormalized( clonedMesh.SharedVertexData, sharedNormals );
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="dstData"></param>
		/// <param name="orgdata"></param>
		/// <param name="indexData"></param>
		/// <param name="normals"></param>
#if AXIOM_SAFE_ONLY
		private void UpdateVertexDataNoiseAndNormals(VertexData dstData, VertexData orgData, IndexData indexData, ITypePointer<float> normals)
#else
		private unsafe void UpdateVertexDataNoiseAndNormals(VertexData dstData, VertexData orgData, IndexData indexData, float* normals)
#endif
		{
			// destination vertex buffer
			VertexElement dstPosElement = dstData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Position );
			HardwareVertexBuffer dstPosBuffer = dstData.vertexBufferBinding.GetBuffer( dstPosElement.Source );

			// source vertex buffer
			VertexElement orgPosElement = orgData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Position );
			HardwareVertexBuffer orgPosBuffer = orgData.vertexBufferBinding.GetBuffer( orgPosElement.Source );

			// lock the buffers
			var dstPosData = dstPosBuffer.Lock( BufferLocking.Discard );
			var orgPosData = orgPosBuffer.Lock( BufferLocking.ReadOnly );

			// get some raw pointer action goin on
			var dstPosPtr = dstPosData.ToFloatPointer();
			var orgPosPtr = orgPosData.ToFloatPointer();

			// make noise
			int numVerts = orgPosBuffer.VertexCount;

			for ( int i = 0; i < 3 * numVerts; i += 3 )
			{
				float n = 1 + displacement *
					Noise3( orgPosPtr[ i ] / density + tm,
								orgPosPtr[ i + 1 ] / density + tm,
								orgPosPtr[ i + 2 ] / density + tm );

				dstPosPtr[ i ] = orgPosPtr[ i ] * n;
				dstPosPtr[ i + 1 ] = orgPosPtr[ i + 1 ] * n;
				dstPosPtr[ i + 2 ] = orgPosPtr[ i + 2 ] * n;
			} // for

			// unlock the original position buffer
			orgPosBuffer.Unlock();

			//dstPosData = dstPosBuffer.Lock( BufferLocking.Discard );

			// calculate normals
			HardwareIndexBuffer indexBuffer = indexData.indexBuffer;

			var vertexIndices = indexBuffer.Lock( BufferLocking.ReadOnly ).ToShortPointer();
			int numFaces = indexData.indexCount / 3;

			for ( int i = 0, index = 0; i < numFaces; i++, index += 3 )
			{
				int p0 = vertexIndices[ index ];
				int p1 = vertexIndices[ index + 1 ];
				int p2 = vertexIndices[ index + 2 ];

				Vector3 v0 = new Vector3( dstPosPtr[ 3 * p0 ], dstPosPtr[ 3 * p0 + 1 ], dstPosPtr[ 3 * p0 + 2 ] );
				Vector3 v1 = new Vector3( dstPosPtr[ 3 * p1 ], dstPosPtr[ 3 * p1 + 1 ], dstPosPtr[ 3 * p1 + 2 ] );
				Vector3 v2 = new Vector3( dstPosPtr[ 3 * p2 ], dstPosPtr[ 3 * p2 + 1 ], dstPosPtr[ 3 * p2 + 2 ] );

				Vector3 diff1 = v1 - v2;
				Vector3 diff2 = v1 - v0;
				Vector3 fn = diff1.Cross( diff2 );

				// update the normal of each vertex in the current face
				normals[ 3 * p0 ] += fn.x;
				normals[ 3 * p0 + 1 ] += fn.y;
				normals[ 3 * p0 + 2 ] += fn.z;

				normals[ 3 * p1 ] += fn.x;
				normals[ 3 * p1 + 1 ] += fn.y;
				normals[ 3 * p1 + 2 ] += fn.z;

				normals[ 3 * p2 ] += fn.x;
				normals[ 3 * p2 + 1 ] += fn.y;
				normals[ 3 * p2 + 2 ] += fn.z;
			}

			// unlock index buffer
			indexBuffer.Unlock();

			// unlock destination vertex buffer
			dstPosBuffer.Unlock();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="vertexData"></param>
		/// <returns></returns>
#if AXIOM_SAFE_ONLY
		private ITypePointer<float> NormalsGetCleared(VertexData vertexData)
#else
		private unsafe float* NormalsGetCleared(VertexData vertexData)
#endif
		{
			VertexElement element = vertexData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Normal );
			HardwareVertexBuffer buffer = vertexData.vertexBufferBinding.GetBuffer( element.Source );
			var data = buffer.Lock( BufferLocking.Discard );
			var normPtr = data.ToFloatPointer();

			for ( int i = 0; i < buffer.VertexCount; i++ )
			{
				normPtr[ i ] = 0.0f;
			}
			buffer.Unlock();
			return normPtr;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="vertexData"></param>
		/// <param name="normals"></param>
#if AXIOM_SAFE_ONLY
		private void NormalsSaveNormalized( VertexData vertexData, ITypePointer<float> normals )
#else
		private unsafe void NormalsSaveNormalized( VertexData vertexData, float* normals )
#endif
		{
			VertexElement element = vertexData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Normal );
			HardwareVertexBuffer buffer = vertexData.vertexBufferBinding.GetBuffer( element.Source );

			var temp = buffer.Lock( BufferLocking.Normal );

			int numVerts = buffer.VertexCount;

			for ( int i = 0, index = 0; i < numVerts; i++, index += 3 )
			{
				Vector3 n = new Vector3( normals[ index ], normals[ index + 1 ], normals[ index + 2 ] );
				n.Normalize();

				normals[ index ] = n.x;
				normals[ index + 1 ] = n.y;
				normals[ index + 2 ] = n.z;
			}

			// don't forget to unlock!
			buffer.Unlock();
		}


		/// <summary>
		///
		/// </summary>
		private void ToggleBlending()
		{
			if ( ++currentLbxIndex == blendModes.Length )
			{
				currentLbxIndex = 0;
			}

			// get the current color blend mode to use
			currentLbx =
				(LayerBlendOperationEx)Enum.Parse( typeof( LayerBlendOperationEx ), blendModes[ currentLbxIndex ], true );

			PrepareEntity( meshes[ currentMeshIndex ] );

			// update the UI
			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/Material" ).Text =
				string.Format( "[M] Material: {0}", blendModes[ currentLbxIndex ] );
		}

		/// <summary>
		///
		/// </summary>
		private void ToggleCubeMap()
		{
			if ( ++currentCubeIndex == cubeMaps.Length )
			{
				currentCubeIndex = 0;
			}

			string cubeMapName = cubeMaps[ currentCubeIndex ];

			// toast the existing textures
			for ( int i = 0; i < material.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).NumFrames; i++ )
			{
				string texName = material.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).GetFrameTextureName( i );
				Texture tex = (Texture)TextureManager.Instance.GetByName( texName );
				TextureManager.Instance.Unload( tex );
			}

			// set the current entity material to the new cubemap texture
			material.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).SetCubicTextureName( cubeMapName, true );

			// get the current skybox cubemap and change it to the new one
			Material skyBoxMat = (Material)MaterialManager.Instance.GetByName( SKYBOX_MATERIAL );

			// toast the existing textures
			for ( int i = 0; i < skyBoxMat.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).NumFrames; i++ )
			{
				string texName = skyBoxMat.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).GetFrameTextureName( i );
				Texture tex = (Texture)TextureManager.Instance.GetByName( texName );
				TextureManager.Instance.Unload( tex );
			}

			// set the new cube texture for the skybox
			skyBoxMat.GetTechnique( 0 ).GetPass( 0 ).GetTextureUnitState( 0 ).SetCubicTextureName( cubeMapName, false );

			// reset the entity based on the new cubemap
			PrepareEntity( meshes[ currentMeshIndex ] );

			// reset the skybox
			scene.SetSkyBox( true, SKYBOX_MATERIAL, 2000.0f );

			// update the UI
			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/CubeMap" ).Text =
				string.Format( "[C] CubeMap: {0}", cubeMapName );
		}

		/// <summary>
		///    Toggles noise and updates the overlay to reflect the setting.
		/// </summary>
		private void ToggleNoise()
		{
			noiseOn = !noiseOn;

			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/Noise" ).Text =
				string.Format( "[N] Noise: {0}", noiseOn ? "on" : "off" );
		}

		/// <summary>
		///
		/// </summary>
		private void ToggleMesh()
		{
			if ( ++currentMeshIndex == meshes.Length )
			{
				currentMeshIndex = 0;
			}

			string meshName = meshes[ currentMeshIndex ];
			PrepareEntity( meshName );

			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/Object" ).Text =
				string.Format( "[O] Object: {0}", meshName );
		}

		void updateInfoDisplacement()
		{
			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/Displacement" ).Text = string.Format( "[1/2] Displacement: {0}", displacement );
		}
		void updateInfoDensity()
		{
			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/Density" ).Text = string.Format( "[3/4] Noise density: {0}", density );
		}
		void updateInfoTimeDensity()
		{
			OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/TimeDensity" ).Text = string.Format( "[5/6] Time density: {0}", timeDensity );
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		protected override void OnFrameStarted( object source, FrameEventArgs evt )
		{
			base.OnFrameStarted( source, evt );
			if ( evt.StopRendering )
				return;

			tm += evt.TimeSinceLastFrame / timeDensity;

			if ( noiseOn )
			{
				UpdateNoise();
			}

			if ( keyDelay > 0.0f )
			{
				keyDelay -= evt.TimeSinceLastFrame;

				if ( keyDelay < 0.0f )
				{
					keyDelay = 0.0f;
				}
			}

			// only check key input if the delay is not active
			if ( keyDelay == 0.0f )
			{
				// Adjust Density
				if ( input.IsKeyPressed( KeyCodes.D3 ) )
				{
					density += 0.1f * evt.TimeSinceLastFrame;
					if ( density >= 2 )
						density = 2;
					updateInfoDensity();
				}
				if ( input.IsKeyPressed( KeyCodes.D4 ) )
				{
					density -= 0.1f * evt.TimeSinceLastFrame;
					if ( density <= -2 )
						density = -2;
					updateInfoDensity();
				}

				// Adjust Displacement
				if ( input.IsKeyPressed( KeyCodes.D1 ) )
				{
					displacement += 10.0f * evt.TimeSinceLastFrame;
					if ( displacement >= 500f )
						displacement = 500f;
					updateInfoDisplacement();
				}
				if ( input.IsKeyPressed( KeyCodes.D2 ) )
				{
					displacement -= 10.0f * evt.TimeSinceLastFrame;
					if ( displacement <= 0.1f )
						displacement = 0.1f;
					updateInfoDisplacement();
				}

				// Adjust TimeDensity
				if ( input.IsKeyPressed( KeyCodes.D5 ) )
				{
					timeDensity += 10.0f * evt.TimeSinceLastFrame;
					if ( timeDensity >= 10.0f )
						timeDensity = 10.0f;
					updateInfoTimeDensity();
				}
				if ( input.IsKeyPressed( KeyCodes.D6 ) )
				{
					timeDensity -= 10.0f * evt.TimeSinceLastFrame;
					if ( timeDensity <= 1.0f )
						timeDensity = 1.0f;
					updateInfoTimeDensity();
				}

				// toggle noise
				if ( input.IsKeyPressed( KeyCodes.N ) )
				{
					ToggleNoise();
					keyDelay = 0.3f;
				}
				// toggle mesh object
				if ( input.IsKeyPressed( KeyCodes.O ) )
				{
					ToggleMesh();
					keyDelay = 0.3f;
				}
				// toggle cubemap texture
				if ( input.IsKeyPressed( KeyCodes.C ) )
				{
					ToggleCubeMap();
					keyDelay = 0.3f;
				}
				// toggle material blending
				if ( input.IsKeyPressed( KeyCodes.M ) )
				{
					ToggleBlending();
					keyDelay = 0.3f;
				}
			}
			updateInfoDensity();
			updateInfoDisplacement();
			updateInfoTimeDensity();
		}

		/// <summary>
		///    Override to do some of our own initialization after the engine is set up.
		/// </summary>
		/// <returns></returns>
		public override bool Setup()
		{
			if ( base.Setup() )
			{

				material = (Material)MaterialManager.Instance.GetByName( MATERIAL_NAME );

				ToggleNoise();
				ToggleMesh();
				ToggleBlending();
				OverlayManager.Instance.Elements.GetElement( "Example/CubeMapping/CubeMap" ).Text =
								string.Format( "[C] CubeMap: {0}", cubeMaps[ currentCubeIndex ] );
				return true;
			}

			return false;
		}

		#endregion Methods
	}
}