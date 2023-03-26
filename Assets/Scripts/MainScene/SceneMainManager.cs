using UnityEngine;
using Chameleon;
using System.Collections;
using System;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public enum eDigitType{Normal,Cow,Bull}
public enum eRoomItem{Potion,CandlePile,Spike,Blade,Key,}

public class SceneMainManager : LoneMonoBehaviour<SceneMainManager>{
	[Header("Object Pooler")]
	[SerializeField] ObjectPooler poolerRoom;
	[SerializeField] ObjectPooler poolerDoorNone;
	[SerializeField] ObjectPooler poolerDoorLocked;

	[Header("Digit Material")]
	[SerializeField] DarkMaterialPair matPairBullDigit;
	[SerializeField] DarkMaterialPair matPairCowDigit;
	[SerializeField] DarkMaterialPair matPairNormalDigit;
	[SerializeField][Layer] int layerDigitNormal;
	[SerializeField][Layer] int layerDigitDark;

	[Header("Player")]
	[SerializeField] GameObject gPlayer;

	private static int[] aDigitExit = new int[3];
	private int[] aDigitStart = new int[3];
	private int[] aNumber = {0,1,2,3,4,5,6,7,8,9};
	
	private readonly float sqrt3 = Mathf.Sqrt(3);
	private float roomSize;
	private float doorHeight;

	private struct RoomItemData{
		public GameObject gItem;
		public eRoomItem itemType;
		public RoomItemData(GameObject gItem,eRoomItem itemType){
			this.gItem = gItem;
			this.itemType = itemType;
		}
	}
	private class RoomInstanceData{
		public GameObject gRoom;
		public GameObject[] aGDoor = new GameObject[6];
		public int connectDirection =-1;
		public int[] aDigit = new int[3];
		public List<RoomItemData> lgItem = new List<RoomItemData>();
	}
	private RoomInstanceData roomDataCurrent = new RoomInstanceData();
	private RoomInstanceData roomDataNeighbor = new RoomInstanceData();
	
	[Header("HUD")]
	[SerializeField] CanvasToggler cvTogglerBalloon;
	[SerializeField] DlgInteract dlgFooter;
	[SerializeField] DlgTwoButton dlgPause;
	[SerializeField] AudioPrefab sfxpfDlgPausePopup;
	[SerializeField] SliderHandler sldHandlerLookSensitivity;
	private CanvasToggler cvTogglerPause;
	public CanvasToggler CvTogglerBalloon{ get{return cvTogglerBalloon;} }
	public DlgInteract DlgFooter{ get{return dlgFooter;} }

	[Header("Icon Picked")]
	[SerializeField] Image imgIconKeyPick;
	//public Image ImgIconKeyPick{ get{return imgIconKeyPick;} }

