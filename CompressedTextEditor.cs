using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Collections.Generic;

public class CompressedTextEditor : EditorWindow
{
    private List<Object> filesToCompress = new List<Object>();
    private Vector2 scrollPos;
    private int characterLimit = 1000;

    [MenuItem("Window/Compressed Text Editor")]
    public static void ShowWindow()
    {
        GetWindow<CompressedTextEditor>("Compressed Text Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Drag and drop .cs files here");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < filesToCompress.Count; i++)
        {
            filesToCompress[i] = EditorGUILayout.ObjectField(filesToCompress[i], typeof(Object), false);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add File"))
        {
            filesToCompress.Add(null);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Compress, Encode, and Save to File"))
        {
            CompressEncodeAndSave(filesToCompress);
        }
    }

    private void CompressEncodeAndSave(List<Object> files)
    {
        StringBuilder resultBuilder = new StringBuilder();

        foreach (Object file in files)
        {
            if (file != null && file.GetType() == typeof(TextAsset))
            {
                string fileContent = (file as TextAsset).text;
                string encodedContent = CompressAndEncode(fileContent);
                resultBuilder.AppendLine(SplitEncodedText(encodedContent));
            }
        }

        string outputPath = Path.Combine(Application.dataPath, "CompressedTextOutput.txt");
        File.WriteAllText(outputPath, resultBuilder.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Compressed and encoded text saved to: {outputPath}");
    }

    private string CompressAndEncode(string text)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(text);

        using (var outputStream = new MemoryStream())
        {
            using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
            {
                brotliStream.Write(inputBytes, 0, inputBytes.Length);
            }

            byte[] compressedBytes = outputStream.ToArray();
            return System.Convert.ToBase64String(compressedBytes);
        }
    }

    private string SplitEncodedText(string encodedText)
    {
        StringBuilder resultBuilder = new StringBuilder();

        for (int i = 0; i < encodedText.Length; i += characterLimit)
        {
            string segment = encodedText.Substring(i, Mathf.Min(characterLimit, encodedText.Length - i));
            resultBuilder.AppendLine(segment);
        }

        return resultBuilder.ToString();
    }
}
