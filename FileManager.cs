using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FileParser;

public class FileManager<T> where T : new()
{
	private readonly string _filePath;
	private List<T> _items;

	public FileManager(string filePath)
	{
		_filePath = filePath;
		_items = [];
		Load();
	}

	// Load from file into memory
	public void Load()
	{
		_items = [];

		if (!File.Exists(_filePath))
			return; // Silently skip if file doesn't exist (or you can throw/log)

		T currentObject = default;
		var lines = File.ReadAllLines(_filePath);

		foreach (var rawLine in lines)
		{
			var line = rawLine.Trim();

			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
				continue;

			if (line.StartsWith("[") && line.EndsWith("]"))
			{
				currentObject = new T();
				_items.Add(currentObject);
				continue;
			}

			var parts = line.Split(':', 2);
			if (parts.Length != 2 || currentObject == null)
				continue;

			var key = parts[0].Trim();
			var value = parts[1].Trim();

			SetProperty(currentObject, key, value);
		}
	}

	// Save from memory back to file
	public void Save()
	{
		var sb = new StringBuilder();

		foreach (var obj in _items)
		{
			sb.AppendLine("[user]"); // Change section name if needed

			foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var val = prop.GetValue(obj);

				if (val == null)
					continue;

				if (val is System.Collections.IEnumerable enumerable && !(val is string))
				{
					var items = new List<string>();
					foreach (var item in enumerable)
					{
						items.Add(item?.ToString());
					}
					sb.AppendLine($"{prop.Name.ToLower()}: [{string.Join(", ", items)}]");
				}
				else
				{
					sb.AppendLine($"{prop.Name.ToLower()}: {val}");
				}
			}

			sb.AppendLine();
		}

		File.WriteAllText(_filePath, sb.ToString());
	}

	// Get all items
	public List<T> GetAll() => _items;

	// Add a new item
	public void Create(T newItem)
	{
		_items.Add(newItem);
	}

	// Update item using callback
	public bool Update(string keyPropertyName, object keyValue, Action<T> updateAction)
	{
		var propInfo = typeof(T).GetProperty(keyPropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
		
		if (propInfo == null)
			throw new ArgumentException($"Property '{keyPropertyName}' not found.");

		var item = _items.FirstOrDefault(x =>
		{
			var val = propInfo.GetValue(x);
			return val != null && val.Equals(keyValue);
		});

		if (item == null)
			return false;

		updateAction(item);
		return true;
	}

	// Delete item by key
	public bool Delete(string keyPropertyName, object keyValue)
	{
		var propInfo = typeof(T).GetProperty(keyPropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
		
		if (propInfo == null)
			throw new ArgumentException($"Property '{keyPropertyName}' not found.");

		var item = _items.FirstOrDefault(x =>
		{
			var val = propInfo.GetValue(x);
			return val != null && val.Equals(keyValue);
		});

		if (item == null)
			return false;

		return _items.Remove(item);
	}

	// Set property using reflection + parsing
	private void SetProperty(T obj, string key, string value)
	{
		var type = typeof(T);
		var prop = type.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

		if (prop == null || !prop.CanWrite)
			return;

		try
		{
			var propType = prop.PropertyType;

			if (propType == typeof(string))
			{
				prop.SetValue(obj, value);
			}
			else if (propType == typeof(int))
			{
				if (int.TryParse(value, out int intVal))
					prop.SetValue(obj, intVal);
			}
			else if (propType.IsEnum)
			{
				var enumVal = Enum.Parse(propType, value, ignoreCase: true);
				prop.SetValue(obj, enumVal);
			}
			else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
			{
				if (value.StartsWith("[") && value.EndsWith("]"))
				{
					var inner = value.Substring(1, value.Length - 2);
					var elements = inner.Split(',').Select(s => s.Trim());

					var elementType = propType.IsGenericType
						? propType.GetGenericArguments()[0]
						: typeof(string); // fallback

					var listType = typeof(List<>).MakeGenericType(elementType);
					var list = (System.Collections.IList)Activator.CreateInstance(listType);

					foreach (var str in elements)
					{
						object converted;

						if (elementType.IsEnum)
							converted = Enum.Parse(elementType, str, ignoreCase: true);
						else
							converted = Convert.ChangeType(str, elementType);

						list.Add(converted);
					}

					prop.SetValue(obj, list);
				}
			}
		}
		catch
		{
			// Optional: log or throw for debugging
		}
	}
}
