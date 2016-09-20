using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WebApplication1
{

    /*** Class holding all business Logic ***/

    public class Logic
    {

        /*** Helper function used to perform the actual business logic for the Anagram Comparator.
         *   Algorithm attempts to match each of the second string's chars to one in the first string,
         *   removing that char from the first string on a match and continuing, and returning an 
         *   object with a false response on any unmatched char. If each char is matched successfully
         *   and the first string is left with no characters, we have two strings that are anagrams of
         *   each other, and the object returned holds a success response to be sent to the front end.
         ***/

        public ResponseHolder isAnagram(string str1, string str2)
        {
            ResponseHolder rh;
            string response;
            string flag = "notAnagram";

            int countMissing = 0;

            string other;
            string othercopy;
            string longer;

            if (str1.Length >= str2.Length)
            {
                longer = str1.ToLower();
                other = str2.ToLower();
                othercopy = other;
            }
            else
            {
                longer = str2;
                other = str1;
                othercopy = other;
            }

            foreach (char ch in longer)
            {
                int x = other.IndexOf(ch);
                if (x >= 0)
                {
                    other = other.Remove(x, 1);
                }
                else
                {
                    countMissing++;
                }
            }

            if (countMissing > 0)
            {
                response = "\"" + str1 + "\" is not an anagram of \"" + str2 + "\"";
            }
            else
            {
                response = "\"" + str1 + "\" is an anagram of \"" + str2 + "\"";
                flag = "isAnagram";

            }
            rh = new ResponseHolder(response, flag, countMissing, str1, str2);
            return rh;
        }

        public String checkLast(ArrayList values, String flag, int index)
        {
            if ((index == values.Count - 1 && flag.Equals("add")) || (values.Count == 1 && flag.Equals("add")))
            {
                return "hidden";
            }
            else
            {
                return "nothidden";
            }
        }

    }
}