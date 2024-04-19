using System;
using System.Collections.Generic;

class Game
{
	// Private fields
	private Parser parser;
	private Player player;
	private bool keyUsed = false;
	Item key = new Item(15, "Key");
	Item medkit = new Item(20, "Medkit");

	// Constructor
	public Game()
	{
		parser = new Parser();
		player = new Player();
		CreateRooms();
	}

	// Initialise the Rooms (and the Items)
	private void CreateRooms()
	{
		Room courtyard = new Room("in the castle courtyard");
		Room hall = new Room("in the grand hall");
		Room armory = new Room("in the armory");
		Room library = new Room("in the castle library");
		Room chamber = new Room("in the royal chamber");
		Room dungeon = new Room("in the castle dungeon");
		Room tower = new Room("in the highest tower");

		courtyard.AddExit("hall", hall);
		courtyard.AddExit("armory", armory);
		courtyard.AddExit("library", library);

		hall.AddExit("courtyard", courtyard);
		hall.AddExit("chamber", chamber);

		armory.AddExit("courtyard", courtyard);
		armory.AddExit("tower", tower);

		library.AddExit("courtyard", courtyard);
		library.AddExit("tower", tower);

		chamber.AddExit("hall", hall);
		chamber.AddExit("dungeon", dungeon);

		tower.AddExit("armory", armory);
		tower.AddExit("library", library);

		dungeon.AddExit("chamber", chamber);

		armory.Chest.Put("key", key);
		hall.Chest.Put("medkit", medkit);

		player.CurrentRoom = courtyard;
	}

	// Main game loop
	public void Play()
	{
		PrintWelcome();

		bool finished = false;
		while (!finished)
		{
			Command command = parser.GetCommand();
			finished = ProcessCommand(command);
		}
		Console.WriteLine("Thank you for playing.");
		Console.WriteLine("Press [Enter] to continue.");
		Console.ReadLine();
	}

	// Print welcome message
	private void PrintWelcome()
	{
		Console.WriteLine();
		Console.WriteLine("Welcome to Text Adventure!");
		Console.WriteLine("You awaken to find yourself trapped in an castle, surrounded by stone walls.");
		Console.WriteLine("Your only chance of escape lies in uncovering its secrets and finding a way out.");
		Console.WriteLine("Type 'help' if you need help.");
		Console.WriteLine();
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
	}

	// Process user commands
	private bool ProcessCommand(Command command)
	{
		bool wantToQuit = false;

		if (!player.IsAlive() && command.CommandWord != "quit")
		{
			Console.WriteLine("You died");
			Console.WriteLine("You can only use the command:");
			Console.WriteLine("quit");
			return wantToQuit;
		}

		if (keyUsed && command.CommandWord != "quit")
		{
			Console.WriteLine("You have already escaped the castle.");
			Console.WriteLine("The only allowed command is 'quit'.");
			return wantToQuit;
		}

		if (command.IsUnknown())
		{
			Console.WriteLine("I don't know what you mean...");
			return wantToQuit;
		}

		switch (command.CommandWord)
		{
			case "help":
				PrintHelp();
				break;
			case "look":
				Look();
				break;
			case "take":
				Take(command);
				break;
			case "drop":
				Drop(command);
				break;
			case "status":
				Health();
				break;
			case "go":
				GoRoom(command);
				break;
			case "use":
				UseItem(command, out keyUsed);
				break;
			case "quit":
				wantToQuit = true;
				break;
		}

		return wantToQuit;
	}

	// Print help information
	private void PrintHelp()
	{
		Console.WriteLine("You are exploring a castle.");
		Console.WriteLine("Your mission: Uncover its secrets and escape.");
		Console.WriteLine();
		parser.PrintValidCommands();
	}

	// Look around the room
	private void Look()
	{
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));

		Dictionary<string, Item> roomItems = player.CurrentRoom.Chest.GetItems();
		if (roomItems.Count > 0)
		{
			Console.WriteLine("Items in this room:");
			foreach (var itemEntry in roomItems)
			{
				Console.WriteLine($"{itemEntry.Value.Description} - ({itemEntry.Value.Weight} kg)");
			}
		}
	}

	// Take an item from the room
	private void Take(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Take what?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.TakeFromChest(itemName);

	}

	// Drop an item to the room
	private void Drop(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Drop what?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.DropToChest(itemName);


	}

	// Display player's health and items
	private void Health()
	{
		Console.WriteLine($"Your health is: {player.GetHealth()}");

		Dictionary<string, Item> items = player.GetItems();

		if (items.Count > 0)
		{
			Console.WriteLine("Your current items:");

			foreach (var itemEntry in items)
			{
				Console.WriteLine($"- {itemEntry.Key}: Weight {itemEntry.Value.Weight}");
			}
		}
		else
		{
			Console.WriteLine("You have no items in your inventory.");
		}
	}

	// Move to another room
	private void GoRoom(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Go where?");
			return;
		}

		string direction = command.SecondWord;

		Room nextRoom = player.CurrentRoom.GetExit(direction);
		if (nextRoom == null)
		{
			Console.WriteLine("There is no path to " + direction + "!");
			return;
		}

		player.Damage(15);
		player.CurrentRoom = nextRoom;
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
		if (!player.IsAlive())
		{
			Console.WriteLine("Your vision blurs, the world fades. Your wounds drain your strength. You collapse, succumbing to the dangers of the castle...");
		}
	}

	// Use an item
	private void UseItem(Command command, out bool keyUsed)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Use what?");
			keyUsed = false;
			return;
		}

		string itemName = command.SecondWord.ToLower();

		if (itemName == "key")
		{
			if (!player.CurrentRoom.IsCourtyard())
			{
				Console.WriteLine("You can only use the key in the courtyard.");
				keyUsed = false;
				return;
			}
		}
		else
		{
			bool itemUsed = player.Use(itemName, out keyUsed);

			if (itemUsed)
			{
				// Additional actions for using other items
			}
			return;
		}

		if (player.CurrentRoom.IsCourtyard())
		{
			bool itemUsed = player.Use(itemName, out keyUsed);

			if (itemUsed)
			{
				if (keyUsed)
				{
					this.keyUsed = true;
					Console.WriteLine(" ");
					Console.WriteLine("You use the key to unlock a hidden door in the courtyard.");
					Console.WriteLine("Stepping through the passage, you find yourself outside the castle walls.");
					Console.WriteLine(" ");
					Console.WriteLine("Congratulations! You have successfully escaped the castle.");
				}
			}
		}
		else
		{
			Console.WriteLine("You can only use the key in the courtyard.");
			keyUsed = false;
		}
	}
}
