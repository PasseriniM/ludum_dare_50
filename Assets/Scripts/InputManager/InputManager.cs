using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	[SerializeField]
	private MapManager mm;
	[SerializeField]
	private HQCharacterAI hq;
	[SerializeField]
	private Tilemap highlight;
	[SerializeField]
	private TileBase messageTile;

	private bool drawingMessage;

	private List<Vector3Int> messageRoute = new List<Vector3Int>();
	private Vector3Int previousCell;

	private Color messageHighlight = Color.red;

	private void Update()
	{
		if (drawingMessage)
		{
			var cell = CellClicked();

			if (CheckValidity(cell))
			{
				AddToRoute(cell);
			}
		}
	}

	private bool CheckValidity(Vector3Int cell)
	{
		var lastCell = messageRoute[messageRoute.Count - 1];

		return cell != lastCell && cell != previousCell && NeighboursOf(lastCell).Contains(cell);
	}

	public void ClickTest(InputAction.CallbackContext context)
	{

		if (context.performed)
		{
			var cell = CellClicked();

			Debug.Log("Mouse clicked " + cell);

			if (hq.cellsOccupied.Contains(cell))
			{
				messageRoute.Add(cell);
				//highlight.SetTile(cell, messageTile);
				drawingMessage = true;
			}

		}
		else if (context.canceled)
		{
			if (drawingMessage)
			{

				drawingMessage = false;

				if (hq.cellsOccupied.Contains(messageRoute[messageRoute.Count - 1]))
				{
					Debug.Log("Valid Route");
				}
				else
				{
					messageRoute.Clear();
					highlight.ClearAllTiles();
					Debug.Log("Invalid Route");
				}
			}
		}
	}

	private void AddToRoute(Vector3Int cell)
	{
		previousCell = messageRoute[messageRoute.Count - 1];
		messageRoute.Add(cell);
		highlight.SetTile(cell, messageTile);
		Debug.Log("Route Length: " + messageRoute.Count);
	}

	private Vector3Int CellClicked()
	{
		var pos = Mouse.current.position.ReadValue();
		var wPos = Camera.main.ScreenToWorldPoint(pos);
		return mm.map.WorldToCell(wPos);
	}

	private HashSet<Vector3Int> NeighboursOf(Vector3Int cell)
	{
		var res = new HashSet<Vector3Int>();

		if (cell.y % 2 == 0)
		{
			res.Add(cell + new Vector3Int(0, 1, 0));
			res.Add(cell + new Vector3Int(1, 0, 0));
			res.Add(cell + new Vector3Int(0, -1, 0));
			res.Add(cell + new Vector3Int(-1, -1, 0));
			res.Add(cell + new Vector3Int(-1, 0, 0));
			res.Add(cell + new Vector3Int(-1, 1, 0));
		}
		else
		{
			res.Add(cell + new Vector3Int(1, 1, 0));
			res.Add(cell + new Vector3Int(1, 0, 0));
			res.Add(cell + new Vector3Int(1, -1, 0));
			res.Add(cell + new Vector3Int(0, -1, 0));
			res.Add(cell + new Vector3Int(-1, 0, 0));
			res.Add(cell + new Vector3Int(0, 1, 0));
		}

		return res;
	}
}
