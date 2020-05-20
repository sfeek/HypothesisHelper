using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HypothesisHelper
{
    class MathFunctions
    {
        // Constants
        private const double Z_MAX = 6.0;
        private const double Z_EPSILON = 0.000001;

        // Structure for min max pairs
        public struct MMPair
        {
            public double min, max;
        }

        public struct HSBins
        {
            public double[] values, x;
            public string[] xlabels;
        }

        public struct SDBins
        {
            public double[] values;
            public int size;
            public int binsize;
        }

        // Calculate Percent difference
        public double PerDiff(double f, double s)
        {
            if (f == s)
                return 0.0;

            if (f < s)
                return (s - f) / f * 100.0;
            else
                return (f - s) / f * 100.0;
        }

        // Calculate average
        public double Avg(double[] buffer, int size)
        {
            int i;
            double total = 0;

            for (i = 0; i < size; i++)
                total += buffer[i];

            return total / size;
        }

        // Calculate median
        public double Median(double[] buffer, int size)
        {
            double[] srt = (double[])buffer.Clone();

            Array.Sort(srt);
            return srt[size / 2];
        }

        // Calculate Standard Deviation of Population
        public double SDPop(double[] buffer, int size)
        {
            double mean, standardDeviation = 0.0;
            int i;
            mean = Avg(buffer, size);

            for (i = 0; i < size; ++i)
                standardDeviation += Math.Pow(buffer[i] - mean, 2);

            return Math.Sqrt(standardDeviation / size);
        }

        // Calculate Standard Deviation of Sample
        public double SDSamp(double[] buffer, int size)
        {
            double mean, standardDeviation = 0.0;
            int i;
            mean = Avg(buffer, size);

            for (i = 0; i < size; ++i)
                standardDeviation += Math.Pow(buffer[i] - mean, 2);

            return Math.Sqrt(standardDeviation / (size - 1));
        }

        // Calculate Standard Error of Sample
        public double StandardError(double[] buffer, int size)
        {
            return SDSamp(buffer, size) / Math.Sqrt(size);
        }

        // Calculate Standard Deviation of Differences
        public double SDofDifferences(double[] array1, double[] array2, int size)
        {
            int i;
            double[] diff = new double[size];

            for ( i=0; i<size; i++)
            {
                diff[i] = array2[i] - array1[i];
            }

            return SDSamp(diff, size);
        }

        // Calculate Standard Error of Differences
        public double SEofDifferences(double[] array1, double[] array2, int size)
        {
            int i;
            double[] diff = new double[size];

            for (i = 0; i < size; i++)
            {
                diff[i] = array2[i] - array1[i];
            }

            return SDSamp(diff, size) / Math.Sqrt(size);
        }

        // Calculate Mean of Differences
        public double MeanofDifferences(double[] array1, double[] array2, int size)
        {
            int i;
            double[] diff = new double[size];

            for (i = 0; i < size; i++)
            {
                diff[i] = array2[i] - array1[i];
            }

            return Avg(diff, size);
        }

        // Normalize Array 0 - 1
        public double[] Normalize(double[] buffer, int size)
        {
            int i;
            MMPair minmax = GetMinMax(buffer, size);

            double[] output = new double[size];

            for ( i=0; i<size; i++)
            {
                output[i] = (buffer[i] - minmax.min) / (minmax.max - minmax.min);
            }

            return output;
        }

        public double[] Differences(double[] x, double[] y)
        {
            int i;
            double[] z = new double[x.Length];

            for (i = 0; i < x.Length; ++i)
            {
                z[i] = y[i] - x[i];
            }

            return z;
        }

        public double[] AbsoluteDifferences(double[] z)
        {
            int i;
            double[] zAbs = new double[z.Length];

            for (i = 0; i < z.Length; ++i) {
                zAbs[i] = Math.Abs(z[i]);
            }

            return zAbs;
        }

        private bool PairedDataConformance(double[] x, double[] y)
        {

            if (x == null || y == null) return true;

            if (x.Length == 0 || y.Length == 0) return true;

            if (y.Length != x.Length) return true;

            return false;
        }

        public double WilcoxonSignedRank(double[] x, double[] y, ref double Wplus, ref double Wminus)
        {
            Wplus = 0;
            int i;

            if (PairedDataConformance(x, y)) return -1;

            double[] z = Differences(x, y);
            double[] zAbs = AbsoluteDifferences(z);

            double[] ranks = Rankify(zAbs);
            
            for (i = 0; i<z.Length; ++i) 
            {
                if (z[i] > 0) Wplus += ranks[i];
            }

            double N = Convert.ToDouble(x.Length);
            Wminus = (((N * (N + 1))) / 2.0) - Wplus;

            return Math.Max(Wplus, Wminus);
        }

        private double WilcoxonExactPValue(double Wmax, int N)
        {
            int m = 1 << N;

            int largerRankSums = 0;

            for (int i = 0; i < m; ++i)
            {
                int rankSum = 0;

                for (int j = 0; j < N; ++j)
                {
                    if (((i >> j) & 1) == 1)
                    {
                        rankSum += j + 1;
                    }
                }

                if (rankSum >= Wmax)
                {
                    ++largerRankSums;
                }
            }

            return 2 * ((double)largerRankSums) / ((double)m);
        }

        public double WilcoxonSignedRankTest(double[] x, double[] y, ref double Wplus, ref double Wminus)
        {
            Wplus = 0;
            Wminus = 0;
            double Wmax = 0;

            if (PairedDataConformance(x, y)) return -1;

            int N = x.Length;
            Wmax = WilcoxonSignedRank(x, y, ref Wplus, ref Wminus);

            if (Wmax < 0.0) return -1.0;

            if (N < 31)
                return WilcoxonExactPValue(Wmax, N);
            else 
            {
                double Wmin = ((double)(N * (N + 1)) / 2.0) - Wmax;
                return WilcoxonAsymptoticPValue(Wmin, N);
            }
        }

        private double WilcoxonAsymptoticPValue(double Wmin, int N)
        {
            double ES = (double)(N * (N + 1)) / 4.0;

            double VarS = ES * ((double)(2 * N + 1) / 6.0);

            double z = (Wmin - ES - 0.5) / Math.Sqrt(VarS);

            var chart = new Chart();
     
            return 2.0 * chart.DataManipulator.Statistics.NormalDistribution(z);
        }

        public double[] ZNormalize(double[] buffer, int size)
        {
            double[] zn = new double[size];
            int i;
            double avg = Avg(buffer, size);
            double sd = SDPop(buffer, size);

            for (i = 0; i < size; i++)
            {
                zn[i] = (buffer[i] - avg) / sd;
            }

            return zn;
        }

        public double Skewness(double[] buffer, int size)
        {
            int i;
            double sz = Convert.ToDouble(size);
            double sk, avg, sdp, tmpsum=0;

            avg = Avg(buffer, size);
            sdp = Math.Pow(SDSamp(buffer, size),3);

            for (i = 0; i < size; i++)
            {
                tmpsum += Math.Pow((buffer[i] - avg), 3) / sdp;
            }

            sk = (sz / ((sz - 1.0) * (sz - 2.0))) * tmpsum;

            return sk;
        }

        public double Kurtosis(double[] buffer, int size)
        {
            int i;
            double sz = Convert.ToDouble(size);
            double kt, avg, sdp, tmpsum = 0;

            avg = Avg(buffer, size);
            sdp = Math.Pow(SDSamp(buffer, size), 4);

            for (i = 0; i < size; i++)
            {
                tmpsum += Math.Pow((buffer[i] - avg), 4) / sdp;
            }

            kt = (((sz * (sz + 1)) / ((sz - 1.0) * (sz - 2.0) * (sz - 3.0))) * tmpsum) - ((3 * (sz - 1) * (sz - 1)) / ((sz - 2) * (sz - 3)));

            return kt;
        }

        public double KSTwo(double[] bufferA, int sizeA, double[] bufferB, int sizeB)
        {
            if (sizeA < 10 || sizeA > 1024) return -1.0;
            if (sizeB < 10 || sizeB > 1024) return -1.0;

            double d = 0;
            double szA = Convert.ToDouble(sizeA);
            double szB = Convert.ToDouble(sizeB);

            sizeA++;
            sizeB++;

            int i,j;

            double[] bA = new double[sizeA];
            double[] bB = new double[sizeB];

            for (i = 1; i < sizeA; i++) bA[i] = bufferA[i-1];
            for (i = 1; i < sizeB; i++) bB[i] = bufferB[i-1];
            bA[0] = double.NegativeInfinity;
            bB[0] = double.NegativeInfinity;

            double[] fA = new double[sizeA];
            double[] fB = new double[sizeB];

            Array.Sort(bA);
            Array.Sort(bB);

            for (i = 1; i < sizeA; i++) fA[i] = (double)i / szA;
            for (i = 1; i < sizeB; i++) fB[i] = (double)i / szB;

            fA[0] = 0.0;
            fB[0] = 0.0;

            j = 1;
            for (i = 1; i < sizeA; i++)
            {
                for (; j < sizeB; j++)
                {
                    if (bB[j] > bA[i])
                    {
                        if (Math.Abs(fA[i] - fB[j - 1]) > d) d = Math.Abs(fA[i] - fB[j - 1]);
                        break;
                    }
                }
            }

            j = 1;
            for (i = 1; i < sizeB; i++)
            {
                for (; j < sizeA; j++)
                {
                    if (bA[j] > bB[i])
                    {
                        if (Math.Abs(fA[j - 1] - fB[i]) > d) d = Math.Abs(fA[j - 1] - fB[i]);
                        break;
                    }
                }
            }

            return d;
        }

        public double KSCritValue(double d, int sizeA, int sizeB, double clevel)
        {
            double szA = Convert.ToDouble(sizeA);
            double szB = Convert.ToDouble(sizeB);

            double p = Math.Sqrt(-Math.Log(clevel / 2) * 0.5) * Math.Sqrt((szA + szB) / (szA * szB));
            return p;
        }

        public double KSpValue(double d, int sizeA, int sizeB)
        {
            double szA = Convert.ToDouble(sizeA);
            double szB = Convert.ToDouble(sizeB);

            double t = d * Math.Sqrt(szA * szB / (szA+szB));
                       
            return 1 - KSSum(t,1e-20,100000);
        }

        public double KSSum(double t, double tolerance, int maxIterations)
        {
            double x = -2 * t * t;
            int sign = -1;
            long i = 1;
            double partialSum = 0.5;
            double delta = 1;
            while (delta > tolerance && i < maxIterations)
            {
                delta = Math.Exp(x * i * i);
                partialSum += sign * delta;
                sign *= -1;
                i++;
            }
            if (i == maxIterations)
                return 0;
            return partialSum * 2;
        }


        public void SWilks(double[] buffer, int size, ref double w, ref double pw, ref int ifault)
        {
            /* Algorithm AS R94, Journal of the Royal Statistical Society Series C
             * (Applied Statistics) vol. 44, no. 4, pp. 547-551 (1995).
             */

            double[] x = (double[]) buffer.Clone();

            int n = size;
            int n1 = size;
            int n2 = size / 2;

            w = 0; 

            double zero = 0.0;
            double one = 1.0;
            double two = 2.0;
            double three = 3.0;

            double z90 = 1.2816;
            double z95 = 1.6449;
            double z99 = 2.3263;
            double zm = 1.7509;
            double zss = 0.56268;
            double bf1 = 0.8378;
            double xx90 = 0.556;
            double xx95 = 0.622;
            double sqrth = .70710678;
            double small_value = 1e-19;
            double pi6 = 1.909859;
            double stqr = 1.047198;

            double[] g = { -2.273, .459 };
            double[] c1 = { 0.0, .221157, -.147981, -2.07119, 4.434685, -2.706056 };
            double[] c2 = { 0.0, .042981, -.293762, -1.752461, 5.682633, -3.582633 };
            double[] c3 = { .544, -.39978, .025054, -6.714e-4 };
            double[] c4 = { 1.3822, -.77857, .062767, -.0020322 };
            double[] c5 = { -1.5861, -.31082, -.083751, .0038915 };
            double[] c6 = { -.4803, -.082676, .0030302 };
            double[] c7 = { .164, .533 };
            double[] c8 = { .1736, .315 };
            double[] c9 = { .256, -.00635 };

            double r__1;

            double[] a = new double[n];
            Array.Sort(x);

            int i, j, ncens, i1, nn2;

            double zbar, ssassx, summ2, ssumm2, gamma, delta, range;
            double a1, a2, an, bf, ld, m, s, sa, xi, sx, xx, y, w1;
            double fac, asa, an25, ssa, z90f, sax, zfm, z95f, zsd, z99f, rsn, ssx, xsx;

            pw = 1.0;
            ifault = 0;
            if (w >= 0.0)
            {
                w = 1.0;
            }
            an = (double)(n);
            nn2 = n / 2;
            if (n2 < nn2)
            {
                ifault = 3;
                return;
            }
            if (n < 3)
            {
                ifault = 1;
                return;
            }

            if (n == 3)
            {
                a[0] = sqrth;
            }
            else
            {
                an25 = an + .25;
                summ2 = zero;
                for (i = 1; i <= n2; ++i)
                {
                    a[i - 1] = (double)Ppnd7((i - .375f) / an25, ref ifault);
                    if (ifault != 0)
                    {
                        ifault = 8;
                        return;
                    }
                    summ2 += (a[i - 1] * a[i - 1]);
                }
                summ2 *= two;
                ssumm2 = Math.Sqrt(summ2);
                rsn = one / Math.Sqrt(an);
                a1 = Poly(c1, 6, rsn) - a[0] / ssumm2;

                if (n > 5)
                {
                    i1 = 3;
                    a2 = -a[1] / ssumm2 + Poly(c2, 6, rsn);
                    fac = Math.Sqrt((summ2 - two * a[0] * a[0] - two * a[1] * a[1])
                                / (one - two * a1 * a1 - two * a2 * a2));
                    a[1] = a2;
                }
                else
                {
                    i1 = 2;
                    fac = Math.Sqrt((summ2 - two * a[0] * a[0]) / (one - two * a1 * a1));
                }
                a[0] = a1;
                for (i = i1; i <= nn2; ++i) a[i - 1] /= -fac;
            }

            if (n1 < 3)
            {
                ifault = 1;
                return;
            }
            ncens = n - n1;
            if (ncens < 0 || (ncens > 0 && n < 20))
            {
                ifault = 4;
                return;
            }
            delta = (double)ncens / an;
            if (delta > 0.8)
            {
                ifault = 5;
                return;
            }

            if (w < zero)
            {
                w1 = 1.0 + w;
                ifault = 0;
                goto L70;
            }

            range = x[n1 - 1] - x[0];
            if (range < small_value)
            {
                ifault = 6;
                return;
            }

            ifault = 0;
            xx = x[0] / range;
            sx = xx;
            sa = -a[0];
            j = n - 1;
            for (i = 2; i <= n1; ++i)
            {
                xi = x[i - 1] / range;
                if (xx - xi > small_value)
                {
                    ifault = 7;
                    return;
                }
                sx += xi;
                if (i != j) sa += Sign(1, i - j) * a[Math.Min(i, j) - 1];
                xx = xi;
                --j;
            }
            if (n > 5000)
            {
                ifault = 2;
            }

            sa /= n1;
            sx /= n1;
            ssa = ssx = sax = zero;
            j = n;
            for (i = 1; i <= n1; ++i, --j)
            {
                if (i != j)
                    asa = Sign(1, i - j) * a[Math.Min(i, j) - 1] - sa;
                else
                    asa = -sa;
                xsx = x[i - 1] / range - sx;
                ssa += asa * asa;
                ssx += xsx * xsx;
                sax += asa * xsx;
            }

            ssassx = Math.Sqrt(ssa * ssx);
            w1 = (ssassx - sax) * (ssassx + sax) / (ssa * ssx);
        L70:
            w = 1.0 - w1;

            if (n == 3)
            {
                pw = pi6 * (Math.Asin(Math.Sqrt(w)) - stqr);
                return;
            }
            y = Math.Log(w1);
            xx = Math.Log(an);
            m = zero;
            s = one;
            if (n <= 11)
            {
                gamma = Poly(g, 2, an);
                if (y >= gamma)
                {
                    pw = small_value;
                    return;
                }
                y = -Math.Log(gamma - y);
                m = Poly(c3, 4, an);
                s = Math.Exp(Poly(c4, 4, an));
            }
            else
            {
                m = Poly(c5, 4, xx);
                s = Math.Exp(Poly(c6, 3, xx));
            }
            
            if (ncens > 0)
            {
                ld = -Math.Log(delta);
                bf = one + xx * bf1;
                r__1 = Math.Pow(xx90, (double)xx);
                z90f = z90 + bf * Math.Pow(Poly(c7, 2, r__1), (double)ld);
                r__1 = Math.Pow(xx95, (double)xx);
                z95f = z95 + bf * Math.Pow(Poly(c8, 2, r__1), (double)ld);
                z99f = z99 + bf * Math.Pow(Poly(c9, 2, xx), (double)ld);

                zfm = (z90f + z95f + z99f) / three;
                zsd = (z90 * (z90f - zfm) +
                            z95 * (z95f - zfm) + z99 * (z99f - zfm)) / zss;
                zbar = zfm - zsd * zm;
                m += zbar * s;
                s *= zsd;
            }
            pw = Alnorm((y - m) / s, true);

            // Results are returned in w, pw and ifault
            return;
        } 

        double Ppnd7(double p, ref int ifault)
        {
            /* Algorithm AS 241, Journal of the Royal Statistical Society Series C
             * (Applied Statistics) vol. 26, no. 3, pp. 118-121 (1977).
             */

            double zero = 0.0;
            double one = 1.0;
            double half = 0.5;
            double split1 = 0.425;
            double split2 = 5.0;
            double const1 = 0.180625;
            double const2 = 1.6;
            double a0 = 3.3871327179E+00;
            double a1 = 5.0434271938E+01;
            double a2 = 1.5929113202E+02;
            double a3 = 5.9109374720E+01;
            double b1 = 1.7895169469E+01;
            double b2 = 7.8757757664E+01;
            double b3 = 6.7187563600E+01;
            double c0 = 1.4234372777E+00;
            double c1 = 2.7568153900E+00;
            double c2 = 1.3067284816E+00;
            double c3 = 1.7023821103E-01;
            double d1 = 7.3700164250E-01;
            double d2 = 1.2021132975E-01;
            double e0 = 6.6579051150E+00;
            double e1 = 3.0812263860E+00;
            double e2 = 4.2868294337E-01;
            double e3 = 1.7337203997E-02;
            double f1 = 2.4197894225E-01;
            double f2 = 1.2258202635E-02;

            double normal_dev;
            double q;
            double r;

            ifault = 0;
            q = p - half;
            if (Math.Abs(q) <= split1)
            {
                r = const1 - q * q;
                normal_dev = q * (((a3 * r + a2) * r + a1) * r + a0) /
                             (((b3 * r + b2) * r + b1) * r + one);
                return normal_dev;
            }
            else
            {
                if (q < zero)
                {
                    r = p;
                }
                else
                {
                    r = one - p;
                }
                if (r <= zero)
                {
                    ifault = 1;
                    normal_dev = zero;
                    return normal_dev;
                }
                r = Math.Sqrt(-Math.Log(r));
                if (r <= split2)
                {
                    r -= const2;
                    normal_dev = (((c3 * r + c2) * r + c1) * r + c0) / ((d2 * r + d1) * r + one);
                }
                else
                {
                    r -= split2;
                    normal_dev = (((e3 * r + e2) * r + e1) * r + e0) / ((f2 * r + f1) * r + one);
                }
                if (q < zero) { normal_dev = -normal_dev; }
                return normal_dev;
            }
        }

        double Alnorm(double x, bool upper)
        {
            /* Algorithm AS 66, Journal of the Royal Statistical Society Series C
             * (Applied Statistics) vol. 22, pp. 424-427 (1973).
             */

            double zero = 0;
            double one = 1;
            double half = 0.5;
            double con = 1.28;
            double ltone = 7.0;
            double utzero = 18.66;
            double p = 0.398942280444;
            double q = 0.39990348504;
            double r = 0.398942280385;
            double a1 = 5.75885480458;
            double a2 = 2.62433121679;
            double a3 = 5.92885724438;
            double b1 = -29.8213557807;
            double b2 = 48.6959930692;
            double c1 = -3.8052E-8;
            double c2 = 3.98064794E-4;
            double c3 = -0.151679116635;
            double c4 = 4.8385912808;
            double c5 = 0.742380924027;
            double c6 = 3.99019417011;
            double d1 = 1.00000615302;
            double d2 = 1.98615381364;
            double d3 = 5.29330324926;
            double d4 = -15.1508972451;
            double d5 = 30.789933034;

            double alnorm;
            double z;
            double y;
            bool up = upper;
            z = x;
            if (z < zero)
            {
                up = !up;
                z = -z;
            }
            if (z <= ltone || (up && z <= utzero))
            {
                y = half * z * z;
                if (z > con)
                {
                    alnorm = r * Math.Exp(-y) / (z + c1 + d1 / (z + c2 + d2 / (z + c3 + d3
                            / (z + c4 + d4 / (z + c5 + d5 / (z + c6))))));
                }
                else
                {
                    alnorm = half - z * (p - q * y / (y + a1 + b1 / (y + a2 + b2 / (y + a3))));
                }
            }
            else
            {
                alnorm = zero;
            }

            if (!up) { alnorm = one - alnorm; }
            return alnorm;
        }

        long Sign(long x, long y)
        {
            if (y < 0)
                return -Math.Abs(x);
            else
                return Math.Abs(x);
        }

        double Poly(double[] cc, int nord, double x)
        {
            /* Algorithm AS 181.2   Appl. Statist.  (1982) Vol. 31, No. 2
  
                Calculates the algebraic polynomial of order nord-1 with
                array of coefficients cc.  Zero order coefficient is cc(1) = cc[0]
            */
            int j;
            double p, ret_val;

            ret_val = cc[0];

            if (nord > 1) 
            {
                p = x* cc[nord - 1];
                for (j = nord - 2; j > 0; j--)
                    p = (p + cc[j]) * x;
  
                ret_val += p;
            }
            return ret_val;
        } 

        public double SumOfSquares(double[] buffer, int size)
        {
            int i;
            double ss = 0;
            double avg = Avg(buffer, size);

            for ( i=0; i < size; i++)
            {
                ss += (buffer[i] - avg) * (buffer[i] - avg);
            }

            return ss;
        }

        public double[] Rankify(double[] x)
        {
            int i,j;
            int n = x.Length;
            double[] rank = new double[n];

            for (i = 0; i < n; i++)
            {
                int r = 1, s = 1;

                for (j = 0; j < i; j++)
                {
                    if (x[j] < x[i]) r++;
                    if (x[j] == x[i]) s++;
                }

                for (j = i + 1; j < n; j++)
                {
                    if (x[j] < x[i]) r++;
                    if (x[j] == x[i]) s++;
                }

                rank[i] = r + (s - 1) * 0.5;
            }

            return rank;
        }

        public SDBins SDMakeBins(double[] buffer, int size)
        {
            int bins = (int)Math.Ceiling(Math.Pow(size, 1.0 / 3.0) * 2); // Rice's rule
            int bin = 0,x,tbsize = 0,binsize;
            SDBins b = new SDBins();

            binsize = size / bins;
            if (binsize < 2) binsize = 2;

            b.binsize = binsize;

            double[] tbuff = new double[binsize];
            b.values = new double[bins + 2];

            for (x = 0; x < size; x++)
            {
                tbuff[tbsize++] = buffer[x];
                if (tbsize == binsize)
                {
                    b.values[bin++] = SDPop(tbuff, tbsize);
                    tbsize = 0;
                }
                
            }

            if (x % binsize > 1) b.values[bin++] = SDPop(tbuff, tbsize);

            b.size = bin;

            return b;
        }

        private double[] HSMakeBins(double[] buffer, int size)
        {
            int bins = (int)Math.Ceiling(Math.Pow(size, 1.0 / 3.0) * 2); // Rice's rule

            MMPair minmax = GetMinMax(buffer, size);

            double width = (minmax.max - minmax.min) / bins; 

            double[] intervals = new double[bins * 2]; 
            intervals[0] = minmax.min;
            intervals[1] = minmax.min + width;

            for (int i = 2; i < intervals.Length - 1; i += 2)
            {
                intervals[i] = intervals[i - 1];
                intervals[i + 1] = intervals[i] + width;
            }

            intervals[0] = Math.Floor(minmax.min);
            intervals[intervals.Length - 1] = Math.Ceiling(minmax.max);

            return intervals;
        }

        public HSBins SortBins(double[] buffer, int size)
        {
            int i, x;

            HSBins b = new HSBins();

            int bins = (int)Math.Ceiling(Math.Pow(size, 1.0 / 3.0) * 2);

            b.values = new double[bins];
            b.x = new double[bins];
            b.xlabels = new string[bins];

            for (i = 0; i < bins; i++) b.values[i] = 0.0;

            double[] intervals = HSMakeBins(buffer, size);

            for (x = 0; x < size; x++)
            {
                double v = buffer[x];
                for (i = 0; i < intervals.Length - 1; i += 2)
                {
                    if (v >= intervals[i] && v <= intervals[i + 1])
                    {
                        b.values[i / 2] += 1.0;
                        break;
                    }
                }
            }

            for (i = 0; i < intervals.Length - 1; i += 2)
            {
                b.xlabels[i / 2] = String.Format("{0:G6}", ((intervals[i] + intervals[i + 1]) / 2));
                b.x[i / 2] = i / 2;
            }
                
            return b;
        }

        // Calculate R Pearson Correlation Coefficient
        public double R(double[] x, double[] y, int size)
        {
            double Mx, My;
            double XMxSum = 0, YMySum = 0;
            double XMxYMySum = 0;
            int i;

            Mx = Avg(x, size);
            My = Avg(y, size);

            for (i = 0; i < size; i++)
            {
                XMxSum += (x[i] - Mx) * (x[i] - Mx);
                YMySum += (y[i] - My) * (y[i] - My);
                XMxYMySum += (x[i] - Mx) * (y[i] - My);
            }

            return XMxYMySum / Math.Sqrt(XMxSum * YMySum);
        }

        // Calculate Slope of data
        public double Slope(double[] y, int size)
        {
            int x;
            double a = 0, b, c = 0, d, b1 = 0, b2 = 0;
            
            for (x = 1; x < size+1; x++ )
            {
                a += x * y[x-1];
                b1 += x;
                b2 += y[x - 1];
                c += Math.Pow(x, 2);
            }
            a *= size;
           
            b = b1 * b2;

            c *= size;

            d = Math.Pow(b1, 2);

            return (a - b) / (c - d);
        }

        public double Intercept(double[] y, int size)
        {
            int x;
            double e = 0, b = 0, f;

            for (x = 1; x < size+1; x++)
            {
                e += y[x - 1];
                b += x;
            }

            f = Slope(y, size) * b;

            return (e - f) / size;
        }

        // Calculate of P-Value from Z-Values
        public double Poz(double z)
        {
            double y, x, w;

            if (z == 0.0)
                x = 0.0;
            else
            {
                y = 0.5 * Math.Abs(z);

                if (y >= (Z_MAX * 0.5))
                    x = 1.0;
                else if (y < 1.0)
                {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                                - 0.001075204047) * w + 0.005198775019) * w
                              - 0.019198292004) * w + 0.059054035642) * w
                            - 0.151968751364) * w + 0.319152932694) * w
                          - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y -= 2.0;
                    x = (((((((((((((-0.000045255659 * y
                                     + 0.000152529290) * y - 0.000019538132) * y
                                   - 0.000676904986) * y + 0.001390604284) * y
                                 - 0.000794620820) * y - 0.002034254874) * y
                               + 0.006549791214) * y - 0.010557625006) * y
                             + 0.011630447319) * y - 0.009279453341) * y
                           + 0.005353579108) * y - 0.002141268741) * y
                         + 0.000535310849) * y + 0.999936657524;
                }
            }

            return (z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5));
        }

        // Calculate Criteria Z for P value
        public double Critz(double p)
        {
            double minz = -Z_MAX;
            double maxz = Z_MAX;
            double zval = 0.0;
            double pval;

            if (p < 0.0 || p > 1.0)
                return (0.0);

            while (maxz - minz > Z_EPSILON)
            {
                pval = Poz(zval);

                if (pval > p)
                    maxz = zval;
                else
                    minz = zval;

                zval = (maxz + minz) * 0.5;
            }

            return (Math.Abs(zval));
        }

        // Calculate Log Gamma
        public double Lgamma(double x)
        {
            double[] coef = new double[6] { 76.18009172947146,
                -86.50532032941677, 24.01409824083091,
                -1.231739572450155, 0.1208650973866179E-2,
                -0.5395239384953E-5 };
            double LogSqrtTwoPi = 0.91893853320467274178;
            double denom = x + 1;
            double y = x + 5.5;
            double series = 1.000000000190015;
            for (int i = 0; i < 6; ++i)
            {
                series += coef[i] / denom;
                denom += 1.0;
            }
            return (LogSqrtTwoPi + (x + 0.5) * Math.Log(y) - y + Math.Log(series / x));
        }

        // Calculate P Value from T statistic 
        public double PfromT(double welch_t_statistic, double dof)
        {

            double a = dof / 2;
            double value = dof / (welch_t_statistic * welch_t_statistic + dof);

            if (Double.IsInfinity(value) || Double.IsNaN(value)) return 1.0;

            Globals.mainform.Writekeyvalue("T = ","G6", welch_t_statistic);
            Globals.mainform.Writekeyvalue("df = ", "G6", dof);

            double beta = Lgamma(a) + 0.57236494292470009 - Lgamma(a + 0.5);
            double acu = 0.1E-14;
            double ai;
            double cx;
            int indx;
            int ns;
            double pp;
            double psq;
            double qq;
            double rx;
            double temp;
            double term;
            double xx;

            if (value < 0.0 || 1.0 < value)
                return value;

            if (value == 0.0 || value == 1.0)
                return value;

            psq = a + 0.5;
            cx = 1.0 - value;

            if (a < psq * value)
            {
                xx = cx;
                cx = value;
                pp = 0.5;
                qq = a;
                indx = 1;
            }
            else
            {
                xx = value;
                pp = a;
                qq = 0.5;
                indx = 0;
            }

            term = 1.0;
            ai = 1.0;
            value = 1.0;
            ns = (int)(qq + cx * psq);
            rx = xx / cx;
            temp = qq - ai;

            if (ns == 0)
                rx = xx;

            while (true)
            {
                term = term * temp * rx / (pp + ai);
                value += term; ;
                temp = Math.Abs(term);

                if (temp <= acu && temp <= acu * value)
                {
                    value = value * Math.Exp(pp * Math.Log(xx) + (qq - 1.0) * Math.Log(cx) - beta) / pp;

                    if (indx != 0)
                        value = 1.0 - value;

                    break;
                }

                ai += 1.0;
                ns -= 1;

                if (0 <= ns)
                {
                    temp = qq - ai;

                    if (ns == 0)
                        rx = xx;
                }
                else
                {
                    temp = psq;
                    psq += 1.0;
                }
            }

            return value;
        }

        // Calculate Power One Tailed
        public double PowerOneTailed(double mean1, double std, double clevel, double mean2, double size)
        {
            double Z = Critz(clevel);
            double p = Z - Math.Abs(mean2 - mean1) / (std / Math.Sqrt(size));

            return Poz(p);
        }

        // Calculate Power Two Tailed
        public double PowerTwoTailed(double mean1, double std, double clevel, double mean2, double size)
        {
            double Z = Critz(clevel / 2);
            double p1 = Z - Math.Abs(mean2 - mean1) / (std / Math.Sqrt(size));
            double p2 =-Z - Math.Abs(mean2 - mean1) / (std / Math.Sqrt(size));
 
            return Poz(p2) + 1 - Poz(p1);
        }

        // Calculate P-Value for unpaired Data
        public double PValueUnpaired(double[] array1, int size1, double[] array2, int size2)
        {
            double fmean1, fmean2;
            double usv1 = 0.0, usv2 = 0.0;

            if (size1 <= 1)
                return 1.0;

            if (size2 <= 1)
                return 1.0;


            fmean1 = Avg(array1, size1);
            fmean2 = Avg(array2, size2);

            if (fmean1 == fmean2)
                return 1.0;

            for (int x = 0; x < size1; x++)
            {
                usv1 += (array1[x] - fmean1) * (array1[x] - fmean1);
            }

            for (int x = 0; x < size2; x++)
            {
                usv2 += (array2[x] - fmean2) * (array2[x] - fmean2);
            }

            usv1 /= (size1 - 1);
            usv2 /= (size2 - 1);

            double welch_t_statistic = (fmean1 - fmean2) / Math.Sqrt(usv1 / size1 + usv2 / size2);
            double dof = Math.Pow((usv1 / size1 + usv2 / size2), 2.0) / ((usv1 * usv1) / (size1 * size1 * (size1 - 1)) + (usv2 * usv2) / (size2 * size2 * (size2 - 1)));

            return PfromT(welch_t_statistic, dof);
        }

        // Calculate P-Value for paired data
        public double PValuePaired(double[] array1, double[] array2, int size)
        {
            double[] ABdiff = new double[size];
            double mean;
            double std;
            double welch_t_statistic;
            double dof = size - 1;
            int i;

            for (i = 0; i < size; i++) ABdiff[i] = array1[i] - array2[i];

            mean = Avg(ABdiff, size);
            std = SDPop(ABdiff, size);
            welch_t_statistic = mean / (std / Math.Sqrt(size - 1));

            return PfromT(welch_t_statistic, dof);
        }

        // Calculate single P-Value against a mean
        public double PValue(double[] array1, int size, double u)
        {

            double mean;
                double std;
                double welch_t_statistic;
                double dof = size - 1;

                mean = Avg(array1, size);
                std = SDPop(array1, size);
                welch_t_statistic = (mean - u) / (std / Math.Sqrt(size - 1));

	        return PfromT(welch_t_statistic, dof);
           }

        // Calculate min and max of an array
        public MMPair GetMinMax(double[] arr, int size)
            {

            MMPair minmax;
            int i;

            if (size % 2 == 0)
            {
                if (arr[0] > arr[1])
                {
                    minmax.max = arr[0];
                    minmax.min = arr[1];
                }
                else
                {
                    minmax.min = arr[0];
                    minmax.max = arr[1];
                }

                i = 2;
            }
            else
            {
                minmax.min = arr[0];
                minmax.max = arr[0];
                i = 1;
            }

            while (i < size - 1)
            {
                if (arr[i] > arr[i + 1])
                {
                    if (arr[i] > minmax.max)
                        minmax.max = arr[i];

                    if (arr[i + 1] < minmax.min)
                        minmax.min = arr[i + 1];
                }
                else
                {
                    if (arr[i + 1] > minmax.max)
                        minmax.max = arr[i + 1];

                    if (arr[i] < minmax.min)
                        minmax.min = arr[i];
                }

                i += 2;
            }

            return minmax;
        }

        // Remove Outlier values from an array
        public int RemoveOutliersUnpaired(ref double[] inp, int size, double sensitivity)
        {
            int c = 0;
            int i;
            double test;
            double a = Avg(inp, size);
            double std = SDSamp(inp, size);

            for (i = 0; i < size; i++)
            {
                test = size * Erfc(Math.Abs(inp[i] - a) / std);

                if (test < sensitivity)
                    Globals.mainform.Writekeyvalue(String.Format("Threw Out #{0} -> ",i+1), "G", inp[i]);
                else
                    inp[c++] = inp[i];
	        }

	        return c;
        }

        // Remove Outlier values from two arrays
        public int RemoveOutliersPaired(ref double[] inp1, ref double[] inp2, int size, double sensitivity)
        {
            int c = 0;
            int i;
            double testa,testb;
            double a = Avg(inp1, size);
            double stda = SDSamp(inp1, size);
            double b = Avg(inp2, size);
            double stdb = SDSamp(inp2, size);


            for (i = 0; i < size; i++)
            {
                testa = size * Erfc(Math.Abs(inp1[i] - a) / stda);
                testb = size * Erfc(Math.Abs(inp2[i] - b) / stdb);

                if (testa < sensitivity || testb < sensitivity)
                {
                    Globals.mainform.Writecolortext(String.Format("Threw Out Pair #{0} -> ", i + 1), Color.Green, false);
                    Globals.mainform.Writecolortext(String.Format("{0:G6} , {1:G6}", inp1[i], inp2[i]), Color.Yellow, true);
                }
                else
                {
                    inp1[c] = inp1[i];
                    inp2[c] = inp2[i];
                    c++;
                }
            }

            return c;
        }

        // Calculate Error Function
        public double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        // Calculate Complimentary Error Function
        public double Erfc(double x)
        {
            return 1.0 - Erf(x);
        }
     }
}
