using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameBoard : MonoBehaviour
{

	public int width;
	public int height;
	private float tileOffset = 0.87f;

	private bool setUpFinished = false;

	// Empty 2D Container to hold game tiles
	private GameObject[,] potionTiles;
	
	public GameObject tilePrefab;
	public GameObject[] potionPrefabs;

	// For storing background and potion tiles neatly
	private GameObject backgroundTilesParent;
	private GameObject potionsParent;

	// Used to track direction traversing tiles
	enum Direction
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	// Use this for initialization
	void Start () {
		backgroundTilesParent = new GameObject();
		potionsParent = new GameObject();
		potionTiles = new GameObject[width, height];
		SetUp();
	}

	private void SetUp ()
	{
		backgroundTilesParent.name = "backgroundTiles";
		backgroundTilesParent.transform.parent = this.transform;
		
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				GameObject backgroundTile =
					Instantiate(tilePrefab, new Vector2(i * tileOffset, j * tileOffset), Quaternion.identity);
				
				backgroundTile.transform.parent = backgroundTilesParent.transform;
				backgroundTile.name = "( " + i + ", " + j + " )";
			}
		}

		StartCoroutine(PopulateGameBoard(0.1f));
	}

	// TODO: Play an animation when generating these
	private GameObject GeneratePotion(Vector2Int position)
	{
		int potionToGenerate = Random.Range(0, potionPrefabs.Length);
		GameObject potion = Instantiate(potionPrefabs[potionToGenerate], new Vector2(position.x*tileOffset, position.y*tileOffset), Quaternion.identity);
		potion.GetComponent<Potion>().SetPosition(position);
		potion.transform.parent = potionsParent.transform;
		potionTiles[position.x, position.y] = potion;
		return potion;
	}
	
	private IEnumerator PopulateGameBoard(float delay)
	{
		Debug.Log("Populating Game Board");
		potionsParent.name = "potionTiles";
		potionsParent.transform.parent = this.transform;
		//for  (int j = 0; j < height; j++)
		//{
		//	for (int i = 0; i < width; i++)
		
		for (int i = 0; i < width; i++)
		{
			for  (int j = 0; j < height; j++)
			{
				GameObject potion = GeneratePotion(new Vector2Int(i, j));
				yield return new WaitForSeconds(delay);
			}
		}

		setUpFinished = true;
		Debug.Log("Game board is finished");
	}

	private void PotionClicked(GameObject potionClicked)
	{
		Potion potion = potionClicked.GetComponent<Potion>();
		Vector2Int potionPosition = potion.GetPosition();
		
		Debug.Log("Potion clicked colour: " + potion.colour + ", at position: "  + potionPosition); //TODO Remove log

		HashSet<GameObject> effectedTiles = TraverseAdjoiningTiles(potionPosition, potion.colour, Direction.None);
		effectedTiles.Add(potionClicked);
		
		// TODO Remove this
		foreach (var tile in effectedTiles)
		{
			Vector2Int pos = tile.GetComponent<Potion>().GetPosition();
			Debug.Log(tile.name + ", position: " + pos.x + ", " + pos.y);
		}
		// ----------------
		
		// TODO: Note if implementing a minimum number of potions, (e.g. 3) then we need to set traveresed back to false
		RemoveTiles(effectedTiles);
		StartCoroutine(ReplaceTiles(0.5f));
	}

	// TODO: Tidy up and make neaters
	private HashSet<GameObject> TraverseAdjoiningTiles(Vector2Int pos, string colour, Direction tileDirection)
	{
		Debug.Log("Traverse Adjoining Tiles");
		
		HashSet<GameObject> effectedTiles = new HashSet<GameObject>();
		HashSet<GameObject> tempSet;

		potionTiles[pos.x, pos.y].GetComponent<Potion>().traveresed = true;
		
		// Check tiles below
		if (pos.y > 0 && potionTiles[pos.x, pos.y - 1] != null && tileDirection != Direction.Up)
		{
			Potion potion = potionTiles[pos.x, pos.y - 1].GetComponent<Potion>();
			
			if (potion.colour == colour && potion.traveresed == false)
			{
				// Add the tile that matches
				effectedTiles.Add(potionTiles[pos.x, pos.y - 1]);
								
				// And recursively call the function
				tempSet = TraverseAdjoiningTiles(new Vector2Int(pos.x, pos.y - 1), colour, Direction.Down);
				foreach (var tile in tempSet)
				{
					effectedTiles.Add(tile);
				}
			}
		}

		// Check tiles above
		if (pos.y < height-1 && potionTiles[pos.x, pos.y + 1] != null && tileDirection != Direction.Down)
		{
			Potion potion = potionTiles[pos.x, pos.y + 1].GetComponent<Potion>();
			
			if (potion.colour == colour && potion.traveresed == false)
			{
				//Add the tile that matches
				effectedTiles.Add(potionTiles[pos.x, pos.y + 1]);
								
				//And recursively call the function
				tempSet = TraverseAdjoiningTiles(new Vector2Int(pos.x, pos.y + 1), colour, Direction.Up);
				foreach (var tile in tempSet)
				{
					effectedTiles.Add(tile);
				}
			}
		}
		
		// Check tiles left
		if (pos.x > 0 && potionTiles[pos.x - 1, pos.y] != null && tileDirection != Direction.Right)
		{
			Potion potion = potionTiles[pos.x - 1, pos.y].GetComponent<Potion>();
			
			if (potion.colour == colour && potion.traveresed == false)
			{
				// Add the tile that matches
				effectedTiles.Add(potionTiles[pos.x - 1, pos.y]);
								
				// And recursively call the function
				tempSet = TraverseAdjoiningTiles(new Vector2Int(pos.x - 1, pos.y), colour, Direction.Left);
				foreach (var tile in tempSet)
				{
					effectedTiles.Add(tile);
				}
			}
		}
		
		// Check tiles to the right
		if (pos.x < width-1 && potionTiles[pos.x + 1, pos.y] != null && tileDirection != Direction.Left)
		{
			Potion potion = potionTiles[pos.x + 1, pos.y].GetComponent<Potion>();
			
			if (potion.colour == colour && potion.traveresed == false)
			{
				//Add the tile that matches
				effectedTiles.Add(potionTiles[pos.x + 1, pos.y]);
								
				//And recursively call the function
				tempSet = TraverseAdjoiningTiles(new Vector2Int(pos.x + 1, pos.y), colour, Direction.Right);
				foreach (var tile in tempSet)
				{
					effectedTiles.Add(tile);
				}
			}
		}
		
		Debug.Log("Returning tiles, size: " + effectedTiles.Count);
		return effectedTiles;
	}

	private void RemoveTiles(HashSet<GameObject> tileSet)
	{
		HashSet<int> effectedColumns = new HashSet<int>();
		
		foreach (var tile in tileSet)
		{
			Vector2Int tilePos = tile.GetComponent<Potion>().GetPosition();
			effectedColumns.Add(tilePos.x);
			potionTiles[tilePos.x, tilePos.y] = null;
			Destroy(tile);
		}
		
		UpdateEffectedColumns(effectedColumns);
	}

	private IEnumerator ReplaceTiles(float delay)
	{
		yield return new WaitForSeconds(delay);

		HashSet<int> columnsEffected = new HashSet<int>();
		
		//TODO Slightly delay the creation, so can been seen to spawn and move down
		do
		{
			columnsEffected.Clear();
			
			for (int x = 0; x < width; x++)
			{
				if (potionTiles[x, height - 1] == null)
				{
					columnsEffected.Add(x);
					//Debug.Log("Empty at: " + x + ", " + (height-1));
					yield return new WaitForSeconds(delay/2);

					GeneratePotion(new Vector2Int(x, height - 1));
				}
			}
			
			yield return new WaitForSeconds(delay);

			UpdateEffectedColumns(columnsEffected);

		} while (columnsEffected.Count() != 0);

	}

	private void UpdateEffectedColumns(HashSet<int> columnsEffected)
	{
		foreach (var col in columnsEffected)
		{
			// Need to go through the column as many times as there is rows
			// TODO Maybe this can be optimised, but might not be the worst thing in the world
			for (int loop = 1; loop < height; loop++)
			{
				// Start one from the bottom of the column
				for (int i = 1; i < height; i++)
				{
					if (potionTiles[col, i] != null)
					{
						if (potionTiles[col, i - 1] == null)
						{
							// Make this move not just instant
							Vector2 currentPosition = potionTiles[col, i].transform.position;
							potionTiles[col, i].transform.position = new Vector2(currentPosition.x, currentPosition.y - 1 * tileOffset);

							potionTiles[col, i - 1] = potionTiles[col, i];
							potionTiles[col, i - 1].GetComponent<Potion>().SetPosition(new Vector2Int(col, i - 1));
							potionTiles[col, i] = null;
						}
					}
				}
			}
		}
		
	}

	// Handle Input
	private void Update()
	{
		// TODO Use something like this for touch control
		//for (var i = 0; i < Input.touchCount; ++i) {
		//	if (Input.GetTouch(i).phase == TouchPhase.Began) {
		// ---------------------------------------------
		
		if (setUpFinished && Input.GetMouseButtonDown (0)) {
			Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
			
			// RaycastHit2D can be either true or null, but has an implicit conversion to bool, so we can use it like this
			if(hitInfo)
			{
				if (hitInfo.transform.gameObject.tag == "potion")
				{
					PotionClicked(hitInfo.transform.gameObject);
				}
			}
		}	
	}
}
