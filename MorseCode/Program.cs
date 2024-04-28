using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO.Compression;
using System.Buffers;
using System.Security.Cryptography;

public static class AES
{
    private const int KeyBytes = 32; // 256-bit key
    private const int SaltSize = 16; // 128-bit salt
    private const int IvSize = 16; // 128-bit IV

    // Generate a cryptographically secure random salt value.
    private static byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Create().GetBytes(bytes);
        return bytes;
    }

    // Derive the key from the password and salt.
    private static byte[] GenerateKey(string password, byte[] salt, int iterations = 10000)
    {
        using var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return keyGenerator.GetBytes(KeyBytes);
    }

    /// <summary>
    /// Encrypts a byte array using AES encryption with a specified password.
    /// The method generates a random salt and initialisation vector (IV),
    /// derives a key from the password, and performs the encryption.
    /// The salt, IV, and encrypted data are concatenated and returned.
    /// </summary>
    /// <param name="password">The password used for generating the encryption key.</param>
    /// <param name="inputBytes">The data to encrypt.</param>
    /// <returns>A byte array containing the salt, IV, and encrypted data.</returns>
    public static byte[] EncryptBytes(string password, byte[] inputBytes)
    {
        // Generate random salt and IV for each encryption to enhance security.
        var salt = GenerateRandomBytes(SaltSize);
        var iv = GenerateRandomBytes(IvSize);
        var key = GenerateKey(password, salt);

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;

        // Set the IV for the AES algorithm. IVs are used to prevent patterns in encrypted data.
        aesAlg.IV = iv;

        // Create an encryptor object from the AES instance.
        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // MemoryStream for holding the encrypted bytes.
        using var msEncrypt = new MemoryStream();

        // CryptoStream for performing the encryption.
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            // Write all the data to be encrypted to the stream.
            csEncrypt.Write(inputBytes, 0, inputBytes.Length);
        }

        // Extract the encrypted bytes from the MemoryStream.
        var encryptedData = msEncrypt.ToArray();

        // Prepare the final byte array which includes salt, IV, and encrypted data.
        var result = new byte[SaltSize + IvSize + encryptedData.Length];

        // Copy salt, IV, and encrypted data into the result byte array.
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
        Buffer.BlockCopy(encryptedData, 0, result, SaltSize + IvSize, encryptedData.Length);

        return result;
    }

    /// <summary>
    /// Decrypts a byte array that was encrypted using AES encryption.
    /// The method extracts the salt and initialisation vector (IV) from the input,
    /// derives the encryption key using the provided password, and performs the decryption.
    /// </summary>
    /// <param name="password">The password used for generating the decryption key.</param>
    /// <param name="inputBufferWithSaltAndIv">The byte array containing the salt, IV, and encrypted data.</param>
    /// <returns>The decrypted data as a byte array.</returns>
    public static byte[] DecryptBytes(string password, byte[] inputBufferWithSaltAndIv)
    {
        // Extract salt and IV from the beginning of the input buffer.
        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        Buffer.BlockCopy(inputBufferWithSaltAndIv, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(inputBufferWithSaltAndIv, SaltSize, iv, 0, IvSize);

        // The rest of the input buffer is the encrypted data.
        var encryptedData = new byte[inputBufferWithSaltAndIv.Length - SaltSize - IvSize];
        Buffer.BlockCopy(inputBufferWithSaltAndIv, SaltSize + IvSize, encryptedData, 0, encryptedData.Length);

        // Derive the key from the password and salt.
        using var aesAlg = Aes.Create();
        aesAlg.Key = GenerateKey(password, salt);
        aesAlg.IV = iv;

        // Create a decryptor object from the AES instance.
        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // MemoryStream for reading the encrypted data.
        using var msDecrypt = new MemoryStream(encryptedData);

        // CryptoStream for performing the decryption.
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

        // MemoryStream for holding the decrypted bytes.
        using var resultStream = new MemoryStream();

        // Read decrypted bytes into the result stream.
        csDecrypt.CopyTo(resultStream);

        return resultStream.ToArray();
    }
}


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

