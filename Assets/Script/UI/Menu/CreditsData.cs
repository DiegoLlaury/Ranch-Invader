using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Données modulaires pour les crédits — ajoute des entrées dans l'Inspector.
/// </summary>
[CreateAssetMenu(fileName = "CreditsData", menuName = "Ranch Invader/Credits Data")]
public class CreditsData : ScriptableObject
{
    [System.Serializable]
    public struct CreditSection
    {
        [Tooltip("Ex: Programmation, Art, Son...")]
        public string sectionTitle;
        public List<CreditEntry> entries;
    }

    [System.Serializable]
    public struct CreditEntry
    {
        public string name;
        [Tooltip("Optionnel — laisse vide si pas de rôle spécifique")]
        public string role;
    }

    public List<CreditSection> sections;
}
