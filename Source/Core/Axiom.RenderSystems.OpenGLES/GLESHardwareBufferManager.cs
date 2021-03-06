#region LGPL License

/*
Axiom Graphics Engine Library
Copyright (C) 2003-2010 Axiom Project Team
This file is part of Axiom.RenderSystems.OpenGLES
C# version developed by bostich.

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

#endregion LGPL License

#region SVN Version Information

// <file>
//     <license see="http://axiomengine.sf.net/wiki/index.php/license.txt"/>
//     <id value="$Id$"/>
// </file>

#endregion SVN Version Information

#region Namespace Declarations

using System;

using Axiom.Core;
using Axiom.Graphics;

using GL = OpenTK.Graphics.ES11.GL;
using GLenum = OpenTK.Graphics.ES11.All;
using All = OpenTK.Graphics.ES11.All;

#endregion Namespace Declarations

namespace Axiom.RenderSystems.OpenGLES
{
	/// <summary>
	///   Implementation of HardwareBufferManager for OpenGL ES.
	/// </summary>
	public class GLESHardwareBufferManager : HardwareBufferManager
	{
		public GLESHardwareBufferManager()
		: base( new GLESHardwareBufferManagerBase() ) {}
		
		protected override void dispose( bool disposeManagedResources )
		{
			if ( !IsDisposed )
			{
				if ( disposeManagedResources )
				{
					_baseInstance.Dispose();
				}
			}
			
			base.dispose( disposeManagedResources );
		}
		
		public static GLenum GetGLUsage( BufferUsage usage )
		{
			return GLESHardwareBufferManagerBase.GetGLUsage( usage );
		}
		
		public static GLenum GetGLType( VertexElementType type )
		{
			return GLESHardwareBufferManagerBase.GetGLType( type );
		}
		
		/// <summary>
		/// Allows us to use a pool of memory as a scratch area for hardware buffers. 
		/// This is because GL.MapBuffer is incredibly inefficient, seemingly no matter 
		/// what options we give it. So for the period of lock/unlock, we will instead 
		/// allocate a section of a local memory pool, and use GL.BufferSubDataARB / GL.GetBufferSubDataARB instead.
		/// </summary>
		/// <param name="size"></param>
		public BufferBase AllocateScratch( int size )
		{
			return ( (GLESHardwareBufferManagerBase) _baseInstance ).AllocateScratch( size );
		}
		
		public void DeallocateScratch( BufferBase ptr )
		{
			( (GLESHardwareBufferManagerBase) _baseInstance ).DeallocateScratch( ptr );
		}
		
		public int MapBufferThreshold
		{
			get { return ( (GLESHardwareBufferManagerBase) _baseInstance ).MapBufferThreshold; }
			set { ( (GLESHardwareBufferManagerBase) _baseInstance ).MapBufferThreshold = value; }
		}
	}
}