class GZIP
{
    public static byte[] Compress(byte[] buffer)
    {
        using var memStream = new MemoryStream();

        using (var gZipStream = new GZipStream(memStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        return memStream.ToArray();
    }

    public static byte[] Decompress(byte[] compressedData)
    {
        using var memStream = new MemoryStream(compressedData);

        using var gZipStream = new GZipStream(memStream, CompressionMode.Decompress);

        using var resultStream = new MemoryStream();

        gZipStream.CopyTo(resultStream);

        return resultStream.ToArray();
    }
}

internal class Program
{
    static private bool authenticate()
    {
        List<string> inputValues = new List<string>();

        string storedName = "user";
        string storedPassword = "password";
        string input = "";

        Console.WriteLine("Username: ");
        input = Console.ReadLine();
        inputValues.Add(input);

        Console.WriteLine("Password: ");
        input = Console.ReadLine();
        inputValues.Add(input);

        if (inputValues[0] == storedName && inputValues[1] == storedPassword)
        {
            Console.WriteLine("User authenticated!");
            return false;
        }
        else if (inputValues[0] != storedName || inputValues[1] != storedPassword)
        {
            Console.WriteLine("Incorrect password or username.");
            return true;
        }
        return true;
    }

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

    static private string validateInput(string input)
    {
        while (input == "")
        {
            Console.WriteLine("Input the line: ");
            input = Console.ReadLine();
        }
        return input;
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
        bool authLoop = true;
        string userInput = "", translatedText = "", key = "";
        while (keepAlive)
        {
            Console.WriteLine("=== Morse Code Translator - Authentication ===");
            while (authenticate()) { }
            Console.WriteLine("=== Morse Code Translator - Initialising ===");
            MorseCodeTranslator translator = new MorseCodeTranslator();
            Console.Clear();
            Console.WriteLine("=== Morse Code Translator - Main Menu ===");
            Console.WriteLine("Choose from the following: ");
            Console.WriteLine("1. Plaintext - Encoding to morse code.");
            Console.WriteLine("2. Plaintext - Decoding from morse code.");
            Console.WriteLine("3. Ecrypted (AES) - Encoding to morse code.");
            Console.WriteLine("4. Ecrypted (AES) - Decoding from morse code.");
            Console.WriteLine("3. Quit.");
            Console.WriteLine("Input: ");
            int userChoice = Int32.Parse(Console.ReadLine());
            switch (userChoice)
            {
                case 1:
                    validateInput(userInput);

                    translatedText = translator.TranslateToMorse(userInput);

                    writeToFile(userInput, translatedText);
                    closingOperation(userInput, translatedText);
                    break;

                case 2:
                    validateInput(userInput);

                    translatedText = translator.TranslateToText(userInput);

                    writeToFile(userInput, translatedText);
                    closingOperation(userInput, translatedText);
                    break;

                case 3:
                    while (userInput == "")
                    {
                        Console.WriteLine("Input the line: ");
                        userInput = Console.ReadLine();
                    }

                    while (key == "")
                    {
                        Console.WriteLine("Input the key: ");
                        key = Console.ReadLine();
                    }

                    translatedText = translator.TranslateToMorse(userInput);
                    byte[] rawInputBytes = Encoding.UTF8.GetBytes(translatedText);
                    byte[] compressedData = GZIP.Compress(rawInputBytes);
                    byte[] encryptedData = AES.EncryptBytes(key, compressedData);
                    writeToFile("ENCRYPTED", Encoding.UTF32.GetString(encryptedData, 0, encryptedData.Length));
                    closingOperation(userInput, translatedText);
                    userInput = "";
                    key = "";

                    break;

                case 4:
                    while (userInput == "")
                    {
                        Console.WriteLine("Input the line: ");
                        userInput = Console.ReadLine();
                    }

                    while (key == "")
                    {
                        Console.WriteLine("Input the key: ");
                        key = Console.ReadLine();
                    }

                    rawInputBytes = Encoding.UTF8.GetBytes(userInput);
                    byte[] decryprtedData = AES.DecryptBytes(key, rawInputBytes);

                    Console.WriteLine(Encoding.UTF8.GetString(decryprtedData));
                    translatedText = translator.TranslateToText(Encoding.UTF8.GetString(decryprtedData));
                    closingOperation(userInput, translatedText);
                    userInput = "";
                    key = "";
                    break;

                default:
                    keepAlive = false;
                    break;
            }
        }
    }
}