using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

class MorseCodeTranslator{
    Dictionary<String, String> translationSet = new Dictionary<String, String>();
    
    public MorseCodeTranslator(string path)
    {
        LoadMorseCode(path);
    }

    private void LoadMorseCode(string path){
        try{
            foreach(String line in File.ReadLines(path)){
                if(String.IsNullOrEmpty(line) || line.StartsWith("#")){
                    continue;
                }
                else{
                    string[] characters = line.Split(" ");
                    translationSet.Add(characters[0], characters[1]);
                }
            }
        }
        catch{
            Console.WriteLine("Importing the Translation set failed. Check if you have the files in the TranslationSets folder/directory.");
        }
    }

    public string TranslateToMorse(string text){
        text = text.ToUpper();
        char[] characters = new char[text.Length];
        string morse = "";
        for(int i = 0; i < text.Length; i++){
             characters[i] = text[i];
        }

        foreach(char c in characters){
            if(translationSet.ContainsKey(c.ToString())){
                    morse += translationSet[c.ToString()] + " ";
            }else{
                    morse +="|";
            }
        }

        return morse;
    }

    //public string TranslateToText(string text){

    //}
}


internal class Program { 

    static public void Main(String[] args) 
    { 
        bool keepAlive = true;
        string path = "";
        while(keepAlive){
            Console.WriteLine("=== Morse Code Translator ===");
            Console.WriteLine("Main Menu");
            Console.WriteLine("Choose from the following: ");
            Console.WriteLine("1. Encoding to morse code.");
            Console.WriteLine("2. Decoding from morse code.");
            Console.WriteLine("Input: ");
            int userChoice = Int32.Parse(Console.ReadLine());
            switch(userChoice){
                case 1:
                    Console.WriteLine("Choose the standard: ");
                    Console.WriteLine("1. International");
                    Console.WriteLine("2. American.");
                    Console.WriteLine("Input: ");
                    userChoice = Int32.Parse(Console.ReadLine());

                    if(userChoice == 1){
                        path = @"TranslationSets/international.txt";
                    }else{
                        path = @"TranslationSets/american.txt";
                    }
                    MorseCodeTranslator translator = new MorseCodeTranslator(path);
                    Console.WriteLine("Input the line: ");
                    string userInput = Console.ReadLine();
                    string translatedText = translator.TranslateToMorse(userInput);
                    Console.WriteLine(translatedText);

                    using (StreamWriter outputFile = new StreamWriter("logs.txt"))
                    {
                        outputFile.WriteLine("Text: " + userInput + "\n Morse: " + translatedText);
                    }
                    break;
            }
        }

    }
}