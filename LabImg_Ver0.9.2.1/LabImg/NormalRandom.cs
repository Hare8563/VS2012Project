using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LabImg
{
    /// <summary>
    /// 正規乱数を発生させるクラス
    /// </summary>
    public class NormalRandom
    {
        private readonly Random _random;
        private readonly double _mean;
        private readonly double _standardDeviation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mean">平均</param>
        /// <param name="standardDeviation">標準偏差</param>
        public NormalRandom(double mean, double standardDeviation)
        {
            _random = new Random(Environment.TickCount);
            _mean = mean;
            _standardDeviation = standardDeviation;
        }

        /// <summary>
        /// 乱数を発生させる
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            const int count = 12;
            var numbers = new double[count];
            for (int i = 0; i < count; ++i)
            {
                numbers[i] = _random.NextDouble();
            }

            return (numbers.Sum() - 6.0) * _standardDeviation + _mean;
        }
    }
}
