using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DjikstraMap {
	static Coord[] offsets = new Coord[]{
		new Coord(0,1),
		new Coord(0,-1),
		new Coord(-1,0),
		new Coord(1,0),
		new Coord(0, 0),
	};
	float[][] map;
	Coord size;
	float unreachable;
	public DjikstraMap(int width, int height) {
		size = new Coord(width, height);
		map = new float[width][];
		unreachable = width*height+1;
		for (int x = 0; x < width; x += 1) {
			map[x] = new float[height];
			for (int y = 0; y < height; y += 1) {
				map[x][y] = unreachable;
			}
		}
	}
	public static DjikstraMap BuildAndCalculate(int width, int height, TileCheck passable, Entity e, params Coord[] goals) {
//		return BuildAndCalculate(width, height, passable, goals);
//	}
//	public static DjikstraMap BuildAndCalculate(int width, int height, TileCheck passable, Coord[] goals) {
		DjikstraMap map = new DjikstraMap(width, height);
		foreach(Coord goal in goals) {
			map.SetGoal(goal);
		}
		map.Calculate(passable, e);
		return map;
	}
	public void SetGoal(Coord p) {
		map[p.x][p.y] = 0;
	}

	public delegate bool TileCheck(Coord c, Entity e);
	public void Calculate(TileCheck passable, Entity e) {
		bool madeChangeLastIteration = true;
		while (madeChangeLastIteration) {
			madeChangeLastIteration = false;
			for ( int x = 0; x < size.x; x += 1) {
				for (int y = 0; y < size.y; y += 1) {
					float minValue = unreachable;
					foreach(Coord o in offsets) {
						Coord c = o.plus(x,y);
						if (InBounds(c)) {
							minValue = Mathf.Min(minValue, map[c.x][c.y]);
						}
					}
					if (minValue+1 < map[x][y] && passable(new Coord (x,y), e)) {
						madeChangeLastIteration = true;
						map[x][y] = minValue+1;
					}
				}
			}
		}
	}
	public float Value(int x, int y) {
		return map[x][y];
	}
	public float Value(Coord c) {
		return Value(c.x, c.y);
	}
	public void Scale(float f) {
		for ( int x = 0; x < size.x; x += 1) {
			for (int y = 0; y < size.y; y += 1) {
				if (map[x][y] != unreachable) {
					map[x][y] *= f;
				}
			}
		}
	}
	public Coord FindBestNeighbor(Coord p, float multiplier=1) {
		float value = unreachable;
		Coord rtn = p;
		foreach(Coord o in offsets) {
			Coord c = o + p;
			if (InBounds(c) && Value(c) != unreachable && Value(c)*multiplier < value) {
				rtn = c;
				value = Value(c)*multiplier;
			}
		}
		return rtn;
	}

	private bool InBounds(Coord c) {
		return c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;
	}
}
