using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public class TextUtils {
	// Properties
	private static string[] LINE_BREAKS_STRINGS = new string[] { "\r\n", "\n" }; // Mac and PC read line breaks differently. :p
	private static CultureInfo parserCulture = CultureInfo.CreateSpecificCulture ("en"); // We ONLY want to parse (number) strings with English culture!

	/** Use THIS function instead of float.Parse!! Because... on PlayStation 4, if the system's language is French, it treats periods as commas. We want ONLY to use English-style punctuation throughout all our backend. */
	public static float ParseFloat (string _string) { return float.Parse (_string, parserCulture); }
	public static int ParseInt (string _string) { return int.Parse (_string, parserCulture); }


	static public string ToTimeString_msm (float timeFloat, string displayStringIf0) {
		if (timeFloat == 0) { return displayStringIf0; }
		return ToTimeString_msm (timeFloat);
	}
	static public string ToTimeString_msm (float timeFloat) {
//		return GameMathUtils.RoundTo2DPs (timeFloat).ToString ();// for dev purposes. So I only have one unit to look at/work with for now: pure seconds.
		int timeInt = (int) timeFloat;
		int minutes = timeInt / 60;
		int seconds = timeInt % 60;
		int millis = (int) ((timeFloat*100) % 100);
		return string.Format ("{0}:{1:00}.{2:00}", minutes, seconds, millis);
//		return string.Format ("{0}:{1:00}:{00}", (int)timeFloat / 60, (int)timeFloat % 60, timeFloat%1);
	}
	static public string ToTimeString_ms (float timeFloat, bool alwaysShowMinutes=true) {
		int timeInt = (int) timeFloat;
		if (!alwaysShowMinutes && timeInt < 60) { // Less than a minute?? ONLY show straight-up seconds!
			return timeInt.ToString ();
		}
		int minutes = timeInt / 60;
		int seconds = timeInt % 60;
		return string.Format ("{0}:{1:00}", minutes, seconds);
	}
	static public string DecimalPlaces (float timeFloat) {
		int millis = (int) ((timeFloat*100) % 100);
		return string.Format ("{0:00}", millis);
	}

	static public string RemoveWhitespace (string _string) {
//		return Regex.Replace(_string, @"\s+", "");public static string RemoveWhitespace(this string str) {
		return string.Join("", _string.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
	}

	static public string[] GetStringArrayFromTextAsset (TextAsset textAsset) {
		if (textAsset == null) { return null; } // If this file doesn't even exist, then return null.
		return GetStringArrayFromStringWithLineBreaks (textAsset.text);
	}
	static public string[] GetStringArrayFromStringWithLineBreaks (string wholeString) {
//		string[] lines = File.ReadAllLines(worldDataRef.GetWorldFileLocation() + Key + ".txt");
//		return lines;
		return wholeString.Split (LINE_BREAKS_STRINGS, StringSplitOptions.None);
	}
	static public Rect GetRectFromString (string str) {
		// This function parses a string AS FORMATTED by Rect's ToString() function. Example: (x:0.68, y:76.18, width:400.00, height:400.00)
		int colonIndex, commaIndex;
		string xString, yString, wString, hString;

		colonIndex = str.IndexOf (':');
		commaIndex = str.IndexOf (',');
		xString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
		colonIndex = str.IndexOf (':', colonIndex+1);
		commaIndex = str.IndexOf (',', commaIndex+1);
		yString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
		colonIndex = str.IndexOf (':', colonIndex+1);
		commaIndex = str.IndexOf (',', commaIndex+1);
		wString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
		colonIndex = str.IndexOf (':', colonIndex+1);
		commaIndex = str.Length - 1;
		hString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));

		Rect returnRect = new Rect (TextUtils.ParseFloat (xString),TextUtils.ParseFloat (yString), TextUtils.ParseFloat (wString),TextUtils.ParseFloat (hString));
		return returnRect;
	}
	static public Vector2 GetVector2FromString (string str) {
		// This function parses a string AS FORMATTED by Vector2's ToString() function.
		int indexOfComma = str.IndexOf (',');
		string xString = str.Substring (1, (-1) + (indexOfComma));
		string yString = str.Substring (indexOfComma+1, -(indexOfComma+1) + (str.Length-1));
//		try { // test
		float x = ParseFloat (xString);
		float y = ParseFloat (yString);
		return new Vector2 (x,y);
//		}
//		catch {
//			Debug.Log ("Error parsing Vector2 string. x: " + xString + ", y: " + yString);
//			return new Vector2 (0,0);
//		}
	}
	static public float[] GetFloatArrayFromString (string _string, char separator=',') {
		string[] stringArray = _string.Split (separator);
		float[] floatArray = new float[stringArray.Length];
		for (int i=0; i<floatArray.Length; i++) {
			floatArray [i] = TextUtils.ParseFloat (stringArray[i]);
		}
		return floatArray;
	}


}
