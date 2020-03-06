using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SymbolTable
{
    class Symbol
    {
        public string symbolName;
        public string type;

        public Symbol(string sn, string t)
        {
            this.symbolName = sn;
            this.type = t;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Symbol> symbols = new List<Symbol>();

            List<string> keywords = new List<string>();

            keywords.Add("int");
            keywords.Add("double");
            keywords.Add("string");
            keywords.Add("float");
            keywords.Add("bool");
            keywords.Add("void");

            List<string> variables = new List<string>();

            string[] lines = File.ReadAllLines(@"data.txt");

            // Normalizing lines for uniform input in form of spaces
             for (int i = 0; i < lines.Length; i++)
             {
                 string tempLine = lines[i];
                 for (int j = 0; j < tempLine.Length; j++)
                 {
                     if (tempLine[j] == '/' && tempLine[j+1] == '/')
                    {
                        tempLine = tempLine.Insert(startIndex: j + 2, value: " ");
                    } 
                     else if (tempLine[j] == '(')
                     {
                         if (tempLine[j - 1] != ' ')
                         {
                             tempLine = tempLine.Insert(startIndex: j, value: " ");
                         }

                         else if (tempLine[j + 1] != ' ')
                         {
                             tempLine = tempLine.Insert(startIndex: j + 1, value: " ");
                         }
                     }

                     else if (tempLine[j] == ')')
                     {
                         if (tempLine[j - 1] != ' ')
                         {
                             tempLine = tempLine.Insert(startIndex: j, value: " ");
                             tempLine = tempLine.Insert(startIndex: j + 2, value: " ");
                         }
                     }

                    else if (tempLine[j] == ';')
                    {
                        if (tempLine[j - 1] != ' ')
                        {
                            tempLine = tempLine.Insert(startIndex: j, value: " ");
                        }
                    }
                    else if (tempLine[j] == '=')
                    {
                        if (tempLine[j - 1] != ' ')
                        {
                            if (tempLine[j - 1] != '+' && tempLine[j - 1] != '-' && tempLine[j - 1] != '<' && tempLine[j - 1] != '>') // Preserving <=, +=, -=, >=
                                tempLine = tempLine.Insert(startIndex: j, value: " ");
                        }

                        else if (tempLine[j + 1] != ' ')
                        {
                            tempLine = tempLine.Insert(startIndex: j + 1, value: " ");
                        }
                    }
                }

                 lines[i] = tempLine;
             }

             // Checking input for varables and functions
             foreach (string line in lines)
             {
                 string[] words = line.Split(' ');

                 for (int i = 0; i < words.Length - 1; i++)
                 {
                     if (keywords.Contains(words[i]))
                     {
                        //Checking for function

                        if (words[i + 2].StartsWith("("))
                         {
                             symbols.Add(new Symbol(words[i + 1], "Function," + words[i]));
                         }
                         else
                         {
                            if (words[i + 1] != ")")
                            { // Condition to check type casts
                                Symbol newSymbol = new Symbol(words[i + 1], words[i]);
                                if (!(symbols.Any(item => item.symbolName == newSymbol.symbolName))) // Might check if variable name is reserved keyword or not - NOT DONE YET
                                    symbols.Add(newSymbol);
                                // If it was varibale only
                            }
                         }
                     }
                 }

             }

            Console.WriteLine("************************SYMBOL TABLE************************\n");
            Console.WriteLine("[Symbol Name]\t[TYPE]\n");
             foreach (Symbol symbol in symbols)
             {
                 Console.WriteLine(" {0}\t\t{1}", symbol.symbolName, symbol.type);
             }

            Console.WriteLine("\n\n************************SYNTAX ANALYSIS************************\n");

            // List containing possible first words in each line
            List<string> start = new List<string>();

            // Adding entires to list
            start.Add("extern");
            start.Add("static");
            start.Add("void");
            start.Add("int");
            start.Add("float");
            start.Add("double");
            start.Add("string");
            start.Add("bool");
            start.Add("return");
            start.Add("for");
            start.Add("while");
            start.Add("{");
            start.Add("}");
            start.Add("//");

            foreach (string line in lines ) {
                //Thread.Sleep(1000);
                Console.WriteLine("Input: {0}", line);
                string[] words = line.Split(' ');

                // Checking if some operation is being done on a variable
                /*
                 * GRAMMAR:
                 * 1: variable = value {+ | - | / | *} value; (Only + in case of string)
                 * 2: variable = value;
                 * 3: variable = variable {+ | - | / | *} value; (Only + in case of string)
                 * 4: variable = variable;
                 */
                if (!start.Contains(words[0]))
                {
                    if ((symbols.Any(item => item.symbolName == words[0])))
                    {
                        int index = symbols.FindIndex(x => x.symbolName == words[0]);
                        // Checks DMAS operations on variables with strict type check
                        if (symbols.ElementAt(index).type == "double")
                        {
                            string a = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+\s*[+*-/]\s*[0-9]+[.|/][0-9]+\s*;)|"; // [+*-/]
                            string b = @"(([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+)\s*;)|";
                            string c = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|";
                            string d = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*[+*-/]\s*[0-9]+[.|/][0-9]+\s*;)";
                            Regex rgx = new Regex(a + b + c + d);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                        if (symbols.ElementAt(index).type == "float")
                        {
                            string a = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+\s*[+*-/]\s*[0-9]+[.|/][0-9]+\s*;)|"; // [+*-/]
                            string b = @"(([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+)\s*;)|";
                            string c = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|";
                            string d = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*[+*-/]\s*[0-9]+[.|/][0-9]+\s*;)";
                            Regex rgx = new Regex(a + b + c + d);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                        else if (symbols.ElementAt(index).type == "int")
                        {
                           
                            string a = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+\s*;)|";
                            string b = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+\s*[/*+-]\s+[0-9]+\s*;)|"; // [+*-/]
                            string c = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|";
                            string d = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*[+*-/]\s*[0-9]+\s*;)";
                            Regex rgx = new Regex(a+ b + c + d);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                        else if (symbols.ElementAt(index).type == "string")
                        {
                            string a = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*""[a-zA-Z_$][a-zA-Z0-9_]*""\s*[+]\s*""[a-zA-Z_$][a-zA-Z0-9_]*""\s*;)|";
                            string b = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*""[a-zA-Z_$][a-zA-Z0-9_]*""\s*;)|";
                            string c = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|";
                            string d = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*[+]\s*""[a-zA-Z_$][a-zA-Z0-9_]*""\s*;)";
                            Regex rgx = new Regex(a + b + c + d);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                        else if (symbols.ElementAt(index).type == "bool")
                        {
                            string a = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|";
                            string b = @"([a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*(true|false)\s*;)";
                            Regex rgx = new Regex(a + b);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Output: Totally unbale to parse it. It does not match anything\n");
                    }
                }
                // Checking for comments
                else if (words[0] == "//")
                {
                    Console.WriteLine("Output: Comment only\n");
                }
                // Checking for opening and closing brackets of functions { }
                else if (words[0] == "{" || words[0] == "}")
                {
                    Console.WriteLine("Output: Bracket only\n");
                }
                // Checking for functions, variables and loops
                else if (start.Contains(words[0]))
                {
                    // Checking if it is a loop
                    if (words[0] == "for" || words[0] == "while")
                    {
                        Console.WriteLine("Output: It is a Loop\n");
                    }
                    // Checking if it is a return statement
                    else if (words[0] == "return")
                    {
                        if (words.Length > 3 )
                        {
                            string b = @"((return\s*[a-zA-Z_$][a-zA-Z0-9_]*)*\s*([+*-/]\s*[a-zA-Z_$][a-zA-Z0-9_]*)*\s*;)";
                            Regex rgx = new Regex(b);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                        else
                        {
                            string b = @"(return\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*([+*-/]\s*[a-zA-Z_$][a-zA-Z0-9_]*)*\s*;)";
                            Regex rgx = new Regex(b);
                            if (rgx.IsMatch(line))
                                Console.WriteLine("Output: Correct Syntax\n");
                            else
                                Console.WriteLine("Output: Incorrect Syntax\n");
                        }
                    }
                    else { 
                    // Differentiating b/w function and variable declarations
                        for (int i = 0; i < words.Length - 1; i++)
                        {
                            if (keywords.Contains(words[i]))
                            {
                            
                                if (words[i + 2].StartsWith("(")) // It is a function
                                {
                                    Console.WriteLine("Output: It is a function\n");
                                    break;
                                }
                                else // It is a variable declaration
                                {
                                    string a = @"(int\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|(int\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+\s*;)|";
                                    string b = @"(string\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|(string\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*""[0-9|!@#$%^*&()_\-+}{:;./>?\\\||a-z|A-Z]+""\s*;)|";
                                    string c = @"(double\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|(double\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+\s*;)|";
                                    string d = @"(float\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|(float\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*[0-9]+[.|/][0-9]+\s*;)|";
                                    string e = @"(bool\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*;)|(bool\s*[a-zA-Z_$][a-zA-Z0-9_]*\s*=\s*(true|false)\s*;)";
                                    string pattern = a + b + c + d + e;
                                    Regex rgx = new Regex(pattern);
                                    if (rgx.IsMatch(line))
                                        Console.WriteLine("Output: Correct Syntax\n");
                                    else
                                        Console.WriteLine("Output: Incorrect Syntax\n");
                                }
                            
                            }
                        }
                    }
                }
            }

            Console.Read();
        }
    }
}