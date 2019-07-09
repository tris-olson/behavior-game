using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
		// variables from built in class
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

		// game objects used in player script
		public GameObject bulletPrefab;
		public GameObject gun;
		public GameObject gun1Prefab;
		public GameObject gun2Prefab;
		public GameObject gun3Prefab;
		public GameObject gun4Prefab;
		public Vector3 gunPos = new Vector3 (1, 3f, 0); // defines gun position relative to player
		public Light lt; // needed for death sequence
		public GameObject cat;

		public List<Bullet> bulletList = new List<Bullet> ();

		// UI objects
		public Text timerLabel;
		public Text introText;
		public Image introBackground;
		public Slider HPBar;
		public Text coinsText;
		public Text foodText;
		public Text fishermanText;
		public Text explorerText;
		public Text warriorText;
		public Text immortalText;
		public Text scavengerText;
		public Text adventurerText;
		public Text catText;
		public Text mushroomText;
		public Text fishText;

		// tracking variables
		int food = 0;
		int gold = 0;
		int hitPoints = 100;
		Vector3 previousPosition;
		public int perimeterHits = 0;
		public int bulletsFired = 0;
		public int numberOfDeaths = 0;
		public int numberOfHeals = 0;
		public int timesHit = 0;
		public float distanceMoved = 0.0f;
		public Vector3 startingPosition;
		public int foodCollected = 0;
		public int goldCollected = 0;
		public float timeAbove50Health = 0.0f;
		public float timeBelow50Health = 0.0f;
		public int fishKilled = 0;
		public int rabbitsKilled = 0;
		public int monstersKilled = 0;
		public float timeAlive = 0.0f;
		public int questsCompleted = 0;
		public int timesGambled = 0;
		public float timeSpentInStore = 0f;

		// achievement variables
		public bool fisherman = false;
		public bool explorer = false;
		public bool warrior = false;
		public bool immortal = false;
		public bool scavenger = false;
		public bool adventurer = false;
		bool[] explore_locs = new bool[8];

		// overlay objects for quests/shop/gambling
		public Image NPCBackground;
		public Text NPCText;
		public Text NPCTitle;
		public Text betText;

		// for managing interactions
		bool atShop = false;
		bool atGambling = false;
		bool atCatQuestMan = false;
		bool atMushroomQuestMan = false;

		bool hasCat = false;
		bool returnedCat = false;
		bool hasMushroom = false;
		bool returnedMushroom = false;
		bool hasFishQuest = false;
		bool handedInFish = false;
		int questFishCounter = 0;
		int currentBet = 0;
		int currentItem = 4;

		// settings + death sequence management
		bool freezePlayer = true;
		bool inIntro = true;
		float deathTime = 2.5f;
		int healthRestoredByFood = 20;
		int damageDoneByAttack = 10;
		float totalTime = 900f; // total seconds for gameplay
		float timeRemaining = 600f;

		// turn off end sequence, add gold for debugging
		bool debugMode = false;

        // Use this for initialization
        private void Start()
		{
			// fpc vars
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);

			// turn on intro text
			introText.text = "Your horse collapsed as you travelled across the desert, stranding you. " + 
				"Fortunately you know that your friend is coming through the area tonight and will be able " +
				"to rescue you, but until then you need to stay alive. Food can be used to restore your health. " +
				"Monsters roam the area, but won't attack unless you get too close - but you have a gun with " +
				"you to protect yourself. Friendly people live in the region, who may be looking for help or " + 
				"willing to sell you goods. Finally, this area is known to be full of gold. Good luck! \n\n" +
				"Controls: Use WASD to move and the mouse to look around. Press spacebar to jump, click to fire " +
				"your weapon, and press alt to use food to heal damage. Clicking will also make dialog boxes disappear.";
			introBackground.color = new Color (1f, 1f, 1f, 1f);

			// instantiate player weapon
			InstantiateGun (gun4Prefab);

			// intialize previousPosition in order to track distance travelled
			previousPosition = transform.position;
			startingPosition = transform.position;

			//
			if (debugMode) {
				gold += 1000;
			}
        }

		void InstantiateGun (GameObject gunPrefab) {
			// instantiate player weapon
			Quaternion rotation = Quaternion.identity;
			rotation.eulerAngles = new Vector3 (0, -90, 0);
			gun = (GameObject)Instantiate (
				gunPrefab,
				(transform.position + gunPos),
				(transform.rotation * rotation));
			gun.transform.parent = transform;
		}


        // Update is called once per frame
        private void Update()
		{
			// check if player should be immobilized before handling movement
			if (!freezePlayer) {
				RotateView ();
				// the jump state needs to read here to make sure it is not missed
				if (!m_Jump && !atGambling && !atShop) { // want space to be used for different purpose in shop & gambling
					m_Jump = CrossPlatformInputManager.GetButtonDown ("Jump");
				}

				if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
					StartCoroutine (m_JumpBob.DoBobCycle ());
					//PlayLandingSound ();
					m_MoveDir.y = 0f;
					m_Jumping = false;
				}
				if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
					m_MoveDir.y = 0f;
				}

				m_PreviouslyGrounded = m_CharacterController.isGrounded;

				// fire bullets
				if (Input.GetButtonDown ("Fire1")) {
					if (!atShop && !atGambling && !atCatQuestMan && !atMushroomQuestMan) { // bc clicking is used to close dialog otherwise
						Fire ();
					}
				}

				// use food to restore health
				if (Input.GetButtonDown ("Fire2")) {
					if (food > 0) {
						UseFood ();
					}
				}

				// UI interaction
				if (atShop) {
					HandleShopping ();
					timeSpentInStore += Time.deltaTime;
				}
				if (atGambling) {
					HandleGambling ();
				}
				if (atCatQuestMan) { // close dialog box with click
					if (Input.GetButtonDown ("Fire1")) {
						TurnOffTextBox ();
						atCatQuestMan = false;
					}
				}
				if (atMushroomQuestMan) { // close dialog box with click
					if (Input.GetButtonDown ("Fire1")) {
						TurnOffTextBox ();
						atMushroomQuestMan = false;
					}
				}
					

				// handle death sequence when health drops to/below zero
				if (hitPoints <= 0) {
					hitPoints = 0;
					freezePlayer = true;
					timeAlive = 0.0f;
					StartCoroutine ("HandleDeath");
				} else if (hitPoints < 50) {
					timeBelow50Health += Time.deltaTime;
					timeAlive += Time.deltaTime;
				} else {
					timeAbove50Health += Time.deltaTime;
					timeAlive += Time.deltaTime;
				}
					
				// every frame need to deal with bullets & update UI
				HandleBulletCollisions ();
				UpdateUI ();

				// track distance moved total
				distanceMoved += Vector3.Magnitude (transform.position - previousPosition);
				previousPosition = transform.position;

			} else {
				if (inIntro) { // allows player to close intro box on click
					if (Input.GetButtonDown ("Fire1")) {
						freezePlayer = false;
						inIntro = false;
						timeRemaining = totalTime;
						introText.text = "";
						introBackground.color = new Color (0f, 0f, 0f, 0f);
					}
				}
			}
				
			// check if game should end on every frame, unless in debug mode
			if (!debugMode) {
				if (timeRemaining <= 0) {
					freezePlayer = true;
					introText.text = "You survived!";
					introBackground.color = new Color (1f, 1f, 1f, 1f);
				}
			}

        }

		void UpdateUI () { // update UI with info for player
			// update time & compute labels
			timeRemaining -= Time.deltaTime;

			var minutes = (timeRemaining / 60); //Divide the guiTime by sixty to get the minutes.
			var seconds = timeRemaining % 60;//Use the euclidean division for the seconds.
			var fraction = (timeRemaining * 100) % 100;

			//update the label value
			timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

			// update hp, items, achievements
			HPBar.value = hitPoints;
			coinsText.text = string.Format ("{0}", gold);
			foodText.text = string.Format ("{0}", food);
			CheckAchievements ();	
			if (fisherman == true) {
				fishermanText.color = new Color (0f, .4f, 0f, 1);
			}
			if (explorer == true) {
				explorerText.color = new Color (0f, .4f, 0f, 1);
			}
			if (warrior == true) {
				warriorText.color = new Color (0f, .4f, 0f, 1);
			}
			if (immortal == true) {
				immortalText.color = new Color (0f, .4f, 0f, 1);
			}
			if (scavenger == true) {
				scavengerText.color = new Color (0f, .4f, 0f, 1);
			}
			if (adventurer == true) {
				adventurerText.color = new Color (0f, .4f, 0f, 1);
			}
			if (hasCat) {
				catText.color = new Color (0f, 0f, 0f, 1);
			} else {
				catText.color = new Color (1f, 1f, 1f, 1);
			}
			if (hasMushroom) {
				mushroomText.color = new Color (0f, 0f, 0f, 1);
			} else {
				mushroomText.color = new Color (1f, 1f, 1f, 1);
			}
			if (hasFishQuest) {
				fishText.color = new Color (0f, 0f, 0f, 1);
			} else {
				fishText.color = new Color (1f, 1f, 1f, 1);
			}
		}

		void CheckAchievements () { // self explanatory, updates bools
			if (fishKilled >= 15) {
				fisherman = true;
			}
			bool exploreCheck = true;
			for (int i = 0; i < explore_locs.Length; i++) {
				if (!explore_locs [i]) {
					exploreCheck = false;
					break;
				}
			}
			if (exploreCheck) {
				explorer = true;
			}
			if (monstersKilled >= 15) {
				warrior = true;
			}
			if (timeAlive >= 360.0f) {
				immortal = true;
			}
			if (goldCollected >= 20) {
				scavenger = true;
			}
			if (returnedCat && returnedMushroom && handedInFish) {
				adventurer = true;
			}
		}

		void TurnOffTextBox () { // shuts text boxes by deleting text & making background transparent
			NPCBackground.color = new Color (1, 1, 1, 0);
			NPCText.text = "";
			NPCTitle.text = "";
			betText.text = "";
		}

		void HandleShopping () { // handles logic at shop so player can purchase items
			GameObject[] gunsToBuy = new GameObject[4];
			gunsToBuy [0] = gun4Prefab;
			gunsToBuy [1] = gun3Prefab;
			gunsToBuy [2] = gun2Prefab;
			gunsToBuy [3] = gun1Prefab;
			// associates input with actions
			if (Input.GetButtonDown ("Fire1")) { // quit shop
				TurnOffTextBox ();
				atShop = false;
			}
			if (Input.GetButtonDown("Jump")) { // make purchase
				if (currentItem == 4) {
					if (gold >= 1) {
						gold -= 1;
						food += 2;
					}
				} else if (currentItem == 2 || currentItem == 3) {
					if (gold >= 12) {
						gold -= 12;
						Destroy (gun.gameObject);
						InstantiateGun (gunsToBuy[currentItem]);
					}
				} else { // if currentItem == 0 || currentItem == 1
					if (gold >= 10) {
						gold -= 10;
						Destroy (gun.gameObject);
						InstantiateGun (gunsToBuy[currentItem]);
					}
				}
				UpdateShoppingText ();
			}
			// change item selected
			if (Input.GetButtonDown ("Cancel")) {
				if (currentItem >= 1) {
					currentItem--;
				}
				UpdateShoppingText ();
			}
			if (Input.GetButtonDown ("Submit")) {
				if (currentItem <= 3) {
					currentItem++;
				}
				UpdateShoppingText ();
			}
		}

		void UpdateShoppingText () { // update shop text
			if (currentItem == 0) {
				betText.text = "Small Frontloader: 10 gold";
			} else if (currentItem == 1) {
				betText.text = "Large Frontloader: 10 gold";
			} else if (currentItem == 2) {
				betText.text = "Small Automatic Rifle: 12 gold";
			} else if (currentItem == 3) {
				betText.text = "Large Automatic Rifle: 12 gold";
			} else if (currentItem == 4) {
				betText.text = "2 Food: 1 gold";
			}
		}

		void HandleGambling () { // handles logic at gambling so player can play game
								 // associates input with actions
			if (Input.GetButtonDown ("Fire1")) {
				TurnOffTextBox ();
				atGambling = false;
				currentBet = 0;
			}
			if (Input.GetButtonDown("Jump")) {
				timesGambled++;
				betText.text = "Current bet: 0 gold";
				float rand = Random.Range (0f, 1f);
				if (rand >= 0.5f) {
					gold += currentBet;
				} else {
					gold -= currentBet;
				}
				currentBet = 0;
			}
			if (Input.GetButtonDown ("Cancel")) {
				if (currentBet <= 0) {
					currentBet = 0;
				} else {
					currentBet--;
				}
				betText.text = string.Format ("Current bet: {0} gold", currentBet);
			}
			if (Input.GetButtonDown ("Submit")) {
				if (currentBet >= gold) {
					currentBet = gold;
				} else {
					currentBet++;
				}
				betText.text = string.Format ("Current bet: {0} gold", currentBet);
				// timesGambled++; was here during testing by accident, not noticed until report compilation
			}
		}

		IEnumerator HandleDeath () {
			numberOfDeaths++;
			// on player death the player freezes and the lighting
			// becomes red over one second
			StartCoroutine ("FadeToRed");
			yield return new WaitForSeconds (deathTime);
			// after one second the player is returned to the starting
			// position and loses half their gold and food, HP is reset,
			// and the lighting resets to normal
			hitPoints = 100;
			gold = gold / 2;
			food = food / 2;
			transform.position = startingPosition;
			freezePlayer = false;
		}

		IEnumerator FadeToRed() { // fades overhead light to red to indicate death
			Color original = lt.color;
			for (float f = 0.0f; f <= (deathTime*0.8f); f += 0.1f) {
				float g = f / deathTime;
				lt.color = Color.Lerp (original, Color.red, g);
				yield return new WaitForSeconds(0.1f);
			}
			yield return new WaitForSeconds (deathTime*0.2f);
			lt.color = original;
		}

		void Fire () { // initialize bullets and move forward
			var newBullet = (GameObject)Instantiate (
				bulletPrefab,
				(gun.transform.position + gun.transform.right),
				transform.rotation);
			newBullet.GetComponent<Rigidbody> ().velocity = gun.transform.right * 80;
			bulletsFired++;
			Bullet newBulletScript = newBullet.GetComponent<Bullet> ();
			bulletList.Add (newBulletScript);
		}

		void HandleBulletCollisions () { // checks to see if bullets have hit anything shootable
			foreach (Bullet bul in bulletList) {
				if (bul.hitFish == true) { 
					bul.playerHandled = true;
					fishKilled++;
					bulletList.Remove (bul);
					// depending on if the player needs fish for the quest, 
					// fish are added to quest counter or food meter
					if (hasFishQuest && questFishCounter < 5) {
						questFishCounter++;
						fishText.text = string.Format ("{0}/5 Fish", questFishCounter);
					} else {
						food++;
					}
				} else if (bul.hitRabbit == true) {
					bul.playerHandled = true;
					rabbitsKilled++;
					bulletList.Remove (bul);
				} else if (bul.hitMonster == true) {
					bul.playerHandled = true;
					monstersKilled++;
					bulletList.Remove (bul);
				}
			}
		}

		void UseFood () { // updates health when food is used
			food--; 
			hitPoints += healthRestoredByFood;
			if (hitPoints > 100) {
				hitPoints = 100;
			}
			numberOfHeals++;
		}

		void OnTriggerEnter (Collider col) { // trigger responses to hitting different world objects
			if (col.gameObject.CompareTag ("death_collider")) { // die on fall off of cat platform or into river
				hitPoints = 0;
			} else if (col.gameObject.CompareTag ("perimeter")) { // how much does player test walls?
				perimeterHits++;
			} else if (col.gameObject.CompareTag ("food")) { // collect food
				col.gameObject.SetActive (false);
				food++;
				foodCollected++;
			} else if (col.gameObject.CompareTag ("coin")) { // collect coins
				col.gameObject.SetActive (false);
				gold++;
				goldCollected++;
			} else if (col.gameObject.CompareTag ("monster_trigger")) { // monsters do damage
				timesHit++;
				hitPoints -= damageDoneByAttack;
			} else if (col.gameObject.CompareTag ("mushroom")) { // pick up mushroom
				hasMushroom = true;
				col.gameObject.SetActive (false);
			} else if (col.gameObject.CompareTag ("cat") && !returnedCat) { // pick up cat the first time
				hasCat = true;
				col.gameObject.SetActive (false);
			} else if (col.gameObject.CompareTag ("store")) { // open up store
				atShop = true;
				NPCBackground.color = new Color (1, 1, 1, 1);
				NPCTitle.text = "Shop";
				NPCText.text = "Would you like to buy something? Press < and > to change your selection, and space to make a purchase.\n\n" +
					"Small Frontloader \nLarge Frontloader \nSmall Automatic Rife \nLarge Automatic Rifle \n2 Food";
			} // logic for cat & fish quests
			else if (col.gameObject.CompareTag ("cat_quest")) { // hand out cat quest
				atCatQuestMan = true;
				NPCBackground.color = new Color (1, 1, 1, 1);
				NPCTitle.text = "Jerome";
				if (!hasCat && !returnedCat) {
					NPCText.text = "My cat is gone! She's gotten stuck behind all the monsters behind me. Will you please rescue her for me?";
				} else if (hasCat && !returnedCat) { // accept cat once found
					NPCText.text = "Thank you! Here's a reward for your trouble. I wonder if you could help me with something else. " +
						"Reggie looks so hungry, would you find her some fish to eat? Five would probably be enough.";
					hasCat = false;
					returnedCat = true;
					questsCompleted++;
					gold += 10;
					cat.SetActive (true);
					cat.transform.position = new Vector3 (-38.7f - 342.3f, -1f, -60.4f - 41.3f);
					hasFishQuest = true;
				} else if (!hasCat && returnedCat && !handedInFish && questFishCounter < 5) { // give out fish quest
					NPCText.text = "Thank you for returning my cat! She still looks pretty hungry though. Please catch her some fish!";
				} else if (!hasCat && returnedCat && !handedInFish && questFishCounter >= 5) { // reward for fish quest
					NPCText.text = "Thank you for bringing Reggie some fish! Here's something for your time.";
					handedInFish = true;
					questsCompleted++;
					hasFishQuest = false;
					gold += 15;
					cat.transform.localScale = cat.transform.localScale * 2f;
				} else { // response post quests
					NPCText.text = "Thanks again! She looks so much healthier now.";
				}
			} // logic for mushroom quest 
			else if (col.gameObject.CompareTag ("mushroom_quest")) { // hands out quest
				atMushroomQuestMan = true;
				NPCBackground.color = new Color (1, 1, 1, 1);
				NPCTitle.text = "Gordon";
				if (!hasMushroom && !returnedMushroom) { // rewards for handing in mushroom
					NPCText.text = "Would you be able to get something for me? There's a magic mushroom in the forest across the river. " +
						"If you find it and bring it back to me, I'll give you a reward.";
				} else if (hasMushroom && !returnedMushroom) {
					NPCText.text = "Thank you for your help!";
					hasMushroom = false;
					returnedMushroom = true;
					questsCompleted++;
					food += 8;
					gold += 8;
				} else { //(!hasMushroom && returnedMushroom)
					NPCText.text = "Thanks again!";
				}
			} else if (col.gameObject.CompareTag ("gambling")) { // open gambling
				atGambling = true;
				NPCBackground.color = new Color (1, 1, 1, 1);
				NPCTitle.text = "Betting Game";
				NPCText.text = "Would you like to play a game? Press '<' to decrease your bet, '>' to increase your bet, " +
				"and space to submit. If I get heads when I flip this coin, I'll double your bet. Otherwise you'll lose it.";
				betText.text = "Current bet: 0 gold";
				currentBet = 0;
			}

			// check to see if player has visited any location for explorer achievement
			if (col.gameObject.name == "explore1") {
				explore_locs [0] = true;
			} else if (col.gameObject.name == "explore2") {
				explore_locs [1] = true;
			} else if (col.gameObject.name == "explore3") {
				explore_locs [2] = true;
			} else if (col.gameObject.name == "explore4") {
				explore_locs [3] = true;
			} else if (col.gameObject.name == "explore5") {
				explore_locs [4] = true;
			} else if (col.gameObject.name == "explore6") {
				explore_locs [5] = true;
			} else if (col.gameObject.name == "explore7") {
				explore_locs [6] = true;
			} else if (col.gameObject.name == "explore8") {
				explore_locs [7] = true;
			}
		}
			
        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
			if (!freezePlayer) { // only move if player isn't frozen
				float speed;
				GetInput (out speed);
				// always move along the camera forward as it is the direction that it being aimed at
				Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

				// get a normal for the surface that is being touched to move along it
				RaycastHit hitInfo;
				Physics.SphereCast (transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
					m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
				desiredMove = Vector3.ProjectOnPlane (desiredMove, hitInfo.normal).normalized;

				m_MoveDir.x = desiredMove.x * speed;
				m_MoveDir.z = desiredMove.z * speed;

				if (m_CharacterController.isGrounded) {
					m_MoveDir.y = -m_StickToGroundForce;

					if (m_Jump) {
						m_MoveDir.y = m_JumpSpeed;
						//PlayJumpSound ();
						m_Jump = false;
						m_Jumping = true;
					}
				} else {
					m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
				}
				m_CollisionFlags = m_CharacterController.Move (m_MoveDir * Time.fixedDeltaTime);

				ProgressStepCycle (speed);
				UpdateCameraPosition (speed);

				// move gun along with camera
				Quaternion rotation = Quaternion.identity;
				rotation.eulerAngles = new Vector3 (0, -90, 0);
				gun.transform.localRotation = m_Camera.transform.localRotation * rotation;

				m_MouseLook.UpdateCursorLock ();
			}
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            //PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
