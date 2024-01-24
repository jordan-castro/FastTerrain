using Godot;
using System;

namespace FastTerrain;

public class Utils {
	/// <summary>
	/// Convert a string to a Vector2I.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Vector2I StringToVector2I(string value) {
		// Remove anythin from the string that is not a number or a comma
		string newValue = "";
		foreach (char c in value) {
			if (c == ',' || c == '-' || char.IsDigit(c)) {
				newValue += c;
			}
		}
		// Split the string into a string array
		string[] values = newValue.Split(',');
		return new Vector2I(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
	}
}