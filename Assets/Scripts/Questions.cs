using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Question
{
    public string questionText;

    public string answer0;
    public string answer1;
    public string answer2;

    public int[] answer0Result;
    public int[] answer1Result;
    public int[] answer2Result;

    public override string ToString()
    {
        return string.Format("{0}, {1}, {2}, {3}, [{4}], [{5}], [{6}]",
                             questionText,
                             answer0,
                             answer1,
                             answer2,
                             string.Join(", ", answer0Result),
                             string.Join(", ", answer1Result),
                             string.Join(", ", answer2Result));
    }
}

public static class QuestionHelper
{
    public const int NUMBER_OF_SPRITS = 5;

    public static List<Question> GetQuestionsFromCSV(TextAsset questionsCSV)
    {
        List<Question> result = new();

        const string separator = ",";
        StringReader reader = new(questionsCSV.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            string[] items = line.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Question q = GetQuestionFromCSVLine(items);
            result.Add(q);
        }

        return result;
    }

    public static Question GetQuestionFromCSVLine(string[] csvLine)
    {
        Question result = new();

        result.questionText = csvLine[0];

        result.answer0 = csvLine[1];
        result.answer1 = csvLine[2];
        result.answer2 = csvLine[3];

        int resultStart = 4;
        result.answer0Result = GetResultFromCSVLine(csvLine.SubArray(resultStart, NUMBER_OF_SPRITS));
        result.answer1Result = GetResultFromCSVLine(csvLine.SubArray(resultStart + NUMBER_OF_SPRITS, NUMBER_OF_SPRITS));
        result.answer2Result = GetResultFromCSVLine(csvLine.SubArray(resultStart + 2 * NUMBER_OF_SPRITS, NUMBER_OF_SPRITS));

        return result;
    }

    public static int[] GetResultFromCSVLine(string[] csvLine)
    {
        int[] result = new int[NUMBER_OF_SPRITS];

        for (int i = 0; i < NUMBER_OF_SPRITS; i++)
        {
            if (!int.TryParse(csvLine[i], out int value))
                Debug.LogError("Parcing Error");

            result[i] = value;
        }

        return result;
    }

    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
}
