using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionLC {
    public actionType type;

	public float value;
	public int times = 1;
	public string spec;
	public string spec2;
	public string spec3;

	public bool isDead = false;

	public bool unShowTarget = false;

	public ActionLC(actionType ty = actionType.meleDamageWS, float va = 1, int ti = 1, string s = "", string s2 = "", string s3 = "", bool isD = false, bool ust = true){
		type = ty;
		value = va;
		trueTimes = ti;
		trueValue = va;
		times = ti;
		spec = s;
		spec2 = s2;
		spec3 = s3;
		isDead = isD;
		unShowTarget = ust;
	}

	//public Sprite sprite;
}
