#region LGPL License

/*
Axiom Graphics Engine Library
Copyright � 2003-2011 Axiom Project Team

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

#region SVN Version Information

// <file>
//     <license see="http://axiom3d.net/wiki/index.php/license.txt"/>
//     <id value="$Id$"/>
// </file>

#endregion SVN Version Information

#region Namespace Declarations

using System;
using Axiom.Math;
using Axiom.Core;
using Axiom.Collections;

#endregion Namespace Declarations

#region Ogre Synchronization Information

// <ogresynchronization>
//     <file name="TagPoint.h"   revision="1.10.2.2" lastUpdated="10/15/2005" lastUpdatedBy="DanielH" />
//     <file name="TagPoint.cpp" revision="1.12" lastUpdated="10/15/2005" lastUpdatedBy="DanielH" />
// </ogresynchronization>

#endregion

namespace Axiom.Animating
{
	public class AttachmentPoint
	{
		private readonly string name;
		private readonly string parentBone;
		private readonly Quaternion orientation;
		private readonly Vector3 position;

		public AttachmentPoint( string name, string parentBone, Quaternion orientation, Vector3 position )
		{
			this.name = name;
			this.parentBone = parentBone;
			this.orientation = orientation;
			this.position = position;
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string ParentBone
		{
			get
			{
				return this.parentBone;
			}
		}

		public Quaternion Orientation
		{
			get
			{
				return this.orientation;
			}
		}

		public Vector3 Position
		{
			get
			{
				return this.position;
			}
		}
	}
}