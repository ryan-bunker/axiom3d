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
using System.IO;

namespace Axiom.FileSystem {
    /// <summary>
    /// Summary description for Folder.
    /// </summary>
    public class Folder : Archive {
		
        public Folder(string archiveName) : base(archiveName) {
        }

        public override void Load() {
        }

        public override Stream ReadFile(string fileName) {
            FileStream file = File.OpenRead(this.archiveName + Path.DirectorySeparatorChar + fileName);

            return file;
        }

        public override string[] GetFileNamesLike(string startPath, string pattern) {
            // TODO: Fix me

            // replace with wildcard if empty
            if(pattern == string.Empty)
                pattern = "*.*";

            string[] files = Directory.GetFiles(archiveName, pattern);

            // replace the full paths with just the file names
            for(int i = 0; i < files.Length; i++) {
                string[] temp = files[i].Split(new char[] { Path.DirectorySeparatorChar });
                files[i] = temp[temp.Length - 1];
            }

            return files;
        }

    }
}
