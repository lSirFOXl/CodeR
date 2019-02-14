using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LvlInfo {
	
	public int lvl;
	public int stamina;
	public int mana;

	public List<ActionLC> actionList = new List<ActionLC>();

	public bool burnAfterPlay = false;

	[TextArea]
	public string descr;
}
