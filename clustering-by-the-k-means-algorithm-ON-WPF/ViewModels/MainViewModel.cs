using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models;
using Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Visualization;

namespace GwasClusteringApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ClusteringService _clusteringService;
        private readonly ILogger<MainViewModel> _logger;
        private readonly VisualizerBase _visualizer;  // Внедрение визуализатора, если требуется DI

        [ObservableProperty]
        private ObservableCollection<Cluster> clusters = new ObservableCollection<Cluster>();

        [ObservableProperty]
        private FilterOptions filterOptions = new FilterOptions();

        [ObservableProperty]
        private ClusteringOptions clusteringOptions = new ClusteringOptions();

        [ObservableProperty]
        private string filePath = string.Empty;

        [ObservableProperty]
        private int optimalK;

        [ObservableProperty]
        private bool is3DMode;  // Флаг для переключения между 2D и 3D

        [ObservableProperty]
        private bool isProcessing;  // Индикатор прогресса для UI

        public MainViewModel(ClusteringService clusteringService, ILogger<MainViewModel> logger, VisualizerBase visualizer)
        {
            _clusteringService = clusteringService ?? throw new ArgumentNullException(nameof(clusteringService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                _logger.LogWarning("Путь к файлу не указан.");
                return;
            }

            isProcessing = true;
            try
            {
                var data = await _clusteringService.LoadAndPrepareDataAsync(FilePath, FilterOptions);
                _logger.LogInformation($"Данные загружены: {data.Count} точек.");
                // Дополнительная логика, если требуется обновление UI
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке данных.");
                // Отобразить сообщение об ошибке в UI (например, через событие или свойство)
            }
            finally
            {
                isProcessing = false;
            }
        }

        [RelayCommand]
        private async Task DetermineOptimalKAsync()
        {
            isProcessing = true;
            try
            {
                var data = await _clusteringService.LoadAndPrepareDataAsync(FilePath, FilterOptions);  // Повторная загрузка, если данные не кэшированы
                //OptimalK = _clusteringService.DetermineOptimalK(data, clusteringOptions.MinK, clusteringOptions.MaxK);
                _logger.LogInformation($"Оптимальное K: {OptimalK}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при определении оптимального K.");
            }
            finally
            {
                isProcessing = false;
            }
        }

        [RelayCommand]
        private async Task PerformClusteringAsync()
        {
            if (OptimalK <= 0)
            {
                _logger.LogWarning("Оптимальное K не определено.");
                return;
            }

            isProcessing = true;
            try
            {
                var data = await _clusteringService.LoadAndPrepareDataAsync(FilePath, FilterOptions);
                //var result = _clusteringService.PerformClustering(data, OptimalK, ClusteringOptions);
                Clusters.Clear();
                //foreach (var cluster in result)
                //{
                //    Clusters.Add(cluster);
                //}
                _logger.LogInformation($"Кластеризация завершена: {Clusters.Count} кластеров.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении кластеризации.");
            }
            finally
            {
                isProcessing = false;
            }
        }

        [RelayCommand]
        private async Task VisualizeAsync()
        {
            if (Clusters.Count == 0)
            {
                _logger.LogWarning("Нет данных для визуализации.");
                return;
            }

            isProcessing = true;
            try
            {
                // Проверка размерности и редукция, если >3D
                int dimensions = Clusters[0].Centroid.Features.Length;  // Пример проверки
                //var reducedData = dimensions > 3 ? DimensionalityReducer.Reduce(Clusters, Is3DMode ? 3 : 2) : Clusters;

                if (Is3DMode)
                {
                    //_visualizer.Render3D(reducedData);
                }
                else
                {
                    //_visualizer.Render2D(reducedData);
                }
                _logger.LogInformation($"Визуализация завершена в режиме {(Is3DMode ? "3D" : "2D")}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при визуализации.");
            }
            finally
            {
                isProcessing = false;
            }
        }
    }
}