using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CoalitionGame
{
    public partial class Form1 : Form
    {
        private Combination combination;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Настройка ToolTips
            toolTip.SetToolTip(txtPlayers, "Введите целое число игроков (например, 3)");
            toolTip.SetToolTip(btnSelectFile, "Выберите текстовый файл с выигрышами");
            toolTip.SetToolTip(txtFilePath, "Путь к выбранному файлу");
            toolTip.SetToolTip(txtManualInput, "Пример: 1:0\n2:200\n123:1000");
            toolTip.SetToolTip(btnCalculate, "Запустить расчет распределения прибыли");
            toolTip.SetToolTip(btnClear, "Очистить все поля ввода и вывода");
            toolTip.SetToolTip(btnSaveResults, "Сохранить результаты в текстовый файл");
            toolTip.SetToolTip(txtOutput, "Результаты расчетов");
            toolTip.SetToolTip(dgvProfits, "Распределение выигрышей по игрокам");
        }

        // Обработчик кнопки выбора файла
        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Выберите файл с выигрышами";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
        }

        // Обработчик кнопки расчета
        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            dgvProfits.Rows.Clear();
            dgvProfits.Columns.Clear();

            // Проверка количества игроков
            if (!int.TryParse(txtPlayers.Text, out int numberOfPlayers) || numberOfPlayers <= 0)
            {
                AppendRichText("Ошибка: введите положительное целое число для количества игроков.", Color.Red);
                return;
            }

            try
            {
                // Настройка колонок DataGridView
                SetupProfitTable(numberOfPlayers);

                combination = new Combination(numberOfPlayers);

                // Ввод выигрышей
                string filePath = txtFilePath.Text;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    combination.InputWinningsForCombinations(filePath);
                }
                else if (!string.IsNullOrEmpty(txtManualInput.Text))
                {
                    // Валидация ручного ввода
                    var lines = txtManualInput.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length != 2 || !int.TryParse(parts[1].Trim(), out _))
                        {
                            AppendRichText($"Ошибка в строке '{line}': неверный формат или выигрыш не является числом.", Color.Red);
                            return;
                        }
                    }

                    // Сохранение ручного ввода во временный файл
                    string tempFile = Path.Combine(Path.GetTempPath(), "winnings_temp.txt");
                    File.WriteAllText(tempFile, txtManualInput.Text, Encoding.UTF8);
                    combination.InputWinningsForCombinations(tempFile);
                    File.Delete(tempFile);
                }
                else
                {
                    AppendRichText("Ошибка: выберите файл или введите выигрыши вручную.", Color.Red);
                    return;
                }

                // Перенаправление консольного вывода в RichTextBox
                using (var sw = new StringWriter())
                {
                    Console.SetOut(sw);
                    combination.DisplayCombinations();
                    combination.CalculateProfitDistribution();
                    string output = sw.ToString();
                    Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

                    // Форматированный вывод в RichTextBox
                    var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        if (line.Contains("[ВНИМАНИЕ] Ни одно распределение не находится в ядре"))
                        {
                            AppendRichText(line, Color.Red, true);
                        }
                        else
                        {
                            AppendRichText(line, Color.Black);
                        }
                    }
                }

                // Заполнение таблицы распределений
                var distributions = combination.GetProfitDistributions();
                foreach (var dist in distributions)
                {
                    var row = new object[1 + numberOfPlayers];
                    row[0] = dist.Permutation;
                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        row[i + 1] = dist.Profits[i];
                    }
                    dgvProfits.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                AppendRichText($"Произошла ошибка: {ex.Message}", Color.Red);
            }
        }

        // Настройка колонок таблицы
        private void SetupProfitTable(int numberOfPlayers)
        {
            // Колонка для комбинации
            var comboColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Комбинация",
                Name = "Combination",
                Width = 100
            };
            dgvProfits.Columns.Add(comboColumn);

            // Колонки для игроков
            for (int i = 1; i <= numberOfPlayers; i++)
            {
                var playerColumn = new DataGridViewTextBoxColumn
                {
                    HeaderText = $"Игрок {i}",
                    Name = $"Player{i}",
                    Width = 80
                };
                dgvProfits.Columns.Add(playerColumn);
            }
        }

        // Метод для добавления текста в RichTextBox с форматированием
        private void AppendRichText(string text, Color color, bool bold = false)
        {
            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.SelectionLength = 0;
            txtOutput.SelectionColor = color;
            txtOutput.SelectionFont = new Font(txtOutput.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            txtOutput.AppendText(text + "\n");
            txtOutput.SelectionColor = txtOutput.ForeColor;
            txtOutput.SelectionFont = txtOutput.Font;
        }

        // Обработчик кнопки очистки
        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtPlayers.Clear();
            txtFilePath.Clear();
            txtManualInput.Clear();
            txtOutput.Clear();
            dgvProfits.Rows.Clear();
            dgvProfits.Columns.Clear();
            combination = null;
        }

        // Обработчик кнопки сохранения результатов
        private void BtnSaveResults_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtOutput.Text))
            {
                MessageBox.Show("Нет результатов для сохранения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveFileDialog.Title = "Сохранить результаты";
                saveFileDialog.FileName = "results.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Сохранение текстового вывода
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Текстовый вывод:");
                        sb.AppendLine(txtOutput.Text);
                        sb.AppendLine("\nРаспределение выигрышей (таблица):");
                        sb.Append("Комбинация\t");
                        foreach (DataGridViewColumn col in dgvProfits.Columns)
                        {
                            if (col.Index > 0) sb.Append($"Игрок {col.Index}\t");
                        }
                        sb.AppendLine();
                        foreach (DataGridViewRow row in dgvProfits.Rows)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                sb.Append($"{cell.Value}\t");
                            }
                            sb.AppendLine();
                        }
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Результаты успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}