using System;
using CredibilityIndex.Domain.Entities;

namespace CredibilityIndex.Application.Common
{
    public static class SnapshotCalculator
    {
        public static void ApplyNewRating(CredibilitySnapshot snapshot, RatingEntity newRating)
        {
            // Formula: NewAvg = ((OldAvg * OldCount) + NewScore) / (OldCount + 1)
            snapshot.AvgAccuracy = ((snapshot.AvgAccuracy * snapshot.RatingCount) + newRating.Accuracy) / (snapshot.RatingCount + 1);
            snapshot.AvgBiasNeutrality = ((snapshot.AvgBiasNeutrality * snapshot.RatingCount) + newRating.BiasNeutrality) / (snapshot.RatingCount + 1);
            snapshot.AvgTransparency = ((snapshot.AvgTransparency * snapshot.RatingCount) + newRating.Transparency) / (snapshot.RatingCount + 1);
            snapshot.AvgSafetyTrust = ((snapshot.AvgSafetyTrust * snapshot.RatingCount) + newRating.SafetyTrust) / (snapshot.RatingCount + 1);

            snapshot.RatingCount += 1;
            
            UpdateOverallScore(snapshot);
        }

        public static void ApplyUpdatedRating(CredibilitySnapshot snapshot, RatingEntity oldRating, RatingEntity newRating)
        {
            // Formula: NewAvg = OldAvg + ((NewScore - OldScore) / TotalCount)
            snapshot.AvgAccuracy += (double)(newRating.Accuracy - oldRating.Accuracy) / snapshot.RatingCount;
            snapshot.AvgBiasNeutrality += (double)(newRating.BiasNeutrality - oldRating.BiasNeutrality) / snapshot.RatingCount;
            snapshot.AvgTransparency += (double)(newRating.Transparency - oldRating.Transparency) / snapshot.RatingCount;
            snapshot.AvgSafetyTrust += (double)(newRating.SafetyTrust - oldRating.SafetyTrust) / snapshot.RatingCount;

            UpdateOverallScore(snapshot);
        }

        private static void UpdateOverallScore(CredibilitySnapshot snapshot)
        {
            double overallAverage = (snapshot.AvgAccuracy + snapshot.AvgBiasNeutrality + snapshot.AvgTransparency + snapshot.AvgSafetyTrust) / 4.0;
            
            double calculatedScore = ((overallAverage - 1.0) / 4.0) * 100.0;
            snapshot.Score = (byte)Math.Clamp(Math.Round(calculatedScore), 0, 100); 
            snapshot.ComputedAt = DateTime.UtcNow;
        }
    }
}