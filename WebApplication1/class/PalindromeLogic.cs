using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApplication1
{
    public class PalindromeLogic
    {
        public PalindromeLogic() { }

        public int isPalindrome(string input)
        {
            // Check string length
            if (input.Length < 1)
            {
                throw new System.ArgumentException("Empty string input");
            }

            if (input.Length > 40)
            {
                throw new System.ArgumentException("Input is too long");
            }

            // Clean input
            string test = Regex.Replace(input.ToLower(), @"[^\w]|[_]", ""); ;

            // Check if string is a palindrome
            char[] charArray = test.ToCharArray();
            char[] reverseArray = new char[charArray.Length];
            charArray.CopyTo(reverseArray, 0);
            Array.Reverse(reverseArray);

            int diff = 0;
            for (int i = 0; i < charArray.Length / 2; i++)
            {
                if (charArray[i] != charArray[charArray.Length - 1 - i])
                {
                    diff++;
                }
            }

            return diff;
        }

        public string[] buildResult(int diff, string input)
        {
            string[] result = new string[4];

            if (diff == 0)
            {
                result[0] = "\"" + input + "\" is a palindrome ";
                result[1] = "isPalindrome";
            }
            else
            {
                result[0] = "\"" + input + "\" is not a palindrome ";
                result[1] = "notPalindrome";
            }
            result[2] = "fadeIn";
            result[3] = input;

            return result;
        }

        public string createErrorMessage(int diff, string input)
        {
            if (diff == 0) { return string.Empty; }
            else { return "*** Change " + diff + " letter(s) in " + input + " to create a palindrome ***"; }
        }

        public String[] findDeleted(ArrayList values)
        {
            foreach (String[] array in values)
            {
                if (array[2] == "deleted")
                {
                    return array;
                }
            }
            // Return dummy if no match is found
            return new String[0];
        }
    }
}