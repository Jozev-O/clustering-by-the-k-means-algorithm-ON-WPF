using Core.Algorithms;

namespace Core.Models
{
    public class FilterOptions
    {
        public double MinInfoScore { get; set; } = 0.8;
        public double MaxPValue { get; set; } = 0.05;
        public double MinMaf { get; set; } = 0.01;
        public bool HasHeader { get; set; } = true;
        public string[] SelectedFeatures { get; set; } = new[] { "LOGP", "ZSCORE", "A1FREQ" };
        public GwasDataNormalizer.NormalizationMethod NormalizationMethod { get; set; } = GwasDataNormalizer.NormalizationMethod.Standard;
    }
}
