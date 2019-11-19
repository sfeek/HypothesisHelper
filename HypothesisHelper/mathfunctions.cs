﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypothesisHelper
{
    class MathFunctions
    {
        // Constants
        private const double Z_MAX = 6.0;
        private const double Z_EPSILON = 0.000001;

        // Structure for min max pairs
        public struct Pair
        {
            public double min, max;
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
        public double Avg(double[] buffer, int count)
        {
            int i;
            double total = 0;

            for (i = 0; i < count; i++)
                total += buffer[i];

            return total / count;
        }

        // Calculate Standard Deviation of Population
        public double SDPop(double[] buffer, int count)
        {
            double mean, standardDeviation = 0.0;
            int i;
            mean = Avg(buffer, count);

            for (i = 0; i < count; ++i)
                standardDeviation += Math.Pow(buffer[i] - mean, 2);

            return Math.Sqrt(standardDeviation / count);
        }

        // Calculate Standard Deviation of Sample
        public double SDSamp(double[] buffer, int count)
        {
            double mean, standardDeviation = 0.0;
            int i;
            mean = Avg(buffer, count);

            for (i = 0; i < count; ++i)
                standardDeviation += Math.Pow(buffer[i] - mean, 2);

            return Math.Sqrt(standardDeviation / (count - 1));
        }

        // Calculate Standard Error of Sample
        public double StandardError(double[] buffer, int count)
        {
            return SDSamp(buffer, count) / Math.Sqrt(count);
        }

        // Normalize Array
        public double[] Normalize(double [] buffer, int count)
        {
            int i;
            Pair minmax = GetMinMax(buffer, count);

            double[] output = new double[count];

            for ( i=0; i<count; i++)
            {
                output[i] = (buffer[i] - minmax.min) / (minmax.max - minmax.min);
            }

            return output;
        }

        // Calculate R Pearson Correlation Coefficient
        public double R(double[] x, double[] y, int array_size)
        {
            double Mx, My;
            double XMxSum = 0, YMySum = 0;
            double XMxYMySum = 0;
            int i;

            Mx = Avg(x, array_size);
            My = Avg(y, array_size);

            for (i=0;i<array_size;i++)
            {
                XMxSum += (x[i] - Mx) * (x[i] - Mx);
                YMySum += (y[i] - My) * (y[i] - My);
                XMxYMySum += (x[i] - Mx) * (y[i] - My);
            }

            return XMxYMySum / Math.Sqrt(XMxSum * YMySum);
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

            Globals.mainform.writekeyvalue("T = ","G6", welch_t_statistic);
            Globals.mainform.writekeyvalue("df = ", "G6", dof);

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

        // Calculate P-Value for unpaired Data
        public double PValueUnpaired(double[] array1, int array1_size, double[] array2, int array2_size)
        {
            double fmean1, fmean2;
            double usv1 = 0.0, usv2 = 0.0;

            if (array1_size <= 1)
                return 1.0;

            if (array2_size <= 1)
                return 1.0;


            fmean1 = Avg(array1, array1_size);
            fmean2 = Avg(array2, array2_size);

            if (fmean1 == fmean2)
                return 1.0;

            for (int x = 0; x < array1_size; x++)
            {
                usv1 += (array1[x] - fmean1) * (array1[x] - fmean1);
            }

            for (int x = 0; x < array2_size; x++)
            {
                usv2 += (array2[x] - fmean2) * (array2[x] - fmean2);
            }

            usv1 = usv1 / (array1_size - 1);
            usv2 = usv2 / (array2_size - 1);

            double welch_t_statistic = (fmean1 - fmean2) / Math.Sqrt(usv1 / array1_size + usv2 / array2_size);
            double dof = Math.Pow((usv1 / array1_size + usv2 / array2_size), 2.0) / ((usv1 * usv1) / (array1_size * array1_size * (array1_size - 1)) +
                (usv2 * usv2) / (array2_size * array2_size * (array2_size - 1)));

            return PfromT(welch_t_statistic, dof);
        }

        // Calculate P-Value for paired data
        public double PValuePaired(double[] array1, double[] array2, int array_size)
        {
            double[] ABdiff = new double[array_size];
            double mean;
            double std;
            double welch_t_statistic;
            double dof = array_size - 1;
            int i;

            for (i = 0; i < array_size; i++) ABdiff[i] = array1[i] - array2[i];

            mean = Avg(ABdiff, array_size);
            std = SDPop(ABdiff, array_size);
            welch_t_statistic = mean / (std / Math.Sqrt(array_size - 1));

            return PfromT(welch_t_statistic, dof);
        }

        // Calculate single P-Value against a mean
        public double PValue(double[] array1, int array_size, double u)
        {

            double mean;
                double std;
                double welch_t_statistic;
                double dof = array_size - 1;

                mean = Avg(array1, array_size);
                std = SDPop(array1, array_size);
                welch_t_statistic = (mean - u) / (std / Math.Sqrt(array_size - 1));

	        return PfromT(welch_t_statistic, dof);
           }

        // Calculate min and max of an array
        public Pair GetMinMax(double[] arr, int n)
            {

            Pair minmax;
            int i;

            if (n % 2 == 0)
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

            while (i < n - 1)
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
        public int RemoveOutliers(ref double[] inp, int n, double sensitivity)
        {
            int c = 0;
            int i;
            double test;
            double a = Avg(inp, n);
            double std = SDSamp(inp, n);

            for (i = 0; i < n; i++)
            {
                test = n * Erfc(Math.Abs(inp[i] - a) / std);

                if (test < sensitivity)
                    Globals.mainform.writekeyvalue(String.Format("Threw Out #{0} -> ",i), "G", inp[i]);
                else
                    inp[c++] = inp[i];
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