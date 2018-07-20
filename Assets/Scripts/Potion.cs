using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
	public string colour;
	
	[SerializeField]
	private Vector2Int position;

	public bool traveresed = false;

	public void SetPosition(Vector2Int pos)
	{
		position = pos;
	}

	public Vector2Int GetPosition()
	{
		return position;
	}
}
