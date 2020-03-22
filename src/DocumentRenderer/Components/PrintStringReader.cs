using System;

namespace PrintRenderer
{
    /// <summary>
    /// Stream-like class that performs reads in line-based chunks.
    /// </summary>
    public class PrintStringReader
    {
        /// <summary>
        /// The internal text buffer.
        /// </summary>
        private string _string;

        /// <summary>
        /// Current index pointer. 
        /// </summary>
        private int _pos;

        /// <summary>
        /// Initialize a PrintStringReader with the given text. 
        /// </summary>
        /// <param name="text">Text to read from.</param>
        public PrintStringReader(string text)
        {
            Set(text);
        }

        /// <summary>
        /// Initialize an empty PrintStringReader.
        /// </summary>
        public PrintStringReader()
        {
            Set("");
        }

        /// <summary>
        /// Set the internal buffer to the given text string. 
        /// </summary>
        /// <param name="text">Text to read from.</param>
        public void Set(string text)
        {
            _pos = 0;
            _string = string.IsNullOrEmpty(text) ? "" : text;
        }

        /// <summary>
        /// Gets the complete internal string owned by this reader.
        /// </summary>
        /// <returns>Text string.</returns>
        public string Get()
        {
            return _string;
        }

        /// <summary>
        /// Returns whether there is any text left to read. 
        /// </summary>
        public bool EOF => _string.Length <= _pos;

        /// <summary>
        /// Read as much text as possible, until EOF or a line break. 
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            return Read(_string.Length - _pos);
        }

        /// <summary>
        /// Consume all leading whitespace starting at the current
        /// string position. 
        /// </summary>
        private void _ConsumeSpace()
        {
            while (!EOF && (_string[_pos] == ' '))
            {
                ++_pos;
            }
        }

        /// <summary>
        /// Helper function to read a substring from the internal
        /// buffer.
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="end">End index (not inclusive)</param>
        /// <returns></returns>
        private string _ReadFrom(int start, int end)
        {
            return _string.Substring(start, end - start);
        }

        /// <summary>
        /// Read up to `count` characters from the stream. Because this reader
        /// is specialized for printing to a Graphics object, perform the following
        /// special behavior:
        ///   * return  empty line if line starts with cr,lf,crlf
        ///   * strip leading spaces
        ///   * return only as much text as would avoid splitting words across lines
        ///   * return the whole line if no spaces are found, always
        /// </summary>
        /// <param name="count">maximum line length</param>
        /// <returns></returns>
        public string Read(int count)
        {
            _ConsumeSpace();

            int line_end = Math.Min(_string.Length, _pos + count);
            int start = _pos;
            int end = line_end;
            int p;

            for (p = _pos; p < line_end; ++p)
            {
                switch (_string[p])
                {
                    case '\n':
                        _pos = p + 1;
                        return _ReadFrom(start, p);

                    case '\r':
                        if (p + 1 < _string.Length && _string[p + 1] == '\n')
                        {
                            _pos = p + 2;
                        }
                        else
                        {
                            _pos = p + 1;
                        }
                        return _ReadFrom(start, p);

                    case ' ':
                        // adjust the end pointer to the new last complete word
                        end = p;
                        break;
                }
            }
            // if we hit EOF before running out of room,
            // return the whole string
            if (line_end == _string.Length)
                end = p;
            _pos = end;
            return _ReadFrom(start, end);
        }
    }
}
