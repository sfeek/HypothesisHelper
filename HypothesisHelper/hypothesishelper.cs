using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HypothesisHelper
{
    public partial class Hypothesishelper : Form
    {
        MathFunctions mf = new MathFunctions();
        Datagraph dg;


        public Hypothesishelper()
        {
            InitializeComponent();
            
            Globals.mainform = this; // Set global variable to allow other classes access to the form

            gbData_SizeChanged(null, null); // Make Data Container control resize once to set initial sizes and locations
        }

        // Make sure Data Container controls are positioned correctly
        private void gbData_SizeChanged(object sender, EventArgs e)
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
        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Really Clear Data?", "Clear Data", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                txtAData.Text = String.Empty;
                txtBData.Text = String.Empty;
                rtbResults.Text = String.Empty;
                if (dg != null)
                {
                    dg.Close();
                    dg.Dispose();
                }
            }
        }

        // Perform Calculations
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            double clevel;
            double predmean;
            double ccfs = 0.5; // Chauvenet Sensitivity
         
            try
            {
                clevel = (100.0 - Convert.ToDouble(txtConfLevel.Text)) / 100;
            }
            catch
            {
                DialogResult dialogResult = MessageBox.Show("Invalid Confidence Level Value", "Confidence Level Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (clevel <= 0.0 || clevel >= 1.0)
            {
                DialogResult dialogResult = MessageBox.Show("Invalid Confidence Level Value, must be between 0 and 100%", "Confidence Level Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtAData.Text == string.Empty)
            {
                DialogResult dialogResult = MessageBox.Show("A Data cannot be empty", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    DialogResult dialogResult = MessageBox.Show("Invalid Predicted Mean Value", "Predicted Mean Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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
        public void writekeyvalue(string key, string fmt, double value, string plusminus = "", string suffix = "")
        {
            writecolortext(key, Color.Green, false);
            writecolortext(String.Format(CultureInfo.InvariantCulture, "{0}{1:" + fmt + "}{2}", plusminus, value, suffix), Color.Yellow, true);
        }

        // Write colored text to Result Window with or without crlf
        public void writecolortext(string txt, Color c, bool crlf)
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
        public void writeblankline()
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
            int countA = 0;
            double p, avgA, SDA,SEA;
            double sig2P, sig1P;

            MathFunctions.Pair minmaxA;

            double Z;
            double cuA, clA;

            double[] bufferA = CSVSplit(txtAData.Text);

            if (bufferA == null)
            {
                DialogResult dialogResult = MessageBox.Show("Invalid A Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countA = bufferA.Length;

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);

            minmaxA = mf.GetMinMax(bufferA, countA);
            SDA = mf.SDSamp(bufferA, countA);
            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            SEA = mf.StandardError(bufferA, countA);

            writecolortext("P-Value criteria for FALSE null hypothesis < ", Color.Cyan, false);
            writecolortext(String.Format("{0:G6}", clevel), Color.Yellow, true);
            writeblankline();

            writecolortext(" *** Performing One Sample Test ***", Color.Cyan, true);

            writeblankline();
            writecolortext(" *** Raw Data *** ", Color.Red, true);

            writeblankline();
            writekeyvalue("A Count = ", "0", countA);

            writeblankline();
            writekeyvalue("A Min = ", "G6", minmaxA.min);
            writekeyvalue("A Max = ", "G6", minmaxA.max);

            writeblankline();
            writekeyvalue("Hypothesis Mean = ", "G6", mean);
            writekeyvalue("Sample Mean A = ", "G6", avgA);

            if (mean < avgA)
            {
                writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "+");
                writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "+", "%");
            }

            if (mean > avgA)
            {
                writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "-");
                writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "-", "%");
            }

            writeblankline();
            writecolortext("A ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clA, cuA), Color.Yellow, true);

            writeblankline();
            writekeyvalue("Sample A SD = ", "G6", SDA);

            writeblankline();
            writekeyvalue("Sample SE A = ", "G6", SEA);

            writeblankline();
            writeblankline();
            writecolortext("*** Welch t-test UnPaired ***", Color.Blue, true);

            p = mf.PValue(bufferA, countA, mean);
            sig2P = mf.Critz(p);
            sig1P = mf.Critz(p * 0.5);

            writeblankline();

            if (p <= clevel)
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" FALSE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }
            else
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" TRUE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }

            writekeyvalue("P-Value Two Sided = ", "G6", p);
            writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

            writeblankline();

            if (avgA < mean)
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A < Hypothesis Mean = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

            }
            else
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A > Hypothesis Mean = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
            }

            writeblankline();
            writeblankline();
            writeblankline();
            writecolortext("*** Data after Chauvenets Criterion Outlier Removal Filter ***", Color.Red, true);

            writeblankline();
            writecolortext("Removing Outliers", Color.Purple, true);
            countA = mf.RemoveOutliers(ref bufferA, countA, ccfs);

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);
            minmaxA = mf.GetMinMax(bufferA, countA);
            SDA = mf.SDSamp(bufferA, countA);
            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            SEA = mf.StandardError(bufferA, countA);

            writeblankline();
            writekeyvalue("A Count = ", "0", countA);

            writeblankline();
            writekeyvalue("A Min = ", "G6", minmaxA.min);
            writekeyvalue("A Max = ", "G6", minmaxA.max);

            writeblankline();
            writekeyvalue("Sample A Mean = ", "G6", avgA);

            if (mean < avgA)
            {
                writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "+");
                writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "+", "%");
            }

            if (mean > avgA)
            {
                writekeyvalue("Sample A Mean Difference = ", "G6", Math.Abs(avgA - mean), "-");
                writekeyvalue("Sample A Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(mean, avgA)), "-", "%");
            }

            writeblankline();
            writecolortext("A ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clA, cuA), Color.Yellow, true);

            writeblankline();
            writekeyvalue("Sample A SD = ", "G6", SDA);

            writeblankline();
            writekeyvalue("Sample SE A = ", "G6", SEA);

            writeblankline();
            writeblankline();
            writecolortext("*** Welch t-test UnPaired ***", Color.Blue, true);

            p = mf.PValue(bufferA, countA, mean);
            sig2P = mf.Critz(p);
            sig1P = mf.Critz(p * 0.5);

            writeblankline();

            if (p <= clevel)
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" FALSE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }
            else
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" TRUE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }

            writekeyvalue("P-Value Two Sided = ", "G6", p);
            writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

            writeblankline();

            if (avgA < mean)
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A < Hypothesis Mean = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

            }
            else
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A > Hypothesis Mean = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
            }
        }

        // Calculate Two sided P-Value
        void TwoSample(double clevel, double ccfs)
        {
            int countA = 0;
            int countB = 0;
            double p, avgA, avgB, SDA, SDB, SEA, SEB;
            double SED, SDD, cuAD, clAD, MoD;
            double sig2P, sig1P;
            double r,pr,tr,sr;
            
            MathFunctions.Pair minmaxA;
            MathFunctions.Pair minmaxB;

            double Z;
            double cuA, clA, cuB, clB;

            double[] bufferA = CSVSplit(txtAData.Text);

            if (bufferA == null)
            {
                DialogResult dialogResult = MessageBox.Show("Invalid A Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countA = bufferA.Length;

            double[] bufferB = CSVSplit(txtBData.Text);

            if (bufferB == null)
            {
                DialogResult dialogResult = MessageBox.Show("Invalid B Data Format", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            countB = bufferB.Length;

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);
            avgB = mf.Avg(bufferB, countB);
            minmaxA = mf.GetMinMax(bufferA, countA);
            minmaxB = mf.GetMinMax(bufferB, countB);
            SDA = mf.SDSamp(bufferA, countA);
            SDB = mf.SDSamp(bufferB, countB);
            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            cuB = avgB + Z * (SDB / Math.Sqrt(countB));
            clB = avgB - Z * (SDB / Math.Sqrt(countB));
            SEA = mf.StandardError(bufferA, countA);
            SEB = mf.StandardError(bufferB, countB);


            writecolortext("P-Value criteria for FALSE null hypothesis < ", Color.Cyan, false);
            writecolortext(String.Format("{0:G6}", clevel), Color.Yellow, true);
            writeblankline();

            writecolortext(" *** Performing Two Sample Test ***", Color.Cyan, true);

            writeblankline();
            writecolortext(" *** Raw Data *** ", Color.Red, true);

            writeblankline();
            writekeyvalue("A Count = ", "0", countA);
            writekeyvalue("B Count = ", "0", countB);

            writeblankline();
            writekeyvalue("A Min = ", "G6", minmaxA.min);
            writekeyvalue("A Max = ", "G6", minmaxA.max);
            writekeyvalue("B Min = ", "G6", minmaxB.min);
            writekeyvalue("B Max = ", "G6", minmaxB.max);

            writeblankline();
            writekeyvalue("Sample Mean A = ", "G6", avgA);
            writekeyvalue("Sample Mean B = ", "G6", avgB);

            if (avgA < avgB)
            {
                writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "+");
                writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "+", "%");
            }

            if (avgA > avgB)
            {
                writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "-");
                writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "-", "%");
            }

            writeblankline();
            writecolortext("A ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clA, cuA), Color.Yellow, true);
            writecolortext("B ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clB, cuB), Color.Yellow, true);

            writeblankline();
            writekeyvalue("Sample SD A = ", "G6", SDA);
            writekeyvalue("Sample SD B = ", "G6", SDB);

            if (SDA < SDB)
            {
                writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "+");
                writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "+", "%");
            }

            if (SDA > SDB)
            {
                writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "-");
                writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "-", "%");
            }

            if (countA == countB)
            {
                SED = mf.SEofDifferences(bufferA, bufferB, countA);
                SDD = mf.SDofDifferences(bufferA, bufferB, countA);
                MoD = mf.MeanofDifferences(bufferA, bufferB, countA);

                cuAD = MoD + Z * (SDD / Math.Sqrt(countA));
                clAD = MoD - Z * (SDD / Math.Sqrt(countA));

                writeblankline();
                writekeyvalue("Mean of Sample Differences = ", "G6", MoD);
                writekeyvalue("SD of Sample Differences = ", "G6", SDD);
                writekeyvalue("SE of Sample Differences = ", "G6", SED);
                writecolortext(String.Format("Sample Differences {0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
                writecolortext(String.Format("{0:G6} - {1:G6}", clAD, cuAD), Color.Yellow, true);
            }

            writeblankline();
            writekeyvalue("Sample SE A = ", "G6", SEA);
            writekeyvalue("Sample SE B = ", "G6", SEB);

            writeblankline();
            writeblankline();
            writecolortext("*** Welch t-test UnPaired ***", Color.Blue, true);

            p = mf.PValueUnpaired(bufferA, countA, bufferB, countB);
            sig2P = mf.Critz(p);
            sig1P = mf.Critz(p * 0.5);

            writeblankline();

            if (p <= clevel)
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" FALSE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }
            else
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" TRUE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }

            writekeyvalue("P-Value Two Sided = ", "G6", p);
            writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

            writeblankline();

            if (avgA < avgB)
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A < B = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

            }
            else
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A > B = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
            }

            if (countA == countB)
            {
                if (dg != null)
                {
                    dg.Close();
                    dg.Dispose();
                }

                dg = new Datagraph();

                dg.x = bufferA;
                dg.y = bufferB;
                dg.count = countA;
                dg.SetPoints();
                dg.Visible = true;

                writeblankline();
                writeblankline();
                writecolortext("*** Welch t-test Paired ***", Color.Blue, true);

                p = mf.PValuePaired(bufferA, bufferB, countA);
                sig2P = mf.Critz(p);
                sig1P = mf.Critz(p * 0.5);

                writeblankline();

                if (p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for Two Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for Two Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value Two Sided = ", "G6", p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

                writeblankline();

                if (avgA < avgB)
                {
                    if (0.5 * p <= clevel)
                    {
                        writecolortext("Null Hypothesis is", Color.Green, false);
                        writecolortext(" FALSE ", Color.Cyan, false);
                        writecolortext("for One Sided test", Color.Green, true);
                    }
                    else
                    {
                        writecolortext("Null Hypothesis is", Color.Green, false);
                        writecolortext(" TRUE ", Color.Cyan, false);
                        writecolortext("for One Sided test", Color.Green, true);
                    }

                    writekeyvalue("P-Value One Sided A < B = ", "G6", 0.5 * p);
                    writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

                }
                else
                {
                    if (0.5 * p <= clevel)
                    {
                        writecolortext("Null Hypothesis is", Color.Green, false);
                        writecolortext(" FALSE ", Color.Cyan, false);
                        writecolortext("for One Sided test", Color.Green, true);
                    }
                    else
                    {
                        writecolortext("Null Hypothesis is", Color.Green, false);
                        writecolortext(" TRUE ", Color.Cyan, false);
                        writecolortext("for One Sided test", Color.Green, true);
                    }

                    writekeyvalue("P-Value One Sided A > B = ", "G6", 0.5 * p);
                    writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
                }

                writeblankline();
                writeblankline();
                writecolortext("*** Pearson Correlation Coefficient ***", Color.Blue, true);

                r = mf.R(bufferA, bufferB, countA);
                tr = r / Math.Sqrt((1 - r * r) / (countA - 2));
                pr = mf.PfromT(tr, countA - 2);

                writeblankline();
                writecolortext("A to B has ", Color.Green, false);

                if (r == 0.0) writecolortext("No", Color.Cyan, false);
                if (r == 1.0) writecolortext("Perfect Positive", Color.Cyan, false);
                if (r == -1.0) writecolortext("Perfect Negative", Color.Cyan, false);

                if (r > 0.0 && r < 0.3) writecolortext("Weak Positive", Color.Cyan, false);
                if (r >= 0.3 && r < 0.7) writecolortext("Moderate Positive", Color.Cyan, false);
                if (r >= 0.7 && r < 1.00) writecolortext("Strong Positive", Color.Cyan, false);

                if (r < 0.0 && r > -0.3) writecolortext("Weak Negative", Color.Cyan, false);
                if (r <= -0.3 && r > -0.7) writecolortext("Moderate Negative", Color.Cyan, false);
                if (r <= -0.7 && r > -1.00) writecolortext("Strong Negative", Color.Cyan, false);

                writecolortext(" Correlation", Color.Green, true);

                writeblankline();

                writekeyvalue("R-Value = ", "G6", r);
                writekeyvalue("Coefficient of Determination = ", "G6", r*r);
                writekeyvalue("P-Value = ", "G6", pr);

                sr = mf.Critz(pr);
                writekeyvalue(String.Format("Sigma Level {0} ", (sr < 5.99) ? "=" : ">"), "0.#", sr);

                if (pr < clevel)
                    writecolortext("P-Value is Significant", Color.Green, true);
                else
                    writecolortext("P-Value is Not Significant", Color.Green, true);
            }

            writeblankline();
            writeblankline();
            writeblankline();

            writecolortext("*** Data after Chauvenets Criterion Outlier Removal Filter ***", Color.Red, true);

            writeblankline();
            writecolortext("Removing Outliers from ", Color.Purple, false);
            writecolortext("A", Color.Yellow, true);
            countA = mf.RemoveOutliers(ref bufferA, countA, ccfs);

            writeblankline();
            writecolortext("Removing Outliers from ", Color.Purple, false);
            writecolortext("B", Color.Yellow, true);
            countB = mf.RemoveOutliers(ref bufferB, countB, ccfs);

            Z = mf.Critz(clevel / 2);
            avgA = mf.Avg(bufferA, countA);
            avgB = mf.Avg(bufferB, countB);
            minmaxA = mf.GetMinMax(bufferA, countA);
            minmaxB = mf.GetMinMax(bufferB, countB);
            SDA = mf.SDSamp(bufferA, countA);
            SDB = mf.SDSamp(bufferB, countB);
            cuA = avgA + Z * (SDA / Math.Sqrt(countA));
            clA = avgA - Z * (SDA / Math.Sqrt(countA));
            cuB = avgB + Z * (SDB / Math.Sqrt(countB));
            clB = avgB - Z * (SDB / Math.Sqrt(countB));
            SEA = mf.StandardError(bufferA, countA);
            SEB = mf.StandardError(bufferB, countB);

            writeblankline();
            writekeyvalue("A Count = ", "0", countA);
            writekeyvalue("B Count = ", "0", countB);

            writeblankline();
            writekeyvalue("A Min = ", "G6", minmaxA.min);
            writekeyvalue("A Max = ", "G6", minmaxA.max);
            writekeyvalue("B Min = ", "G6", minmaxB.min);
            writekeyvalue("B Max = ", "G6", minmaxB.max);

            writeblankline();
            writekeyvalue("Sample Mean A = ", "G6", avgA);
            writekeyvalue("Sample Mean B = ", "G6", avgB);

            if (avgA < avgB)
            {
                writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "+");
                writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "+", "%");
            }

            if (avgA > avgB)
            {
                writekeyvalue("Sample Mean Difference = ", "G6", Math.Abs(avgB - avgA), "-");
                writekeyvalue("Sample Mean % Change = ", "0.#", Math.Abs(mf.PerDiff(avgA, avgB)), "-", "%");
            }

            writeblankline();
            writecolortext("A ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clA, cuA), Color.Yellow, true);
            writecolortext("B ", Color.Green, false);
            writecolortext(String.Format("{0}% CI = ", (1.0 - clevel) * 100), Color.Green, false);
            writecolortext(String.Format("{0:G6} - {1:G6}", clB, cuB), Color.Yellow, true);

            writeblankline();
            writekeyvalue("Sample SD A = ", "G6", SDA);
            writekeyvalue("Sample SD B = ", "G6", SDB);

            if (SDA < SDB)
            {
                writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "+");
                writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "+", "%");
            }

            if (SDA > SDB)
            {
                writekeyvalue("Sample SD Difference = ", "G6", Math.Abs(SDB - SDA), "-");
                writekeyvalue("Sample SD % Change = ", "0.#", Math.Abs(mf.PerDiff(SDA, SDB)), "-", "%");
            }

            writeblankline();
            writekeyvalue("Sample SE A = ", "G6", SEA);
            writekeyvalue("Sample SE B = ", "G6", SEB);

            writeblankline();
            writeblankline();
            writecolortext("*** Welch t-test UnPaired ***", Color.Blue, true);

            p = mf.PValueUnpaired(bufferA, countA, bufferB, countB);
            sig2P = mf.Critz(p);
            sig1P = mf.Critz(p * 0.5);

            writeblankline();

            if (p <= clevel)
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" FALSE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }
            else
            {
                writecolortext("Null Hypothesis is", Color.Green, false);
                writecolortext(" TRUE ", Color.Cyan, false);
                writecolortext("for Two Sided test", Color.Green, true);
            }

            writekeyvalue("P-Value Two Sided = ", "G6", p);
            writekeyvalue(String.Format("Sigma Level {0} ", (sig2P < 5.99) ? "=" : ">"), "0.#", sig2P);

            writeblankline();

            if (avgA < avgB)
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A < B = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);

            }
            else
            {
                if (0.5 * p <= clevel)
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" FALSE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }
                else
                {
                    writecolortext("Null Hypothesis is", Color.Green, false);
                    writecolortext(" TRUE ", Color.Cyan, false);
                    writecolortext("for One Sided test", Color.Green, true);
                }

                writekeyvalue("P-Value One Sided A > B = ", "G6", 0.5 * p);
                writekeyvalue(String.Format("Sigma Level {0} ", (sig1P < 5.99) ? "=" : ">"), "0.#", sig1P);
            }
        }
    }
}
