using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vault", menuName = "Vault", order = 1)]
public class VaultDefinition : ScriptableObject {
	public Coord size;
	[TextArea]
	public string rawMap;

	public List<Vault> Process() {
		Vault rtn1 = new Vault();
		rtn1.size = size;
		rtn1.map = rawMap.Replace("\n", "");
		Vault rtn2 = new Vault();
		rtn2.size = size;
		rtn2.map = rawMap.Replace("\n", "");
		rtn2.Rotate();
		return new List<Vault>{ rtn1, rtn2 };
	}
}
