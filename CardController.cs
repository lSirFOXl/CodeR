using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour {
	
	public int cardId = 0; //ID карты в колоде игрока
	public int id = 0; //глобальный ID карты
	public string title; //Название карты

	public float titleSize = 50; //Размер шрифта у названия карты
	public Sprite picture; //Картинка карты
	
	//
	public LvlInfo lvl1; 
	public LvlInfo lvl2;
	public LvlInfo lvl3;

	[HideInInspector]
	public List<LvlInfo> LvlInfoList = new List<LvlInfo>(); //

	public SpriteRenderer Shirt; //Спрайт-рендер карты
	public SpriteRenderer picturePace; //Спрай-трендер картинки карты
	public TextMesh staminaPlace; //Текст-меш ресурса "Выносливость"
	public TextMesh manaPlace; //Текст-меш ресурса "Выносливость"
	public TextMesh titlePlace; //Текст-меш названия карты

	public TextMeshPro descrPlaceTMP; //Текст-меш описания карты

	public GameObject CardAnimate; //Внутряняя оболочка карты

	public ShirtCollider shirtCollider; //Колайдер карты

	[HideInInspector]
	public Animator animator; 

	[HideInInspector]
	public int AnimationState = 0; //Текущий статус анимации

	[HideInInspector]
	public wUse whereUse = wUse.hand; //Где используется карта (Например, в руке или просто в списке карт колоды)
	private bool cardHover = false; //Выбрана ли карта в данный момент пользователем
	public int lvl = 0; //Текущий уровень карты

	public bool canTargetOnEnemy = false; //Может ли карта быть применена ко врагу
	public bool canTargetHero = false; //Может ли карта быть применена к игроку

	[HideInInspector]
	public bool inHand = true; //Находится ли карта в руке

	[HideInInspector]
	public Vector3 defaultPosition; //Стандартная позиция карты в руке (устанавливается при добавлении карты в руку в классе CardPannelController)
	[HideInInspector]
	public Quaternion defaultRotation; //Стандартный поворот карты в руке (устанавливается при добавлении карты в руку в классе CardPannelController)
	[HideInInspector]
	public Vector3 defaultSize; //Стандартный размер карты в руке (устанавливается при добавлении карты в руку в классе CardPannelController)

	[HideInInspector]
	public Vector3 currentPosition; //Текущая позиция карты в руке
	[HideInInspector]
	public Quaternion currentRotation; //Текущий поворот карты в руке
	[HideInInspector]
	public Vector3 currentSize; //Текущий размер карты в руке
	[HideInInspector]
	private Vector3 cardAnimatePosition = new Vector3(0,8.84f,0); //Текущая позиция карты в руке для CardAnimate
	[HideInInspector]
	public Quaternion cardAnimateRotation = new Quaternion(); //Текущий поворот карты в руке для CardAnimate
	[HideInInspector]
	private Vector3 cardAnimateSize = new Vector3(1,1,1); //Текущий размер карты в руке для CardAnimate
	[HideInInspector]
	public float goSpeed = 10; //Скорость анимации
	

	void Awake()
	{	
		Messenger.AddListener(GameEvents.REFRESH, setCardInfo);
		animator = GetComponent<Animator>();
		LvlInfoList = new List<LvlInfo>(){lvl1,lvl2,lvl3};
		currentSize = defaultSize;
	}

	void Start () {
		if(GlobalVars.gameController.autoCardInit)
		setCardInfo();	
	}

	public void setID(){
		id = GlobalVars.gameController.idCounter;
		GlobalVars.gameController.idCounter ++;
	}

	public CardController(){
		
	}

	public void CardControllerCopy(CardController copy){
		
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(GameEvents.REFRESH, setCardInfo);
	}

	public void setLocalPosition(Vector3 pos){
		transform.localPosition = pos;
	}

	void FixedUpdate()
	{
		/*Анимация карты*/
		if(whereUse == wUse.hand){
			transform.localPosition = Vector3.Lerp(transform.localPosition, currentPosition, Time.fixedDeltaTime*goSpeed);
			transform.localScale = Vector3.Lerp(transform.localScale, currentSize, Time.fixedDeltaTime*goSpeed);
			transform.rotation = Quaternion.Lerp(transform.rotation, currentRotation, Time.fixedDeltaTime*goSpeed);

			CardAnimate.transform.localPosition = Vector3.Lerp(CardAnimate.transform.localPosition, cardAnimatePosition, Time.fixedDeltaTime*goSpeed);
			CardAnimate.transform.localScale = Vector3.Lerp(CardAnimate.transform.localScale, cardAnimateSize, Time.fixedDeltaTime*goSpeed);
			CardAnimate.transform.rotation = Quaternion.Lerp(CardAnimate.transform.rotation, cardAnimateRotation, Time.fixedDeltaTime*goSpeed);
		}
	}

	void Update () {
		/*Реакция карты на действия мыши*/
		if (Input.GetMouseButtonDown(0) && AnimationState == 1){
			AnimationState = 2;
			shirtCollider.colliderObj.enabled = true;
			animator.SetTrigger("Light");
			if(canTargetHero)
			GlobalVars.heroUnit.targetIcon.SetBool("Targeted", true);
			
		}
		if (Input.GetMouseButtonUp(0) && AnimationState == 2 && cardHover == true){
			AnimationState = 1;
			animator.SetTrigger("UnLight");
			shirtCollider.colliderObj.enabled = false;
			if(canTargetHero)
			GlobalVars.heroUnit.targetIcon.SetBool("Targeted", false);
		}
		if (Input.GetMouseButtonUp(0) && AnimationState == 2 && cardHover == false){
			AnimationState = 0;
			if((!canTargetOnEnemy && !inHand) || (canTargetOnEnemy && GlobalVars.gameController.cardTarget != null )) {
				GlobalVars.gameController.cardAction(GlobalVars.heroUnit);
			}
			else {
				GlobalVars.gameController.activeCard = null;
				currentPosition = defaultPosition;
				currentRotation = defaultRotation;
				cardAnimateSize = new Vector3(1f,1f,1f);
				cardAnimateRotation = currentRotation;
				cardAnimatePosition = new Vector3(0,8.84f,0);
				animator.SetTrigger("UnLight");
				inHand = true;
				GlobalVars.gameController.cardPosition(false);
			}
			GlobalVars.lineGenGontroller.destroyLine();
			GlobalVars.lineGenGontroller.destroyLineAoe();

			shirtCollider.colliderObj.enabled = false;
			
			GlobalVars.gameController.clearCardSpacing();
			if(canTargetHero)
			GlobalVars.heroUnit.targetIcon.SetBool("Targeted", false);
		}
		if(AnimationState == 2 && !shirtCollider.isCollision && Camera.main.ScreenToViewportPoint(Input.mousePosition).y >= 0.26f && inHand){
			inHand = false;
			currentPosition = new Vector3(0,GlobalVars.SceneFirst.GetComponent<RectTransform>().rect.height/2-100,0);
			currentRotation = Quaternion.Euler(0,0,0);
			GlobalVars.gameController.cardPosition(false);
		}
		else if(AnimationState == 2 && Camera.main.ScreenToViewportPoint(Input.mousePosition).y <= 0.26f && !inHand){
			inHand = true;
			currentPosition = defaultPosition;
			currentRotation = defaultRotation;
			GlobalVars.gameController.cardPosition(false);
		}
	}
	
	public void mouseExit(){

	}

	void OnMouseOver()
    {
		cardHover = true;
		if(GlobalVars.gameController.activeCard == null && GlobalVars.gameController.menuOpen == false){
			GlobalVars.gameController.activeCard = this;
			if(AnimationState == 0)
			AnimationState = 1;

			cardAnimateSize = new Vector3(1.3f,1.3f,1.3f);
			cardAnimateRotation = Quaternion.Euler(0,0,0);
			cardAnimatePosition = new Vector3(0,8.84f-currentPosition.y/10+40/10,-1f);
		}
		else if(GlobalVars.gameController.inChooseCard){
			AnimationState = 50;
			GlobalVars.gameController.cardSpacing(this);
		}		
    }
	void OnMouseEnter()
	{
		if(GlobalVars.gameController.inMenuCardList){
			animator.SetTrigger("Light");
		}
	}

	void OnMouseExit()
    {
		cardHover = false;
		if(AnimationState == 1){
			GlobalVars.gameController.activeCard = null;

			cardAnimateSize = new Vector3(1f,1f,1f);
			cardAnimateRotation = currentRotation;
			cardAnimatePosition = new Vector3(0,8.84f,0);

			GlobalVars.gameController.clearCardSpacing();
			AnimationState = 0;
			
		}
		else if(AnimationState == 2){
		}
		else if(AnimationState == 50){
			GlobalVars.gameController.clearCardSpacing();
			AnimationState = 0;
		}
		
		if(GlobalVars.gameController.inMenuCardList){
			animator.SetTrigger("UnLight");
		}
    }

	void OnMouseUp()
	{
		if(GlobalVars.gameController.inChooseCard){
			if(GlobalVars.cardChose.addCardToChoise(this)){
				animator.SetTrigger("Light");
			}
			else if(GlobalVars.cardChose.removeCardFormChoise(this)){
				animator.SetTrigger("UnLight");
			}
		}
		if(GlobalVars.gameController.inMenuCardList){
			GlobalVars.gameController.uiSettings.openMenuCardDetail(this);
		}
	}

	/*Метод, отвечающий за отображение информации о карте*/
	public void setCardInfo(){
		picturePace.sprite = picture;
		staminaPlace.text = LvlInfoList[lvl].stamina.ToString();
		manaPlace.text = LvlInfoList[lvl].mana.ToString();
		descrPlaceTMP.text  = infoReplace(LvlInfoList[lvl].descr); //Вывод детальной информации о карте.

		titlePlace.text = title.ToString();

		titlePlace.fontSize = (int)titleSize;

		setShirtMaterial();
	}

	public string infoReplace(string descr){
		string text = descr;
		List<Vector2> listSpecInfo = new List<Vector2>();
		List<string> listStrSpecInfo = new List<string>();
		Vector2 prepareToSpeClis = new Vector2();
		bool isStart = false;
		int charCount = 0;

		/*
		В подробном описании используются специальные вставки (Например, {0.a.v}), которые замещаются необходмыми числами из дейстий карты (например, урон по существу)
		Ниже представлен код, который парсит описание и замещает вставки
		*/
		for (int i = 0; i < descr.Length; i++)
		{
			if(isStart == false && descr.Substring(i, 1) == "{"){
				prepareToSpeClis.x = i;
				isStart = true;
			}
			if(isStart == true){
				charCount++;
			}

			if(isStart == true && descr.Substring(i, 1) == "}"){
				prepareToSpeClis.y = charCount;
				charCount = 0;
				listStrSpecInfo.Add(descr.Substring((int)prepareToSpeClis.x, (int)prepareToSpeClis.y));
				listSpecInfo.Add(prepareToSpeClis);
				prepareToSpeClis = new Vector2();
				isStart = false;
			}
		}
		for (int i = 0; i < listStrSpecInfo.Count; i++)
		{
			string replace = "";
			string strWork = listStrSpecInfo[i].Substring(1, listStrSpecInfo[i].Length-2);
			
			List<string> replaceMeta  = new List<string>(strWork.Split('.'));
			float value;
			
			EvolveElement evolve = GlobalVars.gameController.returnEvolveTypes(title, int.Parse(replaceMeta[1]));
			EvolveOneCardInfo evolveCard = GlobalVars.gameController.returnEvolveCard(this, int.Parse(replaceMeta[1]));


			if(replaceMeta[0] == "a"){
				ActionLC actInf = LvlInfoList[lvl].actionList[int.Parse(replaceMeta[1])];
				ActionIcoTypeLC actMetaInf = new ActionIcoTypeLC();

				

				foreach (var item in GlobalVars.ActionMetaInfo)
				{
					if(item.type == actInf.type) {
						actMetaInf = item;
						break;
					}
				}
				
				if(replaceMeta[2] == "v"){
					
					value = actInf.value;
					if(evolve.name != "-222") value += evolve.value;
					if(evolveCard.spec != "asd111") value += evolveCard.value;

					if(actMetaInf.physical == true)
					value = Mathf.Ceil(value*GlobalVars.heroUnit.getPassiveEffectInfoByEffect(passiveEffects.damageMultipler, getParamsInfoPassiveItems.percentValueToMultiply).value);

					replace = value.ToString();
				}
				if(replaceMeta[2] == "t"){
					value = actInf.value;

					value = LvlInfoList[lvl].actionList[int.Parse(replaceMeta[1])].times;
					replace = value.ToString();
				}
				if(replaceMeta[2] == "s"){
					replace = LvlInfoList[lvl].actionList[int.Parse(replaceMeta[1])].spec;
				}
			}
			text = text.Replace(listStrSpecInfo[i], replace);
		}

		return text;
	}

	public void genLine(UnitController unit){
		GlobalVars.lineGenGontroller.genLine(CardAnimate.transform, unit.center.transform);
	}

	public void destroyLine(UnitController unit){
		GlobalVars.lineGenGontroller.destroyLine();
	}

	void setShirtMaterial(){
		Shirt.material = GlobalVars.cardMaterials[lvl];
	}


}
