﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MovingObject {

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;

	// stores our animator ( component added to the player prefab)
	private Animator animator;
	private int food;

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

		horizontal = (int) Input.GetAxisRaw ("Horizontal");
		vertical = (int) Input.GetAxisRaw ("Vertical");

		// keeps from moving diagonally
		if (horizontal != 0)
			vertical = 0;

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
			foodText.text = "Food: " + food;
			// seems to turn off the game object 
			// that was dynamically added to the game board
			// question: does it remove it from memory?
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "Food: " + food;

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
		checkIfGameOver ();
	}

	private void checkIfGameOver()
	{
		if (food <= 0)
		{
			GameManager.instance.GameOver ();
		}
	}
}