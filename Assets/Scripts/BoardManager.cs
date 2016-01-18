using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

	[Serializable]
	public class Count
	{
		public int minimum;
		public int maximum;

		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}

	// Gameboard dimensions
	public int columns = 8;
	public int rows = 8;

	// Count classs is "Serializable". It seems to 
	// allow adjustments of min, max from unity
	public Count wallCount = new Count(5, 9);
	public Count foodCount = new Count(1, 5);

	// For Prefabs
	// NB: there is only one exit at a time?
	public GameObject exit;
	// Arrays of prefabs for these guys
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] foodTiles;
	public GameObject[] enemyTiles;
	public GameObject[] outerWallTiles;

	// container of in game objects
	// will hold hierarchy of game objects
	private Transform boardHolder;
	// used to track all possible positions on game board
	// and if object has spawned in a position
	private List <Vector3> gridPositions = new List<Vector3> ();

	void InitializeList()
	{
		gridPositions.Clear();
		// sets up all possible positions on a 6x6 area
		// starting from coordinates (1,1) and looping 
		// For example (x,1) -> (x,6)
		for (int x = 1; x < columns - 1; x++)
		{
			// gets us thru 6 rows, y-axis (x,y)
			for (int y = 1; y < rows - 1; y++)
			{
				gridPositions.Add (new Vector3 (x, y, 0f));
			}
		}
	}

	void BoardSetup()
	{
		// holds everything on the "board"
		boardHolder = new GameObject ("Board").transform;

		// setups our walls
		// -1 is because the walls are built outside the 8x8 grid
		for (int x = -1; x < columns + 1; x++) {
			for (int y = -1; y < rows + 1; y++) {
				// get a random floor tile prefab and stuff it in a game object
				GameObject toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];
				// unless you are an outer wall (either -1 or equal to the outer dimensions)
				if (x == -1 || x == columns || y == -1 || y == rows)
				{
					// overwrites the default floor tile if it needs to be an outer wall
					toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
				}
				// Now create the actual instance, at (x,y) with no rotation (Quaternion) and cast 
				GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
				// add it to our board
				instance.transform.SetParent (boardHolder);
			}
		}
	}

	Vector3 RandomPosition()
	{
		int randomIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPosition = gridPositions [randomIndex];
		gridPositions.RemoveAt (randomIndex);

		return randomPosition;
	}

	void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
	{
		int objectCount = Random.Range (minimum, maximum + 1);

		for (int i = 0; i < objectCount; i++)
		{
			Vector3 randomPosition = RandomPosition ();
			GameObject tileChoice = tileArray [Random.Range (0, tileArray.Length)];
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	public void SetupScene(int level)
	{
		BoardSetup ();
		InitializeList ();
		LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
		LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);

		// will scale difficulty logarithmically by level like so:
		// 0 @ lvl 1, 1 @ level 2, 2 @ lvl 4, 3 @ lvl 8 
		int enemyCount = (int)Mathf.Log (level, 2f);
		// we use the same count for min & max because we dont want 
		// a range of object counts
		LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);
		Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
	}

	/* Tutorial said get rid of you...sorry
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	*/
}
