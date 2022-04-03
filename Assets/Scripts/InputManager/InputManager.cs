using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

enum MessageDefinition
{
	DISABLED,
	MESSAGE_PATH,
	COMMAND_START,
	COMMAND_PATH
}

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
	[SerializeField]
	private TileBase commandTile;
	[SerializeField]
	private List<MessengerAI> availableMessengers;
	private List<MessengerAI> busyMessengers = new List<MessengerAI>();

	private MessageDefinition msg = MessageDefinition.DISABLED;

	private List<Vector3Int> messageRoute = new List<Vector3Int>();
	private Dictionary<Vector3Int, List<Vector3Int>> commands = new Dictionary<Vector3Int, List<Vector3Int>>();
	private Vector3Int previousCell;
	private Vector3Int currentCommand;

	private void Update()
	{
		switch (msg)
		{
			case (MessageDefinition.MESSAGE_PATH):
				{
					var cell = CellClicked();

					if (CheckValidity(cell))
					{
						AddToMessage(cell);
					}
					break;
				}
			case (MessageDefinition.COMMAND_START):
				{
					break;
				}
			case (MessageDefinition.COMMAND_PATH):
				{
					var cell = CellClicked();

					if (CheckValidity(currentCommand, cell))
					{
						AddToCommand(currentCommand, cell);
					}
					break;
				}
			default:
				{
					break;
				}
		}
	}

	private bool CheckValidity(Vector3Int cell)
	{
		var lastCell = messageRoute[messageRoute.Count - 1];

		return cell != lastCell && cell != previousCell && NeighboursOf(lastCell).Contains(cell);
	}

	private bool CheckValidity(Vector3Int key, Vector3Int cell)
	{
		var comm = commands[key];
		var lastCell = comm[comm.Count - 1];



		return cell != lastCell && cell != previousCell && !hq.cellsOccupied.Contains(cell) && NeighboursOf(lastCell).Contains(cell);
	}

	public void ClickTest(InputAction.CallbackContext context)
	{

		switch (msg)
		{
			case (MessageDefinition.MESSAGE_PATH):
				{
					if (context.canceled)
					{
						if (hq.cellsOccupied.Contains(messageRoute[messageRoute.Count - 1]))
						{
							Debug.Log("Valid Route");
							msg = MessageDefinition.COMMAND_START;
						}
						else
						{

							Reset();
							Debug.Log("Invalid Route");
						}
					}

					break;
				}
			case (MessageDefinition.COMMAND_START):
				{
					if (context.performed)
					{
						var cell = CellClicked();

						if (messageRoute.Contains(cell))
						{
							commands.Add(cell, new List<Vector3Int>());
							commands[cell].Add(cell);
							previousCell = cell;
							currentCommand = cell;
							msg = MessageDefinition.COMMAND_PATH;
						}
					}

					break;
				}
			case (MessageDefinition.COMMAND_PATH):
				{
					if (context.canceled)
					{
						msg = MessageDefinition.COMMAND_START;
					}

					break;
				}
			default:
				{
					if (context.performed)
					{
						var cell = CellClicked();

						Debug.Log("Mouse clicked " + cell);

						if (hq.cellsOccupied.Contains(cell))
						{
							messageRoute.Add(cell);
							//highlight.SetTile(cell, messageTile);
							msg = MessageDefinition.MESSAGE_PATH;
						}

					}

					break;
				}
		}
	}

	private void AddToMessage(Vector3Int cell)
	{
		previousCell = messageRoute[messageRoute.Count - 1];
		messageRoute.Add(cell);
		highlight.SetTile(cell, messageTile);
		//Debug.Log("Route Length: " + messageRoute.Count);
	}

	private void AddToCommand(Vector3Int key, Vector3Int cell)
	{
		var comm = commands[key];
		previousCell = comm[comm.Count - 1];
		comm.Add(cell);
		highlight.SetTile(cell, commandTile);
		//Debug.Log("Route Length: " + messageRoute.Count);
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

	private void Reset()
	{
		msg = MessageDefinition.DISABLED;
		messageRoute.Clear();
		commands.Clear();
		highlight.ClearAllTiles();
	}

	public void ConfirmMessage()
	{
		if (msg == MessageDefinition.COMMAND_START)
		{

			var m = availableMessengers[0];

			availableMessengers.RemoveAt(0);
			busyMessengers.Add(m);

			foreach (var comm in commands)
			{
				m.AddMessage(comm.Key, comm.Value);
			}

			m.StartPath(messageRoute);
			Reset();
		}
	}
}
