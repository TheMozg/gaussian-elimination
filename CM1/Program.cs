﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CM1
{
    class MatrixSolver
    {
        private int RowCount;
        public double[] ResultVec { get; private set; }
        public double[] ResidualVec { get; set; }
        private double[,] M;
        private double[,] M_orig;
        public double[,] Matrix
        {
            get { return M; }
            set { M = value; M_orig = value; RowCount = value.GetUpperBound(0) + 1; }
        }
        private void SwapRows(int row1, int row2)
        {
            for (int col = 0; col < M.GetUpperBound(1) + 1; col++)
            {
                double temp = M[row1, col];
                M[row1, col] = M[row2, col];
                M[row2, col] = temp;
            }
        }
        private void DoPartialPivoting(int sourceRow)
        {
            double pivot = 0;
            int pivotRow = sourceRow;
            for (int row = sourceRow; row < RowCount; row++)
            {
                double pivotCandidate = Math.Abs(M[row, sourceRow]);
                if (pivotCandidate > pivot)
                {
                    pivot = pivotCandidate;
                    pivotRow = row;
                }
            }
            if (pivotRow > sourceRow)
                SwapRows(pivotRow, sourceRow);
        }
        public void DoElimination()
        {
            for (int sourceRow = 0; sourceRow < RowCount - 1; sourceRow++)
            {
                DoPartialPivoting(sourceRow);
                for (int destRow = sourceRow + 1; destRow < RowCount; destRow++)
                {
                    if (M[sourceRow, sourceRow] != 0)
                    {
                        double multiplier = M[destRow, sourceRow] / M[sourceRow, sourceRow];
                        for (int col = 0; col < RowCount + 1; col++)
                            M[destRow, col] -= M[sourceRow, col] * multiplier;
                    }
                }
            }
        }
        public bool DoBackInsertion()
        {
            ResultVec = new double[RowCount];
            for (int row = RowCount - 1; row >= 0; row--)
            {
                if (M[row, row] == 0)
                    return false;
                ResultVec[row] = M[row, RowCount];
                for (int col = RowCount - 1; col > row; col--)
                    ResultVec[row] -= M[row, col] * ResultVec[col];
                ResultVec[row] /= M[row, row];
            }
            return true;
        }
        public void ComputeResidualVec()
        {
            ResidualVec = new double[RowCount];
            for (int row = 0; row < RowCount; row++)
            {
                double appr = 0;
                for (int col = 0; col < RowCount - 1; col++)
                {
                    appr += M_orig[row, col] * ResultVec[col];
                }
                ResidualVec[row] = M_orig[row, RowCount] - appr;
            }
        }
        public double ComputeDeterminant()
        {
            double d = 1;
            for (int row = 0; row < RowCount; row++)
                d *= M[row, row];
            return d;
        }
    }

    class CLI
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Gaussian elimination with partial pivoting");
            Console.WriteLine("Fedor Kalugin, P3210");
            Console.WriteLine();

            Console.WriteLine("Enter the number of action and press [Enter]. Then follow instructions.");
            bool exit = false;
            while (!exit)
            {
                try
                {
                    Console.WriteLine("Menu:\n1. Load matrix\n2. Input matrix\n3. Generate matrix\n4. Exit");
                    int choice = getValidatedInt(new List<int> { 1, 2, 3, 4 });
                    switch (choice)
                    {
                        case 1:
                            Load();
                            break;
                        case 2:
                            Input();
                            break;
                        case 3:
                            Gen();
                            break;
                        case 4:
                            exit = true;
                            break;
                    }
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        static int getValidatedInt(List<int> options)
        {
            int res = 0;
            for (;;)
            {
                Console.Write("> ");
                if (Int32.TryParse(Console.ReadLine(), out res))
                {
                    if (options.Contains(res))
                        break;
                }
                Console.WriteLine("Please enter valid NUMBER.");
            }
            return res;
        }
        static void Gen()
        {
            Console.WriteLine("Enter matrix size (up to 20)");
            var list = new List<int>();
            for (int i = 1; i <= 20; i++)
                list.Add(i);
            int size = getValidatedInt(list);
            double[,] matrix = new double[size, size + 1];

            int rowCount = size;
            int colCount = size + 1;
            Random rnd = new Random();
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    matrix[row, col] = rnd.Next(0, 10);
                }
            }
            DoMatrix(matrix);
        }
        static void Input()
        {
            double[,] matrix = ReadInput();
            DoMatrix(matrix);
        }
        static void Load()
        {
            Console.WriteLine("Looking for 'input.txt' file");
            while (!File.Exists("input.txt"))
            {
                Console.WriteLine("File not found. Please place 'input.txt' file to working directory");
                Console.ReadLine();
            }
            Console.WriteLine("Reading file. Hope it's well formatted");
            MatrixSolver les = new MatrixSolver();
            double[,] matrix = ReadFile("input.txt");
            DoMatrix(matrix);
        }

        static void DoMatrix(double[,] matrix)
        {
            MatrixSolver les = new MatrixSolver();
            les.Matrix = matrix;
            Console.WriteLine("Input matrix:");
            PrintMatrix(les.Matrix);
            les.DoElimination();
            Console.WriteLine("Matrix after elimination:");
            PrintMatrix(les.Matrix);
            Console.WriteLine("Determinant:");
            Console.WriteLine(les.ComputeDeterminant());
            if (les.DoBackInsertion())
            {
                Console.WriteLine("Result vector:");
                foreach (var res in les.ResultVec)
                    Console.WriteLine("{0}", res);
                les.ComputeResidualVec();
                Console.WriteLine("Residual vector:");
                foreach (var res in les.ResidualVec)
                    Console.WriteLine("{0}", res);
            }
            else
            {
                Console.WriteLine("Sorry, there is no unique solution");
            }
            Console.WriteLine();
        }

        static double[,] ReadFile(string path)
        {
            if (!File.Exists(path))
                return new double[0, 0];
            string[] lines = File.ReadAllLines("input.txt");
            int rowCount = lines.Length;
            int colCount = lines[0].Split(' ').Length;
            double[,] matrix = new double[rowCount, colCount];
            for (int row = 0; row < rowCount; row++)
            {
                string[] words = lines[row].Split(' ');
                for (int col = 0; col < colCount; col++)
                {
                    matrix[row, col] = double.Parse(words[col]);
                }
            }
            return matrix;
        }
        static double[,] ReadInput()
        {
            Console.WriteLine("Enter matrix size (up to 20)");
            var list = new List<int>();
            for (int i = 1; i <= 20; i++)
                list.Add(i);
            int size = getValidatedInt(list);
            double[,] matrix = new double[size, size+1];

            int rowCount = size;
            int colCount = size+1;
            for (int row = 0; row < rowCount; row++)
            {
                Console.WriteLine("Row {0}: input {1} space separated numbers", row+1, size+1);
                string line = Console.ReadLine();
                string[] words = line.Split(' ');
                for (int col = 0; col < colCount; col++)
                {
                    matrix[row, col] = double.Parse(words[col]);
                }
            }
            return matrix;
        }

        static void PrintMatrix(double[,] matrix)
        {
            int rowCount = matrix.GetUpperBound(0) + 1;
            int colCount = matrix.GetUpperBound(1) + 1;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    Console.Write("{0:F0} ", matrix[row, col]);
                }
                Console.WriteLine();
            }
        }
    }
}
