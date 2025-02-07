using UnityEngine;
using UnityEditor;
using System.IO;
public class HandleTextFile
{
    [MenuItem("Tools/Write file")]
    public static void WriteString(string input)
    {
        string path = "Assets/Resources/test.txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(input);
        writer.Close();
    }
    [MenuItem("Tools/Read file")]
    public static void ReadString()
    {
        string path = "Assets/Resources/test.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}