	private eDigitType getDigitType(int digit,int index){
		if(digit==aDigitExit[index])
			return eDigitType.Bull;
		else if(digit==aDigitExit[(index+1)%3] || digit==aDigitExit[(index+2)%3])
			return eDigitType.Cow;
		else
			return eDigitType.Normal;
	}
	public static int getDigitExit(int index){
		return aDigitExit[index];
	}
//--------------------------------------------------------------------------------------------
	#region MONOBEHAVIOUR FUNCTIONS
	protected override void Awake(){
		base.Awake();
		initializeRoomRouteData();

		subitrFadeInScreenCover = imgScreenCover.tweenAlpha(1.0f,0.0f,durationFade);

		dlgPause.setButtonAction(
			onEndPause,
			()=>{
				onEndPause();
				onDie();
			},
			onEndPause
		);
		cvTogglerPause = dlgPause.GetComponent<CanvasToggler>();

		aPlacementNumber = new int[av2PlacementPos.Length];
		for(int i=0; i<aPlacementNumber.Length; ++i){
			aPlacementNumber[i] = i;}

		sldHandlerLookSensitivity.setOnValueChangeAction(
			(float fraction) => {PlayerController.inputLookMultiplier = fraction;});
	}
	void Start(){
		RoomScaler rawRoomScaler =
			poolerRoom.getObjectRawInactive().GetComponent<RoomScaler>();
		roomSize = rawRoomScaler.LengthSide;
		doorHeight = rawRoomScaler.DoorHeight;
		Algorithm.shuffle(aNumber); //for total mismatch, use aNumber[3,4,5] for current
		roomDataCurrent.gRoom = spawnRoom(Vector3.zero,aDigitStart,roomDataCurrent.lgItem);
		roomDataCurrent.aDigit[0] = aDigitStart[0];
		roomDataCurrent.aDigit[1] = aDigitStart[1];
		roomDataCurrent.aDigit[2] = aDigitStart[2];
		for(int i=0; i<6; ++i)
			roomDataCurrent.aGDoor[i] = spawnDoor(roomDataCurrent.gRoom,roomDataCurrent.aDigit,i);
		Debug.Log(aDigitExit[0]+" "+aDigitExit[1]+" "+aDigitExit[2]);
		
		//PlayerController.Instance.ShowCursor = false;
		StartCoroutine(rfStartSequence());

		((RectTransform)dlgPause.transform).anchoredPosition = Vector3.zero;
		sldHandlerLookSensitivity.Fraction = PlayerController.inputLookMultiplier;
		//dlgPause.close();

		volumeNormal = apfBgm.volume;
		BgmPlayer.Instance.playBgm(apfBgm);
	}
	#endregion
//--------------------------------------------------------------------------------------------
	#region ROOM
	private enum eDoorType{DoorNone,DoorLocked,}
	private struct ItemPlacement{
		public eRoomItem item;
		public Vector2 v2Position;
		public ItemPlacement(eRoomItem item,Vector2 v2Position){
			this.item = item;
			this.v2Position = v2Position;
		}
	}
	private class RoomState{
		public bool bDark;
		public List<ItemPlacement> lRoomItem = new List<ItemPlacement>();
	}
	[Header("Room Item")]
	[Tooltip("INCLUDING lock on the last room")]
	[SerializeField] int lockCount;
	[SerializeField] int extraKeyCount;

	[SerializeField][WideVector2] Vector2[] av2PlacementPos;
	private Dictionary<int,RoomState> dictRoomState = new Dictionary<int,RoomState>();
	private Dictionary<int,int> dictLockDirection = new Dictionary<int,int>();
	private bool[] abExitRoomUnlockedDirection = new bool[6]; //exit room is special that it has more than 1 lock
	private int roomCounter = 0;
	private int[] aPlacementNumber;

	[Serializable]
	private class ChanceLevel{
		[SerializeField][FixedSize("Level 0","Level 1","Level 2")] float[] aChance;
		[SerializeField][FixedSize("Level 0->1","Level 1->2")] int[] aLevelTrigger;
		private int level = 0;
		public float getCurrent(){
			return aChance[level];
		}
		public void updateLevel(int counter){
			if(level<aLevelTrigger.Length && counter>=aLevelTrigger[level]){
				level = Mathf.Min(aChance.Length-1,level+1);}
		}
	}
	[SerializeField][FixedSize(typeof(eRoomItem),0,3)] ChanceLevel[] aChanceTable;
	[SerializeField] ChanceLevel chanceDark;
	[SerializeField][FixedSize(typeof(eRoomItem))] ObjectPooler[] aPoolerItem;
	[SerializeField] GameObject gExitPortal;
	[SerializeField] Vector2 v2PosExitPortal;

