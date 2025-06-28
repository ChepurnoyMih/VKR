using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CooperativeGameSolver
{
    public partial class Form1 : Form
    {
        private Dictionary<string, TextBox> coalitionTextBoxes = new Dictionary<string, TextBox>();
        private CooperativeGame game;
        private GamePaymentPredictor predictor;
        private const int MaxPlayers = 10;

        public Form1()
        {
            InitializeComponent();
            game = new CooperativeGame();
            predictor = new GamePaymentPredictor();
            btnCalculate.Enabled = false;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(txtNumPlayers.Text, out int numPlayers) || numPlayers < 1)
                {
                    txtResult.Text = "Ошибка: введите положительное число игроков.";
                    return;
                }
                if (numPlayers > MaxPlayers)
                {
                    txtResult.Text = $"Ошибка: число игроков не должно превышать {MaxPlayers}.";
                    return;
                }

                coalitionTextBoxes.Clear();
                panelCoalitions.Controls.Clear();
                var coalitions = game.GenerateCoalitions(numPlayers);
                int yOffset = 0;

                foreach (var coalition in coalitions)
                {
                    string coalitionKey = string.Join(",", coalition);
                    var label = new Label
                    {
                        Text = $"v({coalitionKey}):",
                        Location = new System.Drawing.Point(0, yOffset),
                        Size = new System.Drawing.Size(100, 20)
                    };

                    var textBox = new TextBox
                    {
                        Location = new System.Drawing.Point(110, yOffset),
                        Size = new System.Drawing.Size(100, 20),
                        Name = $"txtV_{coalitionKey}",
                        Text = numPlayers == 3 ? GetDefaultValue(coalitionKey) : "0"
                    };

                    coalitionTextBoxes[coalitionKey] = textBox;
                    panelCoalitions.Controls.Add(label);
                    panelCoalitions.Controls.Add(textBox);
                    yOffset += 30;
                }

                btnCalculate.Enabled = true;
                txtResult.Text = "Поля для коалиций сгенерированы. Введите выигрыши и нажмите 'Рассчитать'.";
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Ошибка: {ex.Message}";
            }
        }

        private string GetDefaultValue(string coalitionKey)
        {
            var defaults = new Dictionary<string, string>
 {
 { "1", "200" }, { "2", "300" }, { "3", "0" },
 { "1,2", "800" }, { "1,3", "500" }, { "2,3", "650" },
 { "1,2,3", "1000" }
 };
            return defaults.ContainsKey(coalitionKey) ? defaults[coalitionKey] : "0";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                var v = new Dictionary<string, double>();
                foreach (var pair in coalitionTextBoxes)
                {
                    if (!double.TryParse(pair.Value.Text, out double value) || string.IsNullOrWhiteSpace(pair.Value.Text))
                    {
                        txtResult.Text = $"Ошибка: введите корректное числовое значение для коалиции {pair.Key}.";
                        return;
                    }
                    if (value < 0)
                    {
                        txtResult.Text = "Ошибка: выигрыши должны быть неотрицательными.";
                        return;
                    }
                    v[pair.Key] = value;
                }

                int numPlayers = int.Parse(txtNumPlayers.Text);
                string grandCoalitionKey = string.Join(",", Enumerable.Range(1, numPlayers));
                if (!v.ContainsKey(grandCoalitionKey))
                {
                    txtResult.Text = "Ошибка: отсутствует значение для полной коалиции.";
                    return;
                }

                game.SetGame(numPlayers, v);
                var shapley = game.CalculateShapleyVector();
                bool isInCore = game.CheckCore(shapley);
                bool isProfitable = game.CheckProfitability();
                var nucleolus = game.CalculateNucleolus();
                bool isNucleolusInCore = game.CheckCore(nucleolus);

                // Входные данные для нейросети
                var mlInput = new GameInput
                {
                    V1 = (float)(v.ContainsKey("1") ? v["1"] : 0),
                    V2 = (float)(v.ContainsKey("2") ? v["2"] : 0),
                    V3 = (float)(v.ContainsKey("3") ? v["3"] : 0),
                    V12 = (float)(v.ContainsKey("1,2") ? v["1,2"] : 0),
                    V13 = (float)(v.ContainsKey("1,3") ? v["1,3"] : 0),
                    V23 = (float)(v.ContainsKey("2,3") ? v["2,3"] : 0),
                    VN = (float)v[grandCoalitionKey]
                };

                // Предсказание нейросети
                var mlPrediction = predictor.Predict(mlInput);

                // Формирование результата
                string result = "Результаты вычислений:\r\n";
                result += $"Вектор Шепли: ({string.Join(", ", shapley.Select(x => x.ToString("F2")))})\r\n";
                result += $"Нуклеолус: ({string.Join(", ", nucleolus.Select(x => x.ToString("F2")))})\r\n";
                result += $"Предсказание нейросети: ({string.Join(", ", mlPrediction.Select(x => x.ToString("F2")))})\r\n";
                result += predictor.GetModelMetrics() + "\r\n";
                result += $"Сумма Шепли: {shapley.Sum():F2} (должно быть равно v({grandCoalitionKey}) = {v[grandCoalitionKey]})\r\n";
                if (Math.Abs(shapley.Sum() - v[grandCoalitionKey]) > 1e-6)
                    result += $"Предупреждение: Сумма Шепли не равна v({grandCoalitionKey})!\r\n";
                result += $"Сумма Нуклеолуса: {nucleolus.Sum():F2}\r\n";
                if (Math.Abs(nucleolus.Sum() - v[grandCoalitionKey]) > 1e-6)
                    result += $"Предупреждение: Сумма Нуклеолуса не равна v({grandCoalitionKey})!\r\n";
                result += $"Сумма нейросети: {mlPrediction.Sum():F2}\r\n";
                if (Math.Abs(mlPrediction.Sum() - v[grandCoalitionKey]) > 1e-6)
                    result += $"Предупреждение: Сумма нейросети не равна v({grandCoalitionKey})!\r\n";
                result += $"Шепли принадлежит ядру: {(isInCore ? "Да" : "Нет")}\r\n";
                result += $"Нуклеолус принадлежит ядру: {(isNucleolusInCore ? "Да" : "Нет")}\r\n";
                result += $"Выгодно ли формировать полную коалицию: {(isProfitable ? "Да" : "Нет")}\r\n";

                var shapleyExcesses = game.CalculateExcesses(shapley);
                var nucleolusExcesses = game.CalculateExcesses(nucleolus);
                result += "\r\nИзбытки для Шепли:\r\n";
                foreach (var excess in shapleyExcesses.OrderByDescending(ex => ex.Excess))
                    result += $"Коалиция {excess.Coalition}: {excess.Excess:F2}\r\n";
                result += "\r\nИзбытки для Нуклеолуса:\r\n";
                foreach (var excess in nucleolusExcesses.OrderByDescending(ex => ex.Excess))
                    result += $"Коалиция {excess.Coalition}: {excess.Excess:F2}\r\n";

                if (!isInCore)
                {
                    result += "\r\nПричина: следующие коалиции недовольны Шепли:\r\n";
                    foreach (var excess in shapleyExcesses.Where(ex => ex.Excess > 1e-6))
                        result += $"Коалиция {excess.Coalition}: избыток {excess.Excess:F2}\r\n";
                }

                txtResult.Text = result;
            }
            catch (FormatException)
            {
                txtResult.Text = "Ошибка: введите корректные числовые значения.";
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Произошла ошибка: {ex.Message}";
            }
        }
    }
}