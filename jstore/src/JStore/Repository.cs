using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JStore
{
	public sealed class Repository<T>
	{
		private readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};
		private string SetName
		{
			get
			{
				var typeName = typeof(T).Name;
				return char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);
			}
		}

		public List<T> Items { get; private set; }

		public Repository()
		{
			if (!File.Exists("store.json"))
			{
				File.WriteAllText("store.json", "{}");
			}

			var jsonElement = GetJsonElement();

			if (jsonElement != null)
			{
				Items = JsonSerializer.Deserialize<List<T>>(jsonElement.Value.GetRawText(), _options);
			}

			else
			{
				Items = new List<T>();
			}
		}

		private async Task<byte[]> GetJsonBytesAsync()
		{
			return await File.ReadAllBytesAsync("store.json");
		}

		private JsonElement? GetJsonElement()
		{
			using var jsonDoc = JsonDocument.Parse(File.ReadAllText("store.json"));
			var root = jsonDoc.RootElement;

			if (!root.TryGetProperty(SetName, out var setElement))
			{
				return null;
			}

			return setElement.Clone();
		}

		private async Task<Stream> GetJsonStreamAsync()
		{
			var stream = new MemoryStream();
			await stream.WriteAsync(await GetJsonBytesAsync());
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public async Task SaveAsync()
		{
			var didFindSet = false;

			using var listStream = new MemoryStream();
			await JsonSerializer.SerializeAsync(listStream, Items, _options);
			var listJson = Encoding.UTF8.GetString(listStream.ToArray());

			using var stream = new MemoryStream();
			using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
			using var streamWriter = new StreamWriter(stream);

			async Task WriteSetAsync(string name, string rawJson)
			{
				writer.WritePropertyName(name);
				writer.WriteStartArray();
				await writer.FlushAsync();
				await streamWriter.WriteAsync(rawJson.TrimStart('[').TrimEnd(']'));
				await streamWriter.FlushAsync();
				writer.WriteEndArray();
				await writer.FlushAsync();
			}

			using var docStream = await GetJsonStreamAsync();
			using var jsonDoc = await JsonDocument.ParseAsync(docStream);
			var root = jsonDoc.RootElement;

			writer.WriteStartObject();
			await writer.FlushAsync();

			foreach (var itemSet in root.EnumerateObject())
			{
				if (!itemSet.NameEquals(SetName))
				{
					await WriteSetAsync(itemSet.Name, itemSet.Value.GetRawText());
				}

				else
				{
					didFindSet = true;
					await WriteSetAsync(SetName, listJson);
				}
			}

			if (!didFindSet)
			{
				await WriteSetAsync(SetName, listJson);
			}

			writer.WriteEndObject();
			await writer.FlushAsync();
			var json = Encoding.UTF8.GetString(stream.ToArray());
			await File.WriteAllTextAsync("store.json", json);
		}
	}
}
