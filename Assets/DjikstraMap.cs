using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DjikstraMap {
	static Coord[] offsets = new Coord[]{
		new Coord(0,1),
		new Coord(0,-1),
		new Coord(-1,0),
		new Coord(1,0),
	};
	int[][] map;
	Coord size;
	int unreachable;
	public DjikstraMap(int width, int height) {
		size = new Coord(width, height);
		map = new int[width][];
		unreachable = width*height+1;
		for (int x = 0; x < width; x += 1) {
			map[x] = new int[height];
			for (int y = 0; y < height; y += 1) {
				map[x][y] = unreachable;
			}
		}
	}
	public static DjikstraMap BuildAndCalculate(int width, int height, TileCheck passable, params Coord[] goals) {
//		return BuildAndCalculate(width, height, passable, goals);
//	}
//	public static DjikstraMap BuildAndCalculate(int width, int height, TileCheck passable, Coord[] goals) {
		DjikstraMap map = new DjikstraMap(width, height);
		foreach(Coord goal in goals) {
			map.SetGoal(goal);
		}
		map.Calculate(passable);
		return map;
	}
	public void SetGoal(Coord p) {
		map[p.x][p.y] = 1;
	}

	public delegate bool TileCheck(Coord c);
	public void Calculate(TileCheck passable) {
		bool madeChangeLastIteration = true;
		while (madeChangeLastIteration) {
			madeChangeLastIteration = false;
			for ( int x = 0; x < size.x; x += 1) {
				for (int y = 0; y < size.y; y += 1) {
					int minValue = unreachable;
					foreach(Coord o in offsets) {
						Coord c = o.plus(x,y);
						if (InBounds(c)) {
							minValue = Mathf.Min(minValue, map[c.x][c.y]);
						}
					}
					if (minValue+1 < map[x][y] && passable(new Coord (x,y))) {
						madeChangeLastIteration = true;
						map[x][y] = minValue+1;
					}
				}
			}
		}
	}
	public int Value(int x, int y) {
		return map[x][y];
	}
	public int Value(Coord c) {
		return Value(c.x, c.y);
	}
	public Coord FindWorstNeighbor(Coord p) {
//		int value = -1;
//		Coord rtn = p;
//		foreach(Coord o in offsets) {
//			Coord c = o + p;
//			if (InBounds(c) && Value(c) != unreachable && Value(c) > value) {
//				rtn = c;
//				value = Value(c);
//			}
//		}
//		return rtn;
		return FindBestNeighbor(p, -1);
	}
	public Coord FindBestNeighbor(Coord p, int multiplier=1) {
		int value = unreachable;
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
