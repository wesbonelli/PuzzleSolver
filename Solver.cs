﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace PuzzleSolver
{
    public class Solver
    {
        private int smallest;
        private int biggest;
        private int solcsize;
        private int solrsize;
        private int algorithm;
        private int solutionstate;                  // ApplicationController checks this at intervals while Solver runs
        private bool pent;
        private bool rotate;
        private bool reflectionoption;
        private bool rotationoption;
        private Tile target;
        private List<Tile> pieces;
        private List<int[,]> colorcodes;
        private int[,] current;
        List<Tile> options;
        char[,] blanksolution;
        int[,] blankcolors;
        private static Thread solvethread;
        private bool running;
        private bool complete;

        //Size of the smallest piece, used for checking empty space possibilities
        public int Smallest
        {
            get { return smallest; }
            set { smallest = value; }
        }

        //Size of the biggest remaining piece, used for checking empty space possiblities
        public int Biggest
        {
            get { return biggest; }
            set { biggest = value; }
        }

        //Number of elements in a column in the solution
        public int solCSize
        {
            get { return solcsize; }
            set { solcsize = value; }
        }

        //Number of elements in a row in the solution
        public int solRSize
        {
            get { return solrsize; }
            set { solrsize = value; }
        }

        //Represents search algorithm chosen
        public int Algorithm
        {
            get { return algorithm; }
            set { algorithm = value; }
        }

        //Represents current state of search
        public int SolutionState
        {
            get { return solutionstate; }
            set { solutionstate = value; }
        }

        //Bool to indicate if this is a pentomino puzzle
        public bool Pent
        {
            get { return pent; }
            set { pent = value; }
        }
        
        //Bool to set piece reflection
        public bool ReflectionOption
        {
            get { return reflectionoption; }
            set { reflectionoption = value; }
        }

        //Bool to set piece rotation
        public bool RotationOption
        {
            get { return rotationoption; }
            set { rotationoption = value; }
        }

        //Bool to indicate if the solutions need to be rotated upon completion or not
        public bool Rotate
        {
            get { return rotate; }
            set { rotate = value; }
        }

        //Solution tile
        public Tile Target
        {
            get { return target; }
            set { target = value; }
        }

        //List of all pieces brought in by the Parser
        public List<Tile> Pieces
        {
            get { return pieces; }
            set { pieces = value; }
        }

        //List of all pieces brought in by the Parser
        public List<int[,]> Colorcodes
        {
            get { return colorcodes; }
            set { colorcodes = value; }
        }

        //Current solution being checked (for display)
        public int[,] Current
        {
            get { return current; }
            set { current = value; }
        }

        //List of non-target tiles
        public List<Tile> Options
        {
            get { return options; }
            set { options = value; }
        }

        //Blank solution to build each on, reset after each is built
        public char[,] Blanksolution
        {
            get { return blanksolution; }
            set { blanksolution = value; }
        }

        //Blank color map to build each on, reset after each is built
        public int[,] Blankcolors
        {
            get { return blankcolors; }
            set { blankcolors = value; }
        }

        //worker thread to run search
        public static Thread Solvethread
        {
            get { return solvethread; }
            set { solvethread = value; }
        }

        //Bool to indicate if search is in progress
        public bool Running
        {
            get { return running; }
            set { running = value; }
        }

        //Bool to indicae if search is complete
        public bool Complete
        {
            get { return complete; }
            set { complete = value; }
        }

        //Constructor
        public Solver()
        {
            Smallest = 0;
            Biggest = 0;
            solCSize = 0;
            solRSize = 0;
            SolutionState = -1;
            Rotate = false;
            Pent = true;
            ReflectionOption = false;
            RotationOption = false;
            Target = new Tile();
            Pieces = new List<Tile>();
            Colorcodes = new List<int[,]>();
            Solvethread = new Thread(new ThreadStart(Run));
            Running = false;
            Complete = false;
        }

        // updates input sources
        public void UpdateInput(Tile targettile, List<Tile> tilepieces)
        {
            Target = targettile;
            Pieces = tilepieces;
        }

        //Resets properties for new solution search
        public void Reset()
        {
            Console.Out.WriteLine("reset");
            Smallest = 0;
            Biggest = 0;
            solCSize = 0;
            solRSize = 0;
            SolutionState = -1;
            Rotate = false;
            Pent = true;
            ReflectionOption = false;
            RotationOption = false;
            Pieces.Clear();
            Colorcodes.Clear();
            Solvethread = new Thread(new ThreadStart(Run));
            Running = false;
            Complete = false;
        }

        //Checks sizes to see if solution is possible, fixes a Tile if applicable to apply that optimization, sorts the Tiles by size, assigns appropriate search algorithm
        public void Run()
        {
            Console.Out.WriteLine("solverrun");
            int i = CheckSizes();
            if (i == -1)
            {
                SolutionState = 0;
                return;               // case 0: no solution possible
            }
            Options = new List<Tile>();
            Blanksolution = new char[Target.cSize, Target.rSize];
            Blankcolors = new int[Target.cSize, Target.rSize];
            bool rotl = false;
            bool refl = false;
            bool turn = false;
            // create blank solution map
            for (int k = 0; k < Target.cSize; k++)
            {
                for (int j = 0; j < Target.rSize; j++)
                {
                    Blanksolution[k, j] = ' ';
                    Blankcolors[k, j] = -1;
                }
            }
            // check pentomino and tile symemetry
            foreach (Tile t in Pieces)
            {
                if (t.Target)
                {
                    rotl = t.CheckRotationalSymmetry();
                    refl = t.CheckReflectedSymmetry();
                    turn = t.Check180Symmetry();
                }
                else
                {
                    Options.Add(t);
                    Pent = Pent && (t.Size == 5);
                }
            }
            Options.Reverse();
            for (int w = 0; w < Options.Count; w++)
            {
                if (Options[w].Orientations.Count == 8)
                {
                    Options[w].Fix(rotl, refl, turn);
                    break;
                }
            }
            if (Options[0].Size > Target.Size / 2)
                Algorithm = 0;
            else if (i == 0)
                Algorithm = 1;
            else
                Algorithm = 2;
            Solvethread = new Thread(new ThreadStart(Solve));
            Solvethread.Start();
        }

        //Stops execution of the search routine
        public void Stop()
        {
            Complete = true;
            Solvethread.Abort();
            return;
        }

        //Runs search algorithm
        public void Solve()
        {
            if (Algorithm == 0)
                SolutionRecursion(Options, Colorcodes, Blanksolution, Blankcolors, Target.cSize, Target.rSize);
            if (Algorithm == 1)
                SolutionBuildingRecursionSubsets(Options, Blanksolution, Blankcolors, 0, 0, 0);
            if (Algorithm == 2)
                SolutionBuildingRecursion(Options, Blanksolution, Blankcolors, 0, 0);
            GetSolutions();
            Stop();
        }

        public void GetSolutions()
        {
            if (Colorcodes.Count == 0)
            {
                SolutionState = 1;
                return;               // case 1: no solution found   
            }
            SolutionState = 2;
        }

        //Check all the sizes of tiles to see if solution tile is the same size or smaller than sum of other tiles
        //If smaller, check to see if equal to some sum of other tile sizes
        public int CheckSizes()
        {
            Rotate = Target.CheckDimensionRotation();
            solCSize = Target.Dimensions.GetLength(0);
            solRSize = Target.Dimensions.GetLength(1);
            //Console.Out.WriteLine(Pieces[0].Size);
            Smallest = Pieces[0].Size;
            int solcolumns = Pieces[Pieces.Count - 1].cSize;
            int solrows = Pieces[Pieces.Count - 1].rSize;
            foreach (Tile t in Pieces)
            {
                t.cSol = solcolumns;
                t.rSol = solrows;
                t.FindOrientations(t.Dimensions, t.cSize, t.rSize, ReflectionOption, RotationOption);
                t.FindPositions();
            }
            int solsize = Pieces[Pieces.Count - 1].Size;
            int sum = 0;
            for (int i = 0; i < Pieces.Count - 1; i++)
                sum += Pieces[i].Size;
            if (sum == solsize)
                return 1;
            else if (sum < solsize)
                return -1;
            else
                return 0;
        }

        //Optimization which finds the size of all enclosed empty spaces in the running solution. If the smallest is too small or if the biggest is too small, the running solution fails
        //Additionally, if its a pentomino puzzle and a space is not a multiple of 5, the solution fails
        public bool EmptySpaceCheck(bool[,] spaces, int csize, int rsize)
        {
            int small = -1;
            int big = -1;
            for (int nn = 0; nn < csize; nn++)
            {
                for (int mm = 0; mm < rsize; mm++)
                {
                    if (spaces[nn, mm] == true)
                    {
                        int i = SpaceRecursion(spaces, nn, mm, csize, rsize);
                        if (i % 5 != 0 && pent)
                            return true;
                        if (i < small || small == -1)
                            small = i;
                        if (i > big)
                            big = i;
                    }
                }
            }
            return (small < Smallest || big < Biggest);
        }

        //Called by EmptySpaceCheck to find all different enclosed empty spaces in the running solution
        public int SpaceRecursion(bool[,] spaces, int i, int j, int csize, int rsize)
        {
            if (i < 0 || j < 0 || i >= csize || j >= rsize)
                return 0;
            else if (spaces[i, j] == false)
                return 0;
            else
            {
                spaces[i, j] = false;
                return (1 + SpaceRecursion(spaces, i - 1, j, csize, rsize) + SpaceRecursion(spaces, i, j - 1, csize, rsize) + SpaceRecursion(spaces, i, j + 1, csize, rsize) + SpaceRecursion(spaces, i + 1, j, csize, rsize));
            }
        }

        //Recursive function to check every combination of tile placements. Starting from the biggest tile, from each position that tile can take it calls the
        //function on the next biggest tile. If at any point there is overlap, the tile is removed and the next possible branch is explored.
        //Once a possible solution is found, it is checked against the solution tile and added (if valid) to the solution list which is returned
        public void SolutionRecursion(List<Tile> pieces, List<int[,]> codes, char[,] runningsolution, int[,] runningcolors, int csize, int rsize)
        {
            Current = runningcolors;
            List<Tile> smallerpieces = new List<Tile>();
            for (int t = 1; t < pieces.Count; t++)
                smallerpieces.Add(pieces[t]);
            Tile biggest = pieces[0];
            Biggest = biggest.Size;
            if (pieces.Count > 1)
                Biggest = pieces[1].Size;
            bool found = false;
            bool[,] spaces = new bool[csize, rsize];
            for (int g = 0; g < biggest.Orientations.Count; g++)
            {
                for (int i = 0; i < biggest.Orientations[g].cOff; i++)
                {
                    for (int j = 0; j < biggest.Orientations[g].rOff; j++)
                    {
                        if (biggest.PlaceInSolution(runningsolution, runningcolors, i, j, g))
                        {
                            if (smallerpieces.Count > 0)
                            {
                                for (int ii = 0; ii < csize; ii++)
                                {
                                    for (int jj = 0; jj < rsize; jj++)
                                    {
                                        spaces[ii, jj] = (runningsolution[ii, jj] == ' ' && Target.Dimensions[ii, jj] != ' ');
                                    }
                                }
                                if (EmptySpaceCheck(spaces, csize, rsize))
                                {
                                    biggest.RemoveFromSolution(runningsolution, runningcolors, i, j, g);
                                    continue;
                                }
                            }
                            if (Target.RunningCheck(runningsolution))
                            {
                                biggest.RemoveFromSolution(runningsolution, runningcolors, i, j, g);
                                continue;
                            }
                            if (smallerpieces.Count == 0)
                            {
                                if (Target.CheckValid(runningsolution) && Target.CheckNewSolution(runningcolors, Colorcodes))
                                {
                                    char[,] newsolution = new char[csize, rsize];
                                    int[,] newcolors = new int[csize, rsize];
                                    for (int n = 0; n < csize; n++)
                                    {
                                        for (int m = 0; m < rsize; m++)
                                        {
                                            newsolution[n, m] = runningsolution[n, m];
                                            newcolors[n, m] = runningcolors[n, m];
                                        }
                                    }
                                    Colorcodes.Add(newcolors);
                                    found = true;
                                }
                                biggest.RemoveFromSolution(runningsolution, runningcolors, i, j, g);
                                if (found)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                SolutionRecursion(smallerpieces, Colorcodes, runningsolution, runningcolors, csize, rsize);
                                biggest.RemoveFromSolution(runningsolution, runningcolors, i, j, g);
                            }
                        }
                        if (found)
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        found = false;
                        break;
                    }
                }
            }
        }

        //Recursive solution finding algorithm which works by filling spaces from the top left across each row until it gets to the bottom right
        //Each piece is tried for each of its orientations to fill that spot, creating a new branch in the solution tree
        //When the entire matrix is filled, the solution is checked to be unique or repeated
        //This is the preferred algorithm when no piece is abnormally large
        public void SolutionBuildingRecursion(List<Tile> puzzle_pieces, char[,] running_solution, int[,] running_colors, int ii, int jj)
        {
            Current = running_colors;
            int j = jj;
            bool[,] spaces = new bool[solCSize, solRSize];
            for (int i = ii; i < solCSize; i++)
            {
                while (j < solRSize)
                {
                    if (running_solution[i, j] == ' ' && Target.Dimensions[i, j] != ' ')
                    {
                        for (int n = 0; n < puzzle_pieces.Count; n++)
                        {
                            for (int m = 0; m < puzzle_pieces[n].Orientations.Count; m++)
                            {
                                if (puzzle_pieces[n].Orientations[m].PlaceInSolution(running_solution, running_colors, i, j, puzzle_pieces[n].Colorcode))
                                {
                                    /*       for(int x = 0; x < sol_csize; x++)
                                           {
                                               for(int y = 0; y < sol_rsize; y++)
                                               {
                                                   Console.Write(running_solution[x, y]);
                                               }
                                               Console.WriteLine();
                                           }
                                        Console.WriteLine();
                                        Console.ReadKey(); */
                                    if (Target.RunningCheck(running_solution))
                                    {
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                        continue;
                                    }
                                    if (puzzle_pieces.Count > 1)
                                    {
                                        for (int iii = 0; iii < solCSize; iii++)
                                        {
                                            for (int jjj = 0; jjj < solRSize; jjj++)
                                            {
                                                spaces[iii, jjj] = (running_solution[iii, jjj] == ' ' && Target.Dimensions[iii, jjj] != ' ');
                                            }
                                        }
                                        if (EmptySpaceCheck(spaces, solCSize, solRSize))
                                        {
                                            puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                            continue;
                                        }
                                    }
                                    if (puzzle_pieces.Count > 1)
                                    {
                                        List<Tile> smaller_pieces = new List<Tile>();
                                        for (int t = 0; t < puzzle_pieces.Count; t++)
                                        {
                                            if (t != n)
                                            {
                                                smaller_pieces.Add(puzzle_pieces[t]);
                                            }
                                        }
                                        SolutionBuildingRecursion(smaller_pieces, running_solution, running_colors, i, j);
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                    }
                                    else if (puzzle_pieces.Count == 1)
                                    {
                                        if (Target.CheckValid(running_solution) && Target.CheckNewSolution(running_colors, Colorcodes))
                                        {
                                            int[,] new_colors = new int[solCSize, solRSize];
                                            for (int x = 0; x < solCSize; x++)
                                            {
                                                for (int y = 0; y < solRSize; y++)
                                                {
                                                    new_colors[x, y] = running_colors[x, y];
                                                }
                                            }
                                            Colorcodes.Add(new_colors);
                                            puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                            return;
                                        }
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                    }
                                }
                            }
                            if (n == puzzle_pieces.Count - 1)
                            {
                                return;
                            }
                        }
                    }
                    j++;
                }
                j = 0;
            }
        }

        //Same method as above, but takes into account potential unused Tiles for puzzles which have too many pieces
        public void SolutionBuildingRecursionSubsets(List<Tile> puzzle_pieces, char[,] running_solution, int[,] running_colors, int ii, int jj, int sum)
        {
            Current = running_colors;
            int j = jj;
            bool[,] spaces = new bool[solCSize, solRSize];
            for (int i = ii; i < solCSize; i++)
            {
                while (j < solRSize)
                {
                    if (running_solution[i, j] == ' ' && Target.Dimensions[i, j] != ' ')
                    {
                        for (int n = 0; n < puzzle_pieces.Count; n++)
                        {
                            if (puzzle_pieces[n].Size + sum > Target.Size)
                            {
                                continue;
                            }
                            for (int m = 0; m < puzzle_pieces[n].Orientations.Count; m++)
                            {
                                if (puzzle_pieces[n].Orientations[m].PlaceInSolution(running_solution, running_colors, i, j, puzzle_pieces[n].Colorcode))
                                {
                                    sum += puzzle_pieces[n].Size;
                                    if (Target.RunningCheck(running_solution))
                                    {
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                        sum -= puzzle_pieces[n].Size;
                                        continue;
                                    }
                                    if (puzzle_pieces.Count > 1 && sum < Target.Size)
                                    {
                                        for (int iii = 0; iii < solCSize; iii++)
                                        {
                                            for (int jjj = 0; jjj < solRSize; jjj++)
                                            {
                                                spaces[iii, jjj] = (running_solution[iii, jjj] == ' ' && Target.Dimensions[iii, jjj] != ' ');
                                            }
                                        }
                                        if (EmptySpaceCheck(spaces, solCSize, solRSize))
                                        {
                                            puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                            sum -= puzzle_pieces[n].Size;
                                            continue;
                                        }
                                    }
                                    if (puzzle_pieces.Count > 1 && sum < Target.Size)
                                    {
                                        List<Tile> smaller_pieces = new List<Tile>();
                                        for (int t = 0; t < puzzle_pieces.Count; t++)
                                        {
                                            if (t != n)
                                            {
                                                smaller_pieces.Add(puzzle_pieces[t]);
                                            }
                                        }
                                        SolutionBuildingRecursionSubsets(smaller_pieces, running_solution, running_colors, i, j, sum);
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                        sum -= puzzle_pieces[n].Size;
                                    }
                                    else if (puzzle_pieces.Count == 1 || sum == Target.Size)
                                    {
                                        if (Target.CheckValid(running_solution) && Target.CheckNewSolution(running_colors, colorcodes))
                                        {
                                            char[,] new_solution = new char[solCSize, solRSize];
                                            int[,] new_colors = new int[solCSize, solRSize];
                                            for (int x = 0; x < solCSize; x++)
                                            {
                                                for (int y = 0; y < solRSize; y++)
                                                {
                                                    new_solution[x, y] = running_solution[x, y];
                                                    new_colors[x, y] = running_colors[x, y];
                                                }
                                            }
                                            Colorcodes.Add(new_colors);
                                            puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                            return;
                                        }
                                        puzzle_pieces[n].Orientations[m].RemoveFromSolution(running_solution, running_colors, i, j);
                                        sum -= puzzle_pieces[n].Size;
                                    }
                                }
                            }
                            if (n == puzzle_pieces.Count - 1)
                            {
                                return;
                            }
                        }
                    }
                    j++;
                }
                j = 0;
            }
        }
    }
}
