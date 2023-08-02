/*
Copyright (C) 1997-2001 Id Software, Inc.

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

*/
namespace QClient.game.types;

using adapters;
using util;

public class mframe_t
{
	public mframe_t(AIAdapter ai, float dist, EntThinkAdapter think)
	{
		this.ai = ai;
		this.dist = dist;
		this.think = think;
	}

	/** Empty constructor. */
	public mframe_t()
	{
	}

	public AIAdapter ai;
	public float dist;
	public EntThinkAdapter think;

	public void write(BinaryWriter f)
	{
		f.Write(this.ai);
		f.Write(this.dist);
		f.Write(this.think);
	}

	public void read(BinaryReader f)
	{
		this.ai = (AIAdapter)f.ReadAdapter();
		this.dist = f.ReadSingle();
		this.think = (EntThinkAdapter)f.ReadAdapter();
	}
}