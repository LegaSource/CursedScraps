using CursedScraps.Behaviours.Curses;
using System.Collections.Generic;

namespace CursedScraps.Managers
{
    public class CurseCSManager
    {
        public static bool IsCursed(string planetName)
        {
            if (new System.Random().Next(1, 100) <= GetValueFromPair(ConfigManager.globalChance.Value, planetName))
                return true;
            return false;
        }

        public static CurseEffect GetRandomCurseEffect(string planetName)
        {
            List<CurseEffect> eligibleEffects = GetEligibleCurseEffects(planetName);
            // Sélectionner un effet aléatoire parmi les effets éligibles
            if (eligibleEffects.Count > 0)
                return eligibleEffects[new System.Random().Next(eligibleEffects.Count)];
            else
                return null;
        }

        public static List<CurseEffect> GetEligibleCurseEffects(string planetName)
        {
            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            foreach (CurseEffect effect in CursedScraps.curseEffects)
            {
                for (int i = 0; i < GetValueFromPair(effect.Weight, planetName); i++)
                    eligibleEffects.Add(effect);
            }
            // Suppression de EXPLORATION pour DevilMansion
            if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name.Equals("SDMLevel"))
                eligibleEffects.RemoveAll(c => c.CurseName.Equals(Constants.EXPLORATION));
            return eligibleEffects;
        }

        public static int GetValueFromPair(string valueByMoons, string planetName)
        {
            // Tableau contenant tous les ensembles clé/valeur pour les noms de planètes/valeurs d'importances
            string[] valuePairs = valueByMoons.Split(',');
            int defaultValue = 0;

            foreach (string valuePair in valuePairs)
            {
                // Ensemble clé/valeur pour nom de la planète/valeur d'importance
                string[] valueTab = valuePair.Split(':');
                if (valueTab.Length == 2 && valueTab[0] == planetName)
                    return int.Parse(valueTab[1]); // Retourne la valeur d'importance si la clé est trouvée
                else if (valueTab.Length == 2 && valueTab[0] == "default")
                    defaultValue = int.Parse(valueTab[1]);
            }
            return defaultValue;
        }
    }
}
