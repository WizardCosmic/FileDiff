using System.Text.RegularExpressions;

namespace NewFileDiff
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<string> TranTypes = new() { "CASH", "CACH", "NGIN", "CURX" };

        private void BtnCompare_Click(object sender, EventArgs e)
        {

            if (!File.Exists(txtFile1.Text))
            {
                MessageBox.Show($"Unable to find {txtFile1.Text}");
                return;
            }
            if (!File.Exists(txtFile2.Text))
            {
                MessageBox.Show($"Unable to find {txtFile2.Text}");
                return;
            }
            if (!Directory.Exists(txtOutputDirectory.Text))
            {
                MessageBox.Show($"Directory {txtOutputDirectory.Text} is not present");
                return;
            }

            try
            {
                var file1Content = File.ReadAllLines(txtFile1.Text).ToList();
                file1Content.RemoveAt(0);

                var file2Content = File.ReadAllLines(txtFile2.Text).ToList();
                file2Content.RemoveAt(0);

                List<string> file1Unique = file1Content.Where(x => TranTypes.Contains(x[120..124])).ToList();
                List<string> file2Unique = file2Content.Where(x => TranTypes.Contains(x[120..124])).ToList();

                List<string> exact = file1Unique.Intersect(file2Unique).ToList();

                file1Unique = file1Unique.Except(exact).ToList();
                file2Unique = file2Unique.Except(exact).ToList();

                List<string> file1ReferenceDifference = new();
                List<string> file2ReferenceDifference = new();

                List<string> file1OtherDifference = new();
                List<string> file2OtherDifference = new();

                for (int i = 0; i < file1Unique.Count(); i++)
                {
                    var entry1 = file1Unique.ElementAt(i);
                    var entry2 = file2Unique.FirstOrDefault(x => AreEqual(entry1, x));

                    if (entry2 != null)
                    {
                        if (Mask(entry1, 216, 246) == Mask(entry2, 216, 246))
                        {
                            file1ReferenceDifference.Add(entry1);
                            file2ReferenceDifference.Add(entry2);
                        }
                        else
                        {
                            file1OtherDifference.Add(entry1);
                            file2OtherDifference.Add(entry2);
                        }
                        file2Unique.Remove(entry2);
                        continue;
                    }
                }

                file1Unique = file1Unique.Except(file1ReferenceDifference).ToList();
                file1Unique = file1Unique.Except(file1OtherDifference).ToList();

                file2Unique = file2Unique.Except(file2ReferenceDifference).ToList();
                file2Unique = file2Unique.Except(file2OtherDifference).ToList();

                FileInfo file1Info = new(txtFile1.Text);
                string file1 = Regex.Replace(file1Info.Name, $".{file1Info.Extension}$", "");

                FileInfo file2Info = new(txtFile2.Text);
                string file2 = Regex.Replace(file2Info.Name, $".{file2Info.Extension}$", "");

                if (file1Unique.Count > 0)
                    MessageBox.Show("File1 should be empty");

                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file1}_{file2}_exact.txt", exact);

                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file1}_reference_difference.txt", file1ReferenceDifference);
                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file2}_reference_difference.txt", file2ReferenceDifference);

                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file1}_other_differences.txt", file1OtherDifference);
                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file2}_other_differences.txt", file2OtherDifference);

                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file1}_unique.txt", file1Unique);

                string header = $"000000000HEADER00000{file2Unique.Count.ToString().PadLeft(10, '0')}{" ",-2508}";


                //file2Unique.Insert(0, header);

                //File.WriteAllLines($@"{txtOutputDirectory.Text}/{file2}_unique.txt", file2Unique);

                File.WriteAllText($@"{txtOutputDirectory.Text}/{file2}_unique.txt", $"{header}{Environment.NewLine}");
                for (int i = 0; i < file2Unique.Count; i++)
                {
                    File.AppendAllText($@"{txtOutputDirectory.Text}/{file2}_unique.txt", $"{file2Unique[i]}{(i != (file2Unique.Count - 1) ? Environment.NewLine : "")}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to compare and write the results");
            }
        }

        private static bool AreEqual(string transaction1, string transaction2)
        {
            return transaction1[..50] == transaction2[..50] && transaction1[106..120] == transaction2[106..120] &&
                transaction1[120..130] == transaction2[120..130] && transaction1[170..190] == transaction2[170..190] &&
                transaction1[190..191] == transaction2[190..191] && transaction1[246..406] == transaction2[246..406];

        }


        private static string Mask(string value, int start, int stop) =>
        $"{value[..start]}{"*".PadLeft(stop - start, '*')}{value[stop..]}";

        private void BtnBrowseFile1_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new()
            {
                RestoreDirectory = true,
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
                {
                    txtFile1.Text = openFileDialog.FileName;

                    FileInfo fileInfo = new(txtFile1.Text);
                    txtOutputDirectory.Text = fileInfo.DirectoryName;
                }
            };
        }

        private void BtnBrowseFile2_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new()
            {
                RestoreDirectory = true,
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
                {
                    txtFile2.Text = openFileDialog.FileName;

                    FileInfo fileInfo = new(txtFile2.Text);
                    txtOutputDirectory.Text = fileInfo.DirectoryName;
                }
            };
        }
    }
}
