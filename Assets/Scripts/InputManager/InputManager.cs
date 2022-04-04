using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public enum MessageState
{
	DISABLED,
	MESSAGE_START,
	MESSAGE_PATH,
	COMMAND_START,
	COMMAND_PATH
}

public class InputManager : MonoBehaviour
{
	[Header("Control Buttons")]
	[SerializeField]
	private GameObject confirmButton;
	[SerializeField]
	private GameObject cancelButton;


	[Header("Map Elements")]
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

	public MessageState state = MessageState.MESSAGE_START;

	private List<Vector3Int> messageRoute = new List<Vector3Int>();
	private Dictionary<Vector3Int, List<Vector3Int>> commands = new Dictionary<Vector3Int, List<Vector3Int>>();
	private Vector3Int previousCell;
	private MessengerAI currentMessenger = null;
	private Vector3Int currentCommand;

	private void Update()
	{
		switch (state)
		{
			case (MessageState.DISABLED):
				{
					confirmButton.SetActive(false);
					cancelButton.SetActive(false);

					if (availableMessengers.Count > 0)
						state = MessageState.MESSAGE_START;

					break;
				}
			case (MessageState.MESSAGE_PATH):
				{
					var cell = CellClicked();

					if (CheckMessageValidity(cell))
					{
						AddToMessage(cell);
					}
					break;
				}
			case (MessageState.COMMAND_PATH):
				{
					var cell = CellClicked();

					if (CheckCommandValidity(currentCommand, cell))
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
		UpdateMessengerState();
	}

	private void UpdateMessengerState()
	{
		for (int i = busyMessengers.Count - 1; i >= 0; i--)
		{
			if (busyMessengers[i].HasArrived())
			{
				if (IsOccupied(busyMessengers[i].CurrentPosition()))
				{
					var free = HQFreeCells();
					//Debug.Log(free.Count);
					var rand = Random.Range(0, free.Count);
					//Debug.Log(rand);
					var newPos = free[rand];
					//Debug.Log(newPos);
					var path = new List<Vector3Int>
					{
						newPos
					};
					busyMessengers[i].StartPath(path);
				}
				else
				{
					availableMessengers.Add(busyMessengers[i]);
					busyMessengers.RemoveAt(i);
				}
			}
		}
	}

	private bool IsOccupied(Vector3Int cell)
	{
		bool res = false;

		foreach (var mess in availableMessengers)
		{
			res |= mess.CurrentPosition() == cell;
		}

		return res;
	}

	private List<Vector3Int> HQFreeCells()
	{
		var res = new List<Vector3Int>(hq.cellsOccupied);

		foreach (var mess in availableMessengers)
		{
			res.Remove(mess.CurrentPosition());
		}

		return res;
	}


	private MessengerAI FindClickedMessenger(Vector3Int cell)
	{
		foreach (var mes in availableMessengers)
		{
			if (mes.CurrentPosition() == cell)
				return mes;
		}

		return null;
	}

	private bool CheckMessageValidity(Vector3Int cell)
	{
		var lastCell = messageRoute[messageRoute.Count - 1];

		return cell != lastCell && cell != previousCell && NeighboursOf(lastCell).Contains(cell);
	}

	private bool CheckCommandValidity(Vector3Int key, Vector3Int cell)
	{
		var comm = commands[key];
		var lastCell = comm[comm.Count - 1];

		return cell != lastCell && cell != previousCell && !hq.cellsOccupied.Contains(cell) && NeighboursOf(lastCell).Contains(cell);
	}

	public void ClickTest(InputAction.CallbackContext context)
	{

		switch (state)
		{
			case (MessageState.MESSAGE_START):
				{
					if (context.performed)
					{
						var cell = CellClicked();
						currentMessenger = FindClickedMessenger(cell);

						if (hq.cellsOccupied.Contains(cell) && currentMessenger != null)
						{
							messageRoute.Add(cell);
							state = MessageState.MESSAGE_PATH;
						}

					}

					break;
				}
			case (MessageState.MESSAGE_PATH):
				{
					if (context.canceled)
					{
						if (hq.cellsOccupied.Contains(messageRoute[messageRoute.Count - 1]) && messageRoute.Count > 1)
						{
							Debug.Log("Valid Route");
							confirmButton.SetActive(true);
							cancelButton.SetActive(true);
							state = MessageState.COMMAND_START;
						}
						else
						{

							Reset();
							Debug.Log("Invalid Route");
						}
					}

					break;
				}
			case (MessageState.COMMAND_START):
				{
					if (context.performed)
					{
						var cell = CellClicked();

						if (messageRoute.Contains(cell))
						{
							if (commands.ContainsKey(cell))
							{
								ClearPath(commands[cell]);
								commands.Remove(cell);
							}

							commands.Add(cell, new List<Vector3Int>());
							commands[cell].Add(cell);
							highlight.SetTile(cell, commandTile);
							previousCell = cell;
							currentCommand = cell;
							state = MessageState.COMMAND_PATH;

						}
					}

					break;
				}
			case (MessageState.COMMAND_PATH):
				{
					if (context.canceled)
					{
						state = MessageState.COMMAND_START;
					}

					break;
				}
			default:
				{
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

	public void Reset()
	{
		messageRoute.Clear();
		commands.Clear();
		highlight.ClearAllTiles();
		state = MessageState.DISABLED;
	}

	public void Clean()
	{
		messageRoute = new List<Vector3Int>();
		commands = new Dictionary<Vector3Int, List<Vector3Int>>();
		highlight.ClearAllTiles();
		state = MessageState.DISABLED;
	}

	private void ClearPath(List<Vector3Int> path)
	{
		foreach (var pos in path)
		{
			highlight.SetTile(pos, null);
		}
	}


	public void ConfirmMessage()
	{
		if (state == MessageState.COMMAND_START)
		{
			availableMessengers.Remove(currentMessenger);
			busyMessengers.Add(currentMessenger);
			//Debug.Log(busyMessengers.Count);

			foreach (var comm in commands)
			{
				currentMessenger.AddMessage(comm.Key, comm.Value);
			}

			currentMessenger.StartPath(messageRoute);
			Debug.Log("Messenger sent");
			Clean();
		}
	}

	public void CancelMessage()
	{
		Reset();
	}
}
