using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MovingObject {

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	// stores our animator ( component added to the player prefab)
	private Animator animator;
	private int food;
	// Where the user's touch starts. We initialize with a point off screen. (-1, -1)
	private Vector2 touchOrigin = -Vector2.one;

	// Use this for initialization
	// we have a different implementation as a player
	// than we do in our base class (MovingObject).
	protected override void Start ()
	{
		// we added this when we made a prefab game object for the player
		animator = GetComponent<Animator> ();

		// Gets food point from gamemanager 
		// we stash food points in gamemanager
		// we retrieve them at the start of every level:
		food = GameManager.instance.playerFoodPoints;

		foodText.text = "Food: " + food;

		base.Start ();
	}

	// part of unity API
	// called when Player game object is disabled
	private void OnDisable()
	{
		GameManager.instance.playerFoodPoints = food;
	}


	// Update is called once per frame
	void Update () {
		if (!GameManager.instance.playersTurn) return;

		int horizontal = 0;
		int vertical = 0;

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
		horizontal = (int) Input.GetAxisRaw ("Horizontal");
		vertical = (int) Input.GetAxisRaw ("Vertical");

		// keeps from moving diagonally
		if (horizontal != 0)
			vertical = 0;
		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		//Check if Input has registered more than zero touches
		if (Input.touchCount > 0)
		{
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began)
			{
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
			}

			//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
				//Set touchEnd to equal the position of this touch
				Vector2 touchEnd = myTouch.position;

				//Calculate the difference between the beginning and end of the touch on the x axis.
				float x = touchEnd.x - touchOrigin.x;

				//Calculate the difference between the beginning and end of the touch on the y axis.
				float y = touchEnd.y - touchOrigin.y;

				//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
				touchOrigin.x = -1;

				//Check if the difference along the x axis is greater than the difference along the y axis.
				// Are we generally swiping vertically (y) or horizontally (x) 
				if (Mathf.Abs(x) > Mathf.Abs(y))
					//If x is greater than zero, set horizontal to 1, otherwise set it to -1
					// left or right?
					horizontal = x > 0 ? 1 : -1;
				else
					//If y is greater than zero, set horizontal to 1, otherwise set it to -1
					// up or down?
					vertical = y > 0 ? 1 : -1;
			}
		}
		#endif
		// do we have a non zero value?
		if (horizontal != 0 || vertical != 0)
			AttemptMove<Wall> (horizontal, vertical);
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		food--;
		foodText.text = "Food: " + food;

		base.AttemptMove <T> (xDir, yDir);

		RaycastHit2D hit;
		// Test if player moved
		if (Move (xDir, yDir, out hit))
		{
			// Play a Moving sound
			// tell our singleton to pick one out any number of args given
			// the more sounds given as args the more variety of sound choice
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
		}

		checkIfGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		if (other.tag == "Exit") {
			// Looks like JS' .call()
			// calls this.Restart() 
			// but not until we've waited for 'restartLevelDelay'
			Invoke ("Restart", restartLevelDelay);
			enabled = false;
		} else if (other.tag == "Food") {
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food: " + food;
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			// seems to turn off the game object 
			// that was dynamically added to the game board
			// question: does it remove it from memory?
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food: " + food;
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);

			other.gameObject.SetActive (false);
		}
	}

	protected override void OnCantMove <T> (T component)
	{
		Wall hitWall = component as Wall;

		hitWall.DamageWall (wallDamage);

		animator.SetTrigger ("playerChop");
	}

	private void Restart()
	{
		// Obsolete, use SceneManager.LoadScene
		// For some reason the tutorial says do this.
		// my guess is they'd rather you reload the current scene
		// SceneManager.LoadScene(SceneManager.getActiveScene().name);
		Application.LoadLevel(Application.loadedLevel);
	}

	public void LoseFood (int loss)
	{
		animator.SetTrigger ("playerHit");

		food -= loss;
		foodText.text = "-" + loss + " Food: " + food;

		checkIfGameOver ();
	}

	private void checkIfGameOver()
	{
		if (food <= 0)
		{
			SoundManager.instance.PlaySingle (gameOverSound);
			SoundManager.instance.musicSource.Stop ();
			GameManager.instance.GameOver ();
		}
	}
}