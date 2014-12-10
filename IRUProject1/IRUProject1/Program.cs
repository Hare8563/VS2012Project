using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace IRUProject1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Step 1 Obtain face images
            List<PGM> trainingFaces = new List<PGM>();
            using(System.IO.StreamReader sr = new System.IO.StreamReader(@"pgm\FILELIST")){
                while (!sr.EndOfStream)
                {
                    string str = sr.ReadLine();
                   // if(System.Text.RegularExpressions.Regex.IsMatch(str, @"Simple(\w+)\.pgm")){//Simple*.pgmの画像だけで平均顔を生成
                        PGM pgmData = new PGM(@"pgm\" + str);
                        trainingFaces.Add(pgmData);
                   // }
                }

                sr.Close();
            }
            int MinWidth = int.MaxValue;
            int MinHeight = int.MaxValue;

            foreach (PGM pgmImage in trainingFaces)
            {
                if (MinWidth > pgmImage.Width) MinWidth = pgmImage.Width;
                if (MinHeight > pgmImage.Height) MinHeight = pgmImage.Height;
            }

            foreach (PGM pgmImage in trainingFaces)
            {
                pgmImage.scaleTo(MinWidth, MinHeight);
            }
            //Step 2 Represent every image In as Vector Γn.  This step has finished in PGM Library
            //Step 3 Compute the average face vector

            List<double> avarageFaceV = new List<double>();

            for (int i = 0; i < trainingFaces[0].Width * trainingFaces[0].Height; i++)
            {
                avarageFaceV.Add(0);
            }

            foreach (PGM pgm in trainingFaces)
            {
                for (int i = 1; i < trainingFaces[0].Height; i++)
                {
                    for (int j = 1; j < trainingFaces[0].Width; j++)
                    {
                        avarageFaceV[i * trainingFaces[0].Width + j] += pgm[i, j];
                    }
                }
            }

            for (int i = 0; i < avarageFaceV.Count(); i++)
            {
                avarageFaceV[i] = avarageFaceV[i] /  trainingFaces.Count();
            }

            //Step 4 subtract the mean face
           CvMat subFaces = new CvMat(trainingFaces.Count,trainingFaces[0].Width * trainingFaces[0].Height, MatrixType.F32C1);

           
            //foreach (PGM pgm in trainingFaces)
            //{
                for (int i = 0; i < subFaces.Rows; i++)
                {
                    for (int j = 0; j < subFaces.Cols; j++)
                    {
                        subFaces[i, j] = (trainingFaces[i])[j] - avarageFaceV[j];
                    }
                }
            //}

            //step 5 compute the covariance matrix C
            CvMat A = subFaces.Transpose();
           // CvMat ConvMat = A * A.Transpose();
           // ConvMat /= trainingFaces.Count;

            //step 6.1 consider the matrix A^T A
            CvMat MM = A.Transpose() * A;

            ////step 6.2 Compute the eigenVectors
            CvMat evects = new CvMat(MM.Rows, MM.Cols, MatrixType.F32C1);//eigenVector of A^T A
            CvMat evals = new CvMat(1, MM.Cols, MatrixType.F32C1);//eigenValue of A^T A
            MM.EigenVV(evects, evals);//Compute EigenVector and EigenValue


            List<CvMat> u = new List<CvMat>();//AA^T eigenVector
            //List<double> val = new List<double>();
            for (int i = 0; i < evects.Rows; i++)
            {
                CvMat evect = evects.GetRow(i);//i番目の固有ベクトル(1行W*H列)
                CvMat m = A * evect.Transpose();//
                
                u.Add(m);
            //    val.Add(evals[i].Val0);
            }



            ////step 6.3 normalize
            List<CvMat> u2 = new List<CvMat>();//normalized eigenVector
            foreach (var eigenVec in u)
           {

               double norm = eigenVec.Norm();
               CvMat mat = new CvMat(eigenVec.Rows, eigenVec.Cols, MatrixType.F32C1);
               for (int i = 0; i < eigenVec.Rows; i++)
               {
                   mat[i] = eigenVec[i].Val0 / norm;
               }
               u2.Add(mat);
           }

            ////step 7 already sorted by eigenVV method

            //Representing faces
           const int K = 100;//上位5つの固有ベクトルを使用し、復元する
           CvMat res = new CvMat(u2[0].Rows, u2[0].Cols, MatrixType.F32C1);
           for (int k = 0; k < K;k++)
           {
               CvMat v = u2[k];
               //showMatrix(u2[k], MinWidth, MinHeight);


               CvMat m = subFaces.GetRow(30).Transpose();
               m = v.Transpose() * m;
               if (!double.IsNaN(m[0].Val0)) res += m[0].Val0 * v;
           }

           showMatrix(res, MinWidth, MinHeight);

        //平均顔の表示
           CvMat Myu = new CvMat(avarageFaceV.Count(), 1, MatrixType.F32C1);
            int count =0;
           foreach(double val in avarageFaceV){
               Myu[count, 0] = val;
               count++;
            }
           showMatrix(Myu, MinWidth, MinHeight);
             
        }

        private static void showMatrix(CvMat m, int width,int height)
        {
            byte[] byteArray = new byte[m.Rows];
            double MaxVal;
            double MinVal;
            Cv.MinMaxLoc(m, out MinVal, out MaxVal);

            for (int i = 0; i < byteArray.Count(); i++)
            {
                var j = m[i].Val0;
                byteArray[i] = (byte)(255 * ((j - MinVal) / (MaxVal - MinVal)));
            }
            PGM img = new PGM(width, height, byteArray);
            img.Show();
        }
    }
}
