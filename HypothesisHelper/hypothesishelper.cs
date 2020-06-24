using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HypothesisHelper
{
    public partial class Hypothesishelper : Form
    {
        readonly MathFunctions mf = new MathFunctions();
        private Datagraph dg,dgA,dgB,hsA,hsB,sdA,sdB;

        public Hypothesishelper()
        {
            InitializeComponent();
            
            Globals.mainform = this; // Set global variable to allow other classes access to the form

            GbData_SizeChanged(null, null); // Make Data Container control resize once to set initial sizes and locations
        }

        // Make sure Data Container controls are positioned correctly
        private void GbData_SizeChanged(object sender, EventArgs e)
        {
            int Ax, Ay, Bx, By, cw, ch;

            cw = gbData.Size.Width;
            ch = gbData.Size.Height;

            Ax = 5;
            Ay = 40;
            Bx = cw / 2 + 5;
            By = 40;

            txtAData.Location = new Point(Ax, Ay);
            txtBData.Location = new Point(Bx, By);

            txtAData.Width = cw / 2 - 10;
            txtBData.Width = cw / 2 - 10;
            txtAData.Height = ch - 45;
            txtBData.Height = ch - 45;

            lblAData.Location = new Point(cw * 1 / 4 - 20, 16);
            lblBData.Location = new Point(cw * 3 / 4 - 20, 16);
        }

        // Clear the data and results windows
        private void BtnClear_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Really Clear Data?", "Clear Data", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                txtAData.Text = String.Empty;
                txtBData.Text = String.Empty;
                rtbResults.Text = String.Empty;
                txtPredMean.Text = "0.0";

                if (dg != null)
                {
                    dg.Close();
                    dg.Dispose();
                }
                if (dgA != null)
                {
                    dgA.Close();
                    dgA.Dispose();
                }
                if (dgB != null)
                {
                    dgB.Close();
                    dgB.Dispose();
                }
                if (hsA != null)
                {
                    hsA.Close();
                    hsA.Dispose();
                }
                if (hsB != null)
                {
                    hsB.Close();
                    hsB.Dispose();
                }
                if (sdA != null)
                {
                    sdA.Close();
                    sdA.Dispose();
                }
                if (sdB != null)
                {
                    sdB.Close();
                    sdB.Dispose();
                }
            }
        }

        // Perform Calculations
        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            double clevel;
            double predmean;
            double ccfs = 0.5; // Chauvenet Sensitivity

            if (dg != null)
            {
                dg.Close();
                dg.Dispose();
            }
            if (dgA != null)
            {
                dgA.Close();
                dgA.Dispose();
            }
            if (dgB != null)
            {
                dgB.Close();
                dgB.Dispose();
            }
            if (hsA != null)
            {
                hsA.Close();
                hsA.Dispose();
            }
            if (hsB != null)
            {
                hsB.Close();
                hsB.Dispose();
            }
            if (sdA != null)
            {
                sdA.Close();
                sdA.Dispose();
            }
            if (sdB != null)
            {
                sdB.Close();
                sdB.Dispose();
            }

            try
            {
                clevel = (100.0 - Convert.ToDouble(txtConfLevel.Text)) / 100;
            }
            catch
            {
                MessageBox.Show("Invalid Confidence Level Value", "Confidence Level Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (clevel <= 0.0 || clevel >= 1.0)
            {
                MessageBox.Show("Invalid Confidence Level Value, must be between 0 and 100%", "Confidence Level Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtAData.Text == string.Empty)
            {
                MessageBox.Show("A Data cannot be empty", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtBData.Text == string.Empty)
            {
                try
                {
                    predmean = Convert.ToDouble(txtPredMean.Text);
                }
                catch
                {
                    MessageBox.Show("Invalid Predicted Mean Value", "Predicted Mean Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (chkPaired.Checked == true) chkPaired.Checked = false;
                
                rtbResults.Text = String.Empty;

                OneSample(clevel, predmean, ccfs);
            }
            else
            {
                rtbResults.Text = String.Empty;
                TwoSample(clevel, ccfs);
            }
        }

        // Write colored text key/value with formatting to Result Window with crlf
        public void Writekeyvalue(string key, string fmt, double value, string plusminus = "", string suffix = "")
        {
            Writecolortext(key, Color.Green, false);
            Writecolortext(String.Format(CultureInfo.InvariantCulture, "{0}{1:" + fmt + "}{2}", plusminus, value, suffix), Color.Yellow, true);
        }

        // Write colored text to Result Window with or without crlf
        public void Writecolortext(string txt, Color c, bool crlf)
        {
            int length = rtbResults.TextLength;  // at end of text

            if (crlf)
                rtbResults.AppendText(txt + "\r\n");
            else
                rtbResults.AppendText(txt);

            rtbResults.SelectionStart = length;
            rtbResults.SelectionLength = txt.Length;
            rtbResults.SelectionColor = c;
            rtbResults.SelectionFont = new Font("Verdana", 10, FontStyle.Bold);
        }

        // Write a blank line
        public void Writeblankline()
        {
            rtbResults.AppendText("\r\n");
        }

        // Get CSV as string and split into array of numbers, return NULL if anything goes wrong
        double[] CSVSplit(string inp)
        {
            string[] fields;
            double[] values;
            int c = 0;

            try
            {
                fields = inp.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);
                values = new double[fields.Length];

                foreach (string s in fields)
                {
                    values[c++] = Convert.ToDouble(s);
                }
            }
            catch 
            {
                return null;
            }

            return values;
        }

        // Calculate One sided P-Value
        void OneSample(double clevel, double mean, double ccfs)
        {
            int countA,x,graph=0;
            double p, avgA, SDA, SEA, SDAP, SS, SK, KT;
            double sig2P, sig1P;
            double[] xpointsA;
            double w = 0, pw = 0;
            int ifault = 0;

            MathFunctions.MMPair minmaxA;
            MathFunctions.HSBins HSbins;
            MathFunctions.SDBins SDbins;

            double Z;
            double cuA, clA;

            double[] bufferA = CSVSplit(txtAData.Text);

            if (bufferA == null)
            {
                MessageBox.Show("Invalid A Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countA = bufferA.Length;

            if (chkShowGraphs.Checked == true)
            {
                HSbins = mf.SortBins(bufferA, countA);

                hsA = new Datagraph(4, "Histogram Plot")
                {
                    xlabels = HSbins.xlabels,
                    y = HSbins.values,
                    count = HSbins.values.Length,
                };

                hsA.SetPoints();
                hsA.Visible = true;

                xpointsA = new double[countA];

                for (x = 1; x < countA + 1; x++) xpointsA[x - 1] = x;

                if (chkNormalize.Checked) graph = 2;
                dgA = new Datagraph(graph, "Series Plot")
                {
                    x = xpointsA,
                    y = bufferA,
                    count = countA,
                };

                dgA.SetPoints();
                dgA.Visible = true;

                SDbins = mf.SDMakeBins(bufferA, countA);

                xpointsA = new double[SDbins.size];

                for (x = 1; x < SDbins.size + 1; x++) xpointsA[x - 1] = x * SDbins.binsize;

                sdA = new Datagraph(0, "Standard Deviation Plot")
                {
                    x = xpointsA,
                    y = SDbins.values,
                    count = SDbins.size,
                };

                sdA.SetPoints();
                sdA.Visible = true;
            }

            Writecolortext("P-Value criteria for FALSE null hypothesis < ", Color.Cyan, false);
            Writecolortext(String.Format("{0:G6}", clevel), Color.Yellow, true);
            Writeblankline();

            Writecolortext(" *** Performing One Sample Test ***", Color.Cyan, true);

            if (chkOutlier.Checked)
            {
                Writeblankline();
                Writecolortext("*** Data after Chauvenets Criterion Outlier Removal Filter ***", Color.Red, true);

                Writeblankline();
                Writecolortext("Removing Outliers", Color.Purple, true);
                countA = mf.RemoveOutliersUnpaired(ref bufferA, countA, ccfs);
            }
            else
            {
                Writeblankline();
                Writecolortext(" *** Raw Data *** ", Color.Red, true);
            }

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);

            minmaxA = mf.GetMinMax(bufferA, countA);
            SDA = mf.SDSamp(bufferA, countA);
            SDAP = mf.SDPop(bufferA, countA);
            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            SEA = mf.StandardError(bufferA, countA);
            SS = mf.SumOfSquares(bufferA, countA);
            SK = mf.Skewness(bufferA, countA);
            KT = mf.Kurtosis(bufferA, countA);
            mf.SWilks(bufferA, countA, ref w, ref pw, ref ifault);
            

            Writeblankline();
            Writekeyvalue("A Count = ", "0", countA);

            Writeblankline();
            Writekeyvalue("A Min = ", "G6", minmaxA.min);
            Writekeyvalue("A Max = ", "G6", minmaxA.max);

            Writeblankline();
            Writekeyvalue("Hypothesis Mean = ", "G6", mean);
            Writekeyvalue("Sample Mean A = ", "G6", avgA);
            Writeblankline();
            Writekeyvalue("Sample Median A = ", "G6", mf.Median(bufferA, countA));


            if (mean < avgA)
            {
                Writeblankline();
                Writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "+");
                Writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "+", "%");
            }

            if (mean > avgA)
            {
                Writeblankline();
                Writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "-");
                Writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "-", "%");
            }

            Writeblankline();
            Writecolortext("A ", Color.Green, false);
            Writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            Writecolortext(String.Format("{0:G6} to {1:G6}", clA, cuA), Color.Yellow, true);

            Writeblankline();
            Writekeyvalue("Sample SD A = ", "G6", SDA);
            Writekeyvalue("Population SD A = ", "G6", SDAP);

            Writeblankline();
            Writekeyvalue("Sample SE A = ", "G6", SEA);

            Writeblankline();
            
            Writekeyvalue("Sum of Squares = ", "G6", SS);

            Writeblankline();
            Writekeyvalue("Skewness = ", "G6", SK);
            Writekeyvalue("Kurtosis = ", "G6", KT);

            Writeblankline();
            Writekeyvalue("Slope A = ", "G6", mf.Slope(bufferA, countA));
            Writekeyvalue("y-Intercept A = ", "G6", mf.Intercept(bufferA, countA));
            Writeblankline();

            if (ifault == 0)
            {
                Writecolortext("*** Shapiro Wilk Normality Test ***", Color.Blue, true);
                Writekeyvalue("A = ", "G6", w);
                Writekeyvalue("A p-Value = ", "G6", pw);
                Writeblankline();
            }

            Writecolortext("*** Welch t-test ***", Color.Blue, true);

            p = mf.PValue(bufferA, countA, mean);
            sig2P = mf.Critz(p);
            sig1P = mf.Critz(p * 0.5);

            Writeblankline();
            Writecolortext("Null Hypothesis is", Color.Green, false);
            if (p <= clevel)
                Writecolortext(" FALSE ", Color.Cyan, false);
            else
                Writecolortext(" TRUE ", Color.Cyan, false);
            Writecolortext("for Two Sided test", Color.Green, true);

            Writekeyvalue("P-Value Two Sided = ", "G6", p);
            Writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

            Writeblankline();
            Writecolortext("Null Hypothesis is", Color.Green, false);
            if (0.5 * p <= clevel)
                Writecolortext(" FALSE ", Color.Cyan, false);
            else
                Writecolortext(" TRUE ", Color.Cyan, false);
            Writecolortext("for One Sided test", Color.Green, true);

            if (avgA < mean)
                Writekeyvalue("P-Value One Sided A < MEAN = ", "G6", 0.5 * p);
            else
                Writekeyvalue("P-Value One Sided A > MEAN = ", "G6", 0.5 * p);
            Writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

        }

        // Calculate Two sided P-Value
        void TwoSample(double clevel, double ccfs)
        {
            int countA;
            int countB;
            int x, graph = 0, ifaultA = 0, ifaultB = 0;
            double p, avgA, avgB, SDA, SDB, SEA, SEB, SDAP, SDBP, SSA, SSB;
            double SED, SDD, cuAD, clAD, MoD, SKA, SKB, KTA, KTB;
            double sig2P, sig1P, p1, p2;
            double r, pr, tr, sr, sp, wA = 0.0, pwA = 0.0, wB = 0.0, pwB = 0.0;

            MathFunctions.MMPair minmaxA;
            MathFunctions.MMPair minmaxB;
            MathFunctions.HSBins binsA;
            MathFunctions.HSBins binsB;
            MathFunctions.SDBins SDbinsA;
            MathFunctions.SDBins SDbinsB;

            double Z;
            double cuA, clA, cuB, clB;

            double[] bufferA = CSVSplit(txtAData.Text);

            if (bufferA == null)
            {
                MessageBox.Show("Invalid A Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countA = bufferA.Length;

            double[] bufferB = CSVSplit(txtBData.Text);

            if (bufferB == null)
            {
                MessageBox.Show("Invalid B Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countB = bufferB.Length;

            if (chkPaired.Checked)
            {
                if (countA != countB)
                {
                    MessageBox.Show("Paired data specified, but unequal number of A/B values entered", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (countA < 2 || countB < 2)
            {
                MessageBox.Show("Data fields must contain at least two values", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Writecolortext("P-Value criteria for FALSE null hypothesis < ", Color.Cyan, false);
            Writecolortext(String.Format("{0:G6}", clevel), Color.Yellow, true);
            Writeblankline();

            Writecolortext(" *** Performing Two Sample Test ***", Color.Cyan, true);
            Writeblankline();

            if (chkOutlier.Checked)
            {
                Writecolortext("*** Data after Chauvenets Criterion Outlier Removal Filter ***", Color.Red, true);

                if (chkPaired.Checked)
                {
                    Writeblankline();
                    Writecolortext("Removing Outliers in pairs ", Color.Purple, true);
                    countA = mf.RemoveOutliersPaired(ref bufferA, ref bufferB, countA, ccfs);
                    countB = countA;
                }
                else
                {
                    Writeblankline();
                    Writecolortext("Removing Outliers from ", Color.Purple, false);
                    Writecolortext("A", Color.Yellow, true);
                    countA = mf.RemoveOutliersUnpaired(ref bufferA, countA, ccfs);

                    Writeblankline();
                    Writecolortext("Removing Outliers from ", Color.Purple, false);
                    Writecolortext("B", Color.Yellow, true);
                    countB = mf.RemoveOutliersUnpaired(ref bufferB, countB, ccfs);
                }
            }
            else
            {
                Writecolortext(" *** Raw Data *** ", Color.Red, true);
            }

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);
            avgB = mf.Avg(bufferB, countB);
            minmaxA = mf.GetMinMax(bufferA, countA);
            minmaxB = mf.GetMinMax(bufferB, countB);
            SDA = mf.SDSamp(bufferA, countA);
            SDB = mf.SDSamp(bufferB, countB);
            SDAP = mf.SDPop(bufferA, countA);
            SDBP = mf.SDPop(bufferB, countB);

            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            cuB = avgB + Z * (SDB / Math.Sqrt(countB));
            clB = avgB - Z * (SDB / Math.Sqrt(countB));
            SEA = mf.StandardError(bufferA, countA);
            SEB = mf.StandardError(bufferB, countB);

            SSA = mf.SumOfSquares(bufferA, countA);
            SSB = mf.SumOfSquares(bufferB, countB);

            SKA = mf.Skewness(bufferA, countA);
            SKB = mf.Skewness(bufferB, countB);
            KTA = mf.Kurtosis(bufferA, countA);
            KTB = mf.Kurtosis(bufferB, countB);

            mf.SWilks(bufferA, countA, ref wA, ref pwA, ref ifaultA);
            mf.SWilks(bufferB, countB, ref wB, ref pwB, ref ifaultB);

            double d = mf.KSTwo(bufferA, countA, bufferB, countB);

            Writeblankline();
            Writekeyvalue("A Count = ", "0", countA);
            Writekeyvalue("B Count = ", "0", countB);

            Writeblankline();
            Writekeyvalue("A Min = ", "G6", minmaxA.min);
            Writekeyvalue("A Max = ", "G6", minmaxA.max);
            Writekeyvalue("B Min = ", "G6", minmaxB.min);
            Writekeyvalue("B Max = ", "G6", minmaxB.max);

            Writeblankline();
            Writekeyvalue("Sample Mean A = ", "G6", avgA);
            Writekeyvalue("Sample Mean B = ", "G6", avgB);

            Writeblankline();
            Writekeyvalue("Sample Median A = ", "G6", mf.Median(bufferA, countA));
            Writekeyvalue("Sample Median B = ", "G6", mf.Median(bufferB, countB));


            if (avgA < avgB)
            {
                Writeblankline();
                Writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "+");
                Writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "+", "%");
            }

            if (avgA > avgB)
            {
                Writeblankline();
                Writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "-");
                Writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "-", "%");
            }

            Writeblankline();
            Writecolortext("A ", Color.Green, false);
            Writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            Writecolortext(String.Format("{0:G6} to {1:G6}", clA, cuA), Color.Yellow, true);
            Writecolortext("B ", Color.Green, false);
            Writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            Writecolortext(String.Format("{0:G6} to {1:G6}", clB, cuB), Color.Yellow, true);

            Writeblankline();
            Writekeyvalue("Sample SD A = ", "G6", SDA);
            Writekeyvalue("Sample SD B = ", "G6", SDB);
            Writekeyvalue("Population SD A = ", "G6", SDAP);
            Writekeyvalue("Population SD B = ", "G6", SDBP);
 

            if (SDA < SDB)
            {
                Writeblankline();
                Writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "+");
                Writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "+", "%");
            }

            if (SDA > SDB)
            {
                Writeblankline();
                Writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "-");
                Writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "-", "%");
            }

            if (SDAP < SDBP)
            {
                Writekeyvalue("Population SD Difference = ", "G6", Math.Abs(SDBP - SDAP), "+");
                Writekeyvalue("Population SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDAP, SDBP)), "+", "%");
            }

            if (SDAP > SDBP)
            {
                Writekeyvalue("Population SD Difference = ", "G6", Math.Abs(SDBP - SDAP), "-");
                Writekeyvalue("Population SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDAP, SDBP)), "-", "%");
            }

            if (chkPaired.Checked)
            {
                SED = mf.SEofDifferences(bufferA, bufferB, countA);
                SDD = mf.SDofDifferences(bufferA, bufferB, countA);
                MoD = mf.MeanofDifferences(bufferA, bufferB, countA);

                cuAD = MoD + Z * (SDD / Math.Sqrt(countA));
                clAD = MoD - Z * (SDD / Math.Sqrt(countA));

                Writeblankline();
                Writekeyvalue("SD of Sample Differences = ", "G6", SDD);
                Writekeyvalue("SE of Sample Differences = ", "G6", SED);
                Writecolortext(String.Format("Sample Differences {0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
                Writecolortext(String.Format("{0:G6} to {1:G6}", clAD, cuAD), Color.Yellow, true);
            }

            Writeblankline();
            Writekeyvalue("Sample SE A = ", "G6", SEA);
            Writekeyvalue("Sample SE B = ", "G6", SEB);

            Writeblankline();
            Writekeyvalue("Sum of Squares A = ", "G6", SSA);
            Writekeyvalue("Sum of Squares B = ", "G6", SSB);

            Writeblankline();
            Writekeyvalue("Skewness A = ", "G6", SKA);
            Writekeyvalue("Skewness B = ", "G6", SKB);
            Writeblankline();
            Writekeyvalue("Kurtosis = ", "G6", KTA);
            Writekeyvalue("Kurtosis = ", "G6", KTB);

            Writeblankline();
            Writekeyvalue("Slope A = ", "G6", mf.Slope(bufferA, countA));
            Writekeyvalue("y-Intercept A = ", "G6", mf.Intercept(bufferA, countA));

            Writeblankline();
            Writekeyvalue("Slope B = ", "G6", mf.Slope(bufferB, countB));
            Writekeyvalue("y-Intercept B = ", "G6", mf.Intercept(bufferB, countB));

            Writeblankline();
            Writecolortext("*** Shapiro Wilk Normality Test ***", Color.Blue, true);
            if (ifaultA == 0)
            {
                Writekeyvalue("A W = ", "G6", wA);
                Writekeyvalue("A p-Value = ", "G6", pwA);
            }

            if (ifaultB == 0)
            {
                Writekeyvalue("B W = ", "G6", wB);
                Writekeyvalue("B p-Value = ", "G6", pwB);
            }

            if (!chkPaired.Checked)
            {
                if (d > -1.0)
                {
                    double KSCrit = mf.KSCritValue(d, countA, countB, clevel);

                    Writeblankline();
                    Writecolortext("*** Kolmogorov-Smirnov Two Sample Test UnPaired ***", Color.Blue, true);
                    Writekeyvalue("D = ", "G6", d);
                    Writekeyvalue("D Critical = ", "G6", KSCrit);
                    Writekeyvalue("P-Value = ", "G6", mf.KSpValue(d, countA, countB));

                    Writecolortext("Null Hypothesis is", Color.Green, false);
                    if (d > KSCrit)
                        Writecolortext(" FALSE ", Color.Cyan, true);
                    else
                        Writecolortext(" TRUE ", Color.Cyan, true);
                }
            } 
            else
            {
                double Wplus = 0, Wminus = 0;
                double wP = mf.WilcoxonSignedRankTest(bufferA, bufferB, ref Wplus, ref Wminus);

                if (wP > -1.0)
                {

                    Writeblankline();
                    Writecolortext("*** Wilcoxon Signed Rank Test Paired ***", Color.Blue, true);
                    Writekeyvalue("W Positive = ", "G6", Wplus);
                    Writekeyvalue("W Negative = ", "G6", Wminus);
                    Writekeyvalue("P-Value = ", "G6", wP);

                    Writecolortext("Null Hypothesis is", Color.Green, false);
                    if (wP <= clevel)
                        Writecolortext(" FALSE ", Color.Cyan, true);
                    else
                        Writecolortext(" TRUE ", Color.Cyan, true);
                }
            }

            Writeblankline();

            if (chkShowGraphs.Checked == true)
            {

                double[] xpointsA = new double[countA];
                double[] xpointsB = new double[countB];

                if (chkNormalize.Checked) graph = 2;

                for (x = 1; x < countA + 1; x++) xpointsA[x - 1] = x;
                for (x = 1; x < countB + 1; x++) xpointsB[x - 1] = x;

                dgA = new Datagraph(graph, "A Series Plot")
                {
                    x = xpointsA,
                    y = bufferA,
                    count = countA,
                };

                dgB = new Datagraph(graph, "B Series Plot")
                {
                    x = xpointsB,
                    y = bufferB,
                    count = countB,
                };

                dgA.SetPoints();
                dgA.Visible = true;

                dgB.SetPoints();
                dgB.Visible = true;

                binsA = mf.SortBins(bufferA, countA);

                hsA = new Datagraph(4, "A Histogram Plot")
                {
                    xlabels = binsA.xlabels,
                    y = binsA.values,
                    count = binsA.values.Length,
                };

                binsB = mf.SortBins(bufferB, countB);

                hsB = new Datagraph(4, "B Histogram Plot")
                {
                    xlabels = binsB.xlabels,
                    y = binsB.values,
                    count = binsB.values.Length,
                };

                hsA.SetPoints();
                hsA.Visible = true;

                hsB.SetPoints();
                hsB.Visible = true;

                SDbinsA = mf.SDMakeBins(bufferA, countA);

                xpointsA = new double[SDbinsA.size];

                for (x = 1; x < SDbinsA.size + 1; x++) xpointsA[x - 1] = x * SDbinsA.binsize;

                sdA = new Datagraph(0, "A Standard Deviation Plot")
                {
                    x = xpointsA,
                    y = SDbinsA.values,
                    count = SDbinsA.size,
                };

                sdA.SetPoints();
                sdA.Visible = true;

                SDbinsB = mf.SDMakeBins(bufferB, countB);

                xpointsA = new double[SDbinsB.size];

                for (x = 1; x < SDbinsB.size + 1; x++) xpointsA[x - 1] = x * SDbinsB.binsize;

                sdB = new Datagraph(0, "B Standard Deviation Plot")
                {
                    x = xpointsA,
                    y = SDbinsB.values,
                    count = SDbinsB.size,
                };

                sdB.SetPoints();
                sdB.Visible = true;
            }


            if (!chkPaired.Checked)
            {
                Writecolortext("*** Welch t-test UnPaired ***", Color.Blue, true);

                p = mf.PValueUnpaired(bufferA, countA, bufferB, countB);
                sig2P = mf.Critz(p);
                sig1P = mf.Critz(p * 0.5);

                Writeblankline();
                Writecolortext("Null Hypothesis is", Color.Green, false);
                if (p <= clevel)
                    Writecolortext(" FALSE ", Color.Cyan, false);
                else
                    Writecolortext(" TRUE ", Color.Cyan, false);
                Writecolortext("for Two Sided test", Color.Green, true);
  
                Writekeyvalue("P-Value Two Sided = ", "G6", p);
                if (p <= clevel)
                {
                    p2 = mf.PowerTwoTailed(avgA, SDAP, clevel, avgB, countB);
                    Writekeyvalue("Power Two Sided = ", "F1", p2 * 100, "", "%");

                    Writeblankline();
                }

                Writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);
                
                Writeblankline();
                Writecolortext("Null Hypothesis is", Color.Green, false);
                if (0.5 * p <= clevel)
                    Writecolortext(" FALSE ", Color.Cyan, false);
                else
                    Writecolortext(" TRUE ", Color.Cyan, false);
                Writecolortext("for One Sided test", Color.Green, true);

                if (avgA < avgB)
                    Writekeyvalue("P-Value One Sided A < B = ", "G6", 0.5 * p);
                else
                    Writekeyvalue("P-Value One Sided A > B = ", "G6", 0.5 * p);

                if (0.5 * p <= clevel)
                {
                    p1 = 1 - mf.PowerOneTailed(avgA, SDAP, clevel, avgB, countB);
                    Writekeyvalue("Power One Sided = ", "F1", p1 * 100, "", "%");

                    Writeblankline();
                }

                Writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
            }
            else
            {
                if (chkShowGraphs.Checked == true)
                {
                    graph = 1;

                    if (chkNormalize.Checked) graph = 3;

                    dg = new Datagraph(graph, "A/B Scatter Plot")
                    {
                        x = bufferA,
                        y = bufferB,
                        count = countA,
                    };

                    dg.SetPoints();
                    dg.Visible = true;
                }

                Writeblankline();
                Writecolortext("*** Welch t-test Paired ***", Color.Blue, true);

                p = mf.PValuePaired(bufferA, bufferB, countA);
                sig2P = mf.Critz(p);
                sig1P = mf.Critz(p * 0.5);

                Writeblankline();
                Writecolortext("Null Hypothesis is", Color.Green, false);
                if (p <= clevel)
                    Writecolortext(" FALSE ", Color.Cyan, false);
                else
                    Writecolortext(" TRUE ", Color.Cyan, false);
                Writecolortext("for Two Sided test", Color.Green, true);

                Writekeyvalue("P-Value Two Sided = ", "G6", p);
                Writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

                Writeblankline();
                Writecolortext("Null Hypothesis is", Color.Green, false);
                if (0.5 * p <= clevel)
                    Writecolortext(" FALSE ", Color.Cyan, false);
                else
                    Writecolortext(" TRUE ", Color.Cyan, false);
                Writecolortext("for One Sided test", Color.Green, true);

                if (avgA < avgB)
                    Writekeyvalue("P-Value One Sided A < B = ", "G6", 0.5 * p);
                else
                    Writekeyvalue("P-Value One Sided A > B = ", "G6", 0.5 * p);
                Writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);


                Writeblankline();
                Writeblankline();

                Writecolortext("*** Pearson's Correlation Coefficient ***", Color.Blue, true);

                r = mf.R(bufferA, bufferB, countA);
                tr = r / Math.Sqrt((1 - r * r) / (countA - 2));
                pr = mf.PfromT(tr, countA - 2);

                Writeblankline();
                Writecolortext("A to B has ", Color.Green, false);

                if (r == 0.0) Writecolortext("No", Color.Cyan, false);
                if (r == 1.0) Writecolortext("Perfect Positive", Color.Cyan, false);
                if (r == -1.0) Writecolortext("Perfect Negative", Color.Cyan, false);

                if (r > 0.0 && r < 0.3) Writecolortext("Weak Positive", Color.Cyan, false);
                if (r >= 0.3 && r < 0.7) Writecolortext("Moderate Positive", Color.Cyan, false);
                if (r >= 0.7 && r < 1.00) Writecolortext("Strong Positive", Color.Cyan, false);

                if (r < 0.0 && r > -0.3) Writecolortext("Weak Negative", Color.Cyan, false);
                if (r <= -0.3 && r > -0.7) Writecolortext("Moderate Negative", Color.Cyan, false);
                if (r <= -0.7 && r > -1.00) Writecolortext("Strong Negative", Color.Cyan, false);

                Writecolortext(" Correlation", Color.Green, true);

                Writeblankline();
                Writekeyvalue("R-Value = ", "G6", r);
                Writekeyvalue("Coefficient of Determination = ", "G6", r * r);
                Writeblankline();
                Writekeyvalue("P-Value = ", "G6", pr);

                sr = mf.Critz(pr);
                Writekeyvalue(String.Format("Sigma Level {0} ", (sr < 5.99) ? "=" : ">"), "0.#", sr);

                Writeblankline();

                if (pr <= clevel)
                {
                    Writecolortext("P-Value is", Color.Green, false);
                    Writecolortext(" Significant", Color.Cyan, true);
                }
                else
                {
                    Writecolortext("P-Value is", Color.Green, false);
                    Writecolortext(" Not Significant", Color.Cyan, true);
                }

                Writeblankline();
                Writeblankline();

                Writecolortext("*** Spearman's Correlation Coefficient ***", Color.Blue, true);

                sp = mf.R(mf.Rankify(bufferA), mf.Rankify(bufferB), countA);
                tr = sp / Math.Sqrt((1 - sp * sp) / (countA - 2));
                pr = mf.PfromT(tr, countA - 2);

                Writeblankline();
                Writecolortext("A to B has ", Color.Green, false);

                if (sp == 0.0) Writecolortext("No", Color.Cyan, false);
                if (sp == 1.0) Writecolortext("Perfect Positive", Color.Cyan, false);
                if (sp == -1.0) Writecolortext("Perfect Negative", Color.Cyan, false);

                if (sp > 0.0 && sp < 0.3) Writecolortext("Weak Positive", Color.Cyan, false);
                if (sp >= 0.3 && sp < 0.7) Writecolortext("Moderate Positive", Color.Cyan, false);
                if (sp >= 0.7 && sp < 1.00) Writecolortext("Strong Positive", Color.Cyan, false);

                if (sp < 0.0 && sp > -0.3) Writecolortext("Weak Negative", Color.Cyan, false);
                if (sp <= -0.3 && sp > -0.7) Writecolortext("Moderate Negative", Color.Cyan, false);
                if (sp <= -0.7 && sp > -1.00) Writecolortext("Strong Negative", Color.Cyan, false);

                Writecolortext(" Correlation", Color.Green, true);

                Writeblankline();
                Writekeyvalue("ρ-Value = ", "G6", sp);
                Writekeyvalue("Coefficient of Determination = ", "G6", sp * sp);
                Writeblankline();
                Writekeyvalue("P-Value = ", "G6", pr);

                sr = mf.Critz(pr);
                Writekeyvalue(String.Format("Sigma Level {0} ", (sr < 5.99) ? "=" : ">"), "0.#", sr);

                Writeblankline();

                if (pr <= clevel)
                {
                    Writecolortext("P-Value is", Color.Green, false);
                    Writecolortext(" Significant", Color.Cyan, true);
                }
                else
                {
                    Writecolortext("P-Value is", Color.Green, false);
                    Writecolortext(" Not Significant", Color.Cyan, true);
                }
            }
        }

        // Make sure graph is closed and disposed
        private void Hypothesishelper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dg != null)
            {
                dg.Close();
                dg.Dispose();
            }
            if (dgA != null)
            {
                dgA.Close();
                dgA.Dispose();
            }
            if (dgB != null)
            {
                dgB.Close();
                dgB.Dispose();
            }
            if (hsA != null)
            {
                hsA.Close();
                hsA.Dispose();
            }
            if (hsB != null)
            {
                hsB.Close();
                hsB.Dispose();
            }
            if (sdA != null)
            {
                sdA.Close();
                sdA.Dispose();
            }
            if (sdB != null)
            {
                sdB.Close();
                sdB.Dispose();
            }
        }
    }
}
