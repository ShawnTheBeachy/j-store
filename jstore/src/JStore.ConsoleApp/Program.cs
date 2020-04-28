using JStore.ConsoleApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JStore.ConsoleApp
{
	public sealed class Program
	{
		private static Repository<Fruit> _fruitRepo;

		private static async Task Main(string[] _)
		{
			Console.Clear();

			var services = new ServiceCollection();
			services.AddJStore();
			var serviceProvider = services.BuildServiceProvider();
			_fruitRepo = serviceProvider.GetRequiredService<Repository<Fruit>>();
			await _fruitRepo.InitializeAsync();

			ShowHelp();
			await LoopAsync();
		}

		private static void AddFruit(string[] parts)
		{
			if (parts.Length != 5)
			{
				Console.WriteLine("Invalid input.");
				return;
			}

			var name = parts[1];
			var color = parts[2];
			var shape = parts[3];
			var count = int.Parse(parts[4]);

			for (var i = 0; i < count; i++)
			{
				var fruit = new Fruit
				{
					Color = color,
					Id = Guid.NewGuid(),
					Name = name,
					Shape = shape
				};
				_fruitRepo.Items.Add(fruit);
			}
		}

		private static async Task DumpJsonAsync()
		{
			var json = await File.ReadAllTextAsync("store.json");
			using var jsonDoc = JsonDocument.Parse(json);

			using var stream = new MemoryStream();
			using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
			jsonDoc.WriteTo(writer);
			await writer.FlushAsync();

			json = Encoding.UTF8.GetString(stream.ToArray());
			Console.Write(json);
			Console.WriteLine();
		}
		
		private static async Task HandleCommandAsync(string[] parts)
		{
			switch (parts[0])
			{
				case "add":
					AddFruit(parts);
					break;
				case "clear":
					_fruitRepo.Items.Clear();
					break;
				case "count":
					Console.WriteLine(_fruitRepo.Items.Count);
					break;
				case "delete-file":
					File.Delete("store.json");
					await Main(new string[0]);
					break;
				case "dump":
					await DumpJsonAsync();
					break;
				case "help":
					ShowHelp();
					break;
				case "remove":
					Remove(parts);
					break;
				case "restart":
					await Main(new string[0]);
					break;
				case "save":
					await _fruitRepo.SaveAsync();
					break;
			}
		}

		private static async Task LoopAsync()
		{
			Console.WriteLine();
			Console.Write("Command: ");
			var input = Console.ReadLine();
			var parts = input.Split(' ');

			if (parts.Length > 0)
			{
				await HandleCommandAsync(parts);
			}

			await LoopAsync();
		}

		private static void Remove(string[] parts)
		{
			var index = int.Parse(parts[1]);

			if (index < 0 || index > _fruitRepo.Items.Count - 1)
			{
				Console.WriteLine("Index out of range.");
				return;
			}

			_fruitRepo.Items.RemoveAt(index);
		}

		private static void ShowHelp()
		{
			Console.WriteLine("Here are all the valid commands:");
			Console.WriteLine("add <name> <color> <shape> <how-many-to-add>");
			Console.WriteLine("clear");
			Console.WriteLine("count");
			Console.WriteLine("delete-file");
			Console.WriteLine("dump");
			Console.WriteLine("help");
			Console.WriteLine("remove <index>");
			Console.WriteLine("restart");
			Console.WriteLine("save");
		}
	}
}
