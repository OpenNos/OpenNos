/*!
@file GridRect.cs
@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
		<http://github.com/juhgiyo/eppathfinding.cs>
@date July 16, 2013
@brief GridRect Interface
@version 2.0

@section LICENSE

The MIT License (MIT)

Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

@section DESCRIPTION

An Interface for the GridRect Struct.

*/

namespace EpPathFinding
{
    public class GridRect
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public GridRect()
        {
            MinX = 0;
            MinY = 0;
            MaxX = 0;
            MaxY = 0;
        }

        public GridRect(int iMinX, int iMinY, int iMaxX, int iMaxY)
        {
            MinX = iMinX;
            MinY = iMinY;
            MaxX = iMaxX;
            MaxY = iMaxY;
        }

        public GridRect(GridRect b)
        {
            MinX = b.MinX;
            MinY = b.MinY;
            MaxX = b.MaxX;
            MaxY = b.MaxY;
        }

        public override int GetHashCode()
        {
            return MinX ^ MinY ^ MaxX ^ MaxY;
        }

        public override bool Equals(object obj)
        {
            // Unlikely to compare incorrect type so removed for performance
            //if (!(obj.GetType() == typeof(GridRect)))
            //    return false;
            GridRect p = (GridRect)obj;
            if (ReferenceEquals(null, p))
            {
                return false;
            }
            // Return true if the fields match:
            return (MinX == p.MinX) && (MinY == p.MinY) && (MaxX == p.MaxX) && (MaxY == p.MaxY);
        }

        public bool Equals(GridRect p)
        {
            if (ReferenceEquals(null, p))
            {
                return false;
            }
            // Return true if the fields match:
            return (MinX == p.MinX) && (MinY == p.MinY) && (MaxX == p.MaxX) && (MaxY == p.MaxY);
        }

        public static bool operator ==(GridRect a, GridRect b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (ReferenceEquals(null, a))
            {
                return false;
            }
            if (ReferenceEquals(null, b))
            {
                return false;
            }
            // Return true if the fields match:
            return (a.MinX == b.MinX) && (a.MinY == b.MinY) && (a.MaxX == b.MaxX) && (a.MaxY == b.MaxY);
        }

        public static bool operator !=(GridRect a, GridRect b)
        {
            return !(a == b);
        }

        public GridRect Set(int iMinX, int iMinY, int iMaxX, int iMaxY)
        {
            MinX = iMinX;
            MinY = iMinY;
            MaxX = iMaxX;
            MaxY = iMaxY;
            return this;
        }
    }
}