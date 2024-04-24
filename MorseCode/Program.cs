using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

class MorseCodeTranslator
{
    Dictionary<String, String> translationSet = new Dictionary<String, String>();

    public MorseCodeTranslator()
    {
        string paths = "";
        Console.WriteLine("Choose the standard: ");
        Console.WriteLine("1. International.");
        Console.WriteLine("2. American.");
        Console.WriteLine("Input: ");
        int userChoice = Int32.Parse(Console.ReadLine());

        switch (userChoice)
        {
            case 1:
                paths = Path.Combine("TranslationSets", "international.txt");
                break;

            default:
                paths = Path.Combine("TranslationSets", "american.txt");
                break;
        }

        LoadMorseCode(paths);
    }

    private void LoadMorseCode(string path)
    {
        try
        {
            foreach (String line in File.ReadLines(path))
            {
                if (String.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }
                else
                {
                    string[] characters = line.Split(" ");
                    translationSet.Add(characters[0], characters[1]);
                }
            }
        }
        catch
        {
            Console.WriteLine("Importing the Translation set failed. Check if you have the files in the TranslationSets folder/directory.");
        }
    }

    public string TranslateToMorse(string text)
    {
        text = text.ToUpper();
        char[] characters = new char[text.Length];
        string morse = "";
        for (int i = 0; i < text.Length; i++)
        {
            characters[i] = text[i];
        }

        foreach (char c in characters)
        {
            if (translationSet.ContainsKey(c.ToString()))
            {
                morse += translationSet[c.ToString()] + " ";
            }
            else
            {
                morse += "|";
            }
        }

        return morse;
    }

    public string TranslateToText(string morse)
    {
        string[] words = morse.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        string text = "";

        foreach (string word in words)
        {
            string[] letters = word.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string letter in letters)
            {
                if (translationSet.ContainsValue(letter))
                {
                    text += translationSet.FirstOrDefault(x => x.Value == letter).Key;
                }
                else
                {
                    text += "?";
                }
            }
            text += " ";
        }

        return text;
    }
}


internal class Program
{

    static private void writeToFile(string input, string translated)
    {
        string currentDateTime = DateTime.Now.ToString();
        currentDateTime = currentDateTime.Replace('/', '-');
        currentDateTime = currentDateTime.Replace(' ', '_');

        using (StreamWriter outputFile = new StreamWriter(Path.Combine("logs", $"{currentDateTime}.txt")))
        {
            outputFile.WriteLine("Text: " + input + "\n Morse: " + translated);
        }
    }

    static private void closingOperation(string input, string translated)
    {
        Console.WriteLine(translated);
        Console.WriteLine("Press Enter To clear the screen once finished...");
        input = Console.ReadLine();
        Console.Clear();
    }

    static public void Main(String[] args)
    {
        bool keepAlive = true;
        string userInput = "", translatedText = "";
        while (keepAlive)
        {
            Console.WriteLine("=== Morse Code Translator - Initialising ===");
            MorseCodeTranslator translator = new MorseCodeTranslator();
            Console.Clear();
            Console.WriteLine("=== Morse Code Translator - Main Menu ===");
            Console.WriteLine("Choose from the following: ");
            Console.WriteLine("1. Encoding to morse code.");
            Console.WriteLine("2. Decoding from morse code.");
            Console.WriteLine("3. Quit.");
            Console.WriteLine("Input: ");
            int userChoice = Int32.Parse(Console.ReadLine());
            switch (userChoice)
            {
                case 1:
                    Console.WriteLine("Input the line: ");

                    userInput = Console.ReadLine();
                    translatedText = translator.TranslateToMorse(userInput);

                    writeToFile(userInput, translatedText);
                    closingOperation(userInput, translatedText);
                    break;

                case 2:
                    Console.WriteLine("Input the line: ");

                    userInput = Console.ReadLine();
                    translatedText = translator.TranslateToText(userInput);

                    writeToFile(userInput, translatedText);
                    closingOperation(userInput, translatedText);
                    break;

                default:
                    keepAlive = false;
                    break;
            }
        }

    }
}