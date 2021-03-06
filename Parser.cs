﻿using System;
using System.IO;
using System.Collections.Generic;
namespace PuzzleSolver
{
    public class Parser
    {
        private string filename = null;
        private int code;
        private Tile target;
        private List<Tile> pieces;

        //File to be parsed by the parser
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        //List of Tiles, the result of the Parsing
        public List<Tile> Pieces
        {
            get { return pieces; }
            set { pieces = value; }
        }

        //Tile which represents the solution
        public Tile Target
        {
            get { return target; }
            set { target = value; }
        }

        //Int to keep track of used color codes and set subsequent Tiles' color codes
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        //Constructor
        public Parser()
        {
            Filename = "";
            Code = 0;
            Target = null;
            Pieces = new List<Tile>();
        }

        //Resets properties for new parsing
        public void Reset()
        {
            Filename = "";
            Code = 0;
            Target = null;
            Pieces.Clear();
        }

        //Updates Parser's Filename property
        public void Update(string name)
        {
            Filename = name;
        }

        //Read the file, return all the lines as an array of strings
        public string[] ReadFile()
        {
            StreamReader reader = File.OpenText(Filename);
            string line;
            List<string> allLines = new List<string>();
            while ((line = reader.ReadLine()) != null)
                allLines.Add(line);
            return allLines.ToArray();
        }

        //Recursive function, takes an all space 2D array and the parsed 2D array (same size), the two indices, and the array sizes (for convenience)
        //Changes the value at i,j in tile to the value at i,j in alltiles, sets that value to be insignificant in alltiles, then checks all relevant directions
        //For the significance of the char at each of those spots, calling itself on that char if that char is significant
        public void CreateTile(char[,] tile, char[,] alltiles, int i, int j, int cl, int rl)
        {
            tile[i, j] = alltiles[i, j];
            alltiles[i, j] = ' ';
            if (i > 0)
            {
                if (alltiles[i - 1, j] != ' ')
                {
                    CreateTile(tile, alltiles, i - 1, j, cl, rl);
                }
            }
            if (j > 0)
            {
                if (alltiles[i, j - 1] != ' ')
                {
                    CreateTile(tile, alltiles, i, j - 1, cl, rl);
                }
            }
            if (i < (cl - 1))
            {
                if (alltiles[i + 1, j] != ' ')
                {
                    CreateTile(tile, alltiles, i + 1, j, cl, rl);
                }
            }
            if (j < (rl - 1))
            {
                if (alltiles[i, j + 1] != ' ')
                {
                    CreateTile(tile, alltiles, i, j + 1, cl, rl);
                }
            }
        }

        //Takes the string array of all lines, finds the number of rows and the max row length to create rectangular 2D array
        //Transfers the string array into the 2D array of chars, iterates through each char in the array, if the char is significant, send it to CreateTile()
        public void Parse()
        {
            string[] allLines = ReadFile();
            int colLength = allLines.Length;
            int rowLength = allLines[0].Length;
            foreach (string s in allLines)
            {
                if (rowLength < s.Length)
                {
                    rowLength = s.Length;
                }
            }
            char[,] lines = new char[colLength, rowLength];
            int i = 0;
            int j = 0;
            foreach (string s in allLines)
            {
                j = 0;
                foreach (char c in s)
                {
                    lines[i, j] = c;
                    j++;
                }
                for (int k = j; k < rowLength; k++)
                {
                    lines[i, k] = ' ';
                }
                i++;
            }
            for (int n = 0; n < colLength; n++)
            {
                for (int m = 0; m < rowLength; m++)
                {
                    if (lines[n, m] != ' ')
                    {
                        char[,] newTile = new char[colLength, rowLength];
                        for (int q = 0; q < colLength; q++)
                        {
                            for (int w = 0; w < rowLength; w++)
                            {
                                newTile[q, w] = ' ';
                            }
                        }
                        CreateTile(newTile, lines, n, m, colLength, rowLength);
                        Tile t = new Tile(newTile, rowLength, colLength, code);
                        Code += 1;
                        Pieces.Add(t);
                    }
                }
            }
            Pieces.Sort();
            Target = Pieces[Pieces.Count - 1];
            Pieces[Pieces.Count - 1].Target = true;
        }
    }
}