using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessengerButton : MonoBehaviour
{
	const string DISABLED_TEXT = "Send Messenger";
	const string CANCEL_MESSAGE = "Cancel Message";
	const string CONFIRM_MESSAGE = "Confirm Message";

	[SerializeField]
	private GameObject cancelButton;
	private Text label;
	private InputManager im;

	private void Awake()
	{
		label = GetComponentInChildren<Text>();
		im = GetComponentInParent<InputManager>();
	}

	private void Update()
	{
		switch (im.state)
		{
			case (MessageState.DISABLED):
				{
					label.text = DISABLED_TEXT;
					cancelButton.SetActive(false);
					break;
				}
			case (MessageState.MESSAGE_START):
				{
					label.text = CANCEL_MESSAGE;
					cancelButton.SetActive(false);
					break;
				}
			case (MessageState.MESSAGE_PATH):
				{
					label.text = CANCEL_MESSAGE;
					cancelButton.SetActive(false);
					break;
				}
			case (MessageState.COMMAND_START):
				{
					label.text = CONFIRM_MESSAGE;
					cancelButton.SetActive(true);
					break;
				}
			case (MessageState.COMMAND_PATH):
				{
					label.text = CONFIRM_MESSAGE;
					cancelButton.SetActive(false);
					break;
				}
		}
	}


}
