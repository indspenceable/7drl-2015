using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[System.Serializable]
public class MapTileComponent : MonoBehaviour {
	private TileTerrain terrain;
	public int x {get; private set;}
	public int y {get; private set;}
	public ItemDefinition item;
	private List<TileEffect> tileEffects = new List<TileEffect>();

	[SerializeField]
	private SpriteRenderer TESpriteRenderer;

	public bool passable {
		get {
			return terrain.passable;
		}
	}

	public TileTerrain.Interaction interaction {
		get {
			return terrain.interaction;
		}
	}

	public bool ShouldAvoid(Entity e) {
		return tileEffects.Exists(te => te.ShouldAvoid(e));
	}

	public bool HasPassive() {
		foreach(TileEffect te in tileEffects) {
			if (te.HasPassive()) {
				return true;
			}
		}
		return false;
	}
	public IEnumerator Passive(GameInstance instance) {
		foreach(TileEffect te in tileEffects) {
			yield return te.Passive(instance);
		}
		tileEffects.RemoveAll(te => te.Expired());
		UpdateTESprite();
	}

	public void SetTerrain(TileTerrain t) {
		this.terrain = t;
		this.GetComponent<SpriteRenderer>().sprite = terrain.sprite;
		tileEffects.Clear();
		if (terrain.tileEffect != null) {
			AddTileEffect(terrain.tileEffect);
		}
		UpdateTESprite();
	}
	public void SetCoords(int x, int y) {
		this.x = x;
		this.y = y;
		transform.position = new Vector3(x,y);
	}
	public void AddTileEffect(TileEffectDefinition t) {
		tileEffects.Add(new TileEffect(t, this));
		UpdateTESprite();
	}
	public void RemoveTileEffect(TileEffect te) {
		tileEffects.Remove(te);
		UpdateTESprite();
	}
	private void UpdateTESprite() {
		Sprite tesprite = null;
		foreach(TileEffect te in tileEffects) {
			if (te.def.sprite) {
				tesprite = te.def.sprite;
				break;
			}
		}
		TESpriteRenderer.sprite = tesprite;
	}
}
