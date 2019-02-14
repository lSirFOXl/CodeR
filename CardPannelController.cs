using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPannelController : MonoBehaviour {

	//Метод для вычисления позиции и поворота карт в руке
	public void cardPosition(List<CardController> hand /*Список карт в руке игрока */, int maxCardInHand){
		int cardCanAnimateCounter = 0;

		//Тут идет провекрка сколько карт в руке(массиве) игрока находятся(видимы) в руке игрока 
		foreach (var item in hand)
		{	
			if(item.inHand == true) cardCanAnimateCounter++;
		}

		//Тут происходят вычисления отступов между картами с учето максимального кол-ва карт и текущим их кол-вом
		float radius = 1000;
		float maxAngle = 30f;
		float angleStep = Mathf.PI/180f * (maxAngle/(maxCardInHand-1));
		float angleOffset = Mathf.PI/180f * (180f+90f+(angleStep*180/Mathf.PI*(cardCanAnimateCounter-1)/2));
		int cardNeedRepos = 0;

		for (int i = 0; i < hand.Count; i++)
		{
			CardController cardV = hand[i];
			if(cardV.inHand == true){
				//Тут задаются дефолтные значения для позиции, поворота и размера карты.
				cardV.defaultPosition = new Vector3(
					radius*Mathf.Cos(angleStep*cardNeedRepos-angleOffset), 
					radius*Mathf.Sin(angleStep*cardNeedRepos-angleOffset)-radius-30f,
					1f*cardNeedRepos);
				cardV.defaultRotation = Quaternion.Euler(0,0,(angleStep*cardNeedRepos-angleOffset)*180/Mathf.PI-90);
				cardV.defaultSize = new Vector3(10,10,10);

				cardV.currentPosition = cardV.defaultPosition;
				cardV.currentRotation = cardV.defaultRotation;
				if(cardV.AnimationState == 0)
				cardV.cardAnimateRotation = cardV.defaultRotation;				
				
				//Если карта неактивна (только пришла в руку), то она убирается за экран игрока
				if(cardV.gameObject.active == false){
					cardV.gameObject.transform.localPosition = new Vector3(0,-200,1f*cardNeedRepos);
					cardV.gameObject.transform.rotation = Quaternion.Euler(0,0,(angleStep*(cardCanAnimateCounter-1)-angleOffset)*180/Mathf.PI-90);
				}

				cardNeedRepos++;
			}
		}
		StartCoroutine(setCardActive(hand));
	}

	public IEnumerator setCardActive(List<CardController> hand){
		//Постепенное включение неактивных карт (только пришедшие в руку)
		for (int i = 0; i < handCC.Count; i++)
		{
			if(hand[i].inHand && hand[i].gameObject.active == false){
				hand[i].gameObject.transform.localPosition = new Vector3(0,-100,1f*i);
				hand[i].gameObject.SetActive(true);
				yield return new WaitForSeconds(0.05f);
			}
		}
		
	}
}
