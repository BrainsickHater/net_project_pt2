using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{

    /*** The object holding the response and a flag that states whether the
     *   result is an anagram or not. This allows a class to be set that colors the
     *   text accordingly.
     ***/

    public class ResponseHolder
    {

        private string response;
        private int countmissing;
        private string flag;
        private string string1;
        private string string2;

        public ResponseHolder(string response, string flag, int countmissing, string String1, string String2)
        {
            this.response = response;
            this.countmissing = countmissing;
            this.flag = flag;
            this.string1 = String1;
            this.string2 = String2;
        }

        public string Response
        {
            get
            {
                return response;
            }
        }

        public int CountMissing
        {
            get
            {
                return countmissing;
            }
        }

        public string Flag
        {
            get
            {
                return flag;
            }
        }

        public string String1
        {
            get
            {
                return string1;
            }
        }

        public string String2
        {
            get
            {
                return string2;
            }
        }
    }
}