using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiOhScraper.Entities
{
    public class PageDoesNotExistException : Exception
    {

        public PageDoesNotExistException()
            : base("Page does not exist!") { }

        public PageDoesNotExistException(string message) : base(message)
        {
        }

        public PageDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