	public bool IsCurrentRoomDark{
		get{
			updateCurrentRoom();
			return dictRoomState[roomDigitAsInt(roomDataCurrent.aDigit)].bDark;
		}
	}
	public bool hasItemInCurrentRoom(eRoomItem itemType){
		updateCurrentRoom();
		List<ItemPlacement> lItem =
			dictRoomState[roomDigitAsInt(roomDataCurrent.aDigit)].lRoomItem;
		for(int i=0; i<lItem.Count; ++i){
			if(lItem[i].item == itemType){
				return true; }
		}
		return false;
	}
	private bool isExitRoom(int[] aDigit){
		return
			aDigit[0]==aDigitExit[0] &&
			aDigit[1]==aDigitExit[1] &&
			aDigit[2]==aDigitExit[2]
		;
	}
	private int roomDigitAsInt(int[] aDigit){ //no bound check!
		return (aDigit[0]*10+aDigit[1])*10+aDigit[2];
	}
	public int getOppositeDirection(int direction){
		return (direction+3)%6;
	}
	private int getDoorDirection(Transform tRoom,Transform tDoor){
		Vector3 vDirection = tDoor.position-tRoom.position;
		float angleDeg = Mathf.Atan2(vDirection.z,vDirection.x)*Mathf.Rad2Deg;
		return ((int)angleDeg+389)/60 % 6;
	}
	public void updateCurrentRoom(){
		if(roomDataNeighbor.gRoom == null){
			return;}
		Vector3 vPlayerPos = gPlayer.transform.position;
		//If gRoom is null, let throw, because it should never happen
		if((vPlayerPos-roomDataCurrent.gRoom.transform.position).sqrMagnitude >
			(vPlayerPos-roomDataNeighbor.gRoom.transform.position).sqrMagnitude
		){
			//update BGM volume
			bool bCurrentDark = dictRoomState[roomDigitAsInt(roomDataCurrent.aDigit)].bDark;
			bool bNextDark = dictRoomState[roomDigitAsInt(roomDataNeighbor.aDigit)].bDark;
			if(bCurrentDark != bNextDark){
				BgmPlayer.Instance.lerpVolume(
					bCurrentDark ? volumeNormal : volumeDark,
					durationDimBgmVolume
				);
			}
			//update RoomInstanceData
			RoomInstanceData temp = roomDataCurrent;
			roomDataCurrent = roomDataNeighbor;
			roomDataNeighbor = temp;
		}
	}
	public void prepareNeighbor(Transform tDoor){
		int direction = getDoorDirection(roomDataCurrent.gRoom.transform,tDoor);
		Debug.Log("Prepare direction: "+direction);
		if(roomDataNeighbor.gRoom != null){
			if(roomDataCurrent.connectDirection == direction)
				return;
			discardNeighbor();
		}
		roomDataNeighbor.aDigit = getRoomNumber(roomDataCurrent.aDigit,direction);
		roomDataNeighbor.gRoom = spawnRoom(
			roomDataCurrent.gRoom.transform.position + getRoomOffset(direction),
			roomDataNeighbor.aDigit,
			roomDataNeighbor.lgItem
		);
		roomDataCurrent.connectDirection = direction;
		int oppositeDirection = getOppositeDirection(direction);
		roomDataNeighbor.connectDirection = oppositeDirection;
		for(int i=0; i<6; ++i){
			if(i != oppositeDirection){
				roomDataNeighbor.aGDoor[i] = spawnDoor(
					roomDataNeighbor.gRoom,
					roomDataNeighbor.aDigit,
					i
				);
			}
		}
		roomDataNeighbor.aGDoor[oppositeDirection] = roomDataCurrent.aGDoor[direction];
	}
	public void discardNeighbor(){
		if(roomDataNeighbor.gRoom){
			for(int i=0; i<6; ++i){
				if(i != roomDataNeighbor.connectDirection)
					roomDataNeighbor.aGDoor[i].SetActive(false);
			}
			//roomDataNeighbor.aGDoor[roomDataNeighbor.connectDirection]
			//	.GetComponent<IDoor>().reset();
			
			/* Disable room items */
			Debug.Log(roomDigitAsInt(roomDataNeighbor.aDigit).ToString());
			List<ItemPlacement> lItemPlacement =
				dictRoomState[roomDigitAsInt(roomDataNeighbor.aDigit)].lRoomItem;
			for(int i=0; i<roomDataNeighbor.lgItem.Count; ++i){
				RoomItemData roomItemData = roomDataNeighbor.lgItem[i];
				if(roomItemData.gItem.activeSelf){
					roomItemData.gItem.SetActive(false); }
				else{ //it has been picked up
					/* j shouldn't be further than 4, so this search should be fairly efficient. */
					/* This works because the game restricts that there can only be ONE pickable
					per room. If not, code logic needs to also check placement */
					for(int j=0; j<lItemPlacement.Count; ++j){
						if(lItemPlacement[j].item == roomItemData.itemType){
							lItemPlacement.RemoveAt(j);
							break;
						}
					}
				}
			}
			if(isExitRoom(roomDataNeighbor.aDigit)){
				gExitPortal.SetActive(false);}

			roomDataNeighbor.gRoom.SetActive(false);
			roomDataNeighbor.gRoom = null;
		}
	}
	private GameObject spawnRoom(Vector3 vPosition,int[] aDigit,List<RoomItemData> lgItem){
		/* Spawn Room Prefab */
		GameObject gRoom = poolerRoom.getObject(vPosition);
		DigitAligner digitAligner = gRoom.GetComponentInChildren<DigitAligner>();
		eDigitType[] aDigitType = new eDigitType[3];

		/* If exit room */
		if(isExitRoom(aDigit)){
			//gExitPortal.transform.SetParent(gRoom.transform);
			gExitPortal.transform.position = gRoom.transform.position+v2PosExitPortal.toVector3xz(0.0f);
			gExitPortal.SetActive(true);
		}
		
		/* Initialize Item Data */
		int roomNumber = roomDigitAsInt(aDigit);
		RoomState roomState;
		bool bKey = dictRoomState.TryGetValue(roomNumber,out roomState);
		if(roomState==null){
			roomState = initializeRoomItemData(roomNumber,bKey);}

		/* Set room Material ambient and torches according to bDark */
		gRoom.GetComponent<AmbientToggler>().setDarkAllChildren(roomState.bDark);
		gRoom.GetComponent<TorchController>().setLight(!roomState.bDark);

		/* Place Items at position */
		lgItem.Clear();
		for(int i=0; i<roomState.lRoomItem.Count; ++i){
			GameObject gItem = aPoolerItem[(int)roomState.lRoomItem[i].item].getObject(
				gRoom.transform.position + roomState.lRoomItem[i].v2Position.toVector3xz(0.0f),
				Quaternion.identity
			);
			gItem.GetComponent<AmbientToggler>()?.setDarkAllChildren(roomState.bDark);
			lgItem.Add(new RoomItemData(
				gItem,
				roomState.lRoomItem[i].item
			));
		}

		/* Set digit material */
		for(int i=0; i<3; ++i){
			digitAligner.setDigit(i,aDigit[i]);
			digitAligner.setLayer(i,roomState.bDark ? layerDigitDark : layerDigitNormal);
			aDigitType[i] = getDigitType(aDigit[i],i);
			switch(aDigitType[i]){
				case eDigitType.Bull:
					digitAligner.setMaterial(i,matPairBullDigit.getMaterial(roomState.bDark));
					break;
				case eDigitType.Cow: 
					digitAligner.setMaterial(i,matPairCowDigit.getMaterial(roomState.bDark));
					break;
				case eDigitType.Normal: 
					digitAligner.setMaterial(i,matPairNormalDigit.getMaterial(roomState.bDark));
					break;
			}
		}

		DigitInspectable digitMessage = gRoom.GetComponentInChildren<DigitInspectable>();
		digitMessage.initText(aDigit,aDigitType);
		#if UNITY_EDITOR
		string sRoomData = roomNumber.ToString();
		for(int i=0; i<dictRoomState[roomNumber].lRoomItem.Count; ++i){
			sRoomData += dictRoomState[roomNumber].lRoomItem[i].item+" ";}
		sRoomData+= "Dark: "+dictRoomState[roomNumber].bDark;
		Debug.Log(sRoomData);
		#endif
		return gRoom;
	}
	private Vector3 getRoomOffset(int direction){
		Vector2 vOffset = Vector2Extension.fromPolar(roomSize*sqrt3,60.0f*direction);
		return new Vector3(vOffset.x,transform.position.y,vOffset.y);
	}
	/* This function maps direction angle with how digit changes.
	You can modify that behaviour here. */
	private int[] getRoomNumber(int[] aDigitCurrent,int direction){
		int[] aDigit = new int[3];
		aDigit[0] = aDigitCurrent[0];
		aDigit[1] = aDigitCurrent[1];
		aDigit[2] = aDigitCurrent[2];
		switch(direction%6){
			case 1:
				stepDigitNoRepeat(aDigit,0,1);
				break;
			case 4:
				stepDigitNoRepeat(aDigit,0,9);
				break;
			case 0:
				stepDigitNoRepeat(aDigit,1,1);
				break;
			case 3:
				stepDigitNoRepeat(aDigit,1,9);
				break;
			case 5:
				stepDigitNoRepeat(aDigit,2,1);
				break;
			case 2:
				stepDigitNoRepeat(aDigit,2,9);
				break;
		}
		return aDigit;
	}
	private int[] getRoomDigit(int roomNumber){
		return new int[3]{
			roomNumber/100,
			(roomNumber/10)%10,
			roomNumber%10
		};
	}
	private void stepDigitNoRepeat(int[] aDigit,int index,int step){
		int arrayLength = aDigit.Length;
		aDigit[index] = (aDigit[index]+step)%10;
		for(int i=1; i<arrayLength; ++i){
			if(aDigit[index] == aDigit[(index+i)%arrayLength]){
				aDigit[index] = (aDigit[index]+step)%10;
				i=0; //start over after ++i
			}
		}
	}
	private GameObject spawnDoor(GameObject gRoom,int[] aDigit,int direction)
	{
		int roomDigit = roomDigitAsInt(aDigit);
		int lockedDirection;
		bool bDark = false;
		if(dictLockDirection.TryGetValue(roomDigit,out lockedDirection)){
			bDark = dictRoomState[roomDigit].bDark;}
		else{
			lockedDirection = -1;}
		Vector3 vDoorPos = gRoom.transform.position + getRoomOffset(direction)/2;
		vDoorPos.y = doorHeight/2;
		return spawnDoor(
			isExitRoom(aDigit)?
				!abExitRoomUnlockedDirection[direction] :
				lockedDirection==direction
			,
			vDoorPos,
			Quaternion.Euler(0.0f,-60.0f*direction+90.0f,0.0f),
			bDark
		);
	}
	private GameObject spawnDoor(bool bLocked,Vector3 vPos,Quaternion qRotation,bool bDark){
		if(bLocked){
			GameObject gDoor = poolerDoorLocked.getObject(vPos,qRotation);
			gDoor.GetComponent<AmbientToggler>().setDarkAllChildren(bDark);
			return gDoor;
		}
		else{
			return poolerDoorNone.getObject(vPos,qRotation);}
	}
	/* Record unlock data and swap locked door with doorNone. Returns gDoorNone. */
	public GameObject unlock(Transform tDoor){
		int direction = getDoorDirection(roomDataCurrent.gRoom.transform,tDoor);
		if(isExitRoom(roomDataCurrent.aDigit)){
			abExitRoomUnlockedDirection[direction] = true; }
		else{
			dictLockDirection.Remove(roomDigitAsInt(roomDataCurrent.aDigit)); }
		
		int[] roomDigitNeighbor = getRoomNumber(roomDataCurrent.aDigit,direction);
		if(isExitRoom(roomDigitNeighbor)){
			abExitRoomUnlockedDirection[getOppositeDirection(direction)] = true;}
		else{
			dictLockDirection.Remove(roomDigitAsInt(getRoomNumber(roomDataCurrent.aDigit,direction)));}
		GameObject gDoorNone = spawnDoor(false,tDoor.position,tDoor.rotation,false);
		roomDataCurrent.aGDoor[direction] = gDoorNone;
		return gDoorNone;
	}
	private RoomState initializeRoomItemData(int roomNumber,bool bKey){
		RoomState roomState = new RoomState();
		int slotPlacement = 0;
		Algorithm.shuffle(aPlacementNumber);
		
		/* Set whether room is dark or not */
		roomState.bDark = Random.value<=chanceDark.getCurrent();
		roomState.lRoomItem = new List<ItemPlacement>();

		/* Set key position */
		if(bKey){
			roomState.lRoomItem.Add(new ItemPlacement(
				eRoomItem.Key,
				av2PlacementPos[aPlacementNumber[slotPlacement]]
			));
			++slotPlacement;
		}

		/* Set room Items */
		//int itemTypeCount = Enum.GetNames(typeof(eRoomItem)).Length; //Credit: Kasper Holdum, SO
		if(Random.value <= aChanceTable[(int)eRoomItem.Potion].getCurrent()){ //potion
			roomState.lRoomItem.Add(new ItemPlacement(
				eRoomItem.Potion,
				av2PlacementPos[aPlacementNumber[slotPlacement]]
			));
			++slotPlacement;
		}
		if(Random.value <= aChanceTable[(int)eRoomItem.CandlePile].getCurrent()){ //candle
			/* Maybe we can make a logic so that candles only exist in lit room,
			but I think there is no problem with candles in dark room, so just random. */
			roomState.lRoomItem.Add(new ItemPlacement(
				eRoomItem.CandlePile,
				av2PlacementPos[aPlacementNumber[slotPlacement]]
			));
			++slotPlacement;
		}
		if(Random.value <= aChanceTable[(int)eRoomItem.Blade].getCurrent()){ //blade
			roomState.lRoomItem.Add(new ItemPlacement(
				eRoomItem.Blade,
				Vector2.zero
			));
			++slotPlacement;
		}
		while(slotPlacement<av2PlacementPos.Length){ //spike (can place multiple)
			if(Random.value <= aChanceTable[(int)eRoomItem.Spike].getCurrent()){
				roomState.lRoomItem.Add(new ItemPlacement(
					eRoomItem.Spike,
					av2PlacementPos[aPlacementNumber[slotPlacement]]
				));
				++slotPlacement;
			}
			else{
				break;}
		}

		dictRoomState[roomNumber] = roomState;
		++roomCounter;
		for(int i=0; i<aChanceTable.Length; ++i){
			aChanceTable[i].updateLevel(roomCounter);}
		chanceDark.updateLevel(roomCounter);
		return roomState;
	}
	private void initializeRoomRouteData(){
		/* For longer sequence, you can either use Linear Congruence Generator (a^x=b mod p)
		or Linear-Feedback Shift Generator to avoid storing long list.
		(Credit: gbarry & starblue, SO) */
		Algorithm.shuffle(aNumber);
		aDigitExit[0] = aNumber[0]; //unroll loop because it is short enough
		aDigitExit[1] = aNumber[1];
		aDigitExit[2] = aNumber[2];
		
		aDigitStart[0] = aNumber[3]; //use aNumber[3,4,5] for total mismatch
		aDigitStart[1] = aNumber[4];
		aDigitStart[2] = aNumber[5];

		int roomNumberExit = roomDigitAsInt(aDigitExit);
		dictLockDirection.Add(
			roomNumberExit,
			-1 //just special number
		);
		dictRoomState.Add(roomNumberExit,new RoomState()); //bDark=false by default
		Debug.Log("Exit: "+roomNumberExit);
		Debug.Log("Start: "+roomDigitAsInt(aDigitStart));

		for(int i=0; i<6; ++i){
			dictLockDirection.Add(
				roomDigitAsInt(getRoomNumber(aDigitExit,i)),
				getOppositeDirection(i)
			);
			//Debug.Log("Exit Neighbor: "+roomDigitAsInt(getRoomNumber(aDigitExit,i)));
		}

		for(int i=0; i<lockCount-6; ++i){
			while(true){
				int roomNumber = randomRoom(dictLockDirection.ContainsKey);
				int direction = Random.Range(0,6);
				int roomNeighborNumber = roomDigitAsInt(getRoomNumber(aNumber,direction));
				/* It is possible that we have a valid room, but the direction we want to
				place the lock leads to room which already has one. If that happens,
				we simply random room and direction again. Again, there should be better
				algorithm, but I think it may be complicate.
				(I will find time to learn Graph Theory, and maybe review this). */
				/* In truth, this would create double duplicate data because a single lock
				must be added to 2 rooms even when one can be inferred from another.
				One way to solve this is to restrict range of lockDirection to, say,
				0,1,2, and if room direction is 3,4,5, just calculate and use neighbor
				room number as key. However, this seems overkill and can be confusing,
				because instead of straight lookup, one must look up (3) neighbors as well
				to see if a room has a locked door, which may not worth the memory overhead. */
				if(!dictLockDirection.ContainsKey(roomNeighborNumber)){
					dictLockDirection.Add(roomNumber,direction);
					dictLockDirection.Add(roomNeighborNumber,getOppositeDirection(direction));
					break; //continue for loop
				}
			}
		}
		#if UNITY_EDITOR
		{
			string s = "Room with Lock: ";
			SortedDictionary<int,int> sortedDictLockDirection =
				new SortedDictionary<int,int>(dictLockDirection);
			foreach(var e in sortedDictLockDirection){
				s += e.Key+"("+e.Value+") "; }
			Debug.Log(s);
		}
		#endif
		for(int i=0; i<lockCount+extraKeyCount; ++i){
			//allow key in the same room with lock, and see what player will do!
			int roomNumber = randomRoom(dictRoomState.ContainsKey);
			dictRoomState.Add(roomNumber,null);
			/* We won't initialize the Value. When this room is entered by player and
			we found its entry with null Value in the dictionary, we will place the key
			in the room during initialization, taking into account position of other items. */
		}
		#if UNITY_EDITOR
		{
			string s = "Room with Key (including exit): ";
			SortedSet<int> sortedSetRoomKey = new SortedSet<int>(dictRoomState.Keys);
			foreach(int i in sortedSetRoomKey){
				s += i+" "; }
			Debug.Log(s);
		}
		#endif
	}
	private int randomRoom(Func<int,bool> predicateNG){
		while(true){
			/* There is a chance that shuffle will never get the non-duplicate value
			and the loop runs infinitely, but the simplest better solution is to
			shuffle all room numbers, which generating the list that conforms to our rule
			is already quite difficult. (the step hack also does not work well because of
			our unusual condition) (Credit: Jean-Francois Fabre, SO),
			Another alternative is to use cryptographic mapping with counter (Credit: Graham Cox, baeldung.com)
			but that looks overkill.
			Anyway, the chance of infinite loop is small as long as locked count is small
			comparing to total numbers of doors. Hence, we take it for this project. */
			Algorithm.shuffle(aNumber);
			int roomNumber = roomDigitAsInt(aNumber);
			if(!predicateNG(roomNumber)){
				return roomNumber;}
		}
	}
	#if UNITY_EDITOR
	public void setItemPositionTable(Vector2[] aV2Pos){
		av2PlacementPos = (Vector2[])aV2Pos.Clone(); //Credit: Felipe Oriani, SO
	}
	#endif
	#endregion
//--------------------------------------------------------------------------------------------
	#region START SEQUENCE
	[Header("Start Sequence")]
	[SerializeField] Image imgScreenCover;
	[SerializeField] float durationFade;
	private TweenRoutineUnit subitrFadeInScreenCover;

