
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
 
[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class CharacterControllerEXPhoton : MonoBehaviourPunCallbacks, IPunObservable {
	
 
	public float speed = 1.5f; //Скорость
	public float maxVelocityChange = 10.0f;

	public Transform bottom; //Тело
	public Transform head; //Голова

	public Camera mainCamera; //Камера персонажа
	public GameObject GvRMain; //Класс для ВР

	public float sensitivity = 5f; // Чувствительность мыши
	public float headMinY = -40f; // Ограничение угла для головы
	public float headMaxY = 40f; // Ограничение угла для головы

	public KeyCode jumpButton = KeyCode.Space; // Клавиша для прыжка
	public float jumpForce = 10; // Сила прыжка
	public float jumpDistance = 1.1f; // Расстояние от центра объекта, до 

	private Vector3 direction;
	private float h, v;
	private int layerMask; //Маска для проверки касается ли персонаж пола
	private Rigidbody body;
	private float rotationY;
	private float rotationX;

	private bool isGrounded = false;
	private bool isTouch = false;

	public bool isVr = true;
	
	//PHOTON

	private Vector3 networkPos = new Vector3();
	private Quaternion networkRot = new Quaternion();




	void Awake()
	{
		//Если запущено с Android, то активирует обект, необходимый для работы VR 
		#if UNITY_ANDROID
			isVr = true;
			GvRMain.SetActive(true);
		#else
			isVr = false;
			GvRMain.SetActive(false);
		#endif

		networkPos = transform.position;
		networkRot = transform.rotation;
		body = GetComponent<Rigidbody>();

		//Если персонаж создан текущим пользователем, то включаем для него камеру, а если нет, то выключаем камеру и замораживаем перемещение у rigidbody
		if(photonView.IsMine) mainCamera.enabled = true;
		else{
			mainCamera.enabled = false;
			body.constraints = RigidbodyConstraints.FreezePosition;
		} 
	}
	
	
	void Start () 
	{
		body = GetComponent<Rigidbody>();
		body.freezeRotation = true;
		layerMask = 1 << gameObject.layer | 1 << 2;
		layerMask = ~layerMask;

		
	}
	
	void FixedUpdate()
	{
		
		Vector3 velocity = body.velocity;
		Vector3 velocityChange = (direction - velocity);
		//Ограничивает скорость перемещения
	    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
	    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
	    velocityChange.y = 0;

		//Толкает персонажа в нужном направлении
		body.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	bool GetJump()
	{
		//Пускает луч из центра персонажа вниз
		RaycastHit hit;
		Ray ray = new Ray(transform.position, Vector3.down);
		//Если длина луча меньше jumpDistance, то персонаж стоит на полу
		if (Physics.Raycast(ray, out hit, jumpDistance, layerMask))
		{
			return true;
		}
		
		return false;
	}

	void Update () 
	{
		//Если персонаж создан текущим пользователем, то даем ему управление этим персонажем, а если нет, то данные о позиции и повороте персонажа берутся с сервера и отображаются пользователю 
		if(photonView.IsMine){
			isGrounded = GetJump(); //Проверка находится ли персонаж на земле
			h = Input.GetAxis("Horizontal");
			v = Input.GetAxis("Vertical");

			//Если запущено с андроида, то управляется в VR-режиме, а если нет, то клавиатурой и мышкой 
			#if UNITY_ANDROID
				bottom.eulerAngles = new Vector3(0, mainCamera.transform.localEulerAngles.y, mainCamera.transform.localEulerAngles.z); //поворот тела задается в соответствии с поворотом камеры, которое устанавливается VR-ассетом 
				if (Input.touchCount > 0) { //Проверяет нажате на экран
					Touch touch = Input.GetTouch(0);
					if (touch.phase == TouchPhase.Began) {
						isTouch = true;
					}
					if (touch.phase == TouchPhase.Ended) {
						isTouch = false;
					}
				}  

				if(isTouch) v = 0.7f;
				else if(!isTouch) v = 0f;

				direction = new Vector3(h, 0, v); //Получение направление перемещения персонажа
				direction = mainCamera.transform.TransformDirection(direction); //Поправка направления персонажа с учетом поворота камеры
				direction = new Vector3(direction.x * speed * Time.deltaTime, 0, direction.z * speed * Time.deltaTime); //Умножение направления персонажа на скорость
			#else
				//Тут всё тоже самое как и для VR, только поворот камеры задается уже мышкой
				rotationX += Input.GetAxis("Mouse X") * sensitivity;
				rotationY += Input.GetAxis("Mouse Y") * sensitivity;
				rotationY = Mathf.Clamp (rotationY, headMinY, headMaxY);
				head.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

				bottom.eulerAngles = new Vector3(0, head.localEulerAngles.y, head.localEulerAngles.z) ;

				direction = new Vector3(h, 0, v);
				direction = head.TransformDirection(direction);
				direction = new Vector3(direction.x * speed * Time.deltaTime, 0, direction.z * speed * Time.deltaTime);
			#endif	
			
			//Если нажата клавиша для прыжка и он стоит на земле, он подпрыгивает
			if(Input.GetKeyDown(jumpButton) && isGrounded)
			{
				body.velocity = new Vector2(0, jumpForce);
			}
		}
		else{
			transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime*5);
			bottom.transform.rotation = Quaternion.Lerp(bottom.transform.rotation, networkRot, Time.deltaTime*10);
		}
		
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
		//Получение данных о позиции и повороте текущего персонажа и отправка этих данных другим пользователям
		networkPos = transform.position;
		networkRot = bottom.transform.rotation;
		stream.Serialize(ref networkPos);
		stream.Serialize(ref networkRot);
    }
}