using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CooperativeGameSolver
{
    // Входные данные (характеристическая функция)
    public class GameInput
    {
        [ColumnName("V1")] public float V1 { get; set; }
        [ColumnName("V2")] public float V2 { get; set; }
        [ColumnName("V3")] public float V3 { get; set; }
        [ColumnName("V12")] public float V12 { get; set; }
        [ColumnName("V13")] public float V13 { get; set; }
        [ColumnName("V23")] public float V23 { get; set; }
        [ColumnName("VN")] public float VN { get; set; }
    }

    // Данные для обучения (входные данные + метка для игрока)
    public class PlayerTrainingData
    {
        [ColumnName("V1")] public float V1 { get; set; }
        [ColumnName("V2")] public float V2 { get; set; }
        [ColumnName("V3")] public float V3 { get; set; }
        [ColumnName("V12")] public float V12 { get; set; }
        [ColumnName("V13")] public float V13 { get; set; }
        [ColumnName("V23")] public float V23 { get; set; }
        [ColumnName("VN")] public float VN { get; set; }
        [ColumnName("Label")] public float Payoff { get; set; }
    }

    // Выходные данные для предсказания (выплата одного игрока)
    public class PlayerPrediction
    {
        [ColumnName("Score")]
        public float Payoff { get; set; }
    }

    public class GamePaymentPredictor
    {
        private readonly MLContext _mlContext;
        private ITransformer[] _models; // По одной модели на игрока
        private readonly string[] _modelPaths = { "model_player1.zip", "model_player2.zip", "model_player3.zip" };
        private double[] _rSquared;
        private double[] _rms;

        public GamePaymentPredictor()
        {
            _mlContext = new MLContext(seed: 42);
            _models = new ITransformer[3];
            _rSquared = new double[3];
            _rms = new double[3];
        }

        // Генерация синтетического датасета
        private List<(GameInput, float[] Payoffs)> GenerateSyntheticData(int count)
        {
            var random = new Random(42);
            var data = new List<(GameInput, float[] Payoffs)>();
            var game = new CooperativeGame();

            for (int i = 0; i < count; i++)
            {
                var v = new Dictionary<string, double>
 {
 { "1", random.Next(0, 300) },
 { "2", random.Next(0, 300) },
 { "3", random.Next(0, 300) },
 { "1,2", random.Next(500, 900) },
 { "1,3", random.Next(400, 800) },
 { "2,3", random.Next(300, 700) },
 { "1,2,3", random.Next(800, 1200) }
 };

                var maxCoalitionValue = v.Where(kvp => kvp.Key != "1,2,3").Max(kvp => kvp.Value);
                v["1,2,3"] = Math.Max(v["1,2,3"], maxCoalitionValue + random.Next(50, 200));

                var input = new GameInput
                {
                    V1 = (float)v["1"],
                    V2 = (float)v["2"],
                    V3 = (float)v["3"],
                    V12 = (float)v["1,2"],
                    V13 = (float)v["1,3"],
                    V23 = (float)v["2,3"],
                    VN = (float)v["1,2,3"]
                };

                game.SetGame(3, v);
                var nucleolus = game.CalculateNucleolus();
                var payoffs = nucleolus.Select(x => (float)x).ToArray();

                data.Add((input, payoffs));
            }

            return data;
        }

        // Обучение моделей для каждого игрока
        public void Train(int dataCount = 1000)
        {
            try
            {
                var data = GenerateSyntheticData(dataCount);

                for (int player = 0; player < 3; player++)
                {
                    // Создаём данные для текущего игрока
                    var playerData = data.Select(d => new PlayerTrainingData
                    {
                        V1 = d.Item1.V1,
                        V2 = d.Item1.V2,
                        V3 = d.Item1.V3,
                        V12 = d.Item1.V12,
                        V13 = d.Item1.V13,
                        V23 = d.Item1.V23,
                        VN = d.Item1.VN,
                        Payoff = d.Payoffs[player]
                    }).ToList();

                    var dataView = _mlContext.Data.LoadFromEnumerable(playerData);

                    // Разделение на обучающую и тестовую выборки
                    var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

                    // Конвейер
                    var pipeline = _mlContext.Transforms
 .Concatenate("Features", nameof(PlayerTrainingData.V1), nameof(PlayerTrainingData.V2),
 nameof(PlayerTrainingData.V3), nameof(PlayerTrainingData.V12), nameof(PlayerTrainingData.V13),
 nameof(PlayerTrainingData.V23), nameof(PlayerTrainingData.VN))
 .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
 .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", numberOfTrees: 50));

                    // Обучение
                    _models[player] = pipeline.Fit(split.TrainSet);

                    // Оценка
                    var predictions = _models[player].Transform(split.TestSet);
                    var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label");
                    _rSquared[player] = metrics.RSquared;
                    _rms[player] = metrics.RootMeanSquaredError;

                    // Сохранение модели
                    using (var fileStream = new FileStream(_modelPaths[player], FileMode.Create))
                    {
                        _mlContext.Model.Save(_models[player], dataView.Schema, fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка обучения модели: {ex.Message}");
            }
        }

        // Предсказание для всех игроков
        public float[] Predict(GameInput input)
        {
            try
            {
                for (int player = 0; player < 3; player++)
                {
                    if (_models[player] == null)
                    {
                        if (File.Exists(_modelPaths[player]))
                        {
                            using (var fileStream = new FileStream(_modelPaths[player], FileMode.Open))
                            {
                                _models[player] = _mlContext.Model.Load(fileStream, out var schema);
                            }
                        }
                        else
                        {
                            Train();
                            break; // После обучения все модели будут загружены
                        }
                    }
                }

                var payoffs = new float[3];
                for (int player = 0; player < 3; player++)
                {
                    var predictionEngine = _mlContext.Model.CreatePredictionEngine<GameInput, PlayerPrediction>(_models[player]);
                    var prediction = predictionEngine.Predict(input);
                    payoffs[player] = prediction.Payoff;
                }

                // Корректировка для эффективности (sum = v(N))
                var sum = payoffs.Sum();
                if (Math.Abs(sum - input.VN) > 1e-6)
                {
                    var adjustment = (input.VN - sum) / 3;
                    for (int i = 0; i < 3; i++)
                        payoffs[i] += adjustment;
                }

                return payoffs;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка предсказания: {ex.Message}");
            }
        }

        // Получение метрик модели
        public string GetModelMetrics()
        {
            return $"Метрики модели: R² = [{string.Join(", ", _rSquared.Select(x => x.ToString("F4")))}], RMS = [{string.Join(", ", _rms.Select(x => x.ToString("F2")))}]";
        }
    }
}