	private IEnumerator rfStartSequence(){
		//PlayerController.Instance.enabled = false;
		//yield return imgScreenCover.tweenAlpha(1.0f,0.0f,durationFade);
		yield return subitrFadeInScreenCover;
		//PlayerController.Instance.enabled = true;
		imgScreenCover.transform.root.gameObject.SetActive(false); //set that canvas inactive
		PlayerController.Instance.InputMode = eInputMode.MainGameplay;
	}
	#endregion
//--------------------------------------------------------------------------------------------
	#region PAUSE SEQUENCE
	public bool IsPause{get; private set;} = false;
	private bool bPrevShowCursor = false;
	public void togglePause(){
		SfxPlayer.Instance.play(sfxpfDlgPausePopup);
		if(IsPause){onEndPause();}
		else{pause();}
	}
	private void pause(){
		if(!IsPause){
			bPrevShowCursor = CursorController.ShowCursor;
			CursorController.ShowCursor = true;
			PlayerController.Instance.IsPause = true;
			//dlgPause.popup();
			cvTogglerPause.setActiveCanvas(true);
			IsPause = true;
		}
	}
	private void onEndPause(){
		//dlgPause.close();
		cvTogglerPause.setActiveCanvas(false);
		PlayerController.Instance.IsPause = false;
		CursorController.ShowCursor = bPrevShowCursor;
		IsPause = false;
	}
	#endregion
//--------------------------------------------------------------------------------------------
	#region END SEQUENCE
	[Header("End Sequence")]
	[SerializeField] float timeEndSuspend;
	[SerializeField] SceneIndex sceneIndexEnd;
	[SerializeField] FollowConstraint camFollowConstraint;
	[SerializeField] Transform tSpine;
	[SerializeField] float angleCameraDie;
	[SerializeField] float distanceCameraDie;
	[SerializeField] float durationPanCamera;
	[SerializeField] AudioPrefab sfxpfPortalActivation;
	public static bool IsWin{get; private set;}
	public void onDie(){
		IsWin = false;
		PlayerController.Instance.InputMode = eInputMode.Freeze;
		Interactable.Focused?.StopAllCoroutines();
		StartCoroutine(rfDyingSequence());
	}
	public void onWin(){
		IsWin = true;
		//PlayerController.Instance.InputMode = eInputMode.Freeze; //do since door approached
		StartCoroutine(rfWinningSequence());
	}
	private IEnumerator rfDyingSequence(){
		/* This is to make camera follow the dying body. We can't do this from the beginning
		otherwise the camera will shake every time player gets hit or doing animation. */
		HeadLookController.Instance.setHeadLookTarget(null);
		HeadLookController.Instance.enabled = false; //in case player falls onto trigger
		if(CandleManager.Instance.IsLit){
			CandleManager.Instance.toggleCandleLight();}
		camFollowConstraint.setTarget(tSpine,Vector3.zero);
		ThirdPersonCameraControl camControl = Camera.main.GetComponent<ThirdPersonCameraControl>();
		Transform tCamTarget = camControl.tTarget;
		Vector3 eulerCam = tCamTarget.rotation.eulerAngles;
		/* These 3 can be made into ParallelEnumerator and LoneCoroutine, but it is unnecessary
		because we don't need to keep track of Coroutine cancelling anymore. */
		StartCoroutine(tCamTarget.tweenRotation(
			tCamTarget.rotation,
			Quaternion.Euler(angleCameraDie,eulerCam.y,eulerCam.z),
			durationPanCamera
		));
		float startCamDistance = camControl.targetCameraDistance;
		StartCoroutine(new TweenRoutineUnit(
			(float t) => {camControl.targetCameraDistance = Mathf.Lerp(
				startCamDistance,distanceCameraDie,t);},
			durationPanCamera
		));
		yield return PlayerController.Instance.rfDie();
		imgScreenCover.transform.root.gameObject.SetActive(true); //set Canvas active
		subitrFadeInScreenCover.bReverse = true;
		yield return subitrFadeInScreenCover;
		yield return RoutineUnitCollection.fadeOutAudioListener(timeEndSuspend);
		SceneManager.LoadSceneAsync(sceneIndexEnd);
	}
	private IEnumerator rfWinningSequence(){
		HeadLookController.Instance.setHeadLookTarget(null);
		HeadLookController.Instance.enabled = false;
		imgScreenCover.transform.root.gameObject.SetActive(true); //set Canvas active
		imgScreenCover.color = Color.white;
		subitrFadeInScreenCover.bReverse = true;
		SfxPlayer.Instance.play(sfxpfPortalActivation);
		yield return subitrFadeInScreenCover;
		yield return new WaitForSeconds(timeEndSuspend);
		yield return imgScreenCover.tweenColor(imgScreenCover.color,Color.black,durationFade);
		yield return RoutineUnitCollection.fadeOutAudioListener(timeEndSuspend);
		SceneManager.LoadSceneAsync(sceneIndexEnd);
	}
	//private IEnumerator rfChangeScene(){
	//	imgScreenCover.color = IsWin ? Color.white : Color.black;
	//	imgScreenCover.transform.root.gameObject.SetActive(true);
	//	subitrFadeInScreenCover.bReverse = true;
	//	yield return subitrFadeInScreenCover;
	//	yield return new WaitForSeconds(timeEndSuspend);
	//	SceneManager.LoadSceneAsync(sceneIndexEnd);
	//}
	#endregion
//--------------------------------------------------------------------------------------------
	#region AUDIO
	[Header("BGM")]
	[SerializeField] AudioPrefab apfBgm;
	[SerializeField] float durationDimBgmVolume;
	[SerializeField] float volumeDark;
	private float volumeNormal;
	#endregion
//--------------------------------------------------------------------------------------------
}

