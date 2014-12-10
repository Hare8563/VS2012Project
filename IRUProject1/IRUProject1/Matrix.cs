using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRUProject1
{
    class Matrix<Type> where Type : struct
    {
        public int ROWS { get; set; }
        public int COLS {get;set;}

        private Type[,] mat;

        public Matrix(int rows, int cols)
        {
            mat = new Type[rows, cols];
            ROWS = rows;
            COLS = cols;
        }

        public Matrix(int rows, int cols, Type[] data)
        {
            mat = new Type[rows, cols];
            ROWS = rows;
            COLS = cols;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat[i, j] = data[i * cols + j];
                }
            }
        }

        public Matrix(int rows, int cols, Type[,] data)
        {
            mat = new Type[rows, cols];
            ROWS = rows;
            COLS = cols;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat[i, j] = data[i , j];
                }
            }
        }

        public Matrix<Type> Transpose()
        {
            Matrix<Type> Tmat = new Matrix<Type>(COLS, ROWS);
            for (int j = 0; j < COLS; j++)
            {
                for (int i = 0; i < ROWS; i++)
                {
                    Tmat[j, i] = this[i, j];
                }
            }

            return Tmat;
        }
        /// <summary>
        /// 固有値、固有ベクトルを求める
        /// http://www.sist.ac.jp/~suganuma/kougi/other_lecture/SE/num/Jacobi/C++/Jacobi.txt
        /// </summary>
        /// <param name="ct">最大繰り返し回数 </param>
        /// <param name="eps">収束判定条件</param>
        /// <param name="A1">作業域（nxnの行列），A1の対角要素が固有値</param>
        /// <param name="A2">作業域（nxnの行列)</param>
        /// <param name="X1">作業域（nxnの行列），X1の各列が固有ベクトル</param>
        /// <param name="X2">作業域（nxnの行列)</param>
        /// <returns>0 : 正常, 1 : 収束せず</returns>
        unsafe public int Eigen(int ct, double eps, out double[,] A1, out double[,] A2,out double[,] X1, out double[,] X2)
        {
            double max, s, t, v, sn, cs;
            int i1, i2, k = 0, ind = 1, p = 0, q = 0;
            int n = ROWS;
            A1 = new double[n, n];
            A2 = new double[n, n];
            X1 = new double[n, n];
            X2 = new double[n, n];
            // 初期設定
            for (i1 = 0; i1 < n; i1++)
            {
                for (i2 = 0; i2 < n; i2++)
                {
                    
                    A1[i1, i2] = (dynamic)this[i1,i2];
                    X1[i1,i2] = 0.0;
                }
                X1[i1,i1] = 1.0;
            }
            // 計算
            while (ind > 0 && k < ct)
            {
                // 最大要素の探索
                 max = 0.0;
                for (i1 = 0; i1 < n; i1++)
                {
                    for (i2 = 0; i2 < n; i2++)
                    {
                        if (i2 != i1)
                        {
                            if (Math.Abs(A1[i1, i2]) > max)
                            {
                                max = Math.Abs(A1[i1, i2]);
                                p = i1;
                                q = i2;
                            }
                        }
                    }
                }
                // 収束判定
                // 収束した
                if (max < eps)
                    ind = 0;
                // 収束しない
                else
                {
                    // 準備
                    s = -A1[p, q];
                    t = 0.5 * (A1[p, p] - A1[q, q]);
                    v = Math.Abs(t) / Math.Sqrt(s * s + t * t);
                    sn = Math.Sqrt(0.5 * (1.0 - v));
                    if (s * t < 0.0)
                        sn = -sn;
                    cs = Math.Sqrt(1.0 - sn * sn);
                    // Akの計算
                    for (i1 = 0; i1 < n; i1++)
                    {
                        if (i1 == p)
                        {
                            for (i2 = 0; i2 < n; i2++)
                            {
                                if (i2 == p)
                                    A2[p,p] = A1[p, p] * cs * cs + A1[q, q] * sn * sn -
                                               2.0 * A1[p, q] * sn * cs;
                                else if (i2 == q)
                                    A2[p,q] = 0.0;
                                else
                                    A2[p,i2] = A1[p, i2] * cs - A1[q, i2] * sn;
                            }
                        }
                        else if (i1 == q)
                        {
                            for (i2 = 0; i2 < n; i2++)
                            {
                                if (i2 == q)
                                    A2[q,q] = A1[p, p] * sn * sn + A1[q, q] * cs * cs +
                                               2.0 * A1[p, q] * sn * cs;
                                else if (i2 == p)
                                    A2[q,p] = 0.0;
                                else
                                    A2[q,i2] = A1[q, i2] * cs + A1[p, i2] * sn;
                            }
                        }
                        else
                        {
                            for (i2 = 0; i2 < n; i2++)
                            {
                                if (i2 == p)
                                    A2[i1,p] = A1[i1, p] * cs - A1[i1, q] * sn;
                                else if (i2 == q)
                                    A2[i1,q] = A1[i1, q] * cs + A1[i1, p] * sn;
                                else
                                    A2[i1,i2] = A1[i1, i2];
                            }
                        }
                    }
                    // Xkの計算
                    for (i1 = 0; i1 < n; i1++)
                    {
                        for (i2 = 0; i2 < n; i2++)
                        {
                            if (i2 == p)
                                X2[i1,p] = X1[i1,p] * cs - X1[i1,q] * sn;
                            else if (i2 == q)
                                X2[i1,q] = X1[i1,q] * cs + X1[i1,p] * sn;
                            else
                                X2[i1,i2] = X1[i1,i2];
                        }
                    }
                    // 次のステップへ
                    k++;
                    for (i1 = 0; i1 < n; i1++)
                    {
                        for (i2 = 0; i2 < n; i2++)
                        {
                            A1[i1,i2] = A2[i1,i2];
                            X1[i1,i2] = X2[i1,i2];
                        }
                    }
                }
            }

            return ind;
        }

        

        public static Matrix<Type> operator *(Matrix<Type> mat1, Matrix<Type> mat2)
        {
            Matrix<Type> returnMat = new Matrix<Type>(mat1.ROWS, mat2.COLS);

            for (int i = 0; i < returnMat.ROWS; i++)
            {
                for (int j = 0; j < returnMat.COLS; j++)
                {
                    dynamic val=0;
                    for (int k = 0; k < mat1.COLS; k++)
                    {
                
                        dynamic val1 = mat1[i, k];
                        dynamic val2 = mat2[k, j];
                        val += val1 * val2;
                    }

                    returnMat[i, j] = (Type)val;
                }
            }

                return returnMat;
                
        }

        public static Matrix<Type> operator *(Matrix<Type> mat1, Matrix<double> mat2)
        {
            Matrix<Type> returnMat = new Matrix<Type>(mat1.ROWS, mat2.COLS);

            for (int i = 0; i < returnMat.ROWS; i++)
            {
                for (int j = 0; j < returnMat.COLS; j++)
                {
                    dynamic val = 0;
                    for (int k = 0; k < mat1.COLS; k++)
                    {

                        dynamic val1 = mat1[i, k];
                        dynamic val2 = mat2[k, j];
                        val += val1 * val2;
                    }

                    returnMat[i, j] = (Type)val;
                }
            }

            return returnMat;

        }

        public Type this[int i, int j]
        {
            set
            {
                mat[i, j] = value;
            }
            get
            {
                return mat[i, j];
            }
        }

        public Matrix<Type> this[int i]
        {
            get
            {
                
                Type[] data = new Type[ROWS];
                for (int k = 0; k < ROWS; k++)
                {
                    data[k] = this[k, i];
                }
                return new Matrix<Type>(data.Count(), 1, data);
            }
        }

        
    }
}
